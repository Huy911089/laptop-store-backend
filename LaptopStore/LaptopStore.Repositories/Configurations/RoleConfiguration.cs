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
    public class RoleConfiguration : IEntityTypeConfiguration<Role>
    {
        // [RoleConfiguration] : Cấu hình bảng Roles.
        public void Configure(EntityTypeBuilder<Role> builder) 
        {
            builder.ToTable("Roles");
            // [RoleConfiguration] : Sử dụng NEWSEQUENTIALID() để sinh Guid tuần tự, tối ưu hiệu năng Index cho SQL Server.
            builder.HasKey(x => x.Id);
            builder.Property(x => x.Name).HasDefaultValueSql("NEWSEQUENTIALID()");

            builder.Property(x => x.Name).IsRequired().HasMaxLength(50);
        }
    }
}
