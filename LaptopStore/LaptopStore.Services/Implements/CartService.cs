using AutoMapper;
using LaptopStore.Repositories.Entities;
using LaptopStore.Repositories.Implements;
using LaptopStore.Repositories.Interfaces;
using LaptopStore.Services.DTOs.Cart;
using LaptopStore.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace LaptopStore.Services.Implements
{
    public class CartService : ICartService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<CartService> _logger;

        public CartService(IUnitOfWork unitOfWork,IMapper mapper, ILogger<CartService> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<CartResponseDto?> AddToCartAsync(Guid userId, AddToCartRequestDto dto)
        {
            _logger.LogInformation("[CartService] : Bắt đầu thêm ProductId = {ProductId} vào cart của user {UserId}.", dto.ProductId, userId);
            var product = await _unitOfWork.Products.GetAsync(
                p => p.Id == dto.ProductId,
                includeProperties: "ProductImages",
                tracked: false
                );
            if (product == null) 
            {
                _logger.LogWarning("[CartService] : Không tìm thấy ProductId = {ProductId}.", dto.ProductId);
                return null;
            }
            if (product.StockQuantity <= 0) 
            {
                _logger.LogWarning("[CartService] : ProductId = {ProductId} đã hết hàng.", dto.ProductId);
                return null;
            }
            var cart = await GetOrCreateCartAsync(userId, createIfMissing: true);
            var cartItem = cart!.CartItems.FirstOrDefault(ci => ci.ProductId == dto.ProductId);

            if (cartItem == null)
            {
                if (dto.Quantity > product.StockQuantity)
                {
                    _logger.LogWarning("[CartService] : Quantity yêu cầu vượt quá tồn kho ProductId = {ProductId}.", dto.ProductId);
                    return null;
                }
                cartItem = new CartItem
                {
                    CartId = cart.Id,
                    ProductId = dto.ProductId,

                    // [CartService] : Chốt snapshot giá tại thời điểm thêm mới item vào cart.
                    UnitPrice = product.Price,
                    Quantity = dto.Quantity
                };
                await _unitOfWork.CartItems.AddAsync(cartItem);
            }
            else 
            {
                int newQuantity = cartItem.Quantity + dto.Quantity;
                if (newQuantity > product.StockQuantity) 
                {
                    _logger.LogWarning("[CartService] : Không thể cộng dồn quantity vì vượt quá tồn kho ProductId = {ProductId}.", dto.ProductId);
                    return null;
                }
                // [CartService] : Khi add lại item đã tồn tại, chỉ tăng quantity, không tự động đổi UnitPrice để tránh âm thầm đổi giá user đã thấy.
                cartItem.Quantity = newQuantity;
            }
            await _unitOfWork.SaveChangesAsync();
            var updatedCart = await LoadCartWithDetailsAsync(userId);
            return MapCartToResponse(updatedCart!);
        }

        public async Task<bool> ClearCartAsync(Guid userId)
        {
            _logger.LogInformation("[CartService] : Bắt đầu xóa toàn bộ cart của user {UserId}.", userId);
            var cart = await LoadCartWithDetailsAsync(userId);
            if (cart == null || !cart.CartItems.Any()) 
            {
                _logger.LogInformation("[CartService] : Cart của user {UserId} đã rỗng hoặc chưa tồn tại.", userId);
                return true;
            }
            _unitOfWork.CartItems.RemoveRange(cart.CartItems.ToList());
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("[CartService] : Đã xóa toàn bộ cart item của user {UserId}.", userId);
            return true;
        }

        public async Task<CartResponseDto> GetMyCartAsync(Guid userId)
        {
            _logger.LogInformation("[CartService] : Bắt đầu lấy giỏ hàng của user {UserId}.", userId);
            var cart = await LoadCartWithDetailsAsync(userId);
            if (cart == null) 
            {
                _logger.LogInformation("[CartService] : User {UserId} chưa có cart, trả về cart rỗng.", userId);
                return new CartResponseDto 
                {
                    CartId = Guid.Empty,
                    UserId = userId,
                    TotalItems = 0,
                    TotalAmount = 0,
                    CurrentTotalAmount = 0,
                    HasPriceChanges = false,
                    HasStockIssues = false,
                    Items = new List<CartItemResponseDto>()
                };
            }
            return MapCartToResponse(cart);
        }

        public async Task<bool> RemoveCartItemAsync(Guid userId, int productId)
        {
            _logger.LogInformation("[CartService] : Bắt đầu xóa ProductId = {ProductId} khỏi cart của user {UserId}.", productId, userId);
            var cart = await LoadCartWithDetailsAsync(userId);
            if (cart == null) 
            {
                _logger.LogWarning("[CartService] : Không thể xóa cart item vì user {UserId} chưa có cart.", userId);
                return false;
            }
            var cartItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == productId);
            if (cartItem == null)
            {
                _logger.LogWarning("[CartService] : Không thể xóa vì không tìm thấy ProductId = {ProductId} trong cart.", productId);
                return false;
            }
            _unitOfWork.CartItems.Remove(cartItem);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("[CartService] : Đã xóa thành công ProductId = {ProductId} khỏi cart.", productId);
            return true;
        }

        public async Task<SyncCartPricesResponseDto?> SyncCartPricesAsync(Guid userId)
        {
            _logger.LogInformation("[CartService] : Bắt đầu đồng bộ giá cart theo giá hiện tại cho user {UserId}.", userId);
            var cart = await LoadCartWithDetailsAsync(userId);
            if (cart == null) 
            {
                _logger.LogWarning("[CartService] : Không tìm thấy cart của user {UserId}.", userId);
                return null;
            }
            int updatedItemsCount = 0;
            foreach (var item in cart.CartItems) 
            {
                var currentPrice = item.Product.Price;
                if (item.UnitPrice != currentPrice) 
                {
                    // [CartService] : Đồng bộ snapshot giá trong cart item theo giá hiện tại sau khi user xác nhận.
                    item.UnitPrice = currentPrice;
                    updatedItemsCount++;
                }
            }
            await _unitOfWork.SaveChangesAsync();
            var UpdatedCart = await LoadCartWithDetailsAsync(userId);
            return new SyncCartPricesResponseDto
            {
                UpdatedItemsCount = updatedItemsCount,
                Cart = MapCartToResponse(cart),
            };
        }

        public async Task<CartResponseDto?> UpdateCartItemQuantityAsync(Guid userId, int productId, int quantity)
        {
            _logger.LogInformation("[CartService] : Bắt đầu cập nhật quantity cho ProductId = {ProductId} trong cart của user {UserId}.", productId, userId);
            var cart = await LoadCartWithDetailsAsync(userId);
            if (cart == null) 
            {
                _logger.LogWarning("[CartService] : Không thể cập nhật vì user {UserId} chưa có cart.", userId);
                return null;
            }
            var cartItem = cart.CartItems.FirstOrDefault(ci => ci.ProductId == productId);
            if (cartItem == null)
            {
                _logger.LogWarning("[CartService] : Không tìm thấy CartItem với ProductId = {ProductId} trong cart của user {UserId}.", productId, userId);
                return null;
            }

            var product = await _unitOfWork.Products.GetAsync(
                p => p.Id == productId,
                tracked: false
                );
            if (product == null) 
            {
                _logger.LogWarning("[CartService] : Không thể cập nhật vì product {ProductId} không còn tồn tại.", productId);
                return null;
            }
            if (quantity > product.StockQuantity) 
            {
                _logger.LogWarning("[CartService] : Quantity mới vượt quá tồn kho của ProductId = {ProductId}.", productId);
                return null;
            }
            cartItem.Quantity = quantity;

            await _unitOfWork.SaveChangesAsync();
            var updatedCart = await LoadCartWithDetailsAsync(userId);
            return MapCartToResponse(updatedCart!);
        }




        private async Task<Cart?> GetOrCreateCartAsync(Guid userId, bool createIfMissing)
        {
            var cart = await LoadCartWithDetailsAsync(userId);

            if (cart == null && createIfMissing)
            {
                _logger.LogInformation("[CartService] : User {UserId} chưa có cart, tiến hành tạo mới.", userId);

                cart = new Cart
                {
                    UserId = userId
                };

                await _unitOfWork.Carts.AddAsync(cart);
                await _unitOfWork.SaveChangesAsync();

                cart = await LoadCartWithDetailsAsync(userId);
            }

            return cart;
        }

        private async Task<Cart?> LoadCartWithDetailsAsync(Guid userId)
        {
            // [CartService] : Include sâu để sau này map response không bị thiếu dữ liệu product, image và line total.
            return await _unitOfWork.Carts.GetAsync(
                c => c.UserId == userId,
                includeProperties: "CartItems.Product.ProductImages",
                tracked: true);
        }

        private CartResponseDto MapCartToResponse(Cart cart)
        {
            var items = cart.CartItems.Select(ci =>
            {
                var currentPrice = ci.Product.Price;
                var isPriceChanged = ci.UnitPrice != currentPrice;
                var thumbnail = ci.Product.ProductImages?.FirstOrDefault()?.ImageUrl;

                string? priceChangeMessage = null;

                if (isPriceChanged)
                {
                    priceChangeMessage = currentPrice > ci.UnitPrice
                        ? "Giá sản phẩm đã tăng so với lúc bạn thêm vào giỏ."
                        : "Giá sản phẩm đã giảm so với lúc bạn thêm vào giỏ.";
                }

                return new CartItemResponseDto
                {
                    ProductId = ci.ProductId,
                    ProductName = ci.Product.Name,
                    UnitPrice = ci.UnitPrice,
                    CurrentPrice = currentPrice,
                    Quantity = ci.Quantity,
                    StockQuantity = ci.Product.StockQuantity,
                    LineTotal = ci.UnitPrice * ci.Quantity,
                    CurrentLineTotal = currentPrice * ci.Quantity,
                    IsPriceChanged = isPriceChanged,
                    PriceChangeMessage = priceChangeMessage,
                    ThumbnailUrl = thumbnail
                };
            }).ToList();

            return new CartResponseDto
            {
                CartId = cart.Id,
                UserId = cart.UserId,
                TotalItems = items.Sum(i => i.Quantity),
                TotalAmount = items.Sum(i => i.LineTotal),
                CurrentTotalAmount = items.Sum(i => i.CurrentLineTotal),
                HasPriceChanges = items.Any(i => i.IsPriceChanged),
                HasStockIssues = items.Any(i => i.Quantity > i.StockQuantity),
                Items = items
            };
        }
    }
}
