using LaptopStore.Services.DTOs.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaptopStore.Services.Interfaces
{
    public interface IAuthService
    {
        // [IAuthService] : Đăng ký tài khoản customer mới và trả token ngay sau khi tạo thành công.
        Task<AuthResponseDto?> RegisterAsync(RegisterRequestDto dto);
        // [IAuthService] : Xác thực email/password và trả JWT nếu hợp lệ.
        Task<AuthResponseDto?> LoginAsync(LoginRequestDto dto);

        // [IAuthService] : Dùng refresh token để cấp lại access token mới khi access token cũ đã hết hạn.
        Task<AuthResponseDto?> RefreshTokenAsync(RefreshTokenRequestDto dto);

        // [IAuthService] : Thu hồi refresh token khi logout hoặc khi cần chặn phiên đăng nhập.
        Task<bool> RevokeRefreshTokenAsync(string refreshToken);
    }
}
