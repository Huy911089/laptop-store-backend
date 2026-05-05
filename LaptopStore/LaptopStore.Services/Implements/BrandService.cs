using AutoMapper;
using LaptopStore.Repositories.Entities;
using LaptopStore.Repositories.Interfaces;
using LaptopStore.Services.DTOs.Brand;
using LaptopStore.Services.Interfaces;
using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaptopStore.Services.Implements
{
    public class BrandService : IBrandService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<BrandService> _logger;
        private readonly ICacheService _cacheService;

        // Cached
        private const string ALL_BRANDS_KEY = "brands:all";
        private const string BRANDS_PREFIX = "brands:id:";

        public BrandService(IUnitOfWork unitOfWork,IMapper mapper, ILogger<BrandService> logger, ICacheService cacheService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _cacheService = cacheService;
        }

        public async Task<BrandResponseDto?> CreateAsync(BrandRequestDto dto)
        {
            string brandName = dto.Name.Trim();
            _logger.LogInformation("[BrandService] : Bắt đầu tạo mới thương hiệu: {BrandName}.", brandName);

            var existingBrand = await _unitOfWork.Brands.GetAsync(
                b => b.Name.ToLower() == brandName.ToLower(),
                tracked: false
                );

            if (existingBrand != null) 
            {
                _logger.LogWarning("[BrandService] : Tạo thất bại vì thương hiệu {BrandName} đã tồn tại.", brandName);
                return null;
            }
            var brand = _mapper.Map<Brand>(dto);
            brand.Name = brandName;

            await _unitOfWork.Brands.AddAsync(brand);
            await _unitOfWork.SaveChangesAsync();

            var mapperBrand = _mapper.Map<BrandResponseDto?>(brand);

            await _cacheService.RemoveAsync(ALL_BRANDS_KEY);
            await _cacheService.SetAsync($"{BRANDS_PREFIX}{brand.BrandId}", mapperBrand,TimeSpan.FromMinutes(5));

            _logger.LogInformation("[BrandService] : Tạo thành công thương hiệu mới với Id = {Id}.", brand.BrandId);
            return mapperBrand;

        }

        public async Task<bool> DeleteAsync(int id)
        {
            _logger.LogInformation("[BrandService] : Bắt đầu xóa mềm thương hiệu Id = {Id}.", id);
            var existingBrand = await _unitOfWork.Brands.GetAsync(
                b => b.BrandId == id, includeProperties: "Products"
                );
            if (existingBrand == null) 
            {
                _logger.LogWarning("[BrandService] : Thất bại khi xóa. Không tìm thấy thương hiệu Id = {Id}.", id);
                return false;
            }
            // [BrandService] : Không cho xóa brand nếu vẫn còn product đang dùng để tránh dữ liệu mồ côi và lỗi logic filter.
            if (existingBrand.Products != null && existingBrand.Products.Any()) 
            {
                _logger.LogWarning("[BrandService] : Không thể xóa thương hiệu Id = {Id} vì vẫn còn sản phẩm đang sử dụng.", id);
                return false;
            }
            existingBrand.IsDeleted = true;
            await _unitOfWork.SaveChangesAsync();

            await _cacheService.RemoveAsync(ALL_BRANDS_KEY);
            await _cacheService.RemoveAsync($"{BRANDS_PREFIX}{id}");


            _logger.LogInformation("[BrandService] : Đã xóa mềm thành công thương hiệu Id = {Id}.", id);
            return true;
        }

        public async Task<IEnumerable<BrandResponseDto>> GetAllAsync()
        {
            _logger.LogInformation("[BrandService] : Bắt đầu lấy danh sách toàn bộ thương hiệu.");
            var cached = await _cacheService.GetAsync<IEnumerable<BrandResponseDto>>(ALL_BRANDS_KEY);
            if (cached != null) 
            {
                _logger.LogInformation("[BrandService] : Lấy danh sách brands từ Redis cache.");
                return cached;
            }
            ICollection <Brand> brands = await _unitOfWork.Brands.GetAllAsync(tracked: false);

            var mapperBrands = _mapper.Map<IEnumerable<BrandResponseDto>>(brands);

            await _cacheService.SetAsync(ALL_BRANDS_KEY,mapperBrands,TimeSpan.FromMinutes(5));
            _logger.LogInformation("[BrandService] : Đã lưu danh sách brands vào Redis cache.");

            _logger.LogInformation("[BrandService] : Đã lấy thành công {Count} thương hiệu.", brands.Count);
            return mapperBrands;

        }

        public async Task<BrandResponseDto?> GetByIdAsync(int id)
        {
            _logger.LogInformation("[BrandService] : Bắt đầu tìm kiếm thương hiệu với Id = {Id}.", id);

            var cached = await _cacheService.GetAsync<BrandResponseDto?>($"{BRANDS_PREFIX}{id}");
            if (cached != null) 
            {
                _logger.LogInformation("[BrandService] : Lấy brand theo {id} từ Redis cache.",id);
                return cached;
            }

            var existingBrand = await _unitOfWork.Brands.GetAsync(b => b.BrandId == id, tracked: false);
            if (existingBrand == null) 
            {
                _logger.LogWarning("[BrandService] : Không tìm thấy thương hiệu với Id = {Id}.", id);
                return null;
            }

            var mapperBrand = _mapper.Map<BrandResponseDto>(existingBrand);

            await _cacheService.SetAsync($"{BRANDS_PREFIX}{id}", mapperBrand, TimeSpan.FromMinutes(5));
            _logger.LogInformation("[BrandService] : Đã lưu brand = {id} vào Redis cache.",id);

            _logger.LogInformation("[BrandService] : Đã lấy thành công thương hiệu có id là {id}.", id);
            return mapperBrand;
        }

        public async Task<bool> UpdateAsync(int id,BrandRequestDto dto)
        {
            string brandName = dto.Name.Trim();
            _logger.LogInformation("[BrandService] : Bắt đầu cập nhật thương hiệu Id = {Id}.", id);

            var existingBrand = await _unitOfWork.Brands.GetAsync(b => b.BrandId == id);
            if (existingBrand == null) 
            {
                _logger.LogWarning("[BrandService] : Thất bại khi cập nhật. Không tìm thấy thương hiệu Id = {Id}.", id);
                return false;
            }
            // [BrandService] : Kiểm tra xem tên mới có bị trùng với brand khác không.
            var duplicatedBrand = await _unitOfWork.Brands.GetAsync(
                b => b.Name.ToLower() == brandName.ToLower() && b.BrandId != id, tracked: false
                );
            if (duplicatedBrand != null) 
            {
                _logger.LogWarning("[BrandService] : Thất bại khi cập nhật. Tên thương hiệu {BrandName} đã tồn tại.", brandName);
                return false;
            }

            _mapper.Map(dto, existingBrand);
            existingBrand.Name = brandName;

            await _unitOfWork.SaveChangesAsync();

            await _cacheService.RemoveAsync(ALL_BRANDS_KEY);
            await _cacheService.RemoveAsync($"{BRANDS_PREFIX}{id}");

            _logger.LogInformation("[BrandService] : Cập nhật thành công thương hiệu Id = {Id}.", id);
            return true;
        }
    }
}
