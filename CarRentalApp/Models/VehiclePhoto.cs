using CarRentalApp.Models;

namespace CarRentalApp.Models
{
    public class VehiclePhoto : BaseEntity
    {
        public int Id { get; set; }
        public string OriginalName { get; set; } = null!;
        public string SavedName { get; set; } = null!;
        public string FilePath { get; set; } = null!;
        public string ContentType { get; set; } = null!;
        public string Extension { get; set; } = null!;
        public int VehicleId { get; set; }
        public Vehicle Vehicle { get; set; } = null!;
    }
}
