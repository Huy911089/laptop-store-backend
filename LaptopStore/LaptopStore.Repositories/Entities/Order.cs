using System;
using System.Collections.Generic;

namespace LaptopStore.Repositories.Entities
{
    public class Order
    {
        public Guid Id { get; set; }

        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        public DateTime OrderDate { get; set; } = DateTime.UtcNow;
        public decimal TotalAmount { get; set; }

        public string Status { get; set; } = "Pending";

        public string ShippingAddress { get; set; } = null!;
        public string PhoneNumber { get; set; } = null!;

        public ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
    }
}