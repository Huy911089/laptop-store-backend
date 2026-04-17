using LaptopStore.Repositories.Entities;
using LaptopStore.Services.DTOs.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace LaptopStore.Services.Interfaces
{
    public interface ITokenService
    {
        // [ITokenService] : Sinh JWT access token từ thông tin user và role.
        TokenResultDto GenerateAccessToken(User user);

        // [ITokenService] : Sinh refresh token dạng chuỗi random đủ mạnh về bảo mật.
        string GenerateRefreshToken();

        // [ITokenService] : Đọc claim từ access token cũ kể cả khi token đã hết hạn để phục vụ flow refresh token.
        ClaimsPrincipal? GetPrincipalFromExpiredToken(string accessToken);
    }
}
