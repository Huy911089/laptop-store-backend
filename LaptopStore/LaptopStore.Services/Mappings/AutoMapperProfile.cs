using AutoMapper;
using LaptopStore.Repositories.Entities;
using LaptopStore.Services.DTOs.Category;
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

        }
    }
}
