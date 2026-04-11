using LaptopStore.Repositories.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaptopStore.Repositories.Configurations
{
    // [ProductConfiguration] : Cấu hình Fluent API cho bảng Products, thiết lập các ràng buộc dữ liệu và khóa ngoại.
    public class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder) 
        {
            builder.ToTable("Products");
            builder.HasKey(x => x.Id);
            // [ProductConfiguration] : Giới hạn độ dài chuỗi để tối ưu Database, thay vì để mặc định nvarchar(max).
            builder.Property(x => x.Name).IsRequired().HasMaxLength(200);
            builder.Property(x => x.Price).HasColumnType("decimal(18,2)");


            //[ProductConfiguration] : Thiết lập quan hệ 1 - Nhiều với Category. 
            // Dùng Restrict để ngăn chặn việc xóa Danh mục nếu Danh mục đó đang có Sản phẩm.

            builder.HasOne(x => x.Category)
                   .WithMany(p => p.Products) // Giả định Category không chứa ICollection<Product> để tránh lặp vòng, nếu có thì điền p => p.Products
                   .HasForeignKey(x => x.CategoryId)
                   .OnDelete(DeleteBehavior.Restrict);
            // [ProductConfiguration] : Thiết lập quan hệ 1-Nhiều với Brand.
            builder.HasOne(x => x.Brand)
                   .WithMany(p => p.Products)
                   .HasForeignKey(x => x.BrandId)
                   .OnDelete(DeleteBehavior.Restrict);
            // [ProductConfiguration] : Global Query Filter. 
            // Từ nay về sau, MỌI câu lệnh Select/Get lấy từ bảng Product sẽ TỰ ĐỘNG lọc bỏ những dòng có IsDeleted = true
            // Cái này sẽ tự động chăn ở dưới database luôn để tránh ở tầng service mình tự thêm (.Where(p => p.IsDeleted == false))
            builder.HasQueryFilter(p => !p.IsDeleted);

        }
    }
}
