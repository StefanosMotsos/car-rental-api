namespace CarRentalApp.DTO.User
{
    public record EmployeeReadOnlyDTO
    {
        public Guid Uuid { get; set; }
        public string Username { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Firstname { get; set; } = null!;
        public string Lastname { get; set; } = null!;
        public string RoleName { get; set; } = null!;
        public bool IsDeleted { get; set; }
        public string PhoneNumber { get; set; } = null!;
    }
}
