using LaptopStore.Services.DTOs.Brand;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaptopStore.Services.Interfaces
{
    public interface IBrandService
    {
        Task<IEnumerable<BrandResponseDto>> GetAllAsync();
        Task<BrandResponseDto?> GetByIdAsync(int id);
        Task<BrandResponseDto?> CreateAsync(BrandRequestDto dto);
        Task<bool> UpdateAsync(int id,BrandRequestDto dto);
        Task<bool> DeleteAsync(int id);
    }
}
