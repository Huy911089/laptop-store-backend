using LaptopStore.Repositories.Entities;
using LaptopStore.Services.Configurations;
using LaptopStore.Services.DTOs.Auth;
using LaptopStore.Services.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace LaptopStore.Services.Implements
{

    public class TokenService : ITokenService
    {
        private readonly JwtSettings _jwtSettings;
        private readonly ILogger<TokenService> _logger;
        public TokenService(IOptions<JwtSettings> jwtOptions, ILogger<TokenService> logger)
        {
            _jwtSettings = jwtOptions.Value;
            _logger = logger;
        }
        /*
          Hàm GenerateAccessToken, chịu trách nhiệm cốt lõi trong luồng Xác thực (Authentication): Tạo ra một JWT (JSON Web Token) sau khi user đăng nhập thành công, sau đó đóng gói nó vào một DTO (Data Transfer Object) để trả về cho Frontend. 
         */
        public TokenResultDto GenerateAccessToken(User user)
        {

            // [TokenService] : Lấy cấu hình JWT từ appsettings để tránh hard-code trong service.
            //string secretKey = _configuration["JwtSettings:Key"]!;
            //string issuer = _configuration["JwtSettings:Issuer"]!;
            //string audience = _configuration["JwtSettings:Audience"]!;
            //int expireMinutes = int.Parse(_configuration["JwtSettings:ExpireMinutes"]!);

            // ------------------------------ "Nguyên liệu sinh ra token ------------------------------//"
            // [GetBytes(secretKey)]: Thuật toán mã hóa của JWT (thường là HMAC SHA256) làm việc với các mảng byte (mã nhị phân) chứ không làm việc trực tiếp với string. Nên ta phải ép kiểu string thành mảng byte chuẩn UTF-8
            //var key = Encoding.UTF8.GetBytes(secretKey);
            var secretKeyBytes = Encoding.UTF8.GetBytes(_jwtSettings.Key);
            // [DateTime.UtcNow]: Đây là một practice cực kỳ chuẩn. Ở Backend, khi lưu thời gian hoặc tính toán thời hạn, luôn luôn dùng giờ UTC thay vì DateTime.Now (giờ local của server). Việc này giúp hệ thống của em chạy đúng ở bất kỳ quốc gia nào, không bị lỗi lệch múi giờ.
            //var expiredAtUtc = DateTime.UtcNow.AddMinutes(expireMinutes);
            var expiresAtUtc = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpireMinutes);

            // [TokenService] : Nhét các claim tối thiểu để hệ thống nhận diện user hiện tại và role của họ.
            // JWT chính là "Căn cước công dân" của User sau khi đăng nhập. Trong căn cước sẽ có tên, tuổi, quê quán... Những thông tin đó trong JWT gọi là Claim
            List<Claim> claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name,user.FullName),
                new Claim(ClaimTypes.Email, user.Email),
                // [ClaimTypes.Role] đặc biệt quan trọng để sau này áp dụng RBAC (Role-Based Access Control). Khi user gửi token lên api lấy dữ liệu, framework sẽ tự động đọc cái role này để biết user là Admin hay Customer để cho phép truy cập.
                new Claim(ClaimTypes.Role,user.Role.Name)
            };

            // "Bản thiết kế" Token (SecurityTokenDescriptor)
            SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = expiresAtUtc,
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience,
                //SigningCredentials (Chữ ký điện tử): Đây là phần quan trọng nhất giúp JWT bảo mật. Hệ thống dùng secretKey cùng với thuật toán HmacSha256Signature để ký lên cái token này.
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(secretKeyBytes),
                    SecurityAlgorithms.HmacSha256Signature)
            };
            // [CreateToken] tạo ra một object trong bộ nhớ
            // [JwtSecurityTokenHandler()]: Đây là class cung cấp sẵn của .NET chuyên làm nhiệm vụ nhào nặn ra cái token hoặc đọc token.
            var tokenHandler = new JwtSecurityTokenHandler();
            var securityToken = tokenHandler.CreateToken(tokenDescriptor);
            // [WriteToken] mới thực sự chuyển nó thành chuỗi text
            var accessToken = tokenHandler.WriteToken(securityToken);
            _logger.LogInformation("[TokenService] : Tạo access token thành công cho user {UserId}.", user.Id);

            return new TokenResultDto
            {
                AccessToken = accessToken,
                AccessTokenExpiredAtUtc = expiresAtUtc
            };
        }

        public string GenerateRefreshToken()
        {
            // [TokenService] : Dùng random cryptographic để refresh token khó bị đoán hoặc brute force.
            var randomBytes = RandomNumberGenerator.GetBytes(64);
            string refreshToken = Convert.ToBase64String(randomBytes);

            _logger.LogInformation("[TokenService] : Tạo refresh token thành công.");
            return refreshToken;
        }

        public ClaimsPrincipal? GetPrincipalFromExpiredToken(string accessToken)
        {

            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                // [TokenService] : Tắt ValidateLifetime để vẫn đọc được claim từ access token đã hết hạn trong flow refresh.
                ValidateLifetime = false,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _jwtSettings.Issuer,
                ValidAudience = _jwtSettings.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtSettings.Key))
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            try
            {
                var principal = tokenHandler.ValidateToken(accessToken, tokenValidationParameters, out SecurityToken securityToken);
                // [TokenService] : Kiểm tra lại thuật toán ký để chắc chắn token được ký đúng chuẩn mong muốn.
                if (securityToken is not JwtSecurityToken jwtSecurityToken ||
                    !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    _logger.LogWarning("[TokenService] : Token không hợp lệ vì sai thuật toán ký.");
                    return null;
                }
                _logger.LogInformation("[TokenService] : Đọc principal từ expired token thành công.");
                return principal;
            }
            catch (Exception ex) 
            {
                _logger.LogWarning(ex, "[TokenService] : Không thể đọc principal từ expired token.");
                return null;
            }
        }
    }
}
