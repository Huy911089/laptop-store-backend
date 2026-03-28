using System;
using System.Collections.Generic;

namespace LaptopStore.Repositories.Entities
{
    public class Category
    {
        public Guid Id { get; set; }

        public string Name { get; set; } = null!;
        public string? Description { get; set; }

        public Guid? ParentCategoryId { get; set; }
        public Category? ParentCategory { get; set; }

        public ICollection<Category> Children { get; set; } = new List<Category>();
        public ICollection<Product> Products { get; set; } = new List<Product>();
    }
}