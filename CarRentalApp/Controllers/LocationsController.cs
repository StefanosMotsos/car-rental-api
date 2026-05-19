using CarRentalApp.DTO.Lookup;
using CarRentalApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CarRentalApp.Controllers
{
    [Route("api/v1/locations")]
    [ApiController]
    public class LocationsController : ControllerBase
    {

        private readonly IApplicationService _applicationService;

        public LocationsController(IApplicationService applicationService)
        {
            _applicationService = applicationService;
        }

        /// <summary>
        /// Get all Locations.
        /// </summary>
        [HttpGet]
        [Authorize]
        [ProducesResponseType(typeof (List<LocationReadOnlyDTO>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<LocationReadOnlyDTO>>> GetLocations()
        {
            var locations = await _applicationService.LookupService.GetAllLocationsAsync();

            return Ok(locations);
        }
    }
}
