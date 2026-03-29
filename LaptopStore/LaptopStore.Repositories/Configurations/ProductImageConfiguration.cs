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
    // [ProductImageConfiguration] : Cấu hình bảng ProductImages.
    public class ProductImageConfiguration : IEntityTypeConfiguration<ProductImage>
    {
        public void Configure(EntityTypeBuilder<ProductImage> builder)
        {
            builder.ToTable("ProductImages");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.ImageUrl).IsRequired().HasMaxLength(500);

            // [ProductImageConfiguration] : Quan hệ 1-Nhiều với Product. 
            // Dùng Cascade: Nếu quản trị viên xóa một Laptop, toàn bộ hình ảnh của Laptop đó sẽ tự động bị xóa theo.
            builder.HasOne(x => x.Product)
                   .WithMany(x => x.ProductImages)
                   .HasForeignKey(x => x.ProductId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
