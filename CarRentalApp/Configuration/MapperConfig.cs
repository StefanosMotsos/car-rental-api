using AutoMapper;
using CarRentalApp.DTO.Lookup;
using CarRentalApp.DTO.Rental;
using CarRentalApp.DTO.User;
using CarRentalApp.DTO.Vehicle;
using CarRentalApp.Models;

namespace CarRentalApp.Configuration
{
    public class MapperConfig : Profile
    {
        public MapperConfig()
        {
            //Customer Registration/Update mapped to User and Customer Entities
            CreateMap<CustomerSignupDTO, User>();
            CreateMap<CustomerSignupDTO, Customer>();
            CreateMap<CustomerUpdateDTO, User>();
            CreateMap<CustomerUpdateDTO, Customer>();

            //Admin's Registration/Update of an employee mapped to User and Employee Entities
            CreateMap<EmployeeSignupDTO, Employee>();
            CreateMap<EmployeeSignupDTO, User>();
            CreateMap<EmployeeUpdateDTO, Employee>();
            CreateMap<EmployeeUpdateDTO, User>();

            //Static Entities mapping
            CreateMap<Location, LocationReadOnlyDTO>();
            CreateMap<Category, CategoryReadOnlyDTO>();

            //Rental request mapped to Rental Entity
            CreateMap<RentalCreateDTO, Rental>()
                .ForMember(dest => dest.PickupLocationId, opt => opt.MapFrom(src => src.PickupLocationId!.Value))
                .ForMember(dest => dest.DropoffLocationId, opt => opt.MapFrom(src => src.DropoffLocationId!.Value));

            //Vehicle Registration/Update mapped to Vehicle Entity
            CreateMap<VehicleCreateDTO, Vehicle>()
                .ForMember(dest => dest.TierType, opt => opt.MapFrom(src => src.TierType!.Value))
                .ForMember(dest => dest.CategoryId, opt => opt.MapFrom(src => src.CategoryId!.Value));
            CreateMap<VehicleUpdateDTO, Vehicle>()
                .ForMember(dest => dest.TierType, opt => opt.MapFrom(src => src.TierType!.Value))
                .ForMember(dest => dest.CategoryId, opt => opt.MapFrom(src => src.CategoryId!.Value));




            //Entities mapped to their ReadOnlyDTO
            CreateMap<Rental, RentalReadOnlyDTO>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));
            CreateMap<User, UserReadOnlyDTO>()
                .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.Role.Name));
            CreateMap<User, CustomerReadOnlyDTO>()
                .ForMember(dest => dest.Uuid, opt => opt.MapFrom(src => src.Customer!.Uuid))
                .ForMember(dest => dest.DriverLicense, opt => opt.MapFrom(src => src.Customer!.DriverLicense))
                .ForMember(dest => dest.DateOfBirth, opt => opt.MapFrom(src => src.Customer!.DateOfBirth))
                .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.Role.Name));
            CreateMap<User, EmployeeReadOnlyDTO>()
                .ForMember(dest => dest.Uuid, opt => opt.MapFrom(src => src.Employee!.Uuid))
                .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.Employee!.PhoneNumber))
                .ForMember(dest => dest.RoleName, opt => opt.MapFrom(src => src.Role.Name));
            CreateMap<Vehicle, VehicleReadOnlyDTO>()
                .ForMember(dest => dest.TierType, opt => opt.MapFrom(src => src.TierType.ToString()))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
                .ForMember(dest => dest.PhotoUrl, opt => opt.MapFrom(src => src.Photo != null ? src.Photo.FilePath : null));
        }
    }
}
