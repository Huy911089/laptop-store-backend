using LaptopStore.API.Common;
using LaptopStore.Services.DTOs.Common;
using LaptopStore.Services.DTOs.Product;
using LaptopStore.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LaptopStore.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;
        private readonly ILogger<ProductsController> _logger;

        public ProductsController(IProductService productService, ILogger<ProductsController> logger)
        {
            _productService = productService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                _logger.LogInformation("[ProductsController] : Nhận request lấy danh sách toàn bộ sản phẩm.");

                var products = await _productService.GetAllAsync();
                return Ok(new ApiResponse<IEnumerable<ProductResponseDto>>
                {
                    Status = 200,
                    Message = "",
                    Data = products
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ProductsController] : Lỗi hệ thống khi lấy danh sách sản phẩm.");
                return StatusCode(500, new ApiResponse<object>
                {
                    Status = 500,
                    Message = "Lỗi máy chủ.",
                    Data = null
                });
            }
        }

        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                _logger.LogInformation("[ProductsController] : Nhận request lấy sản phẩm Id = {ProductId}.", id);

                var product = await _productService.GetByIdAsync(id);
                if (product == null)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Status = 404,
                        Message = $"Không tìm thấy sản phẩm với Id = {id}.",
                        Data = null
                    });
                }

                return Ok(new ApiResponse<ProductResponseDto>
                {
                    Status = 200,
                    Message = "Lấy chi tiết sản phẩm thành công.",
                    Data = product
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ProductsController] : Lỗi hệ thống khi lấy sản phẩm Id = {ProductId}.", id);
                return StatusCode(500, new ApiResponse<object>
                {
                    Status = 500,
                    Message = "Lỗi máy chủ.",
                    Data = null
                });
            }
        }
        [Authorize(Roles = "Manager")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ProductRequestDto dto)
        {
            try
            {
                _logger.LogInformation("[ProductsController] : Nhận request tạo mới sản phẩm.");

                var created = await _productService.CreateAsync(dto);
                return CreatedAtAction(nameof(GetById), new { id = created.ProductId }, new ApiResponse<ProductResponseDto>
                {
                    Status = 201,
                    Message = "Tạo sản phẩm thành công.",
                    Data = created
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ProductsController] : Lỗi hệ thống khi tạo mới sản phẩm.");
                return StatusCode(500, new ApiResponse<object>
                {
                    Status = 500,
                    Message = "Lỗi máy chủ.",
                    Data = null
                });
            }
        }
        [Authorize(Roles = "Manager")]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] ProductRequestDto dto)
        {
            try
            {
                _logger.LogInformation("[ProductsController] : Nhận request cập nhật sản phẩm Id = {ProductId}.", id);

                var updated = await _productService.UpdateAsync(id, dto);
                if (!updated)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Status = 404,
                        Message = $"Không tìm thấy sản phẩm Id = {id} để cập nhật.",
                        Data = null
                    });
                }

                return Ok(new ApiResponse<object>
                {
                    Status = 200,
                    Message = "Cập nhật sản phẩm thành công.",
                    Data = null
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ProductsController] : Lỗi hệ thống khi cập nhật sản phẩm Id = {ProductId}.", id);
                return StatusCode(500, new ApiResponse<object>
                {
                    Status = 500,
                    Message = "Lỗi máy chủ.",
                    Data = null
                });
            }
        }
        [Authorize(Roles = "Manager")]
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                _logger.LogInformation("[ProductsController] : Nhận request xóa sản phẩm Id = {ProductId}.", id);

                var deleted = await _productService.DeleteAsync(id);
                if (!deleted)
                {
                    return NotFound(new ApiResponse<object>
                    {
                        Status = 404,
                        Message = $"Không tìm thấy sản phẩm Id = {id} để xóa.",
                        Data = null
                    });
                }

                return Ok(new ApiResponse<object>
                {
                    Status = 200,
                    Message = "Xóa sản phẩm thành công.",
                    Data = null
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[ProductsController] : Lỗi hệ thống khi xóa sản phẩm Id = {ProductId}.", id);
                return StatusCode(500, new ApiResponse<object>
                {
                    Status = 500,
                    Message = "Lỗi máy chủ.",
                    Data = null
                });
            }
        }

        [HttpGet("query")]
        public async Task<IActionResult> GetProducts([FromQuery] ProductQueryParametersDto dto)
        {
            try
            {
                _logger.LogInformation(
                        "[ProductsController] : Nhận request lấy danh sách sản phẩm có phân trang. PageIndex = {PageIndex}, PageSize = {PageSize}.",
                        dto.PageIndex,
                        dto.PageSize);

                var result = await _productService.GetProductsAsync(dto);
                return Ok(new ApiResponse<PagedResultDto<ProductResponseDto>>
                {
                    Status = 200,
                    Message = "Lấy danh sách sản phẩm thành công.",
                    Data = result
                });
            }
            catch (Exception ex) 
            {
                _logger.LogError(ex, "[ProductsController] : Lỗi hệ thống khi lấy danh sách sản phẩm có filter/search/sort/paging.");
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
