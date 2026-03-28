using System;
using System.Collections.Generic;

namespace LaptopStore.Repositories.Entities
{
    public class Product
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = null!;
        public string? Description { get; set; }

        public decimal Price { get; set; }
        public int StockQuantity { get; set; }

        public string? ImageUrl { get; set; }
        public bool IsActive { get; set; } = true;

        public Guid BrandId { get; set; }
        public Brand Brand { get; set; } = null!;

        public Guid CategoryId { get; set; }
        public Category Category { get; set; } = null!;

        public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    }
}