using LaptopStore.API.Common;
using LaptopStore.Repositories.Entities;
using LaptopStore.Services.DTOs.Category;
using LaptopStore.Services.Implements;
using LaptopStore.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Net.NetworkInformation;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace LaptopStore.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;
        private readonly ILogger<CategoriesController> _logger;

        public CategoriesController(ICategoryService categoryService, ILogger<CategoriesController> logger)
        {
            _categoryService = categoryService;
            _logger = logger;
        }
        /// <summary>
        /// Lấy danh sách toàn bộ danh mục sản phẩm (Categories).
        /// </summary>
        /// <returns>Trả về danh sách các Category dưới dạng CategoryResponseDto.</returns>
        /// <response code="200">Lấy danh sách thành công.</response>
        /// <response code="500">Lỗi hệ thống từ phía máy chủ.</response>
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                _logger.LogInformation("[CategoriesController] : Nhận request lấy danh sách toàn bộ danh mục.");
                IEnumerable<CategoryResponseDto> categories = await _categoryService.GetAllAsync();
                return Ok(new ApiResponse<IEnumerable<CategoryResponseDto>>
                {
                    Status = 200,
                    Message = "",
                    Data = categories
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[CategoriesController] : Lỗi hệ thống khi lấy danh sách danh mục.");
                return StatusCode(500, new ApiResponse<object>
                {
                    Status = 500,
                    Message = "Lỗi máy chủ cục bộ. Vui lòng thử lại sau.",
                    Data = null
                });
            }
        }

        // GET: api/categories/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById (int id) 
        {
            try
            {
                _logger.LogInformation($"[CategoriesController] : Nhận request lấy danh mục Id = {id}.");
                CategoryResponseDto category = await _categoryService.GetByIdAsync(id);
                if (category == null) 
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Status = 404,
                        Message = $"Không tìm thấy danh mục với Id = {id}.",
                        Data = null
                    });
                }
                return Ok(new ApiResponse<CategoryResponseDto> 
                {
                    Status = 200,
                    Message = "Lấy chi tiết danh mục thành công.",
                    Data = category,
                });
            }
            catch(Exception ex) 
            {
                _logger.LogError(ex, $"[CategoriesController] : Lỗi hệ thống khi lấy danh mục Id = {id}.");
                return StatusCode(500, new ApiResponse<CategoryResponseDto> 
                {
                    Status = 500,
                    Message = "Lỗi máy chủ.",
                    Data = null
                });
            }
        }

        /// <summary>
        /// Tạo mới một danh mục sản phẩm.
        /// </summary>
        /// <param name="dto">Đối tượng chứa thông tin danh mục cần tạo mới (Name).</param>
        /// <returns>Trả về thông tin danh mục vừa được tạo kèm theo đường dẫn tới resource đó.</returns>
        /// <response code="201">Tạo mới danh mục thành công.</response>
        /// <response code="400">Dữ liệu đầu vào không hợp lệ (ví dụ: để trống tên).</response>
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CategoryRequestDto dto) 
        {
            try
            {
                _logger.LogInformation("[CategoriesController] : Nhận request tạo mới danh mục.");
                var result = await _categoryService.CreateAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = result.Id }, new ApiResponse<CategoryResponseDto>
                {
                    Status = 201,
                    Message = "Tạo danh mục thành công.",
                    Data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[CategoriesController] : Lỗi hệ thống khi tạo mới danh mục.");
                return StatusCode(500, new ApiResponse<object> { Status = 500, Message = "Lỗi máy chủ.", Data = null });
            }
        }
        /// <summary>
        /// Cập nhật thông tin của một danh mục hiện có.
        /// </summary>
        /// <param name="id">Id của danh mục cần cập nhật.</param>
        /// <param name="dto">Dữ liệu mới của danh mục.</param>
        /// <returns>Kết quả của quá trình cập nhật.</returns>
        /// <response code="200">Cập nhật danh mục thành công.</response>
        /// <response code="400">Dữ liệu đầu vào không hợp lệ.</response>
        /// <response code="404">Không tìm thấy danh mục để cập nhật.</response>
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] CategoryRequestDto dto)
        {
            try
            {
                _logger.LogInformation($"[CategoriesController] : Nhận request cập nhật danh mục Id = {id}.");

                var isSuccess = await _categoryService.UpdateAsync(id, dto);

                if (!isSuccess)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Status = 404,
                        Message = $"Không tìm thấy danh mục Id = {id} để cập nhật.",
                        Data = null
                    });
                }

                return Ok(new ApiResponse<object>
                {
                    Status = 200,
                    Message = "Cập nhật danh mục thành công.",
                    Data = null
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[CategoriesController] : Lỗi hệ thống khi cập nhật danh mục Id = {id}.");
                return StatusCode(500, new ApiResponse<object> { Status = 500, Message = "Lỗi máy chủ.", Data = null });
            }
        }

        /// <summary>
        /// Thực hiện xóa mềm (Soft Delete) một danh mục.
        /// </summary>
        /// <param name="id">Id của danh mục cần xóa.</param>
        /// <returns>Kết quả của quá trình xóa mềm.</returns>
        /// <response code="200">Xóa mềm danh mục thành công.</response>
        /// <response code="404">Không tìm thấy danh mục để xóa.</response>
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                _logger.LogInformation($"[CategoriesController] : Nhận request xóa mềm danh mục Id = {id}.");

                var isSuccess = await _categoryService.DeleteAsync(id);

                if (!isSuccess)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Status = 404,
                        Message = $"Không tìm thấy danh mục Id = {id} để xóa.",
                        Data = null
                    });
                }

                return Ok(new ApiResponse<object>
                {
                    Status = 200,
                    Message = "Xóa danh mục thành công.",
                    Data = null
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"[CategoriesController] : Lỗi hệ thống khi xóa danh mục Id = {id}.");
                return StatusCode(500, new ApiResponse<object> { Status = 500, Message = "Lỗi máy chủ.", Data = null });
            }
        }
    }
}
