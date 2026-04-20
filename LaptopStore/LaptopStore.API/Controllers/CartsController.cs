using LaptopStore.API.Common;
using LaptopStore.Services.DTOs.Cart;
using LaptopStore.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LaptopStore.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CartsController : ControllerBase
    {
        private readonly ICartService _cartService;
        private readonly ILogger<CartsController> _logger;

        public CartsController(ICartService cartService,ILogger<CartsController> logger)
        {
            _cartService = cartService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetMyCart() 
        {
            try 
            {
                var userId = GetCurrentUserId();
                _logger.LogInformation("[CartController] : Nhận request lấy cart của user {UserId}.", userId);
                var cart = await _cartService.GetMyCartAsync(userId);
                return Ok(new ApiResponse<CartResponseDto> 
                {
                    Status = 200,
                    Message = "Lấy giỏ hàng thành công.",
                    Data = cart,
                });
            }
            catch(Exception ex) 
            {
                _logger.LogError(ex, "[CartController] : Lỗi hệ thống khi lấy giỏ hàng.");
                return StatusCode(500, new ApiResponse<object>
                {
                    Status = 500,
                    Message = "Lỗi máy chủ.",
                    Data = null
                });
            }
        }

        [HttpPost("items")]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartRequestDto dto)
        {
            try
            {
                var userId = GetCurrentUserId();
                _logger.LogInformation("[CartController] : Nhận request thêm sản phẩm vào cart của user {UserId}.", userId);
                var result = await _cartService.AddToCartAsync(userId,dto);
                if (result == null) 
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Status = 400,
                        Message = "Không thể thêm sản phẩm vào giỏ hàng. Có thể sản phẩm không tồn tại, hết hàng hoặc vượt quá tồn kho.",
                        Data = null
                    });
                }

                return Ok(new ApiResponse<CartResponseDto>
                {
                    Status = 200,
                    Message = "Thêm sản phẩm vào giỏ hàng thành công.",
                    Data = result
                });
            }
            catch (Exception ex) 
            {
                _logger.LogError(ex, "[CartController] : Lỗi hệ thống khi thêm vào cart.");
                return StatusCode(500, new ApiResponse<object>
                {
                    Status = 500,
                    Message = "Lỗi máy chủ.",
                    Data = null
                });
            }
        }

        [HttpPut("items/{productId}")]
        public async Task<IActionResult> UpdatedQuantity(int productId, [FromBody] UpdateCartItemQuantityDto dto)
        {

            try
            {
                var userId = GetCurrentUserId();
                _logger.LogInformation("[CartController] : Nhận request cập nhật quantity cart item của user {UserId}.", userId);
                var result = await _cartService.UpdateCartItemQuantityAsync(userId, productId, dto.Quantity);
                if (result == null)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Status = 400,
                        Message = "Không thể cập nhật số lượng. Có thể item không tồn tại hoặc vượt quá tồn kho.",
                        Data = null
                    });
                }
                return Ok(new ApiResponse<CartResponseDto>
                {
                    Status = 200,
                    Message = "Cập nhật số lượng sản phẩm trong giỏ hàng thành công.",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[CartController] : Lỗi hệ thống khi cập nhật cart item.");
                return StatusCode(500, new ApiResponse<object>
                {
                    Status = 500,
                    Message = "Lỗi máy chủ.",
                    Data = null
                });

            }
        }

        [HttpDelete("items/{productId}")]
        public async Task<IActionResult> RemoveItem(int productId) 
        {
            try
            {
                var userId = GetCurrentUserId();
                _logger.LogInformation("[CartController] : Nhận request xóa cart item của user {UserId}.", userId);
                var isSuccess = await _cartService.RemoveCartItemAsync(userId, productId);
                if (!isSuccess) 
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Status = 404,
                        Message = "Không tìm thấy sản phẩm trong giỏ hàng.",
                        Data = null
                    });
                }
                return Ok(new ApiResponse<object>
                {
                    Status = 200,
                    Message = "Xóa sản phẩm khỏi giỏ hàng thành công.",
                    Data = null
                });
            }
            catch (Exception ex) 
            {
                _logger.LogError(ex, "[CartController] : Lỗi hệ thống khi xóa cart item.");
                return StatusCode(500, new ApiResponse<object>
                {
                    Status = 500,
                    Message = "Lỗi máy chủ.",
                    Data = null
                });
            }
        }

        [HttpDelete("clear")]
        public async Task<IActionResult> ClearCart() 
        {
            try
            {
                var userId = GetCurrentUserId();
                _logger.LogInformation("[CartController] : Nhận request xóa toàn bộ cart của user {UserId}.", userId);

                await _cartService.ClearCartAsync(userId);

                return Ok(new ApiResponse<object>
                {
                    Status = 200,
                    Message = "Xóa toàn bộ giỏ hàng thành công.",
                    Data = null
                });

            }
            catch (Exception ex) 
            {
                _logger.LogError(ex, "[CartController] : Lỗi hệ thống khi clear cart.");
                return StatusCode(500, new ApiResponse<object>
                {
                    Status = 500,
                    Message = "Lỗi máy chủ.",
                    Data = null
                });
            }
        }

        [HttpPost("sync-prices")]
        public async Task<IActionResult> SyncPrices() 
        {
            try
            {
                var userId = GetCurrentUserId();
                _logger.LogInformation("[CartController] : Nhận request đồng bộ giá cart của user {UserId}.", userId);

                var result = await _cartService.SyncCartPricesAsync(userId);
                if (result == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Status = 404,
                        Message = "Không tìm thấy giỏ hàng.",
                        Data = null
                    });
                }
                return Ok(new ApiResponse<SyncCartPricesResponseDto>
                {
                    Status = 200,
                    Message = "Đồng bộ giá giỏ hàng thành công.",
                    Data = result
                });
            }
            catch (Exception ex) 
            {
                _logger.LogError(ex, "[CartController] : Lỗi hệ thống khi đồng bộ giá giỏ hàng.");
                return StatusCode(500, new ApiResponse<object>
                {
                    Status = 500,
                    Message = "Lỗi máy chủ.",
                    Data = null
                });
            }
        }

        private Guid GetCurrentUserId()
        {
            // [CartController] : Lấy UserId từ JWT claim thay vì cho client truyền lên để tránh giả mạo giỏ hàng của người khác.
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrWhiteSpace(userIdClaim) || !Guid.TryParse(userIdClaim, out Guid userId))
            {
                throw new UnauthorizedAccessException("Token không hợp lệ hoặc không chứa UserId.");
            }

            return userId;
        }
    }
}
