using CarRentalApp.Models.Enums;

namespace CarRentalApp.Core.Filters
{
    public class RentalFiltersDTO
    {
        public RentalStatus? Status { get; set; }
        public int? CustomerId { get; set; }
        public int? EmployeeId { get; set; }
        public int? VehicleId { get; set; }
        public DateOnly? StartDateFrom { get; set; }
        public DateOnly? StartDateTo { get; set; }
        public decimal? MinTotalCost { get; set; }
        public decimal? MaxTotalCost { get; set; }
    }
}
