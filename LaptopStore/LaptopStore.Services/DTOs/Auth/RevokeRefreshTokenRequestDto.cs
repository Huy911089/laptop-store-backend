using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaptopStore.Services.DTOs.Auth
{
    public class RevokeRefreshTokenRequestDto
    {
        // [RevokeRefreshTokenRequestDto] : Refresh token cần thu hồi khi logout hoặc quản lý session.
        [Required]
        public string RefreshToken { get; set; } = string.Empty;
    }
}
