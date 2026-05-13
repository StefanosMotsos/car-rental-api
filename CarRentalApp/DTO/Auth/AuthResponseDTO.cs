namespace CarRentalApp.DTO.Auth
{
    public record AuthResponseDTO
    {
        public string Token { get; set; } = null!;
        public Guid Uuid { get; set; }
        public string Username { get; set; } = null!;
        public string UserRole { get; set; } = null!;
        public string Firstname { get; set; } = null!;
        public string Lastname { get; set; } = null!;
    }
}
