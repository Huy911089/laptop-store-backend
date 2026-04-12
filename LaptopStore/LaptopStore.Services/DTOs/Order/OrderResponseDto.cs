using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaptopStore.Services.DTOs.Order
{
    public class OrderResponseDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public string ShippingAddress { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;

        // Kèm theo danh sách chi tiết đơn hàng
        public List<OrderDetailResponseDto> OrderDetails { get; set; } = new List<OrderDetailResponseDto>();
    }
    public class OrderDetailResponseDto
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }

}
