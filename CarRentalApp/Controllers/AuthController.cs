using CarRentalApp.DTO;
using CarRentalApp.DTO.Auth;
using CarRentalApp.DTO.User;
using CarRentalApp.Exceptions;
using CarRentalApp.Models;
using CarRentalApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CarRentalApp.Controllers
{
    [Route("api/v1/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IApplicationService _applicationService;

        public AuthController(IApplicationService applicationService)
        {
            _applicationService = applicationService;
        }


        /// <summary>
        /// Register a new Customer account.
        /// </summary>
        [HttpPost("register")]
        [AllowAnonymous]
        [ProducesResponseType(typeof (CustomerReadOnlyDTO), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<CustomerReadOnlyDTO>> RegisterCustomer([FromBody] CustomerSignupDTO dto)
        {
            CustomerReadOnlyDTO customer = await _applicationService.CustomerService.SignupCustomerAsync(dto);

            return Created("", customer);
        }

        /// <summary>
        /// As an Admin, register a new Employee account.
        /// </summary>
        [HttpPost("register/employee")]
        [Authorize(Roles = "ADMIN")]
        [ProducesResponseType(typeof (EmployeeReadOnlyDTO), StatusCodes.Status201Created)]
        [ProducesResponseType (StatusCodes.Status400BadRequest)]
        [ProducesResponseType (StatusCodes.Status409Conflict)]
        public async Task<ActionResult<EmployeeReadOnlyDTO>> RegisterEmployee([FromBody] EmployeeSignupDTO dto)
        {
            EmployeeReadOnlyDTO employee = await _applicationService.EmployeeService.SignUpEmployeeAsync(dto);

            return Created("", employee);
        }

        /// <summary>
        /// Authenticates a user and returns a JWT token.
        /// </summary>
        [HttpPost("login")]
        [AllowAnonymous]
        [ProducesResponseType(typeof (JwtTokenDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<JwtTokenDTO>> Login([FromBody] UserLoginDTO credentials)
        {
            User user = await _applicationService.UserService.VerifyAndGetUserAsync(credentials);
            var token = _applicationService.UserService.CreateUserToken(user);

            return Ok(new JwtTokenDTO(token));
        }
    }
}
