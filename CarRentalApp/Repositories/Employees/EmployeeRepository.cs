using CarRentalApp.Core;
using CarRentalApp.Data;
using CarRentalApp.Models;
using CarRentalApp.Repositories.Base;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace CarRentalApp.Repositories.Employees
{
    public class EmployeeRepository : BaseRepository<Employee>, IEmployeeRepository
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
                .Include(c => c.User)
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

        public virtual async Task<Employee?> GetEmployeeByUserIdAsync(int userId)
        {
            return await _dbSet.Include(c => c.UserId).FirstOrDefaultAsync(e => e.UserId == userId);
        }
    }
}
