using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaptopStore.Services.DTOs.Category
{
    // [CategoryRequestDto] : Chứa dữ liệu Client gửi lên khi Thêm mới hoặc Cập nhật danh mục.
    public class CategoryRequestDto
    {
        // [CategoryRequestDto] : Bắt lỗi Validation ngay tại DTO, nếu sai Controller sẽ tự động từ chối.
        [Required(ErrorMessage = "Tên danh mục không được để trống")]
        [MaxLength(100, ErrorMessage = "Tên danh mục không được vượt quá 100 ký tự")]
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;
    }
}
