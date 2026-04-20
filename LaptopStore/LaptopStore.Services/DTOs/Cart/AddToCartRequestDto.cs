using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaptopStore.Services.DTOs.Cart
{
    public class AddToCartRequestDto
    {
        // [AddToCartRequestDto] : ProductId là bắt buộc để xác định sản phẩm cần thêm vào giỏ hàng.
        [Required]
        public int ProductId { get; set; }

        // [AddToCartRequestDto] : Quantity phải lớn hơn 0 vì không thể thêm 0 hoặc số âm vào giỏ.
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0.")]
        public int Quantity { get; set; }
    }
}
