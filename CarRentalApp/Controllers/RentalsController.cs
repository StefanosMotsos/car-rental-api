using CarRentalApp.Core;
using CarRentalApp.Core.Filters;
using CarRentalApp.DTO.Rental;
using CarRentalApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CarRentalApp.Controllers
{
    [Route("api/v1/rentals")]
    [ApiController]
    public class RentalsController : ControllerBase
    {

        private readonly IApplicationService _applicationService;

        public RentalsController(IApplicationService applicationService)
        {
            _applicationService = applicationService;
        }

        /// <summary>
        /// Create a new Rental. Accessible by Customers only.
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "CUSTOMER")]
        [ProducesResponseType(typeof (RentalReadOnlyDTO), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<RentalReadOnlyDTO>> CreateRental([FromBody] RentalCreateDTO dto)
        {
            int callerUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var newRental = await _applicationService.RentalService.CreateRentalAsync(dto, callerUserId);

            return Created("", newRental);
        }

        /// <summary>
        /// Accept or Reject a rental request. Accessible by Admins or Employees only.
        /// </summary>
        [HttpPatch("{uuid}")]
        [Authorize(Roles = "ADMIN,EMPLOYEE")]
        [ProducesResponseType(typeof(RentalReadOnlyDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<RentalReadOnlyDTO>> UpdateRental(Guid uuid, [FromBody] RentalUpdateDTO dto)
        {
            var updatedRental = await _applicationService.RentalService.UpdateRentalAsync(dto, uuid);

            return Ok(updatedRental);
        }

        /// <summary>
        /// Customer's rental history.
        /// </summary>
        [HttpGet("rental-history")]
        [Authorize(Roles = "CUSTOMER")]
        [ProducesResponseType(typeof(PaginatedResult<RentalReadOnlyDTO>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PaginatedResult<RentalReadOnlyDTO>>> GetMyRentals(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] RentalFiltersDTO? filters = null)
        {
            Guid customerUuid = Guid.Parse(User.FindFirstValue("uuid")!);
            int callerUserId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var rentalHistory = await _applicationService.RentalService
                .CustomerRentalHistoryAsync(customerUuid, callerUserId, pageNumber, pageSize, filters ?? new RentalFiltersDTO());

            return Ok(rentalHistory);
        }

        /// <summary>
        /// Get all Rentals. Accessible by Admin and Employee.
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "ADMIN,EMPLOYEE")]
        [ProducesResponseType(typeof(PaginatedResult<RentalReadOnlyDTO>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PaginatedResult<RentalReadOnlyDTO>>> GetAllRentals(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] RentalFiltersDTO? filters = null)
        {
            var allRentals = await _applicationService.RentalService
                .GetPaginatedFilteredRentalsAsync(pageNumber, pageSize, filters ?? new RentalFiltersDTO());

            return Ok(allRentals);
        }
    }
}
