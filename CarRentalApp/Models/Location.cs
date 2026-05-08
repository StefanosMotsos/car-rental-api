namespace CarRentalApp.Models
{
    public class Location
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Address { get; set; } = null!;
        public string City { get; set; } = null!;
        public string Phone { get; set; } = null!;

        public ICollection<Rental> PickupRentals { get; set; } = new HashSet<Rental>();
        public ICollection<Rental> DropoffRentals { get; set; } = new HashSet<Rental>();
    }
}
