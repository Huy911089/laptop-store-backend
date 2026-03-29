using System;
using System.Collections.Generic;

namespace LaptopStore.Repositories.Entities
{
    // [RoleEntity] : Phân quyền hệ thống (Admin, Customer). Dùng Guid đồng bộ với User.
    public class Role
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = string.Empty; // Ví dụ: "Admin", "Customer"

        public ICollection<User> Users { get; set; } = new List<User>();
    }
}