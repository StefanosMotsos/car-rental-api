namespace CarRentalApp.Repositories.Base
{
    public interface IBaseRepository<T>
    {
        Task AddSync(T entity);
        Task AddRangeAsync(IEnumerable<T> entities);
        Task UpdateAsync(T entity);
        Task<bool> DeleteAsync(T entity);
        Task<T?> GetByIdAsync(int id);
        Task<IEnumerable<T>> GetAllAsync();
        Task<int> GetCountAsync();
    }
}
