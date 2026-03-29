using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaptopStore.Repositories.Entities
{
    // [ProductImageEntity] : Lưu trữ các hình ảnh phụ và chính của laptop. Dùng int cho khóa chính để truy xuất nhanh.
    public class ProductImage
    {
        public int Id { get; set; }
        public string ImageUrl { get; set; } = string.Empty;

        // [ProductImageEntity] : Cờ đánh dấu đây có phải là ảnh đại diện (thumbnail) hiển thị ở trang danh sách hay không.
        public bool IsMain { get; set; }

        // Khóa ngoại liên kết về bảng Product
        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;
    }
}
