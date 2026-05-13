namespace CarRentalApp.DTO.Lookup
{
    public record CategoryReadOnlyDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
    }
}
