using LaptopStore.Services.DTOs.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaptopStore.Services.DTOs.Product
{
    // [ProductQueryParameters] : Kế thừa từ PaginationParams để có sẵn PageIndex và PageSize. Bổ sung các tiêu chí lọc riêng của Product.
    public class ProductQueryParametersDto : PaginationParams
    {
        // [ProductQueryParametersDto] : Keyword dùng để search theo tên sản phẩm.
        public string? Keyword { get; set; }


        // [ProductQueryParametersDto] : Sử dụng int? (nullable) để biết được khi nào user KHÔNG truyền filter.
        // [ProductQueryParametersDto] : Lọc theo danh mục.
        public int? CategoryId { get; set; }
        // [ProductQueryParametersDto] : Lọc theo nhiều danh mục.
        public List<int>? CategoryIds { get; set; }
        // [ProductQueryParametersDto] : Lọc theo thương hiệu.
        public int? BrandId { get; set; }
        // [ProductQueryParametersDto] : Lọc theo nhiều thương hiệu.
        public List<int>? BrandIds { get; set; }

        public List<string>? Cpus { get; set; }
        public List<string>? Rams { get; set; }
        public List<string>? Storages { get; set; }
        public List<string>? Vgas { get; set; }
        public List<string>? ScreenSizes { get; set; }


        // [ProductQueryParametersDto] : Lọc theo giá tối thiểu.
        public decimal? MinPrice { get; set; }
        // [ProductQueryParametersDto] : Lọc theo giá tối đa.
        public decimal? MaxPrice { get; set; }


        // [ProductQueryParametersDto] : SortBy cho phép frontend chọn cách sắp xếp sản phẩm.
        public string? SortBy { get; set; }
    }
}
