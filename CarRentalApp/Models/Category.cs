namespace CarRentalApp.Models
{
    public class Category
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;
        public string? Description { get; set; }

        public ICollection<Vehicle> Vehicles { get; set; } = new HashSet<Vehicle>();
    }
}
