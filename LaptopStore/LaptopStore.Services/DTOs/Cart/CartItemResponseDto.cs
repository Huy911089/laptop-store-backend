using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaptopStore.Services.DTOs.Cart
{
    public class CartItemResponseDto
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        // [CartItemResponseDto] : Giá đang lưu trong cart item, là giá user đã từng thấy/chấp nhận.
        public decimal UnitPrice { get; set; }
        // [CartItemResponseDto] : Giá bán hiện tại của product trên hệ thống.
        public decimal CurrentPrice { get; set; }
        public  int Quantity { get; set; }
        public int StockQuantity { get; set; }

        // [CartItemResponseDto] : Tổng tiền theo giá đang lưu trong cart.
        public decimal LineTotal { get; set; }
        // [CartItemResponseDto] : Tổng tiền nếu tính theo giá hiện tại của hệ thống.
        public decimal CurrentLineTotal { get; set; }
        // [CartItemResponseDto] : Đánh dấu item này đã bị đổi giá so với lúc đang lưu trong cart.
        public bool IsPriceChanged { get; set; }

        // [CartItemResponseDto] : Dùng để frontend hiển thị badge như tăng giá/giảm giá.
        public string? PriceChangeMessage { get; set; }
        public string? ThumbnailUrl { get; set; }
    }
}
