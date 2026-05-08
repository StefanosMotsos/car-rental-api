using CarRentalApp.Models.Enums;

namespace CarRentalApp.Models
{
    public class Vehicle : BaseEntity
    {
        public int Id { get; set; }
        public string Model { get; set; } = null!;
        public int Year { get; set; }
        public string LicensePlate { get; set; } = null!;
        public decimal DailyRate { get; set; }
        public TierType TierType { get; set; }
        public VehicleStatus Status { get; set; }
        public VehiclePhoto? Photo { get; set; }
        public int CategoryId { get; set; }
        public Category Category { get; set; } = null!;
        public ICollection<Rental> Rentals { get; set; } = new HashSet<Rental>();
    }
}
