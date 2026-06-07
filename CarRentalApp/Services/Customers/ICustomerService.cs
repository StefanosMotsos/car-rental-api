using CarRentalApp.Core;
using CarRentalApp.Core.Filters;
using CarRentalApp.DTO.User;
using CarRentalApp.Models;
using System.Linq.Expressions;
using System.Security.Claims;

namespace CarRentalApp.Services.Customers
{
    public interface ICustomerService
    {
        Task<CustomerReadOnlyDTO> SignupCustomerAsync(CustomerSignupDTO dto);
        Task<CustomerReadOnlyDTO> UpdateCustomerAsync(CustomerUpdateDTO dto, int callerUserId);
        Task<bool> DeleteCustomerByUuidAsync(Guid uuid);

        Task<CustomerReadOnlyDTO> GetCustomerByUuidAsync(Guid uuid);
        Task<CustomerReadOnlyDTO> GetActiveCustomerByUserIdAsync(int userId);

        Task<PaginatedResult<CustomerReadOnlyDTO>> GetPaginatedFilteredCustomersAsync(int pageNumber, int pageSize,
            CustomerFiltersDTO dto);
        Task<PaginatedResult<CustomerReadOnlyDTO>> GetPaginatedFilteredActiveCustomersAsync(int pageNumber, int pageSize,
            CustomerFiltersDTO dto);
    }
}
