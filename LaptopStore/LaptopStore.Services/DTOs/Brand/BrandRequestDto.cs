using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaptopStore.Services.DTOs.Brand
{
    public class BrandRequestDto
    {
        // [BrandRequestDto] : Tên thương hiệu là dữ liệu bắt buộc vì đây là thông tin cốt lõi để phân loại sản phẩm.
        [Required(ErrorMessage = "Tên thương hiệu là bắt buộc.")]
        [MaxLength(100, ErrorMessage = "Tên thương hiệu không được vượt quá 100 ký tự.")]
        public string Name { get; set; } = string.Empty;
    }
}
