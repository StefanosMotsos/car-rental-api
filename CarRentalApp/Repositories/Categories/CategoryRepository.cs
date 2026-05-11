using CarRentalApp.Data;
using CarRentalApp.Models;
using CarRentalApp.Repositories.Base;
using Microsoft.EntityFrameworkCore;

namespace CarRentalApp.Repositories.Categories
{
    public class CategoryRepository : BaseRepository<Category>, ICategoryRepository
    {
        public CategoryRepository(CarRentalDbContext context) : base(context)
        {
        }

        public virtual async Task<Category?> GetCategoryByNameAsync(string name)
        {
            return await _dbSet.SingleOrDefaultAsync(c => c.Name == name);
        }
    }
}
