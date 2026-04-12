using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaptopStore.Services.DTOs.Order
{
    public class OrderStatusUpdateRequestDto
    {
        [Required(ErrorMessage = "Trạng thái không được để trống")]
        public string Status { get; set; } = string.Empty; // VD: "Pending", "Processing", "Shipped", "Cancelled"
    }
}
