using CarRentalApp.Core;
using CarRentalApp.Core.Filters;
using CarRentalApp.DTO.Vehicle;

namespace CarRentalApp.Services.Vehicles
{
    public interface IVehicleService
    {
        Task<VehicleReadOnlyDTO> AddVehicleAsync(VehicleCreateDTO dto);
        Task SaveVehiclePhoto(Guid uuid, IFormFile photo);
        Task<VehicleReadOnlyDTO> UpdateVehicleAsync(Guid uuid, VehicleUpdateDTO dto);
        Task<bool> DeleteVehicleByUuidAsync(Guid uuid);
        Task<VehicleReadOnlyDTO> GetVehicleByUuidAsync(Guid uuid);
        Task<PaginatedResult<VehicleReadOnlyDTO>> GetPaginatedFilteredVehiclesAsync(int pageNumber, int pageSize, VehicleFiltersDTO filters);
        Task<PaginatedResult<VehicleReadOnlyDTO>> GetPaginatedFilteredAvailableVehiclesAsync(int pageNumber, int pageSize, VehicleFiltersDTO filters);
    }
}
