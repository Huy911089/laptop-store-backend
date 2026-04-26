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

            // [ProductQueryBuilder] : Filter theo BrandId nếu frontend có truyền.
            (!query.BrandId.HasValue || p.BrandId == query.BrandId.Value) &&

            // [ProductQueryBuilder] : Filter theo khoảng giá.
            (!query.MinPrice.HasValue || p.Price >= query.MinPrice.Value) &&
            (!query.MaxPrice.HasValue || p.Price <= query.MaxPrice.Value);
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
