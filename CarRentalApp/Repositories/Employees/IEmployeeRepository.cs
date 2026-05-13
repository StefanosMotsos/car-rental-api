using CarRentalApp.Core;
using CarRentalApp.Models;
using CarRentalApp.Repositories.Base;
using System.Linq.Expressions;

namespace CarRentalApp.Repositories.Employees
{
    public interface IEmployeeRepository : IBaseAuditRepository<Employee>
    {
        Task<Employee?> GetEmployeeByUserIdAsync(int userId);
        Task<PaginatedResult<Employee>> GetPaginatedFilteredEmployeesAsync(int pageNumber, int pageSize,
            List<Expression<Func<Employee, bool>>> predicates);
    }
}
