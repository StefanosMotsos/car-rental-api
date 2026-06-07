using CarRentalApp.DTO.Lookup;
using CarRentalApp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CarRentalApp.Controllers
{
    [Route("api/v1/lookup")]
    [ApiController]
    public class LookupController : ControllerBase
    {

        private readonly IApplicationService _applicationService;

        public LookupController(IApplicationService applicationService)
        {
            _applicationService = applicationService;
        }

        /// <summary>
        /// Get all Locations.
        /// </summary>
        [HttpGet("locations")]
        [Authorize]
        [ProducesResponseType(typeof (List<LocationReadOnlyDTO>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<LocationReadOnlyDTO>>> GetLocations()
        {
            var locations = await _applicationService.LookupService.GetAllLocationsAsync();

            return Ok(locations);
        }


        /// <summary>
        /// Get all Categories
        /// </summary>
        [HttpGet("categories")]
        [Authorize]
        [ProducesResponseType(typeof (List<CategoryReadOnlyDTO>), StatusCodes.Status200OK)]
        public async Task<ActionResult<List<CategoryReadOnlyDTO>>> GetCategories()
        {
            var categories = await _applicationService.LookupService.GetAllCategoriesAsync();

            return Ok(categories);
        }
    }
}
