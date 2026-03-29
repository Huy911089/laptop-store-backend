using System;

namespace LaptopStore.Repositories.Entities
{
    // [OrderDetailEntity] : Chi tiết đơn hàng. Lưu lại UnitPrice để không bị ảnh hưởng nếu giá Product thay đổi trong tương lai.
    public class OrderDetail
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid OrderId { get; set; }
        public Order Order { get; set; } = null!;

        public int ProductId { get; set; } // Liên kết với Product (kiểu int)
        public Product Product { get; set; } = null!;

        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}