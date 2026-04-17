using AutoMapper;
using BCrypt.Net;
using LaptopStore.Repositories.Entities;
using LaptopStore.Repositories.Interfaces;
using LaptopStore.Services.DTOs.Auth;
using LaptopStore.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using static System.Net.Mime.MediaTypeNames;
using System.Security.Cryptography;
using Microsoft.Extensions.Options;
using LaptopStore.Services.Configurations;
namespace LaptopStore.Services.Implements
{
    public class AuthService : IAuthService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<AuthService> _logger;
        private readonly ITokenService _tokenService;
        private readonly JwtSettings _jwtSettings;

        public AuthService(IUnitOfWork unitOfWork, IMapper mapper,ILogger<AuthService> logger, ITokenService tokenService, IOptions<JwtSettings> JwtOptions)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _tokenService = tokenService;
            _jwtSettings = JwtOptions.Value;
        }

        public async Task<AuthResponseDto?> LoginAsync(LoginRequestDto dto)
        {
            _logger.LogInformation("[AuthService] : Bắt đầu xử lý đăng nhập cho email {Email}.", dto.Email);
            User? user = await _unitOfWork.Users.GetAsync(u => u.Email.ToLower() == dto.Email.ToLower(), includeProperties: "Role", tracked: false);
            if (user == null) 
            {
                _logger.LogWarning("[AuthService] : Đăng nhập thất bại vì không tìm thấy email {Email}.", dto.Email);
                return null;
            }
            // [AuthService] : So sánh password người dùng nhập với PasswordHash lưu trong DB.
            bool isPasswordValid = BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);
            if (!isPasswordValid)
            {
                _logger.LogWarning("[AuthService] : Đăng nhập thất bại vì sai mật khẩu cho email {Email}.", dto.Email);
                return null;
            }
            _logger.LogInformation("[AuthService] : Đăng nhập thành công cho user {UserId}.", user.Id);
            return await BuildAuthResponseWithRefreshTokenAsync(user);
        }

        public async Task<AuthResponseDto?> RefreshTokenAsync(RefreshTokenRequestDto dto)
        {
            _logger.LogInformation("[AuthService] : Bắt đầu xử lý refresh token.");
            // [AuthService] : Đọc claim từ access token cũ để biết token thuộc về user nào.
            var principal = _tokenService.GetPrincipalFromExpiredToken(dto.AccessToken);
            if (principal == null)
            {
                _logger.LogWarning("[AuthService] : Refresh token thất bại vì access token không hợp lệ.");
                return null;
            }
            var userIdCalim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(userIdCalim) || !Guid.TryParse(userIdCalim, out Guid userId)) 
            {
                _logger.LogWarning("[AuthService] : Refresh token thất bại vì không đọc được userId từ access token.");
                return null;
            }
            var storedRefreshToken = await _unitOfWork.RefreshTokens.GetAsync(rt => rt.Token == dto.RefreshToken && rt.UserId == userId, includeProperties: "User,User.Role", tracked: true);
            if (storedRefreshToken == null)
            {
                _logger.LogWarning("[AuthService] : Refresh token thất bại vì token không tồn tại trong DB.");
                return null;
            }
            if (storedRefreshToken.RevokedAtUtc != null) 
            {
                _logger.LogWarning("[AuthService] : Refresh token thất bại vì token đã bị revoke.");
                return null;
            }
            if (storedRefreshToken.ExpiresAtUtc <= DateTime.UtcNow)
            {
                _logger.LogWarning("[AuthService] : Refresh token thất bại vì token đã hết hạn.");
                return null;
            }
            // [AuthService] : Revoke token cũ ngay khi refresh thành công để tránh token cũ bị tái sử dụng nhiều lần.
            storedRefreshToken.RevokedAtUtc = DateTime.UtcNow;
            string newRefreshTokenValue = _tokenService.GenerateRefreshToken();
            DateTime newRefreshTokenExpiredAtUtc = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpireDays);

            storedRefreshToken.ReplacedByToken = newRefreshTokenValue;

            var newRefreshTokenEntity = new RefreshToken
            {
                UserId = userId,
                Token = newRefreshTokenValue,
                CreatedAtUtc = DateTime.UtcNow,
                ExpiresAtUtc = newRefreshTokenExpiredAtUtc
            };
            await _unitOfWork.RefreshTokens.AddAsync(newRefreshTokenEntity);
            // [AuthService] : Gọi TokenService để sinh access token mới sau khi refresh token được xác minh hợp lệ.
            var accessTokenResult = _tokenService.GenerateAccessToken(storedRefreshToken.User);

            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("[AuthService] : Refresh token thành công cho user {UserId}.", userId);
            return new AuthResponseDto
            {
                AccessToken = accessTokenResult.AccessToken,
                ExpiredAtUtc = accessTokenResult.AccessTokenExpiredAtUtc,
                RefreshToken = newRefreshTokenValue,
                RefreshTokenExpiredAtUtc = newRefreshTokenExpiredAtUtc,
                UserId = storedRefreshToken.User.Id,
                FullName = storedRefreshToken.User.FullName,
                Email = storedRefreshToken.User.Email,
                RoleName = storedRefreshToken.User.Role.Name
            };
        }

        public async Task<AuthResponseDto?> RegisterAsync(RegisterRequestDto dto)
        {
            _logger.LogInformation("[AuthService] : Bắt đầu xử lý đăng ký cho email {Email}.", dto.Email);
            // [AuthService] : Kiểm tra email đã tồn tại chưa để tránh tạo trùng tài khoản.
            User? existingUser = await _unitOfWork.Users.GetAsync(u => u.Email.ToLower() == dto.Email.ToLower(), includeProperties: "Role", tracked: false);
            if (existingUser != null) 
            {
                _logger.LogWarning("[AuthService] : Đăng ký thất bại vì email {Email} đã tồn tại.", dto.Email);
                return null;
            }
            // [AuthService] : Tìm role Customer để gán mặc định cho user mới đăng ký.
            Role? customerRole = await _unitOfWork.Roles.GetAsync(r => r.Name == "Customer", tracked: false);
            if (customerRole == null) 
            {
                _logger.LogError("[AuthService] : Không tìm thấy role Customer trong database.");
                throw new Exception("Role Customer chưa được seed trong database.");
            }
            // [AuthService] : Hash password trước khi lưu để nếu DB lộ thì password gốc vẫn không bị lộ theo.
            string passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

            var newUser = _mapper.Map<User>(dto);
            newUser.PasswordHash = passwordHash;
            newUser.RoleId = customerRole.Id;

            await _unitOfWork.Users.AddAsync(newUser);
            await _unitOfWork.SaveChangesAsync();

            // [AuthService] : Đọc lại user kèm role để tạo token đầy đủ claim.
            User? createdUser = await _unitOfWork.Users.GetAsync(u => u.Id == newUser.Id, includeProperties: "Role", tracked: false);

            _logger.LogInformation("[AuthService] : Đăng ký thành công cho user {UserId}.", newUser.Id);

            return await BuildAuthResponseWithRefreshTokenAsync(createdUser!);
        }

        public async Task<bool> RevokeRefreshTokenAsync(string refreshToken)
        {
            _logger.LogInformation("[AuthService] : Bắt đầu revoke refresh token.");

            var storedToken = await _unitOfWork.RefreshTokens.GetAsync(
                x => x.Token == refreshToken,
                tracked: true);

            if (storedToken == null)
            {
                _logger.LogWarning("[AuthService] : Revoke thất bại vì không tìm thấy refresh token trong DB.");
                return false;
            }

            if (storedToken.RevokedAtUtc != null)
            {
                _logger.LogWarning("[AuthService] : Revoke thất bại vì refresh token đã bị revoke trước đó.");
                return false;
            }

            storedToken.RevokedAtUtc = DateTime.UtcNow;
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("[AuthService] : Revoke refresh token thành công.");
            return true;
        }

        private async Task<AuthResponseDto> BuildAuthResponseWithRefreshTokenAsync(User user) 
        {
            // [AuthService] : Gọi TokenService để sinh access token thay vì tự xử lý JWT trong AuthService.
            var accessTokenResult = _tokenService.GenerateAccessToken(user);
            // [AuthService] : Gọi TokenService để sinh refresh token ngẫu nhiên.
            string refreshToken = _tokenService.GenerateRefreshToken();
            DateTime refreshTokenExpiresAtUtc = DateTime.UtcNow.AddDays(7);
            var refreshTokenEntity = new RefreshToken
            {
                UserId = user.Id,
                Token = refreshToken,
                CreatedAtUtc = DateTime.UtcNow,
                ExpiresAtUtc = refreshTokenExpiresAtUtc,
            };
            await _unitOfWork.RefreshTokens.AddAsync(refreshTokenEntity);
            await _unitOfWork.SaveChangesAsync();

            return new AuthResponseDto
            {
                AccessToken = accessTokenResult.AccessToken,
                ExpiredAtUtc = accessTokenResult.AccessTokenExpiredAtUtc,
                RefreshToken = refreshToken,
                RefreshTokenExpiredAtUtc = refreshTokenExpiresAtUtc,
                UserId = user.Id,
                FullName = user.FullName,
                Email = user.Email,
                RoleName = user.Role.Name
            };
        }


    }
}
