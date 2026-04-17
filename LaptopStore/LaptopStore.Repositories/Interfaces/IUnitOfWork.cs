using LaptopStore.Repositories.Entities;

namespace LaptopStore.Repositories.Interfaces
{
    // [IUnitOfWork] : Interface quản lý Transaction. Giúp Service gọi nhiều Repository cùng lúc nhưng chỉ lưu (Commit) 1 lần duy nhất, tránh rác dữ liệu nếu có lỗi giữa chừng.
    public interface IUnitOfWork : IDisposable
    {
        // [IUnitOfWork] : Tập hợp các Repository cụ thể. Chỉ khai báo interface, không khởi tạo trực tiếp để tránh tight coupling.
        IGenericRepository<Product> Products { get; }
        IGenericRepository<Category> Categories { get; }
        IGenericRepository<Order> Orders { get; }
        IGenericRepository<CartItem> CartItems { get; }
        IGenericRepository<Cart> Carts { get; }
        IGenericRepository<Brand> Brands { get; }
        IGenericRepository<OrderDetail> OrderDetails { get; }
        IGenericRepository<ProductImage> ProductImages { get; }
        IGenericRepository<Role> Roles { get; }
        IGenericRepository<User> Users { get; }
        IGenericRepository<RefreshToken> RefreshTokens { get; }
        // [IUnitOfWork] : Hàm quan trọng nhất, gọi SaveChangesAsync() của DbContext để áp dụng mọi thay đổi xuống DB.
        Task<int> SaveChangesAsync();
    }
}
