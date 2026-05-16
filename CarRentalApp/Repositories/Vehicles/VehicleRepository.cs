using CarRentalApp.Core;
using CarRentalApp.Data;
using CarRentalApp.Models;
using CarRentalApp.Repositories.Base;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace CarRentalApp.Repositories.Vehicles
{
    public class VehicleRepository : BaseAuditRepository<Vehicle>, IVehicleRepository
    {
        public VehicleRepository(CarRentalDbContext context) : base(context)
        {
        }

        public virtual async Task<PaginatedResult<Vehicle>> GetPaginatedFilteredVehiclesAsync(int pageNumber, int pageSize, 
            List<Expression<Func<Vehicle, bool>>> predicates)
        {
            int totalRecords;
            IQueryable<Vehicle> query = _context.Vehicles;

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
                .OrderBy(v => v.Id)
                .Include(v => v.Category)
                .Include(v => v.Photo)
                .Skip(skip)
                .Take(pageSize)
                .ToListAsync();

            return new PaginatedResult<Vehicle>
            {
                Data = data,
                TotalRecords = totalRecords,
                PageNumber = pageNumber,
                PageSize = pageSize
            };
        }

        public override async Task<Vehicle?> GetByUuidAsync(Guid uuid)
        {
            return await _dbSet
                .Include(v => v.Category)
                .Include(v => v.Photo)
                .FirstOrDefaultAsync(v  => v.Uuid == uuid);
        }

        public async Task<Vehicle?> GetVehicleByLicensePlateAsync(string licensePlate)
        {
            return await _dbSet.Include(v => v.Category).Include(v => v.Photo).FirstOrDefaultAsync(v => v.LicensePlate == licensePlate);
        }
    }
}
