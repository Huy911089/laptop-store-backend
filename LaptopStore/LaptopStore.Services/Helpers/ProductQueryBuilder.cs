using LaptopStore.Repositories.Entities;
using LaptopStore.Services.DTOs.Product;
using LaptopStore.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace LaptopStore.Services.Helpers
{
    public class ProductQueryBuilder : IProductQueryBuilder
    {
        // [ProductQueryBuilder] : Ghép toàn bộ điều kiện filter vào 1 biểu thức duy nhất.
        public Expression<Func<Product, bool>> BuildFilter(ProductQueryParametersDto query)
        {
            return p => !p.IsDeleted &&
            // [ProductQueryBuilder] : Search theo từ khóa. Nếu keyword rỗng thì bỏ qua điều kiện này.
            (string.IsNullOrWhiteSpace(query.Keyword) ||
            p.Name.Contains(query.Keyword) ||
            p.Description.Contains(query.Keyword) ||
            p.Cpu.Contains(query.Keyword) ||
            p.Ram.Contains(query.Keyword) ||
            p.Storage.Contains(query.Keyword) ||
            p.Vga.Contains(query.Keyword)
            ) &&
            // [ProductQueryBuilder] : Filter theo CategoryId nếu frontend có truyền.
            (!query.CategoryId.HasValue || p.CategoryId == query.CategoryId.Value) &&

            // [ProductQueryBuilder] : Filter theo nhiều CategoryIds nếu frontend có truyền.
            (query.CategoryIds == null || !query.CategoryIds.Any() || query.CategoryIds.Contains(p.CategoryId)) &&

            // [ProductQueryBuilder] : Filter theo BrandId nếu frontend có truyền.
            (!query.BrandId.HasValue || p.BrandId == query.BrandId.Value) &&

            // [ProductQueryBuilder] : Filter theo nhiều BrandIds nếu frontend có truyền.
            (query.BrandIds == null || !query.BrandIds.Any() || query.BrandIds.Contains(p.BrandId)) &&

            // [ProductQueryBuilder] : Filter theo nhiều CPUs nếu frontend có truyền.
            (query.Cpus == null || !query.Cpus.Any() || query.Cpus.Any(cpu => p.Cpu.Contains(cpu))) &&
            // [ProductQueryBuilder] : Filter theo nhiều RAMs nếu frontend có truyền.
            (query.Rams == null || !query.Rams.Any() || query.Rams.Any(ram => p.Ram.Contains(ram)) &&
            // [ProductQueryBuilder] : Filter theo nhiều Storages nếu frontend có truyền.
            (query.Storages == null || !query.Storages.Any() || query.Storages.Any(storage => p.Storage.Contains(storage))) &&
            // [ProductQueryBuilder] : Filter theo nhiều Vgas nếu frontend có truyền.
            (query.Vgas == null || !query.Vgas.Any() || query.Vgas.Any(vga => p.Vga.Contains(vga))) &&
            // [ProductQueryBuilder] : Filter theo nhiều ScreenSizes nếu frontend có truyền.
            (query.ScreenSizes == null || !query.ScreenSizes.Any() || query.ScreenSizes.Any(screenSize => p.ScreenSize.Contains(screenSize))) &&

            // [ProductQueryBuilder] : Filter theo khoảng giá.
            (!query.MinPrice.HasValue || p.Price >= query.MinPrice.Value) &&
            (!query.MaxPrice.HasValue || p.Price <= query.MaxPrice.Value));
        }


        public Func<IQueryable<Product>, IQueryable<Product>> BuildInclude()
        {
            // [ProductQueryBuilder] : Include dữ liệu liên quan để response có đủ Brand, Category, ProductImages.
            return q => q
                .Include(p => p.Brand)
                .Include(p => p.Category)
                .Include(p => p.ProductImages);
        }

        public Func<IQueryable<Product>, IOrderedQueryable<Product>> BuildOrderBy(string? sortBy)
        {
            // [ProductQueryBuilder] : Tách riêng phần sort để dễ bảo trì và mở rộng về sau.
            return sortBy?.ToLower() switch
            {
                "price_asc" => q => q.OrderBy(p => p.Price),
                "price_desc" => q => q.OrderByDescending(p => p.Price),
                "name_asc" => q => q.OrderBy(p => p.Name),
                "name_desc" => q => q.OrderByDescending(p => p.Name),
                "oldest" => q => q.OrderBy(p => p.CreatedAt),

                // [ProductQueryBuilder] : Mặc định ưu tiên sản phẩm mới nhất.
                _ => q => q.OrderByDescending(p => p.CreatedAt)
            };
        }
    }
}
