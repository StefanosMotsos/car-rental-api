using CarRentalApp.Models;
using CarRentalApp.Repositories.Base;
using CarRentalApp.Repositories.Categories;
using CarRentalApp.Repositories.Customers;
using CarRentalApp.Repositories.Employees;
using CarRentalApp.Repositories.Rentals;
using CarRentalApp.Repositories.Users;
using CarRentalApp.Repositories.Vehicles;

namespace CarRentalApp.Repositories
{
    public interface IUnitOfWork
    {
        ICategoryRepository CategoryRepository { get; }
        IUserRepository UserRepository { get; }
        ICustomerRepository CustomerRepository { get; }
        IEmployeeRepository EmployeeRepository { get; }
        IVehicleRepository VehicleRepository { get; }
        IRentalRepository RentalRepository { get; }

        IBaseRepository<Role> RoleRepository { get; }
        IBaseRepository<Location> LocationRepository { get; }

        Task<bool> SaveChanges();
    }
}
