using CarRentalApp.Services.Customers;
using CarRentalApp.Services.Employees;
using CarRentalApp.Services.Lookup;
using CarRentalApp.Services.Rentals;
using CarRentalApp.Services.Users;
using CarRentalApp.Services.Vehicles;

namespace CarRentalApp.Services
{
    public static class ServicesDIExtension
    {
        public static IServiceCollection AddServices(this IServiceCollection services)
        {
            services.AddScoped<ICustomerService, CustomerService>();
            services.AddScoped<IEmployeeService, EmployeeService>();
            services.AddScoped<ILookupService, LookupService>();
            services.AddScoped<IRentalService, RentalService>();
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IVehicleService, VehicleService>();
            services.AddScoped<IApplicationService, ApplicationService>();
            return services;
        }
    }
}
