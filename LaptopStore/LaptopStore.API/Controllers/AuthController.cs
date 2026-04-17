using LaptopStore.API.Common;
using LaptopStore.Services.DTOs.Auth;
using LaptopStore.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace LaptopStore.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService,ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto dto)
        {
            try
            {
                _logger.LogInformation("[AuthController] : Nhận request đăng ký tài khoản với email {Email}.", dto.Email);

                var result = await _authService.RegisterAsync(dto);

                if (result == null) 
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Status = 400,
                        Message = "Email đã tồn tại hoặc đăng ký thất bại.",
                        Data = null
                    });
                }
                return Ok(new ApiResponse<AuthResponseDto>
                {
                    Status = 200,
                    Message = "Đăng ký tài khoản thành công.",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AuthController] : Lỗi hệ thống khi đăng ký.");
                return StatusCode(500, new ApiResponse<object>
                {
                    Status = 500,
                    Message = "Lỗi máy chủ.",
                    Data = null
                });
            }
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
        {
            try
            {
                _logger.LogInformation("[AuthController] : Nhận request đăng nhập với email {Email}.", dto.Email);

                var result = await _authService.LoginAsync(dto);

                if (result == null)
                {
                    return Unauthorized(new ApiResponse<object>
                    {
                        Status = 401,
                        Message = "Email hoặc mật khẩu không đúng.",
                        Data = null
                    });
                }

                return Ok(new ApiResponse<AuthResponseDto>
                {
                    Status = 200,
                    Message = "Đăng nhập thành công.",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AuthController] : Lỗi hệ thống khi đăng nhập.");
                return StatusCode(500, new ApiResponse<object>
                {
                    Status = 500,
                    Message = "Lỗi máy chủ.",
                    Data = null
                });
            }
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto dto)
        {
            try
            {
                _logger.LogInformation("[AuthController] : Nhận request refresh token.");

                var result = await _authService.RefreshTokenAsync(dto);

                if (result == null)
                {
                    return Unauthorized(new ApiResponse<object>
                    {
                        Status = 401,
                        Message = "Refresh token không hợp lệ hoặc đã hết hạn.",
                        Data = null
                    });
                }

                return Ok(new ApiResponse<AuthResponseDto>
                {
                    Status = 200,
                    Message = "Làm mới token thành công.",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AuthController] : Lỗi hệ thống khi refresh token.");
                return StatusCode(500, new ApiResponse<object>
                {
                    Status = 500,
                    Message = "Lỗi máy chủ.",
                    Data = null
                });
            }
        }

        [HttpPost("revoke-refresh-token")]
        public async Task<IActionResult> RevokeRefreshToken([FromBody] RevokeRefreshTokenRequestDto dto)
        {
            try
            {
                _logger.LogInformation("[AuthController] : Nhận request revoke refresh token.");

                var result = await _authService.RevokeRefreshTokenAsync(dto.RefreshToken);

                if (!result)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Status = 400,
                        Message = "Refresh token không hợp lệ hoặc đã bị thu hồi.",
                        Data = null
                    });
                }

                return Ok(new ApiResponse<object>
                {
                    Status = 200,
                    Message = "Thu hồi refresh token thành công.",
                    Data = null
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[AuthController] : Lỗi hệ thống khi revoke refresh token.");
                return StatusCode(500, new ApiResponse<object>
                {
                    Status = 500,
                    Message = "Lỗi máy chủ.",
                    Data = null
                });
            }
        }

    }
}
