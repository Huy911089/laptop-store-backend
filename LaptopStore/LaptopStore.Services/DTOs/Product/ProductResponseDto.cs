namespace LaptopStore.Services.DTOs.Product
{
    public class ProductResponseDto
    {
        public int ProductId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }

        public string Cpu { get; set; } = string.Empty;
        public string Ram { get; set; } = string.Empty;
        public string Storage { get; set; } = string.Empty;
        public string ScreenSize { get; set; } = string.Empty;
        public string Vga { get; set; } = string.Empty;

        public System.DateTime CreatedAt { get; set; }

        public int BrandId { get; set; }
        public string BrandName { get; set; } = string.Empty;

        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;

        public List<ProductImageDto> ProductImages { get; set; } = new();
    }
}
