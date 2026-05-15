using CarRentalApp.Models.Enums;

namespace CarRentalApp.Core.Filters
{
    public class RentalFiltersDTO
    {
        public RentalStatus? Status { get; set; }
        public Guid? CustomerUuid { get; set; }
        public Guid? EmployeeUuid { get; set; }
        public Guid? VehicleUuid { get; set; }
        public DateOnly? StartDateFrom { get; set; }
        public DateOnly? StartDateTo { get; set; }
        public decimal? MinTotalCost { get; set; }
        public decimal? MaxTotalCost { get; set; }
    }
}
