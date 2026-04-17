using AutoMapper;
using LaptopStore.Repositories.Entities;
using LaptopStore.Services.DTOs.Auth;
using LaptopStore.Services.DTOs.Category;
using LaptopStore.Services.DTOs.Product;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaptopStore.Services.Mappings
{
    // [AutoMapperProfile] : Lớp cấu hình quy tắc chuyển đổi(Mapping) giữa Entities và DTOs.
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile() 
        {

            // CategoryRequestDto => Entity Category
            CreateMap<CategoryRequestDto, Category>();
            // [AutoMapperProfile] : Cấu hình ánh xạ từ Entity Category sang CategoryResponseDto.
            CreateMap<Category, CategoryResponseDto>();
            // ProductRequestDto => Entity Product
            CreateMap<ProductRequestDto, Product>();
            // Entity ProductImage => ProductImageDto
            CreateMap<ProductImage, ProductImageDto>();
            // Entity Product => ProductRequestDto
            CreateMap<Product, ProductResponseDto>()
                .ForMember(dest => dest.BrandName, opt => opt.MapFrom(src => src.Brand.Name))
                .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Category.Name));
            // RegisterRequestDto => Entity User
            CreateMap<RegisterRequestDto, User>()
                // Bỏ qua PasswordHash vì trong DTO chỉ có Password (text thường)
                .ForMember(dest => dest.PasswordHash,opt => opt.Ignore())
                // Bỏ qua RoleId vì ta sẽ tự gán sau khi query DB
                .ForMember(dest => dest.RoleId, opt => opt.Ignore());
        }
    }
}
