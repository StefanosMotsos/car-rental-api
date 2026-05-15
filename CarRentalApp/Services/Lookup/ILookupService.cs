using CarRentalApp.DTO.Lookup;

namespace CarRentalApp.Services.Lookup
{
    public interface ILookupService
    {
        Task<IEnumerable<LocationReadOnlyDTO>> GetAllLocationsAsync();
        Task<IEnumerable<CategoryReadOnlyDTO>> GetAllCategoriesAsync();
    }
}
