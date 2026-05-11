using CarRentalApp.Models;
using CarRentalApp.Repositories.Base;

namespace CarRentalApp.Repositories.Categories
{
    public interface ICategoryRepository : IBaseRepository<Category>
    {
        Task<Category?> GetCategoryByNameAsync(string name);
    }
}
