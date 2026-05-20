using CarRentalApp.Core;
using CarRentalApp.Core.Filters;
using CarRentalApp.DTO.User;
using CarRentalApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CarRentalApp.Controllers
{
    [Route("api/v1/customers")]
    [ApiController]
    public class CustomersController : ControllerBase
    {

        private readonly IApplicationService _applicationService;

        public CustomersController(IApplicationService applicationService)
        {
            _applicationService = applicationService;
        }

        /// <summary>
        /// Get the authenticated Customer's profile.
        /// </summary>
        [HttpGet("me")]
        [Authorize(Roles = "CUSTOMER")]
        [ProducesResponseType(typeof(CustomerReadOnlyDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<CustomerReadOnlyDTO>> GetCustomerByUserId()
        {
            int callerUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var customer = await _applicationService.CustomerService.GetActiveCustomerByUserIdAsync(callerUserId);

            return Ok(customer);
        }

        /// <summary>
        /// Get all Customers. Accessible by Admins only.
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "ADMIN")]
        [ProducesResponseType(typeof(PaginatedResult<CustomerReadOnlyDTO>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PaginatedResult<CustomerReadOnlyDTO>>> GetCustomers(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] CustomerFiltersDTO? filters = null)
        {
            var allCustomers = await _applicationService.CustomerService
                .GetPaginatedFilteredCustomersAsync(pageNumber, pageSize, filters ?? new CustomerFiltersDTO());

            return Ok(allCustomers);
        }

        /// <summary>
        /// Update the authenticated Customer's profile.
        /// </summary>
        [HttpPut("{uuid}")]
        [Authorize(Roles = "CUSTOMER")]
        [ProducesResponseType(typeof(CustomerReadOnlyDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<CustomerReadOnlyDTO>> UpdateCustomer(Guid uuid, [FromBody] CustomerUpdateDTO dto)
        {
            int callerUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var updatedCustomer = await _applicationService.CustomerService.UpdateCustomerAsync(uuid, dto, callerUserId);
            return Ok(updatedCustomer);
        }

        /// <summary>
        /// Delete a Customer. Accessible by Admins only.
        /// </summary>
        [HttpDelete("{uuid}")]
        [Authorize(Roles = "ADMIN")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DeleteCustomer(Guid uuid)
        {
            await _applicationService.CustomerService.DeleteCustomerByUuidAsync(uuid);
            return NoContent();
        }
    }
}
