using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaptopStore.Services.DTOs.Auth
{
    public class RefreshTokenRequestDto
    {
        // [RefreshTokenRequestDto] : Access token cũ dùng để backend đọc claim user ngay cả khi token đã hết hạn.
        [Required]
        public string AccessToken { get; set; } = string.Empty;

        // [RefreshTokenRequestDto] : Refresh token hiện tại mà client đang giữ để xin cấp token mới.
        [Required]
        public string RefreshToken { get; set; } = string.Empty;
    }
}
