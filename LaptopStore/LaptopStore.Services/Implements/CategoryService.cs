using AutoMapper;
using LaptopStore.Repositories.Entities;
using LaptopStore.Repositories.Implements;
using LaptopStore.Repositories.Interfaces;
using LaptopStore.Services.DTOs.Category;
using LaptopStore.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaptopStore.Services.Implements
{
    public class CategoryService : ICategoryService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<CategoryService> _logger;

        public CategoryService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<CategoryService> logger) 
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }
        public async Task<CategoryResponseDto> CreateAsync(CategoryRequestDto dto)
        {
            _logger.LogInformation($"[CategoryService] : Bắt đầu tạo mới danh mục: {dto.Name}.");
            Category category = _mapper.Map < Category > (dto);
            await _unitOfWork.Categories.AddAsync(category);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation($"[CategoryService] : Tạo thành công danh mục mới với Id = {category.Id}.");
            return _mapper.Map < CategoryResponseDto > (category);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            _logger.LogInformation($"[CategoryService] : Bắt đầu xóa mềm danh mục Id = {id}.");
            var existingCategory = await _unitOfWork.Categories.GetAsync(c => c.Id == id);
            if (existingCategory == null)
            {
                _logger.LogWarning($"[CategoryService] : Thất bại khi xóa. Không tìm thấy danh mục Id = {id}.");
                return false;
            }
            existingCategory!.IsDeleted = true;
            await _unitOfWork.SaveChangesAsync();
            _logger.LogInformation($"[CategoryService] : Đã xóa mềm thành công danh mục Id = {id}.");
            return true;
        }

        public async Task<IEnumerable<CategoryResponseDto>> GetAllAsync()
        {
            _logger.LogInformation("[CategoryService] : Bắt đầu lấy danh sách toàn bộ danh mục.");
            var categories = await _unitOfWork.Categories.GetAllAsync(tracked: false);
            _logger.LogInformation($"[CategoryService] : Đã lấy thành công {categories.Count} danh mục.");
            return _mapper.Map<IEnumerable<CategoryResponseDto>>(categories);
        }

        public async Task<CategoryResponseDto> GetByIdAsync(int id)
        {
            _logger.LogInformation($"[CategoryService] : Bắt đầu tìm kiếm danh mục với Id = {id}.");
            var existingCategory = await _unitOfWork.Categories.GetAsync(c => c.Id == id, tracked: false);
            if (existingCategory == null)
            {
                _logger.LogWarning($"[CategoryService] : Không tìm thấy danh mục với Id = {id}.");
                return null!;
            }
            return _mapper.Map<CategoryResponseDto>(existingCategory);
        }

        public async Task<bool> UpdateAsync(int id, CategoryRequestDto dto)
        {
            _logger.LogInformation($"[CategoryService] : Bắt đầu cập nhật danh mục Id = {id}.");
            var existingCategory = await _unitOfWork.Categories.GetAsync(c => c.Id == id);
            if (existingCategory == null) 
            {
                _logger.LogWarning($"[CategoryService] : Thất bại khi cập nhật. Không tìm thấy danh mục Id = {id}.");
                return false;
            }
            // Dùng AutoMapper để đè dữ liệu mới từ dto vào existingCategory
            // 2. Tự động đổ dữ liệu mới. Tracking sẽ tự nhận diện sự thay đổi.
            _mapper.Map(dto, existingCategory);

            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation($"[CategoryService] : Cập nhật thành công danh mục Id = {id}.");
            return true;
        }
    }
}
