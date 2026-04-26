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
        private readonly ICacheService _cacheService;

        // cache key
        private const string ALL_CATEGORIES_KEY = "categories:all";
        private const string CATEGORIES_PREFIX = "categories:id:";
        public CategoryService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<CategoryService> logger,ICacheService cacheService) 
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _cacheService = cacheService;
        }
        public async Task<CategoryResponseDto> CreateAsync(CategoryRequestDto dto)
        {
            _logger.LogInformation($"[CategoryService] : Bắt đầu tạo mới danh mục: {dto.Name}.");
            Category category = _mapper.Map < Category > (dto);
            await _unitOfWork.Categories.AddAsync(category);
            await _unitOfWork.SaveChangesAsync();

            var mapperCategogy = _mapper.Map<CategoryResponseDto>(category);

            await _cacheService.RemoveAsync(ALL_CATEGORIES_KEY);
            await _cacheService.SetAsync($"{CATEGORIES_PREFIX}{category.Id}",mapperCategogy);
            _logger.LogInformation("[CategoryService] : Đã lưu category = {id} vào Redis cache.", category.Id);

            _logger.LogInformation($"[CategoryService] : Tạo thành công danh mục mới với Id = {category.Id}.");
            return mapperCategogy;
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

            await _cacheService.RemoveAsync(ALL_CATEGORIES_KEY);
            await _cacheService.RemoveAsync($"{CATEGORIES_PREFIX}{id}");

            _logger.LogInformation($"[CategoryService] : Đã xóa mềm thành công danh mục Id = {id}.");
            return true;
        }

        public async Task<IEnumerable<CategoryResponseDto>> GetAllAsync()
        {
            _logger.LogInformation("[CategoryService] : Bắt đầu lấy danh sách toàn bộ danh mục.");

            var cached = await _cacheService.GetAsync<IEnumerable<CategoryResponseDto>>(ALL_CATEGORIES_KEY);
            if (cached != null) 
            {
                _logger.LogInformation("[CategoryService] : Lấy danh sách category từ Redis cache.");
                return cached;
            }

            var categories = await _unitOfWork.Categories.GetAllAsync(tracked: false);

            var mapperCategories = _mapper.Map<IEnumerable<CategoryResponseDto>>(categories);

            await _cacheService.SetAsync(ALL_CATEGORIES_KEY, mapperCategories, TimeSpan.FromMinutes(5));
            _logger.LogInformation("[CategoryService] : Đã lưu danh sách categories vào Redis cache.");

            _logger.LogInformation($"[CategoryService] : Đã lấy thành công {categories.Count} danh mục.");
            return mapperCategories;
        }

        public async Task<CategoryResponseDto> GetByIdAsync(int id)
        {
            _logger.LogInformation($"[CategoryService] : Bắt đầu tìm kiếm danh mục với Id = {id}.");
            var cached = await _cacheService.GetAsync<CategoryResponseDto>($"{CATEGORIES_PREFIX}{id}");
            if (cached != null)
            {
                _logger.LogInformation("[CategoryService] : Lấy category theo {id} từ Redis cache.", id);
                return cached;
            }

            var existingCategory = await _unitOfWork.Categories.GetAsync(c => c.Id == id, tracked: false);
            if (existingCategory == null)
            {
                _logger.LogWarning($"[CategoryService] : Không tìm thấy danh mục với Id = {id}.");
                return null!;
            }
            var mapperCategogy = _mapper.Map<CategoryResponseDto>(existingCategory);

            await _cacheService.SetAsync($"{CATEGORIES_PREFIX}{existingCategory.Id}", mapperCategogy,TimeSpan.FromMinutes(5));
            _logger.LogInformation("[CategoryService] : Đã lưu category = {id} vào Redis cache.",id);
            return mapperCategogy;
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

            await _cacheService.RemoveAsync(ALL_CATEGORIES_KEY);
            await _cacheService.RemoveAsync($"{CATEGORIES_PREFIX}{id}");

            _logger.LogInformation($"[CategoryService] : Cập nhật thành công danh mục Id = {id}.");
            return true;
        }
    }
}
