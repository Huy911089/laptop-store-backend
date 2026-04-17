using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaptopStore.Services.DTOs.Auth
{
    public class RegisterRequestDto
    {
        // [RegisterRequestDto] : Họ tên dùng để hiển thị thông tin người dùng sau này.
        [Required]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty;
        // [RegisterRequestDto] : Email dùng làm định danh đăng nhập duy nhất.
        [Required]
        [EmailAddress]
        [MaxLength(150)]
        public string Email { get; set; } = string.Empty;
        // [RegisterRequestDto] : Password raw chỉ dùng lúc nhận request, tuyệt đối không lưu trực tiếp xuống DB.
        [Required]
        [MaxLength(6)]
        public string Password { get; set; } = string.Empty;
        // [RegisterRequestDto] : Có thể cho phép null để customer cập nhật sau.
        [MaxLength(20)]
        public string? PhoneNumber { get; set; }
        // [RegisterRequestDto] : Địa chỉ ban đầu có thể chưa nhập cũng không sao.
        [MaxLength(500)]
        public string? Address { get; set; }
    }
}
