using CarRentalApp.Core;
using CarRentalApp.Models;
using CarRentalApp.Repositories.Base;
using System.Linq.Expressions;

namespace CarRentalApp.Repositories.Customers
{
    public interface ICustomerRepository : IBaseAuditRepository<Customer>
    {
        Task<PaginatedResult<Customer>> GetPaginatedFilteredCustomersAsync(int pageNumber,  int pageSize, 
            List<Expression<Func<Customer, bool>>> predicates);

        Task<Customer?> GetCustomerByDriverLicenseAsync(string driverLicense);
    }
}
