using CarRentalApp.Services.Customers;
using CarRentalApp.Services.Employees;
using CarRentalApp.Services.Lookup;
using CarRentalApp.Services.Rentals;
using CarRentalApp.Services.Users;
using CarRentalApp.Services.Vehicles;

namespace CarRentalApp.Services
{
    public class ApplicationService : IApplicationService
    {
        public ICustomerService CustomerService { get; }
        public IEmployeeService EmployeeService { get; }
        public ILookupService LookupService { get; }
        public IRentalService RentalService { get; }
        public IUserService UserService { get; }
        public IVehicleService VehicleService { get; }


        public ApplicationService(ICustomerService customerService, IEmployeeService employeeService, 
            ILookupService lookupService, IRentalService rentalService, 
            IUserService userService, IVehicleService vehicleService)
        {
            CustomerService = customerService;
            EmployeeService = employeeService;
            LookupService = lookupService;
            RentalService = rentalService;
            UserService = userService;
            VehicleService = vehicleService;
        }
    }
}
