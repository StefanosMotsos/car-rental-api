using CarRentalApp.Core;
using CarRentalApp.Data;
using CarRentalApp.Models;
using CarRentalApp.Repositories.Base;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace CarRentalApp.Repositories.Customers
{
    public class CustomerRepository : BaseAuditRepository<Customer>, ICustomerRepository
    {
        public CustomerRepository(CarRentalDbContext context) : base(context)
        {
        }

        public virtual async Task<PaginatedResult<Customer>> GetPaginatedFilteredCustomersAsync(int pageNumber, int pageSize, 
            List<Expression<Func<Customer, bool>>> predicates)
        {
            int totalRecords;
            IQueryable<Customer> query = _context.Customers;

            if (predicates != null && predicates.Count > 0)
            {
                foreach(var predicate in predicates)
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

            return new PaginatedResult<Customer>
            {
                Data = data,
                TotalRecords = totalRecords,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public override async Task<Customer?> GetByUuidAsync(Guid uuid)
        {
            return await _dbSet
                .Include(c => c.User)
                    .ThenInclude(u => u.Role)
                .FirstOrDefaultAsync(c => c.Uuid == uuid);
        }

        public async Task<Customer?> GetCustomerByDriverLicenseAsync(string driverLicense)
        {
            return await _dbSet.Include(c => c.User).FirstOrDefaultAsync(c => c.DriverLicense == driverLicense);
        }
    }
}
