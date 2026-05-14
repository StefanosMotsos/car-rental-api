using CarRentalApp.Core;
using CarRentalApp.Models;
using CarRentalApp.Repositories.Base;
using System.Linq.Expressions;

namespace CarRentalApp.Repositories.Users
{
    public interface IUserRepository : IBaseAuditRepository<User>
    {
        Task<User?> GetUserByUsernameAsync(string username);

        Task<User?> GetUserByEmailAsync(string email);

        Task<PaginatedResult<User>> GetPaginatedFilteredUsersAsync(int pageNumber, int pageSize,
            List<Expression<Func<User, bool>>> predicates);
    }
}
