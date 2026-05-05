using System;
using System.Collections.Generic;

namespace LaptopStore.Repositories.Entities
{
    // [BrandEntity] : Thương hiệu (Dell, Asus, HP). Tương tự Category, dùng int tối ưu hiệu năng.
    public class Brand
    {
        public int BrandId { get; set; }
        public string Name { get; set; } = string.Empty;
        public bool IsDeleted { get; set; } = false;
        public string Description { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public ICollection<Product> Products { get; set; } = new List<Product>();

    }
}