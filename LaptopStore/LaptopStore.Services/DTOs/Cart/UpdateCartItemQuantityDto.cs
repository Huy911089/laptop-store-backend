using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaptopStore.Services.DTOs.Cart
{
    public class UpdateCartItemQuantityDto
    {
        // [UpdateCartItemQuantityDto] : Quantity mới phải lớn hơn 0 để tránh trạng thái item vô nghĩa trong cart.
        [Range(1, int.MaxValue, ErrorMessage = "Số lượng phải lớn hơn 0.")]
        public int Quantity { get; set; }
    }
}
