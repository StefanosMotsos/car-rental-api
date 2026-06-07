using CarRentalApp.Models.Enums;

namespace CarRentalApp.Core.Filters
{
    public class RentalFiltersDTO
    {
        public RentalStatus? Status { get; set; }
        public string? CustomerName { get; set; }
        public string? EmployeeName { get; set; }
        public string? Search { get; set; }
        public decimal? MinTotalCost { get; set; }
        public decimal? MaxTotalCost { get; set; }
    }
}
