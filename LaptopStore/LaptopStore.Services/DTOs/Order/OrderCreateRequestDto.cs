using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaptopStore.Services.DTOs.Order
{
    public class OrderCreateRequestDto
    {
        [Required(ErrorMessage = "Mã người dùng không được để trống")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Địa chỉ giao hàng không được để trống")]
        public string ShippingAddress { get; set; } = string.Empty;

        [Required(ErrorMessage = "Số điện thoại không được để trống")]
        [Phone(ErrorMessage = "Số điện thoại không đúng định dạng")]
        public string PhoneNumber { get; set; } = string.Empty;

        // [OrderCreate] : Một đơn hàng phải có danh sách các món đồ đi kèm
        [Required]
        [MinLength(1, ErrorMessage = "Giỏ hàng không được trống")]
        public List<OrderItemRequestDto> Items { get; set; } = new List<OrderItemRequestDto>();
    }
    public class OrderItemRequestDto
    {
        [Required]
        public int ProductId { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0")]
        public int Quantity { get; set; }
    }
}
