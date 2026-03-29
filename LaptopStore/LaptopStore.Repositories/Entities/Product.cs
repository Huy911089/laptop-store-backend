using System;
using System.Collections.Generic;

namespace LaptopStore.Repositories.Entities
{
    // [ProductEntity] : Sản phẩm cốt lõi. Dùng int giúp URL thân thiện (vd: /products/105) và tăng tốc độ indexing khi search.
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }

        // Thông số kỹ thuật
        public string Cpu { get; set; } = string.Empty;
        public string Ram { get; set; } = string.Empty;
        public string Storage { get; set; } = string.Empty;
        public string ScreenSize { get; set; } = string.Empty;
        public string Vga { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Khóa ngoại
        public int BrandId { get; set; }
        public Brand Brand { get; set; } = null!;

        public int CategoryId { get; set; }
        public Category Category { get; set; } = null!;

        // Navigation
        public ICollection<ProductImage> ProductImages { get; set; } = new List<ProductImage>();
    }
}