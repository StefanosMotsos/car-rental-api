using CarRentalApp.Core;
using CarRentalApp.Data;
using CarRentalApp.Models;
using CarRentalApp.Repositories.Base;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace CarRentalApp.Repositories.Employees
{
    public class EmployeeRepository : BaseAuditRepository<Employee>, IEmployeeRepository
    {
        public EmployeeRepository(CarRentalDbContext context) : base(context)
        {
        }

        public virtual async Task<PaginatedResult<Employee>> GetPaginatedFilteredEmployeesAsync(int pageNumber, int pageSize,
            List<Expression<Func<Employee, bool>>> predicates)
        {
            int totalRecords;
            IQueryable<Employee> query = _context.Employees;

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
                .OrderBy(c => c.Id)
                .Include(c => c.User).ThenInclude(u => u.Role)
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();

            return new PaginatedResult<Employee>
            {
                Data = data,
                TotalRecords = totalRecords,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public override async Task<Employee?> GetByUuidAsync(Guid uuid)
        {
            return await _dbSet
                .Include(c => c.User)
                    .ThenInclude(u => u.Role)
                .FirstOrDefaultAsync(c => c.Uuid == uuid);
        }

        public async Task<Employee?> GetByUserIdAsync(int userId)
        {
            return await _dbSet
                .Include(e => e.User)
                    .ThenInclude(u => u.Role)
                .FirstOrDefaultAsync(e => e.UserId == userId);
        }
    }
}
