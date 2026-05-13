namespace CarRentalApp.DTO.Vehicle
{
    public record VehicleReadOnlyDTO
    {
        public Guid Uuid { get; set; }
        public string Make { get; set; } = null!;
        public string Model { get; set; } = null!;
        public short Year { get; set; }
        public string LicensePlate { get; set; } = null!;
        public decimal DailyRate { get; set; }
        public string TierType { get; set; } = null!;
        public string Status { get; set; } = null!;
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = null!;
        public string? PhotoUrl { get; set; }
    }
}
