namespace CarRentalApp.Models
{
    public class Customer : BaseEntity
    {
        public int Id { get; set; }
        public string DriverLicense { get; set; } = null!;
        public DateOnly DateOfBirth { get; set; }
        public int UserId { get; set; }
        public User User { get; set; } = null!;
        public ICollection<Rental> Rentals { get; set; } = new HashSet<Rental>();
    }
}
