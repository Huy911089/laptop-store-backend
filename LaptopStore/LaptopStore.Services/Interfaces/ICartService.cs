using LaptopStore.Services.DTOs.Cart;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaptopStore.Services.Interfaces
{
    public interface ICartService
    {
        Task<CartResponseDto> GetMyCartAsync(Guid userId);
        Task<CartResponseDto?> AddToCartAsync(Guid userId, AddToCartRequestDto dto);
        Task<CartResponseDto?> UpdateCartItemQuantityAsync(Guid userId, int productId, int quantity);
        Task<bool> RemoveCartItemAsync(Guid userId, int productId);
        Task<bool> ClearCartAsync(Guid userId);
        // [ICartService] : Đồng bộ UnitPrice của cart item theo Product.Price hiện tại sau khi user chấp nhận giá mới.
        Task<SyncCartPricesResponseDto?> SyncCartPricesAsync(Guid userId);
    }
}
