using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaptopStore.Services.DTOs.Brand
{
    public class BrandResponseDto
    {
        // [BrandResponseDto] : Id dùng để frontend thao tác update, delete hoặc lấy chi tiết thương hiệu.
        public int Id { get; set; }
        // [BrandResponseDto] : Tên thương hiệu hiển thị ra giao diện.
        public string Name { get; set; } = string.Empty;
    }
}
