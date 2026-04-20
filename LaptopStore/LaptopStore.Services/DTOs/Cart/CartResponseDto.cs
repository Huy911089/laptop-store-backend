using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaptopStore.Services.DTOs.Cart
{
    public class CartResponseDto
    {
        public Guid CartId { get; set; }
        public Guid UserId { get; set; }
        public int TotalItems { get; set; }
        // [CartResponseDto] : Tổng tiền theo giá đang lưu trong cart item.
        public decimal TotalAmount { get; set; }
        // [CartResponseDto] : Tổng tiền theo giá hiện tại của hệ thống.
        public decimal CurrentTotalAmount { get; set; }

        // [CartResponseDto] : Dùng để biết toàn bộ cart có item nào bị đổi giá hay không.
        public bool HasPriceChanges { get; set; }

        // [CartResponseDto] : Dùng để biết cart có item nào vượt quá tồn kho hiện tại hay không.
        public bool HasStockIssues { get; set; }
        public List<CartItemResponseDto> Items { get; set; } = new List<CartItemResponseDto>();
    }
}
