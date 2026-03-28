using System;
using System.Collections.Generic;

namespace LaptopStore.Repositories.Entities
{
    public class Brand
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = null!;
        public string? Description { get; set; }

        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}