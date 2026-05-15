using CarRentalApp.Resources;
using System.ComponentModel.DataAnnotations;

namespace CarRentalApp.DTO
{
    public record UserLoginDTO
    {
        [Required(ErrorMessageResourceType = typeof(ErrorMessages),
            ErrorMessageResourceName = "Required")]
        [StringLength(50, MinimumLength = 2,
            ErrorMessageResourceType = typeof(ErrorMessages),
            ErrorMessageResourceName = "StringLength")]
        public string? Username { get; set; }

        [Required(ErrorMessageResourceType = typeof(ErrorMessages),
            ErrorMessageResourceName = "Required")]
        [RegularExpression(@"^(?=.*?[A-Z])(?=.*?[a-z])(?=.*?\d)(?=.*?\W).{8,}$",
            ErrorMessageResourceType = typeof(ErrorMessages),
            ErrorMessageResourceName = "RegularExpression")]
        public string? Password { get; set; }
    }
}
