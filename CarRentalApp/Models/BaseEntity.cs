namespace CarRentalApp.Models
{
    public abstract class BaseEntity
    {
        public DateTime InsertedAt { get; set; } = DateTime.UtcNow;
        public DateTime ModifiedAt { get; set; }
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeletedAt { get; set; }

        public Guid Uuid { get; set; } = Guid.NewGuid();

    }
}
