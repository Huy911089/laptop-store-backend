using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaptopStore.Services.DTOs.Auth
{
    public class AuthResponseDto
    {
        // [AuthResponseDto] : Trả về token để frontend lưu localStorage hoặc context.
        public string AccessToken { get; set; } = string.Empty;
        // [AuthResponseDto] : Thời gian hết hạn để frontend biết khi nào cần đăng nhập lại.
        public DateTime ExpiredAtUtc { get; set; }

        // [AuthResponseDto] : Refresh token được trả về để frontend dùng khi access token hết hạn.
        public string RefreshToken { get; set; } = string.Empty;
        // [AuthResponseDto] : Thời gian hết hạn của refresh token để frontend biết phiên đăng nhập còn hay không.
        public DateTime RefreshTokenExpiredAtUtc { get; set; }

        // [AuthResponseDto] : Thông tin cơ bản để frontend render header/profile.
        public Guid UserId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string RoleName { get; set; } = string.Empty;
    }
}
