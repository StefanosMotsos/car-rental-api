using CarRentalApp.Core;
using CarRentalApp.Core.Filters;
using CarRentalApp.DTO.Rental;

namespace CarRentalApp.Services.Rentals
{
    public interface IRentalService
    {
        Task<RentalReadOnlyDTO> CreateRentalAsync(RentalCreateDTO dto, int callerUserId);
        Task<RentalReadOnlyDTO> UpdateRentalAsync(RentalUpdateDTO dto, Guid uuid);
        Task<PaginatedResult<RentalReadOnlyDTO>> CustomerRentalHistoryAsync(int callerUserId,
            int pageNumber, int pageSize, RentalFiltersDTO filters);
        Task<PaginatedResult<RentalReadOnlyDTO>> GetPaginatedFilteredRentalsAsync(int pageNumber, int pageSize, RentalFiltersDTO filters);
    }
}
