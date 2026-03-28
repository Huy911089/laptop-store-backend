using System;
using System.Collections.Generic;

namespace LaptopStore.Repositories.Entities
{
    public class User
    {
        public Guid Id { get; set; }

        public string UserName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public string FullName { get; set; } = null!;

        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Guid RoleId { get; set; }
        public Role Role { get; set; } = null!;

        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}   