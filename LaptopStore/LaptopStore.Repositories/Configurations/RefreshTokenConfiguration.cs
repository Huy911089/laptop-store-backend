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
    public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(EntityTypeBuilder<RefreshToken> builder) 
        {
            // [RefreshTokenConfiguration] : Cấu hình khóa chính.
            builder.HasKey(x => x.Id);

            // [RefreshTokenConfiguration] : Token là bắt buộc và nên có độ dài đủ lớn.
            builder.Property(x => x.Token)
                   .IsRequired()
                   .HasMaxLength(500);

            // [RefreshTokenConfiguration] : Mỗi refresh token phải thuộc về một user.
            builder.HasOne(x => x.User)
                   .WithMany(x => x.RefreshTokens)
                   .HasForeignKey(x => x.UserId)
                   .OnDelete(DeleteBehavior.Cascade);

            // [RefreshTokenConfiguration] : Index unique để tránh trùng token.
            builder.HasIndex(x => x.Token).IsUnique();
        }
    }
}
