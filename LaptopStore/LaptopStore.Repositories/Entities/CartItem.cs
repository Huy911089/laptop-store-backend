using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaptopStore.Repositories.Entities
{
    // [CartItemEntity] : Lưu trữ từng dòng sản phẩm bên trong giỏ hàng. Dùng Guid đồng bộ với Cart.
    public class CartItem
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        public Guid CartId { get; set; }
        public Cart Cart { get; set; } = null!;

        // Khóa ngoại liên kết với Product (kiểu int)
        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;

        public int Quantity { get; set; }

        // [CartItemEntity] : Có thể lưu thêm giá tại thời điểm thêm vào giỏ để tiện tính toán hiển thị.
        public decimal UnitPrice { get; set; }
    }
}
