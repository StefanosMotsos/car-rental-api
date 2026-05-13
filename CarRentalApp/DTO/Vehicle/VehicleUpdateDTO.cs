using CarRentalApp.Models.Enums;
using CarRentalApp.Resources;
using System.ComponentModel.DataAnnotations;

namespace CarRentalApp.DTO.Vehicle
{
    public record VehicleUpdateDTO
    {
        [Required(ErrorMessageResourceType = typeof(ErrorMessages),
            ErrorMessageResourceName = "Required")]
        [StringLength(50, MinimumLength = 2,
            ErrorMessageResourceType = typeof(ErrorMessages),
            ErrorMessageResourceName = "StringLength")]
        public string? Make { get; set; }

        [Required(ErrorMessageResourceType = typeof(ErrorMessages),
            ErrorMessageResourceName = "Required")]
        [StringLength(50, MinimumLength = 2,
            ErrorMessageResourceType = typeof(ErrorMessages),
            ErrorMessageResourceName = "StringLength")]
        public string? Model { get; set; }

        [Required(ErrorMessageResourceType = typeof(ErrorMessages),
            ErrorMessageResourceName = "Required")]
        [Range(1900, 2038,
            ErrorMessageResourceType = typeof(ErrorMessages),
            ErrorMessageResourceName = "Range")]
        public short? Year { get; set; }

        [Required(ErrorMessageResourceType = typeof(ErrorMessages),
            ErrorMessageResourceName = "Required")]
        [StringLength(20, MinimumLength = 2,
            ErrorMessageResourceType = typeof(ErrorMessages),
            ErrorMessageResourceName = "StringLength")]
        public string? LicensePlate { get; set; }

        [Required(ErrorMessageResourceType = typeof(ErrorMessages),
            ErrorMessageResourceName = "Required")]
        [Range(1, 9999.99,
            ErrorMessageResourceType = typeof(ErrorMessages),
            ErrorMessageResourceName = "Range")]
        public decimal? DailyRate { get; set; }

        [Required(ErrorMessageResourceType = typeof(ErrorMessages),
            ErrorMessageResourceName = "Required")]
        public TierType? TierType { get; set; }

        [Required(ErrorMessageResourceType = typeof(ErrorMessages),
            ErrorMessageResourceName = "Required")]
        public int? CategoryId { get; set; }
    }
}
