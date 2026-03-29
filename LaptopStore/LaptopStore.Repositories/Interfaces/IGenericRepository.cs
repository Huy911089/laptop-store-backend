using System.Linq.Expressions;

namespace LaptopStore.Repositories.Interfaces
{
    public interface IGenericRepository<T> where T : class
    {

        Task<List<T>> GetAllAsync(
            // Cái này được sử dụng để lọc (option)
            Expression<Func<T, bool>>? filter = null,
            // Sắp xếp (option)
            Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
            // Nối đến bảng khác (option)
            string? includeProperties = null
            );

        Task<T?> GetAsync(
            Expression<Func<T, bool>> filter,
            string? includeProperties = null,
            bool tracked = true);
        Task AddAsync(T entity);
        Task AddRangeAsync(IEnumerable<T> entities);
        void Update(T entity);
        void Remove(T entity);
        void RemoveRange(IEnumerable<T> entities);
    }
}
