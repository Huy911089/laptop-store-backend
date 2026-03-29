using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaptopStore.Repositories.Entities
{
    // [CartEntity] : Quản lý phiên giỏ hàng của khách hàng. Bắt buộc dùng Guid để bảo mật.
    public class Cart
    {
        public Guid Id { get; set; } = Guid.NewGuid();

        // Khóa ngoại liên kết 1-1 với User
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        // Navigation property tới chi tiết giỏ hàng
        public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
    }
}
