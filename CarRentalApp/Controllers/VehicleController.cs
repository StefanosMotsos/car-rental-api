using CarRentalApp.Core.Filters;
using CarRentalApp.DTO.Vehicle;
using CarRentalApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Reflection.Metadata.Ecma335;
using System.Security.Claims;

namespace CarRentalApp.Controllers
{
    [Route("api/v1/vehicles")]
    [ApiController]
    public class VehicleController : ControllerBase
    {
        private readonly IApplicationService _applicationService;

        public VehicleController(IApplicationService applicationService)
        {
            _applicationService = applicationService;
        }


        /// <summary>
        /// Get all Vehicles as Admin/Employee, and only the available Vehicles as a Customer
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        [Authorize]
        [ProducesResponseType(typeof (List<VehicleReadOnlyDTO>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<VehicleReadOnlyDTO>>> GetVehicles(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] VehicleFiltersDTO? filters = null)
        {
            string callerRole = User.FindFirstValue(ClaimTypes.Role)!;

            if (callerRole == "CUSTOMER")
            {
                var customerResult = await _applicationService.VehicleService
                    .GetPaginatedFilteredAvailableVehiclesAsync(pageNumber, pageSize, filters ?? new VehicleFiltersDTO());

                return Ok(customerResult);
            }

            var employeeResult = await _applicationService.VehicleService
                .GetPaginatedFilteredVehiclesAsync(pageNumber, pageSize, filters ?? new VehicleFiltersDTO());

            return Ok(employeeResult);
        }

        /// <summary>
        /// Get a Vehicle by its UUID. Accessible by all authenticated users.
        /// </summary>
        [HttpGet("{uuid}")]
        [Authorize]
        [ProducesResponseType(typeof (VehicleReadOnlyDTO), StatusCodes.Status200OK)]
        [ProducesResponseType (StatusCodes.Status404NotFound)]
        public async Task<ActionResult<VehicleReadOnlyDTO>> GetVehicleByUuid(Guid uuid)
        {
            var vehicle =  await _applicationService.VehicleService.GetVehicleByUuidAsync(uuid);

            return Ok(vehicle);
        }

        /// <summary>
        /// Add a new Vehicle. Accessible by Admins and Employees.
        /// </summary>
        [HttpPost]
        [Authorize(Roles = "ADMIN,EMPLOYEE")]
        [ProducesResponseType( typeof (VehicleReadOnlyDTO), StatusCodes.Status201Created)]
        [ProducesResponseType (StatusCodes.Status400BadRequest)]
        [ProducesResponseType (StatusCodes.Status409Conflict)]
        public async Task<ActionResult<VehicleReadOnlyDTO>> AddVehicle([FromBody] VehicleCreateDTO dto)
        {
            VehicleReadOnlyDTO vehicle = await _applicationService.VehicleService.AddVehicleAsync(dto);

            return CreatedAtAction(
                actionName: nameof(GetVehicleByUuid),
                routeValues: new { uuid = vehicle.Uuid },
                value: vehicle);
        }

        /// <summary>
        /// Upload or replace the photo for a Vehicle. Accessible by Admins and Employees.
        /// </summary>
        [HttpPost("{uuid}/photo")]
        [Authorize(Roles = "ADMIN,EMPLOYEE")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult> UploadVehiclePhoto(Guid uuid,[FromForm] IFormFile photo)
        {
            await _applicationService.VehicleService.SaveVehiclePhoto(uuid, photo);

            return NoContent();
        }

        /// <summary>
        /// Update a Vehicle. Accessible by Admins only.
        /// </summary>
        [HttpPut("{uuid}")]
        [Authorize(Roles = "ADMIN")]
        [ProducesResponseType(typeof(VehicleReadOnlyDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<VehicleReadOnlyDTO>> UpdateVehicle(Guid uuid, [FromBody] VehicleUpdateDTO dto)
        {
            var updatedVehicle = await _applicationService.VehicleService.UpdateVehicleAsync(uuid, dto);

            return Ok(updatedVehicle);
        }

        /// <summary>
        /// Soft-delete a Vehicle. Fails if active rentals exist. Accessible by Admins only.
        /// </summary>
        [HttpDelete("{uuid}")]
        [Authorize(Roles = "ADMIN")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> DeleteVehicle(Guid uuid)
        {
            await _applicationService.VehicleService.DeleteVehicleByUuidAsync(uuid);

            return NoContent();
        }
    }
}
