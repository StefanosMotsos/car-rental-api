using CarRentalApp.Resources;
using System.ComponentModel.DataAnnotations;

namespace CarRentalApp.DTO.Rental
{
    public record RentalCreateDTO
    {
        [Required(ErrorMessageResourceType = typeof(ErrorMessages),
           ErrorMessageResourceName = "Required")]
        public Guid? VehicleUuid { get; set; }

        [Required(ErrorMessageResourceType = typeof(ErrorMessages),
            ErrorMessageResourceName = "Required")]
        public int? PickupLocationId { get; set; }

        [Required(ErrorMessageResourceType = typeof(ErrorMessages),
            ErrorMessageResourceName = "Required")]
        public int? DropoffLocationId { get; set; }

        [Required(ErrorMessageResourceType = typeof(ErrorMessages),
            ErrorMessageResourceName = "Required")]
        public DateOnly? StartDate { get; set; }

        [Required(ErrorMessageResourceType = typeof(ErrorMessages),
            ErrorMessageResourceName = "Required")]
        public DateOnly? EndDate { get; set; }
    }
}
