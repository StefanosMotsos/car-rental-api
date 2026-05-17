using CarRentalApp.Core;
using CarRentalApp.Core.Filters;
using CarRentalApp.DTO;
using CarRentalApp.DTO.User;
using CarRentalApp.Models;

namespace CarRentalApp.Services.Users
{
    public interface IUserService
    {
        Task<User> VerifyAndGetUserAsync(UserLoginDTO credentials);
        Task<UserReadOnlyDTO> GetUserByUuidAsync(Guid uuid);
        Task<PaginatedResult<UserReadOnlyDTO>> GetUsersPaginatedFilteredAsync(int pageNumber, int pageSize, UserFiltersDTO filters);

        string CreateUserToken(User user);
    }
}
