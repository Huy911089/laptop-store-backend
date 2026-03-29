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

        // Khóa ngoại tới Role
        public Guid RoleId { get; set; }
        public Role Role { get; set; } = null!;

        // Navigation properties
        public Cart? Cart { get; set; }
        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}