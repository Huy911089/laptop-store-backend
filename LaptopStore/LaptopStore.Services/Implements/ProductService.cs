using AutoMapper;
using LaptopStore.Repositories.Entities;
using LaptopStore.Repositories.Interfaces;
using LaptopStore.Services.DTOs.Common;
using LaptopStore.Services.DTOs.Product;
using LaptopStore.Services.Helpers;
using LaptopStore.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Linq.Expressions;

namespace LaptopStore.Services.Implements
{
    public class ProductService : IProductService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<ProductService> _logger;
        private readonly IProductQueryBuilder _productQueryBuilder;
        private readonly ICacheService _cacheService;
        // cache key
        private const string ALL_PRODUCTS_KEY = "Products:all";
        private const string PRODUCTS_PREFIX = $"Products:id:";
        private static string BuildProductQueryCacheKey(ProductQueryParametersDto query) 
        {
            // [ProductService] : Chuẩn hóa chuỗi string (Xóa khoảng trắng thừa và đưa về chữ thường) để tránh lưu trùng lặp cache.
            string keyword = string.IsNullOrWhiteSpace(query.Keyword) ? "none" : query.Keyword.Trim().ToLower();
            string sortBy = string.IsNullOrWhiteSpace(query.SortBy) ? "default" : query.SortBy.Trim().ToLower();

            // [ProductService] : Xử lý an toàn các giá trị nullable, gán cho chúng một giá trị mặc định rõ nghĩa.
            string brandId = query.BrandId?.ToString() ?? "all";
            string categoryId = query.CategoryId?.ToString() ?? "all";
            string minPrice = query.MinPrice?.ToString() ?? "0";
            string maxPrice = query.MaxPrice?.ToString() ?? "max";

            // [ProductService] : Sắp xếp mảng trước khi join để [1,2] và [2,1] đều ra chung 1 key là "1-2"
            string brandIds = query.BrandIds == null ? "all" : string.Join("-", query.BrandIds.OrderBy(id => id));
            string categoryIds = query.CategoryIds == null ? "all" : string.Join("-", query.CategoryIds.OrderBy(id => id));
            string cpus = query.Cpus == null ? "all" : string.Join("-", query.Cpus.OrderBy(c => c));
            string rams = query.Rams == null ? "all" : string.Join("-", query.Rams.OrderBy(r => r));
            string storages = query.Storages == null ? "all" : string.Join("-", query.Storages.OrderBy(s => s));
            string vgas = query.Vgas == null ? "all" : string.Join("-", query.Vgas.OrderBy(v => v));
            string screens = query.ScreenSizes == null ? "all" : string.Join("-", query.ScreenSizes.OrderBy(s => s));

            // [ProductService] : Format key rõ ràng, dễ dàng debug trên các công cụ quản lý Redis.
            return $"products:query:{query.PageIndex}:{query.PageSize}:{keyword}:b={brandId}:bs={brandIds}:c={categoryId}:cs={categoryIds}:cpu={cpus}:ram={rams}:storage={storages}:vga={vgas}:screen={screens}:min={minPrice}:max={maxPrice}:sort={sortBy}";
        }

        public ProductService(IUnitOfWork unitOfWork, IMapper mapper, ILogger<ProductService> logger, IProductQueryBuilder productQueryBuilder, ICacheService cacheService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _productQueryBuilder = productQueryBuilder;
            _cacheService = cacheService;
        }

        public async Task<ProductResponseDto> CreateAsync(ProductRequestDto dto)
        {
            _logger.LogInformation($"[ProductService] : Bắt đầu tạo mới sản phẩm: {dto.Name}.");

            var product = _mapper.Map<Product>(dto);
            await _unitOfWork.Products.AddAsync(product);
            await _unitOfWork.SaveChangesAsync();

            var createdProduct = await _unitOfWork.Products.GetAsync(
                p => p.ProductId == product.ProductId,
                includeProperties: "Brand,Category,ProductImages",
                tracked: false);
            var mappedProduct = _mapper.Map<ProductResponseDto>(createdProduct);

            await _cacheService.RemoveAsync(ALL_PRODUCTS_KEY);
            // [ProductService] : Có thể cache luôn chi tiết sản phẩm mới tạo.
            await _cacheService.SetAsync($"{PRODUCTS_PREFIX}{product.ProductId}", mappedProduct, TimeSpan.FromMinutes(10));


            _logger.LogInformation($"[ProductService] : Tạo thành công sản phẩm mới với Id = {product.ProductId}.");
            return mappedProduct;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            _logger.LogInformation($"[ProductService] : Bắt đầu xóa mềm sản phẩm Id = {id}.");

            var existingProduct = await _unitOfWork.Products.GetAsync(p => p.ProductId == id);
            if (existingProduct == null)
            {
                _logger.LogWarning($"[ProductService] : Thất bại khi xóa. Không tìm thấy sản phẩm Id = {id}.");
                return false;
            }

            existingProduct.IsDeleted = true;
            await _unitOfWork.SaveChangesAsync();

            await _cacheService.RemoveAsync(ALL_PRODUCTS_KEY);
            await _cacheService.RemoveAsync($"{PRODUCTS_PREFIX}{id}");

            _logger.LogInformation($"[ProductService] : Đã xóa mềm thành công sản phẩm Id = {id}.");
            return true;
        }

        public async Task<IEnumerable<ProductResponseDto>> GetAllAsync()
        {
            _logger.LogInformation("[ProductService] : Bắt đầu lấy danh sách toàn bộ sản phẩm.");

            var cached = await _cacheService.GetAsync<List<ProductResponseDto>>(ALL_PRODUCTS_KEY);
            if (cached != null) 
            {
                _logger.LogInformation("[ProductService] : Trả danh sách sản phẩm từ Redis cache.");
                return cached;
            }

            var products = await _unitOfWork.Products.GetAllAsync(
                filter: p => !p.IsDeleted,
                includeProperties: "Brand,Category,ProductImages",
                tracked: false);
            var mappedProducts = _mapper.Map<IEnumerable<ProductResponseDto>>(products);

            // [ProductService] : Lưu lại danh sách product vào Redis để các request sau đọc nhanh hơn.
            await _cacheService.SetAsync(ALL_PRODUCTS_KEY, mappedProducts,TimeSpan.FromMinutes(5));

            _logger.LogInformation($"[ProductService] : Đã lấy thành công {products.Count} sản phẩm.");
            return mappedProducts;
        }

        public async Task<ProductResponseDto> GetByIdAsync(int id)
        {
            _logger.LogInformation($"[ProductService] : Bắt đầu tìm kiếm sản phẩm với Id = {id}.");

            var cached = await _cacheService.GetAsync<ProductResponseDto>($"{PRODUCTS_PREFIX}{id}");
            if (cached != null) 
            {
                _logger.LogInformation("[ProductService] : Trả chi tiết sản phẩm Id = {ProductId} từ Redis cache.", id);
                return cached;
            }
            var product = await _unitOfWork.Products.GetAsync(
                p => p.ProductId == id && !p.IsDeleted,
                includeProperties: "Brand,Category,ProductImages",
                tracked: false);

            if (product == null)
            {
                _logger.LogWarning($"[ProductService] : Không tìm thấy sản phẩm với Id = {id}.");
                return null!;
            }
            var mappedProduct = _mapper.Map<ProductResponseDto>(product);

            await _cacheService.SetAsync($"{PRODUCTS_PREFIX}{id}", mappedProduct, TimeSpan.FromMinutes(5));
            _logger.LogInformation("[ProductService] : Đã lưu chi tiết sản phẩm Id = {ProductId} vào cache.", id);
            return mappedProduct;
        }

        public async Task<PagedResultDto<ProductResponseDto>> GetProductsAsync(ProductQueryParametersDto query)
        {
            
            _logger.LogInformation("[ProductService] : Bắt đầu lấy danh sách sản phẩm có phân trang, lọc, sắp xếp và tìm kiếm.");

            var cached = await _cacheService.GetAsync<PagedResultDto<ProductResponseDto>>(BuildProductQueryCacheKey(query));
            if (cached != null) 
            {
                _logger.LogInformation("[ProductService] : Lấy danh sách sản phẩm từ Redis Cache (Key: {CacheKey}).", BuildProductQueryCacheKey(query));
                return cached;
            }

            var filter = _productQueryBuilder.BuildFilter(query);
            var orderBy = _productQueryBuilder.BuildOrderBy(query.SortBy);
            var include = _productQueryBuilder.BuildInclude();

            var (items, totalRecords) = await _unitOfWork.Products.GetPagedAsync
                (
                    filter: filter,
                    orderBy: orderBy,
                    include: include,
                    pageIndex: query.PageIndex,
                    pageSize: query.PageSize,
                    tracked: false
                );


            var mapperProducts = new PagedResultDto<ProductResponseDto>
            {
                Items = _mapper.Map<List<ProductResponseDto>>(items),
                TotalRecords = totalRecords,
                PageIndex = query.PageIndex,
                PageSize = query.PageSize
            };

            await _cacheService.SetAsync(BuildProductQueryCacheKey(query), mapperProducts, TimeSpan.FromMinutes(5));

            _logger.LogInformation(
            "[ProductService] : Lấy danh sách sản phẩm thành công. PageIndex = {PageIndex}, PageSize = {PageSize}, TotalRecords = {TotalRecords}.",
            query.PageIndex,
            query.PageSize,
            totalRecords);

            return mapperProducts;
        }

        public async Task<bool> UpdateAsync(int id, ProductRequestDto dto)
        {
            _logger.LogInformation($"[ProductService] : Bắt đầu cập nhật sản phẩm Id = {id}.");

            var existingProduct = await _unitOfWork.Products.GetAsync(p => p.ProductId == id && !p.IsDeleted);
            if (existingProduct == null)
            {
                _logger.LogWarning($"[ProductService] : Thất bại khi cập nhật. Không tìm thấy sản phẩm Id = {id}.");
                return false;
            }

            _mapper.Map(dto, existingProduct);
            await _unitOfWork.SaveChangesAsync();
            // [ProductService] : Xóa cache cũ để request sau lấy dữ liệu mới nhất từ DB.
            await _cacheService.RemoveAsync(ALL_PRODUCTS_KEY);
            await _cacheService.RemoveAsync($"{PRODUCTS_PREFIX}{id}");

            _logger.LogInformation($"[ProductService] : Cập nhật thành công sản phẩm Id = {id}.");
            return true;
        }
    }
}
