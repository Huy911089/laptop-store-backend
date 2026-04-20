using LaptopStore.API.Common;
using LaptopStore.Repositories.Entities;
using LaptopStore.Services.DTOs.Brand;
using LaptopStore.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace LaptopStore.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BrandsController : ControllerBase
    {
        private readonly IBrandService _brandService;
        private readonly ILogger<BrandsController> _logger;

        public BrandsController(IBrandService brandService, ILogger<BrandsController> logger)
        {
            _brandService = brandService;
            _logger = logger;
        }
        /// <summary>
        /// Lấy danh sách toàn bộ thương hiệu.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                _logger.LogInformation("[BrandsController] : Nhận request lấy danh sách toàn bộ thương hiệu.");
                IEnumerable<BrandResponseDto> brands = await _brandService.GetAllAsync();
                return Ok(new ApiResponse<IEnumerable<BrandResponseDto>>
                {
                    Status = 200,
                    Message = "Lấy danh sách thương hiệu thành công.",
                    Data = brands
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[BrandsController] : Lỗi hệ thống khi lấy danh sách thương hiệu.");
                return StatusCode(500, new ApiResponse<IEnumerable<object>>
                {
                    Status = 500,
                    Message = "Lỗi máy chủ.",
                    Data = null,
                });
            }
        }

        /// <summary>
        /// Lấy chi tiết một thương hiệu theo Id.
        /// </summary>

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                _logger.LogInformation("[BrandsController] : Nhận request lấy thương hiệu Id = {Id}.", id);
                BrandResponseDto? brand = await _brandService.GetByIdAsync(id);
                if (brand == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Status = 404,
                        Message = $"Không tìm thấy thương hiệu với Id = {id}.",
                        Data = null,
                    });
                }
                return Ok(new ApiResponse<BrandResponseDto>
                {
                    Status = 200,
                    Message = "Lấy chi tiết thương hiệu thành công.",
                    Data = brand
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[BrandsController] : Lỗi hệ thống khi lấy thương hiệu Id = {Id}.", id);

                return StatusCode(500, new ApiResponse<object>
                {
                    Status = 500,
                    Message = "Lỗi máy chủ.",
                    Data = null
                });
            }
        }

        /// <summary>
        /// Tạo mới một thương hiệu.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] BrandRequestDto dto)
        {
            try
            {
                _logger.LogInformation("[BrandsController] : Nhận request tạo mới thương hiệu.");
                var newBrand = await _brandService.CreateAsync(dto);
                if (newBrand == null)
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Status = 400,
                        Message = "Tên thương hiệu đã tồn tại.",
                        Data = null
                    });
                }
                return Ok(new ApiResponse<BrandResponseDto>
                {
                    Status = 200,
                    Message = "Tạo thương hiệu thành công.",
                    Data = newBrand
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[BrandsController] : Lỗi hệ thống khi tạo mới thương hiệu.");

                return StatusCode(500, new ApiResponse<object>
                {
                    Status = 500,
                    Message = "Lỗi máy chủ.",
                    Data = null
                });
            }
        }

        /// <summary>
        /// Cập nhật thông tin thương hiệu.
        /// </summary>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] BrandRequestDto dto)
        {
            try
            {
                _logger.LogInformation("[BrandsController] : Nhận request cập nhật thương hiệu Id = {Id}.", id);
                bool isSuccess = await _brandService.UpdateAsync(id,dto);
                if (!isSuccess) 
                {
                    return NotFound(new ApiResponse<object> 
                    {
                        Status = 404,
                        Message = $"Không tìm thấy thương hiệu Id = {id} hoặc tên đã bị trùng.",
                        Data = null
                    });
                }

                return Ok(new ApiResponse<object> 
                {
                    Status = 200,
                    Message = "Cập nhật thương hiệu thành công.",
                    Data = null
                });
            }
            catch (Exception ex) 
            {
                _logger.LogError(ex, "[BrandsController] : Lỗi hệ thống khi cập nhật thương hiệu Id = {Id}.", id);

                return StatusCode(500, new ApiResponse<object>
                {
                    Status = 500,
                    Message = "Lỗi máy chủ.",
                    Data = null
                });
            }
        }
        /// <summary>
        /// Xóa mềm một thương hiệu.
        /// </summary>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Detele(int id)
        {
            try
            {
                bool isSuccess = await _brandService.DeleteAsync(id);
                if (!isSuccess) 
                {
                    return BadRequest(new ApiResponse<object>
                    {
                        Status = 400,
                        Message = $"Không thể xóa thương hiệu Id = {id}. Có thể không tồn tại hoặc vẫn còn sản phẩm đang sử dụng.",
                        Data = null
                    });
                }
                return Ok(new ApiResponse<object> 
                {
                    Status = 200,
                    Message = "Xóa thương hiệu thành công.",
                    Data = null,
                });
            }
            catch (Exception ex) {
                _logger.LogError(ex, "[BrandsController] : Lỗi hệ thống khi xóa thương hiệu Id = {Id}.", id);

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
