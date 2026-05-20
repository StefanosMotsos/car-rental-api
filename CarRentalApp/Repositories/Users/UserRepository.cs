using CarRentalApp.Core;
using CarRentalApp.Data;
using CarRentalApp.Models;
using CarRentalApp.Repositories.Base;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;


namespace CarRentalApp.Repositories.Users
{
    public class UserRepository : BaseAuditRepository<User>, IUserRepository
    {
        public UserRepository(CarRentalDbContext context) : base(context)
        {
        }

        public virtual async Task<PaginatedResult<User>> GetPaginatedFilteredUsersAsync(int pageNumber, int pageSize, 
            List<Expression<Func<User, bool>>> predicates)
        {
            int totalRecords;
            IQueryable<User> query = _context.Users;

            if (predicates != null && predicates.Count > 0)
            {
                foreach (var predicate in predicates)
                {
                    query = query.Where(predicate);
                }
            }
            totalRecords = await query.CountAsync();
            int skip = (pageNumber - 1) * pageSize;

            var data = await query
                .OrderBy(u => u.Id)
                .Include(u => u.Role)
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();

            return new PaginatedResult<User>
            {
                Data = data,
                TotalRecords = totalRecords,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public virtual async Task<User?> GetUserByUsernameAsync(string username)
        {
            return await _dbSet.Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Username == username);
        }

        public virtual async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _dbSet.Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Email == email);
        }

        public override async Task<User?> GetByUuidAsync(Guid uuid)
        {
            return await _dbSet
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Uuid == uuid);
        }

        public virtual async Task<User?> GetUserCustomerByIdAsync(int id)
        {
            return await _dbSet
                .Include(u => u.Customer)
                .Include(u => u.Role)
                .FirstOrDefaultAsync(u => u.Id == id);
        }
    }
}
