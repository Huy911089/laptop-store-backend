using LaptopStore.Repositories.Context;
using LaptopStore.Repositories.Entities;
using LaptopStore.Repositories.Interfaces;
using System;

namespace LaptopStore.Repositories.Implements
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly LaptopStoreDbContext _context;
        // Khai báo một biến private để lưu trữ ngầm
        private IGenericRepository<Product>? _products;
        private IGenericRepository<Category>? _categories;
        private IGenericRepository<Order>? _orders;
        private IGenericRepository<OrderDetail>? _orderDetail;
        private IGenericRepository<CartItem>? _cartItems;
        private IGenericRepository<Cart>? _carts;
        private IGenericRepository<Brand>? _brands;
        private IGenericRepository<ProductImage>? _productImages;
        private IGenericRepository<Role>? _roles;
        private IGenericRepository<User>? _users;
        private IGenericRepository<RefreshToken>? _refreshTokens;
        public UnitOfWork(LaptopStoreDbContext context) 
        {
            _context = context;
        }
        // [UnitOfWork] : Khởi tạo các Repository (Lazy loading/hoặc khởi tạo trực tiếp). Ở đây khởi tạo trực tiếp cho gọn.
        //Lazy Initialization(Khởi tạo trễ).
        public IGenericRepository<Product> Products => _products ??= new GenericRepository<Product>(_context);

        public IGenericRepository<Category> Categories => _categories ??= new GenericRepository<Category>(_context);

        public IGenericRepository<Order> Orders => _orders ??= new GenericRepository<Order>(_context);

        public IGenericRepository<CartItem> CartItems => _cartItems ??= new GenericRepository<CartItem>(_context);

        public IGenericRepository<Cart> Carts => _carts ??= new GenericRepository<Cart>(_context);

        public IGenericRepository<Brand> Brands => _brands ??= new GenericRepository<Brand>(_context);

        public IGenericRepository<OrderDetail> OrderDetails => _orderDetail ??= new GenericRepository<OrderDetail>(_context);

        public IGenericRepository<ProductImage> ProductImages => _productImages ??= new GenericRepository<ProductImage>(_context);

        public IGenericRepository<Role> Roles => _roles ??= new GenericRepository<Role>(_context);

        public IGenericRepository<User> Users => _users ??= new GenericRepository<User>(_context);
        public IGenericRepository<RefreshToken> RefreshTokens => _refreshTokens ??= new GenericRepository<RefreshToken>(_context);

        public void Dispose()
        {
            _context.Dispose();
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
    }
}
