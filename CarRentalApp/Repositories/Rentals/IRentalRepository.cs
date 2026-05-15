using CarRentalApp.Core;
using CarRentalApp.Models;
using CarRentalApp.Repositories.Base;
using System.Linq.Expressions;

namespace CarRentalApp.Repositories.Rentals
{
    public interface IRentalRepository : IBaseAuditRepository<Rental>
    {
        Task<PaginatedResult<Rental>> GetPaginatedFilteredRentalsAsync(int pageNumber, int pageSize,
            List<Expression<Func<Rental, bool>>> predicates);

        Task<PaginatedResult<Rental>> GetPaginatedRentalsByCustomerIdAsync(int customerId, int pageNumber, int pageSize);

        Task<bool> HasOverlappingRentalAsync(int vehicleId, DateOnly startDate, DateOnly endDate);
    }
}
