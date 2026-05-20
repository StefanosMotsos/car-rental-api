using CarRentalApp.Core;
using CarRentalApp.Core.Filters;
using CarRentalApp.DTO.User;
using CarRentalApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CarRentalApp.Controllers
{
    [Route("api/v1/employees")]
    [ApiController]
    public class EmployeesController : ControllerBase
    {

        private readonly IApplicationService _applicationService;

        public EmployeesController(IApplicationService applicationService)
        {
            _applicationService = applicationService;
        }

        /// <summary>
        /// Get all Employees. Accessible by Admins only.
        /// </summary>
        [HttpGet]
        [Authorize(Roles = "ADMIN")]
        [ProducesResponseType(typeof(PaginatedResult<EmployeeReadOnlyDTO>), StatusCodes.Status200OK)]
        public async Task<ActionResult<PaginatedResult<EmployeeReadOnlyDTO>>> GetEmployees(
            [FromQuery] int pageNumber = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] EmployeeFiltersDTO? filters = null)
        {
            var result = await _applicationService.EmployeeService
                .GetPaginatedFilteredActiveEmployeesAsync(pageNumber, pageSize, filters ?? new EmployeeFiltersDTO());
            return Ok(result);
        }

        /// <summary>
        /// Get an Employee by UUID. Accessible by Admins only.
        /// </summary>
        [HttpGet("{uuid}")]
        [Authorize(Roles = "ADMIN")]
        [ProducesResponseType(typeof(EmployeeReadOnlyDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<EmployeeReadOnlyDTO>> GetEmployeeByUuid(Guid uuid)
        {
            var employee = await _applicationService.EmployeeService.GetActiveEmployeeByUuidAsync(uuid);
            return Ok(employee);
        }

        /// <summary>
        /// Update an Employee. Accessible by Admins only.
        /// </summary>
        [HttpPut("{uuid}")]
        [Authorize(Roles = "ADMIN")]
        [ProducesResponseType(typeof(EmployeeReadOnlyDTO), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<EmployeeReadOnlyDTO>> UpdateEmployee(Guid uuid, [FromBody] EmployeeUpdateDTO dto)
        {
            var updatedEmployee = await _applicationService.EmployeeService.UpdateEmployeeAsync(uuid, dto);
            return Ok(updatedEmployee);
        }

        /// <summary>
        /// Delete an Employee. Accessible by Admins only.
        /// </summary>
        [HttpDelete("{uuid}")]
        [Authorize(Roles = "ADMIN")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> DeleteEmployee(Guid uuid)
        {
            await _applicationService.EmployeeService.DeleteEmployeeByUuidAsync(uuid);
            return NoContent();
        }
    }
}
