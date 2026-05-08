namespace CarRentalApp.Models
{
    public class Employee : BaseEntity
    {
        public int Id { get; set; }
        public string PhoneNumber { get; set; } = null!;
        public int UserId { get; set; }
        public User User { get; set; } = null!;

        public ICollection<Rental> Rentals { get; set; } = new HashSet<Rental>();
    }
}
