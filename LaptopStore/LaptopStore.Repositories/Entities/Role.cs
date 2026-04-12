using System;
using System.Collections.Generic;

namespace LaptopStore.Repositories.Entities
{
    // [RoleEntity] : Phân quyền hệ thống (Admin, Customer).
    public class Role
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty; // Ví dụ: "Admin", "Customer"

        public ICollection<User> Users { get; set; } = new List<User>();
    }
}