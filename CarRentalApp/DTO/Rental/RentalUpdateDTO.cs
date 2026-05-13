using CarRentalApp.Resources;
using System.ComponentModel.DataAnnotations;

namespace CarRentalApp.DTO.Rental
{
    public record RentalUpdateDTO
    {
         [Required(ErrorMessageResourceType = typeof(ErrorMessages),
             ErrorMessageResourceName = "Required")]
         public string? Status { get; set; }

         public Guid? EmployeeUuid { get; set; }
    }
}
