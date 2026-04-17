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
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("Users");
            builder.HasKey(x => x.Id);
            // Hàm NEWSEQUENTIALID() tạo ra các Guid có tính thứ tự (tăng dần) thay vì hoàn toàn ngẫu nhiên.
            // Nếu dùng Guid ngẫu nhiên (Guid.NewGuid()), dữ liệu sẽ bị chèn vào các vị trí lung tung trong ổ đĩa, gây ra hiện tượng "phân mảnh chỉ mục" (Index Fragmentation).
            builder.Property(x => x.Id).HasDefaultValueSql("NEWSEQUENTIALID()");

            builder.Property(x => x.FullName).IsRequired().HasMaxLength(100);
            builder.Property(x => x.Email).IsRequired().HasMaxLength(150);
            builder.Property(x => x.PhoneNumber).HasMaxLength(20);
            builder.Property(x => x.Address).HasMaxLength(500);

            // [UserConfiguration] : Tạo Unique Index cho Email để đảm bảo không có 2 tài khoản trùng Email.
            builder.HasIndex(x => x.Email).IsUnique();

            // [UserConfiguration] : Quan hệ 1-Nhiều với Role. Không cho phép xóa Role nếu đang có User thuộc Role đó (Restrict).
            builder.HasOne(x => x.Role)
                   .WithMany(x => x.Users)
                   .HasForeignKey(x => x.RoleId)
                   //Restrict (Hạn chế) có nghĩa là: "Nếu một Role đang có ít nhất một User thuộc về nó, thì không ai được phép xóa Role đó".
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasQueryFilter(u => !u.IsDeleted);
        }
    }
}
