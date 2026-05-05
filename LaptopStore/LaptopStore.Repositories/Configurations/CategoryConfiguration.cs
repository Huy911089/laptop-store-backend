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
    // [CategoryConfiguration] : Cấu hình bảng Categories.
    public class CategoryConfiguration : IEntityTypeConfiguration<Category>
    {
        public void Configure(EntityTypeBuilder<Category> builder)
        {
            builder.ToTable("Categories");
            builder.HasKey(x => x.CategoryId);
            builder.Property(x => x.Name).IsRequired().HasMaxLength(100);
            // [CategoryConfiguration] : Tự động lọc bỏ các danh mục đã bị xóa mềm trong MỌI câu truy vấn (GetAll, GetById...)
            builder.HasQueryFilter(c => !c.IsDeleted);

        }
    }


}
