namespace CarRentalApp.Models.Enums
{
    public enum TierType
    {
        Economy,
        Standard,
        Luxury,
        VIP
    }

    public enum VehicleStatus
    {
        Available,
        Rented,
        Maintenance
    }

    public enum RentalStatus
    {
        Pending,
        Approved,
        Rejected,
        Returned
    }
}
