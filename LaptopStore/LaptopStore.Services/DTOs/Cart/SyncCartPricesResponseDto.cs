using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaptopStore.Services.DTOs.Cart
{
    public class SyncCartPricesResponseDto
    {
        // [SyncCartPricesResponseDto] : Số item đã được đồng bộ lại theo giá hiện tại.
        public int UpdatedItemsCount { get; set; }

        public CartResponseDto Cart { get; set; } = new();
    }
}
