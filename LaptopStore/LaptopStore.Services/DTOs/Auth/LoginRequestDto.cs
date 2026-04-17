using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaptopStore.Services.DTOs.Auth
{
    public class LoginRequestDto
    {
        // [LoginRequestDto] : Email là username trong flow đăng nhập của dự án này.
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;
        // [LoginRequestDto] : Password raw sẽ được so sánh với PasswordHash bằng BCrypt.
        [Required]
        public string? Password { get; set; }
    }
}
