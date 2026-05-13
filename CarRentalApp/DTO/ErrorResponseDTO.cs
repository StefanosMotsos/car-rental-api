namespace CarRentalApp.DTO
{
    public record ErrorResponseDTO
    {
        public string? Code { get; set; }
        public string? Message { get; set; }
        public string? Field { get; set; }
    }
}
