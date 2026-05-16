using CarRentalApp.Services.Customers;
using CarRentalApp.Services.Employees;
using CarRentalApp.Services.Lookup;
using CarRentalApp.Services.Rentals;
using CarRentalApp.Services.Users;
using CarRentalApp.Services.Vehicles;

namespace CarRentalApp.Services
{
    public interface IApplicationService
    {
        ICustomerService CustomerService { get; }
        IEmployeeService EmployeeService { get; }
        ILookupService LookupService { get; }
        IRentalService RentalService { get; }
        IUserService UserService { get; }
        IVehicleService VehicleService { get; }
    }
}
