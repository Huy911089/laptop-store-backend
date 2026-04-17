using LaptopStore.Repositories.Entities;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace LaptopStore.Repositories.Context
{
    // [LaptopStoreDbContext] : Lớp DbContext chính quản lý kết nối và ánh xạ thực thể xuống cơ sở dữ liệu.
    public class LaptopStoreDbContext : DbContext
    {
        // [LaptopStoreDbContext] : Constructor nhận Options từ file Program.cs (chuẩn Dependency Injection).
        public LaptopStoreDbContext(DbContextOptions<LaptopStoreDbContext> options)
            : base(options)
        {
        }
        // [LaptopStoreDbContext] : Khai báo các DbSet tương ứng với các bảng trong Database.
        public DbSet<Role> Roles { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Brand> Brands { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductImage> ProductImages { get; set; }
        public DbSet<Cart> Carts { get; set; }
        public DbSet<CartItem> CartItems { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderDetail> OrderDetails { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // [LaptopStoreDbContext] : Tự động quét và áp dụng TẤT CẢ các cấu hình Fluent API (IEntityTypeConfiguration) đang có trong project này.
            // Điều này giúp file DbContext không bị phình to hàng ngàn dòng code khi dự án lớn lên.
            modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        }
    }
}