using CarRentalApp.Resources;
using System.ComponentModel.DataAnnotations;

namespace CarRentalApp.DTO
{
    public record EmployeeSignupDTO
    {
        [Required(ErrorMessageResourceType = typeof(ErrorMessages),
            ErrorMessageResourceName = "Required")]
        [StringLength(50, MinimumLength = 2,
            ErrorMessageResourceType = typeof(ErrorMessages),
            ErrorMessageResourceName = "StringLength")]
        public string? Username { get; set; }

        [Required(ErrorMessageResourceType = typeof(ErrorMessages),
            ErrorMessageResourceName = "Required")]
        [StringLength(50,
            ErrorMessageResourceType = typeof(ErrorMessages),
            ErrorMessageResourceName = "StringLength")]
        [EmailAddress(ErrorMessageResourceType = typeof(ErrorMessages),
            ErrorMessageResourceName = "EmailAddress")]
        public string? Email { get; set; }

        [Required(ErrorMessageResourceType = typeof(ErrorMessages),
            ErrorMessageResourceName = "Required")]
        [RegularExpression(@"^(?=.*?[A-Z])(?=.*?[a-z])(?=.*?\d)(?=.*?\W).{8,}$",
            ErrorMessageResourceType = typeof(ErrorMessages),
            ErrorMessageResourceName = "RegularExpression")]
        public string? Password { get; set; }

        [Required(ErrorMessageResourceType = typeof(ErrorMessages),
            ErrorMessageResourceName = "Required")]
        [StringLength(50, MinimumLength = 2,
            ErrorMessageResourceType = typeof(ErrorMessages),
            ErrorMessageResourceName = "StringLength")]
        public string? Firstname { get; set; }

        [Required(ErrorMessageResourceType = typeof(ErrorMessages),
            ErrorMessageResourceName = "Required")]
        [StringLength(50, MinimumLength = 2,
            ErrorMessageResourceType = typeof(ErrorMessages),
            ErrorMessageResourceName = "StringLength")]
        public string? Lastname { get; set; }

        [Required(ErrorMessageResourceType = typeof(ErrorMessages),
            ErrorMessageResourceName = "Required")]
        [StringLength(20, MinimumLength = 10,
            ErrorMessageResourceType = typeof(ErrorMessages),
            ErrorMessageResourceName = "StringLength")]
        public string? PhoneNumber { get; set; }
    }
}
