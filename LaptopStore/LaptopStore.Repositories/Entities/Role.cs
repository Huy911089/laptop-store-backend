using System;
using System.Collections.Generic;

namespace LaptopStore.Repositories.Entities
{
    public class Role
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = null!;
        public string? Description { get; set; }

        public ICollection<User> Users { get; set; } = new List<User>();
    }
}