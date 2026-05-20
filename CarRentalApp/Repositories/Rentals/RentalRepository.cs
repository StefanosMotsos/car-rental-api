using CarRentalApp.Core;
using CarRentalApp.Data;
using CarRentalApp.Models;
using CarRentalApp.Models.Enums;
using CarRentalApp.Repositories.Base;
using Microsoft.EntityFrameworkCore;
using System.Data;
using System.Linq.Expressions;

namespace CarRentalApp.Repositories.Rentals
{
    public class RentalRepository : BaseAuditRepository<Rental>, IRentalRepository
    {
        public RentalRepository(CarRentalDbContext context) : base(context)
        {
        }

        public virtual async Task<PaginatedResult<Rental>> GetPaginatedFilteredRentalsAsync(int pageNumber, int pageSize, 
            List<Expression<Func<Rental, bool>>> predicates)
        {
            int totalRecords;
            IQueryable<Rental> query = _context.Rentals;

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
                .OrderBy(r => r.Id)
                .Include(r => r.Customer).ThenInclude(c => c.User)
                .Include(r => r.Employee).ThenInclude(e => e!.User)
                .Include(r => r.PickupLocation)
                .Include(r => r.DropoffLocation)
                .Include(r => r.Vehicle)
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();

            return new PaginatedResult<Rental>
            {
                Data = data,
                TotalRecords = totalRecords,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public virtual async Task<PaginatedResult<Rental>> GetPaginatedRentalsByCustomerIdAsync(int customerId, int pageNumber, int pageSize)
        {
            int totalRecords;
            IQueryable<Rental> query = _context.Rentals.Where(r => r.CustomerId == customerId);

            totalRecords = await query.CountAsync();
            int skip = (pageNumber - 1) * pageSize;

            var data = await query
                .OrderBy(r => r.Id)
                .Include(r => r.Employee).ThenInclude(e => e!.User)
                .Include(r => r.PickupLocation)
                .Include(r => r.DropoffLocation)
                .Include(r => r.Vehicle)
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();

            return new PaginatedResult<Rental>
            {
                Data = data,
                TotalRecords = totalRecords,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public override async Task<Rental?> GetByUuidAsync(Guid uuid)
        {
            return await _dbSet
                .Include(r => r.Customer).ThenInclude(c => c.User)
                .Include(r => r.Employee).ThenInclude(e => e!.User)
                .Include(r => r.PickupLocation)
                .Include(r => r.DropoffLocation)
                .Include(r => r.Vehicle)
                .FirstOrDefaultAsync(r => r.Uuid == uuid);
        }

        public virtual async Task<bool> HasOverlappingRentalAsync(int vehicleId, DateOnly startDate, DateOnly endDate)
        {
            return await _dbSet.AnyAsync(r =>
                r.VehicleId == vehicleId &&
                r.StartDate < endDate &&
                r.EndDate > startDate);
        }

        public virtual async Task<bool> HasActiveRentalsForVehicleAsync(int vehicleId)
        {
            return await _dbSet.AnyAsync(r =>
                r.VehicleId == vehicleId &&
                (r.Status == RentalStatus.Pending || r.Status == RentalStatus.Approved));
        }
    }
}
