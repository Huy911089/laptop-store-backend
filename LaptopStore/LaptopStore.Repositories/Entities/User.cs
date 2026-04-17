using System;
using System.Collections.Generic;

namespace LaptopStore.Repositories.Entities
{
    // [UserEntity] : Lưu trữ thông tin khách hàng và nhân viên. Dùng Guid để ẩn danh số lượng user trong hệ thống.
    public class User
    {
        public Guid Id { get; set; } = Guid.NewGuid(); // Tự động sinh Guid mới khi khởi tạo
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public bool IsDeleted { get; set; } = false;
        // Khóa ngoại tới Role
        public int RoleId { get; set; }
        public Role Role { get; set; } = null!;

        // Navigation properties
        public Cart? Cart { get; set; }
        public ICollection<Order> Orders { get; set; } = new List<Order>();
        // [User] : Một user có thể có nhiều refresh token nếu đăng nhập trên nhiều thiết bị.
        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
    }
}