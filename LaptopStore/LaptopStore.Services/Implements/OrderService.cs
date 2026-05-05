using AutoMapper;
using LaptopStore.Repositories.Entities;
using LaptopStore.Repositories.Interfaces;
using LaptopStore.Services.DTOs.Kafka;
using LaptopStore.Services.DTOs.Order;
using LaptopStore.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaptopStore.Services.Implements
{
    public class OrderService : IOrderService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<OrderService> _logger;
        private readonly IMapper _mapper;
        private readonly IKafkaProducerService _kafkaProducerService;
        private static readonly HashSet<string> ValidStatuses = new(StringComparer.OrdinalIgnoreCase)
        {
            "Pending",
            "Processing",
            "Shipped",
            "Cancelled"
        };

        public OrderService(IUnitOfWork unitOfWork, ILogger<OrderService> logger, IMapper mapper, IKafkaProducerService kafkaProducerService)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _mapper = mapper;
            _kafkaProducerService = kafkaProducerService;
        }

        public async Task<CheckoutOrderResultDto> CheckoutAsync(Guid userId, OrderCreateRequestDto dto)
        {
            _logger.LogInformation("[OrderService] : Bắt đầu checkout cho user {UserId}.", userId);
            Cart? cart = await _unitOfWork.Carts.GetAsync(
                    c => c.UserId == userId,
                    includeProperties: "CartItems.Product",
                    tracked: true
                );

            if (cart == null || !cart.CartItems.Any()) 
            {
                _logger.LogWarning("[OrderService] : Checkout thất bại vì cart của user {UserId} đang rỗng.", userId);
                return new CheckoutOrderResultDto 
                {
                    IsSuccess = false,
                    Message = "Giỏ hàng đang trống.",
                    IsCartEmpty = true
                };
            }
            // [OrderService] : Thu thập toàn bộ issue business trước khi quyết định có cho checkout hay không.
            var issues = new List<CheckoutIssueItemDto>();
            foreach (var item in cart.CartItems) //  Cart(asus , dell) asus , dell goi la cartitem   
            {
                Product product = item.Product; 
                if (product == null) 
                {
                    issues.Add(new CheckoutIssueItemDto
                    {
                        ProductId = item.ProductId,
                        ProductName = "Unknown Product",
                        IssueType = "ProductNotFound",
                        Message = "Sản phẩm không còn tồn tại trong hệ thống.",
                        RequestedQuantity = item.Quantity,
                        AvailableStock = 0,
                        CartUnitPrice = item.UnitPrice,
                        CurrentPrice = 0
                    });
                    continue;
                }
                if (item.Quantity > product.StockQuantity)
                {
                    issues.Add(new CheckoutIssueItemDto
                    {
                        ProductId = item.ProductId,
                        ProductName = product.Name,
                        IssueType = "StockChanged",
                        Message = "Số lượng trong giỏ hàng vượt quá tồn kho hiện tại.",
                        RequestedQuantity = item.Quantity,
                        AvailableStock = product.StockQuantity,
                        CartUnitPrice = item.UnitPrice,
                        CurrentPrice = product.Price,
                    });
                }
            }

            if (issues.Any()) 
            {
                _logger.LogWarning("[OrderService] : Checkout bị chặn cho user {UserId} vì cart có thay đổi business.", userId);
                return new CheckoutOrderResultDto
                {
                    IsSuccess = false,
                    Message = "Không thể checkout vì giỏ hàng đã thay đổi. Vui lòng kiểm tra lại.",
                    HasPriceChanges = issues.Any(i => i.IssueType == "PriceChanged"),
                    HasStockIssues = issues.Any(i => i.IssueType == "StockChanged" || i.IssueType == "ProductNotFound"),
                    IsCartEmpty = false,
                    Issues = issues
                };
            }

            await _unitOfWork.BeginTransactionAsync();
            Order? createdOrder = null;
            try
            {
                // [OrderService] : Tới đây mới được xem là cart hợp lệ để chốt đơn và snapshot giá từ CartItem.UnitPrice.
                decimal totalAmount = cart.CartItems.Sum(ci => ci.UnitPrice * ci.Quantity);
                Order order = new Order
                {
                    UserId = userId,
                    OrderDate = DateTime.UtcNow,
                    ShippingAddress = dto.ShippingAddress.Trim(),
                    PhoneNumber = dto.PhoneNumber.Trim(),
                    Status = "Pending",
                    TotalAmount = totalAmount,
                };
                await _unitOfWork.Orders.AddAsync(order);
                await _unitOfWork.SaveChangesAsync();

                List<OrderDetail> orderDetails = new List<OrderDetail>();

                foreach (CartItem item in cart.CartItems)
                {
                    Product product = item.Product;
                    orderDetails.Add(new OrderDetail
                    {
                        OrderId = order.Id,
                        ProductId = product.ProductId,
                        Quantity = item.Quantity,

                        // [OrderService] : UnitPrice của OrderDetail phải lấy từ CartItem.UnitPrice đã được user đồng bộ/chấp nhận trước đó.
                        UnitPrice = item.UnitPrice
                    });
                    product.StockQuantity -= item.Quantity;
                }

                await _unitOfWork.OrderDetails.AddRangeAsync(orderDetails);
                // [OrderService] : Sau khi tạo order thành công thì xóa toàn bộ cart item để giỏ hàng quay về trạng thái rỗng.
                _unitOfWork.CartItems.RemoveRange(cart.CartItems.ToList());
                await _unitOfWork.SaveChangesAsync();

                // [OrderService] : Chốt transaction DB trước để đảm bảo dữ liệu đơn hàng đã thật sự tồn tại.
                await _unitOfWork.CommitTransactionAsync();

                _logger.LogInformation("[OrderService] : Checkout thành công. OrderId = {OrderId}, UserId = {UserId}.", order.Id, userId);

                createdOrder = await _unitOfWork.Orders.GetAsync(
                    o => o.Id == order.Id,
                    includeProperties: "OrderDetails.Product",
                    tracked: false);

                // [OrderService] : Chỉ bắn Kafka sau khi commit DB thành công.
                try
                {
                    OrderCreatedEventDto? orderCreatedEvent = new OrderCreatedEventDto
                    {
                        OrderId = order.Id,
                        UserId = order.UserId,
                        TotalAmount = order.TotalAmount,
                        ShippingAddress = order.ShippingAddress,
                        PhoneNumber = order.PhoneNumber,
                        Status = order.Status,
                        OrderDate = order.OrderDate,
                    };

                    await _kafkaProducerService.ProduceAsync(
                            KafkaTopics.OrderCreated,
                            order.Id.ToString(),
                            orderCreatedEvent
                        );
                    _logger.LogInformation("[OrderService] : Đã publish event OrderCreated cho OrderId = {OrderId}.", order.Id);

                }
                catch (Exception ex)
                {
                    // [OrderService] : Không rollback nữa vì DB đã commit thành công. Kafka fail chỉ log để xử lý sau.
                    _logger.LogError(ex, "[OrderService] : Order đã tạo thành công nhưng publish Kafka thất bại. OrderId = {OrderId}.", order.Id);
                }

                return new CheckoutOrderResultDto
                {
                    IsSuccess = true,
                    Message = "Đặt hàng thành công.",
                    Order = _mapper.Map<OrderResponseDto>(createdOrder!)
                };
            }
            catch (Exception ex) 
            {
                await _unitOfWork.RollbackTransactionAsync();
                _logger.LogError(ex, "[OrderService] : Lỗi khi checkout cho userId = {UserId}. Đã rollback dữ liệu.", userId);
                throw; // Throw lại lỗi hoặc trả về Dto mang thông báo lỗi tuỳ thiết kế của bạn.
            }
        }
            
        public async Task<List<OrderResponseDto>> GetAllOrdersAsync()
        {
            _logger.LogInformation("[OrderService] : Bắt đầu lấy toàn bộ đơn hàng cho admin.");
            List<Order> orders = await _unitOfWork.Orders.GetAllAsync(
                orderBy: q => q.OrderByDescending(o => o.OrderDate),
                includeProperties: "OrderDetails.Product",
                tracked: false
                );
            return _mapper.Map<List<OrderResponseDto>>(orders); 
        }

        public async Task<List<OrderResponseDto>> GetMyOrderAsync(Guid userId)
        {
            _logger.LogInformation("[OrderService] : Bắt đầu lấy danh sách đơn hàng của user {UserId}.", userId);
            List<Order> orders = await _unitOfWork.Orders.GetAllAsync(
                filter: o => o.UserId == userId,
                orderBy: q => q.OrderByDescending(o => o.OrderDate),
                includeProperties: "OrderDetails.Product",
                tracked: false
                );
            return _mapper.Map<List<OrderResponseDto>>(orders);
        }

        public async Task<OrderResponseDto> GetOrderByIdAsync(Guid userId, Guid orderId, bool isManager = false)
        {
            _logger.LogInformation("[OrderService] : Bắt đầu lấy chi tiết OrderId = {OrderId}.", orderId);
            Order? order = await _unitOfWork.Orders.GetAsync(
                o => isManager ? o.Id == orderId : o.Id == orderId && o.UserId == userId,
                includeProperties: "OrderDetails.Product",
                tracked: false
                );
            if (order == null) 
            {
                _logger.LogWarning("[OrderService] : Không tìm thấy OrderId = {OrderId}.", orderId);
                return null!;
            }
            return _mapper.Map<OrderResponseDto>(order);
        }

        public async Task<bool> UpdateOrderStatusAsync(Guid orderId, string status)
        {
            _logger.LogInformation("[OrderService] : Bắt đầu cập nhật trạng thái OrderId = {OrderId} sang {Status}.", orderId, status);
            if (!ValidStatuses.Contains(status))
            {
                _logger.LogWarning("[OrderService] : Trạng thái {Status} không hợp lệ.", status);
                return false;
            }
            Order? order = await _unitOfWork.Orders.GetAsync(
                o => o.Id == orderId,
                includeProperties: "OrderDetails.Product",
                tracked: true
                );
            if (order == null) 
            {
                _logger.LogWarning("[OrderService] : Không tìm thấy OrderId = {OrderId} để cập nhật trạng thái.", orderId);
                return false;
            }
            // [OrderService] : Nếu đơn đã bị hủy thì không nên cho đổi sang trạng thái khác nữa để tránh rối nghiệp vụ.
            if (order.Status.Equals("Cancelled", StringComparison.OrdinalIgnoreCase) && !status.Equals("Cancelled", StringComparison.OrdinalIgnoreCase)) 
            {
                _logger.LogWarning("[OrderService] : Không thể đổi trạng thái của đơn đã bị hủy.");
                return false;
            }

            var oldStatus = order.Status;

            if (!order.Status.Equals("Cancelled", StringComparison.OrdinalIgnoreCase) && status.Equals("Cancelled", StringComparison.OrdinalIgnoreCase)) 
            {
                foreach(var detail in order.OrderDetails) 
                {
                    detail.Product.StockQuantity += detail.Quantity;
                }
            }
            order.Status = status;
            await _unitOfWork.SaveChangesAsync();

            try
            {
                var statusChangedEvent = new OrderStatusChangedEventDto
                {
                    OrderId = orderId,
                    OldStatus = oldStatus,
                    NewStatus = status,
                    ChangedAt = DateTime.UtcNow,
                };

                await _kafkaProducerService.ProduceAsync(KafkaTopics.OrderStatusChanged, order.Id.ToString(), statusChangedEvent);
                _logger.LogInformation("[OrderService] : Đã publish event OrderStatusChanged cho OrderId = {OrderId}.", order.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[OrderService] : Update status thành công nhưng publish Kafka thất bại. OrderId = {OrderId}.", order.Id);
            }

            _logger.LogInformation("[OrderService] : Cập nhật trạng thái OrderId = {OrderId} thành công.", orderId);
            return true;    


        }


        //private OrderResponseDto MapOrderToResponse(Order order)
        //{
        //    return new OrderResponseDto
        //    {
        //        Id = order.Id,
        //        UserId = order.UserId,
        //        OrderDate = order.OrderDate,
        //        TotalAmount = order.TotalAmount,
        //        Status = order.Status,
        //        ShippingAddress = order.ShippingAddress,
        //        PhoneNumber = order.PhoneNumber,
        //        OrderDetails = order.OrderDetails.Select(od => new OrderDetailResponseDto
        //        {
        //            ProductId = od.ProductId,
        //            ProductName = od.Product?.Name ?? string.Empty,
        //            Quantity = od.Quantity,
        //            UnitPrice = od.UnitPrice,
        //            LineTotal = od.UnitPrice * od.Quantity
        //        }).ToList()
        //    };
        //}
    }
}
