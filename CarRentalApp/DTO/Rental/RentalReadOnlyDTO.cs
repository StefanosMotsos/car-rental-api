namespace CarRentalApp.DTO.Rental
{
    public record RentalReadOnlyDTO
    {
        public Guid Uuid { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly EndDate { get; set; }
        public decimal? TotalCost { get; set; }
        public string Status { get; set; } = null!;
        public DateTime InsertedAt { get; set; }

        public Guid CustomerUuid { get; set; }
        public string CustomerFirstname { get; set; } = null!;
        public string CustomerLastname { get; set; } = null!;

        public Guid? EmployeeUuid { get; set; }
        public string? EmployeeFirstname { get; set; }
        public string? EmployeeLastname { get; set; }

        public Guid VehicleUuid { get; set; }
        public string VehicleMake { get; set; } = null!;
        public string VehicleModel { get; set; } = null!;
        public string VehicleLicensePlate { get; set; } = null!;

        public int PickupLocationId { get; set; }
        public string PickupLocationName { get; set; } = null!;

        public int DropoffLocationId { get; set; }
        public string DropoffLocationName { get; set; } = null!;
    }
}
