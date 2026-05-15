using CarRentalApp.Core;
using CarRentalApp.Core.Filters;
using CarRentalApp.DTO.User;

namespace CarRentalApp.Services.Employees
{
    public interface IEmployeeService
    {
        Task<EmployeeReadOnlyDTO> SignUpEmployeeAsync(EmployeeSignupDTO dto);
        Task<EmployeeReadOnlyDTO> GetEmployeeByUuidAsync(Guid uuid);
        Task<EmployeeReadOnlyDTO> GetActiveEmployeeByUuidAsync(Guid uuid);
        Task<EmployeeReadOnlyDTO> UpdateEmployeeAsync(Guid uuid, EmployeeUpdateDTO dto);
        Task<bool> DeleteEmployeeByUuidAsync(Guid uuid);
        Task<PaginatedResult<EmployeeReadOnlyDTO>> GetPaginatedFilteredEmployeesAsync(int pageNumber, int pageSize, EmployeeFiltersDTO filters);
        Task<PaginatedResult<EmployeeReadOnlyDTO>> GetPaginatedFilteredActiveEmployeesAsync(int pageNumber, int pageSize, EmployeeFiltersDTO filters);
    }
}
