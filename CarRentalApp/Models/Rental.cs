using CarRentalApp.Models.Enums;

namespace CarRentalApp.Models
{
    public class Rental : BaseEntity
    {
        public int Id { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public decimal? TotalCost { get; set; }
        public RentalStatus Status { get; set; }


        public int CustomerId { get; set; }
        public Customer Customer { get; set; } = null!;


        public int? EmployeeId { get; set; }
        public Employee? Employee { get; set; }


        public int VehicleId { get; set; }
        public Vehicle Vehicle { get; set; } = null!;


        public int PickupLocationId { get; set; }
        public Location PickupLocation { get; set; } = null!;


        public int DropoffLocationId { get; set; }
        public Location DropoffLocation { get; set; } = null!;

    }
}
