using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaptopStore.Services.DTOs.Auth
{
    public class TokenResultDto
    {
        // [TokenResultDto] : JWT access token để client gọi API được bảo vệ.
        public string AccessToken { get; set; } = string.Empty;

        // [TokenResultDto] : Thời gian hết hạn của access token.
        public DateTime AccessTokenExpiredAtUtc { get; set; }
    }
}
