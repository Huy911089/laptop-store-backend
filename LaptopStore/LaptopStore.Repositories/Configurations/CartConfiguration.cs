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
    // [CartConfiguration] : Cấu hình bảng Carts.
    public class CartConfiguration : IEntityTypeConfiguration<Cart>
    {
        public void Configure(EntityTypeBuilder<Cart> builder)
        {
            builder.ToTable("Carts");
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Id).HasDefaultValueSql("NEWSEQUENTIALID()");

            // [CartConfiguration] : Quan hệ 1-1 cực kỳ quan trọng giữa User và Cart.
            // Khi User bị xóa khỏi hệ thống, Giỏ hàng của họ cũng sẽ bị xóa (Cascade).
            builder.HasOne(x => x.User)
                   .WithOne(x => x.Cart)
                   .HasForeignKey<Cart>(x => x.UserId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }


}
