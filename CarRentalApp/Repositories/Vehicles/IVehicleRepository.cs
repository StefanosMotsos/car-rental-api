using CarRentalApp.Core;
using CarRentalApp.Models;
using CarRentalApp.Repositories.Base;
using System.Linq.Expressions;

namespace CarRentalApp.Repositories.Vehicles
{
    public interface IVehicleRepository : IBaseAuditRepository<Vehicle>
    {
        Task<PaginatedResult<Vehicle>> GetPaginatedFilteredVehiclesAsync(int pageNumber, int pageSize,
            List<Expression<Func<Vehicle, bool>>> predicates);
    }
}
