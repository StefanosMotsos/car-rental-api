using CarRentalApp.Models.Enums;

namespace CarRentalApp.Core.Filters
{
    public class VehicleFiltersDTO
    {
        public string? Make { get; set; }
        public string? Model { get; set; }
        public short? MinYear { get; set; }
        public short? MaxYear { get; set; }
        public decimal? MinDailyRate { get; set; }
        public decimal? MaxDailyRate { get; set; }
        public VehicleStatus? Status { get; set; }
        public TierType? TierType { get; set; }
        public int? CategoryId { get; set; }
    }
}
