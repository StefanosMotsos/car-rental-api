using CarRentalApp.Data;
using CarRentalApp.Models;

namespace CarRentalApp.Repositories.Base
{
    public class BaseAuditRepository<T> : BaseRepository<T> where T : BaseEntity
    {
        public BaseAuditRepository(CarRentalDbContext context) : base(context)
        {
        }

        public virtual async Task<bool> SoftDeleteAsync(int id)
        {
            T? entity = await _dbSet.FindAsync(id);
            if (entity is null) return false;

            entity.IsDeleted = true;
            entity.DeletedAt = DateTime.UtcNow;
            return true;
        }
    }
}
