using CarRentalApp.Core;
using CarRentalApp.Core.Filters;
using CarRentalApp.DTO.User;
using CarRentalApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CarRentalApp.Controllers
{
    [Route("api/v1/users")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IApplicationService _applicationService;

        public UsersController(IApplicationService applicationService)
        {
            _applicationService = applicationService;
        }

        /// <summary>
        /// Get all Users. Accessible by Admins only.
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "ADMIN")]
        [ProducesResponseType(typeof(PaginatedResult<UserReadOnlyDTO>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PaginatedResult<UserReadOnlyDTO>>> GetAllUsers(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] UserFiltersDTO? filters = null)
        {
            var result = await _applicationService.UserService
                .GetUsersPaginatedFilteredAsync(pageNumber, pageSize, filters ?? new UserFiltersDTO());

            return Ok(result);
        }
    }
}
