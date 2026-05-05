using System;
using System.Collections.Generic;

namespace LaptopStore.Repositories.Entities
{
    // [CategoryEntity] : Danh mục sản phẩm (Gaming, Office). Dùng int vì số lượng ít, cần truy vấn cực nhanh để hiển thị Menu.
    public class Category
    {
        public int CategoryId { get; set; }
        public string Name { get; set; } = string.Empty;
        // [CategoryEntity] : Cờ đánh dấu xóa mềm, mặc định là chưa xóa
        public bool IsDeleted { get; set; } = false;
        public string Description { get; set; } = string.Empty;
        public ICollection<Product> Products { get; set; } = new List<Product>();

    }
}