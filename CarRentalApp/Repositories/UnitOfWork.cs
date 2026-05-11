using CarRentalApp.Data;
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
    public class UnitOfWork : IUnitOfWork
    {
        private readonly CarRentalDbContext _context;
        public ICategoryRepository CategoryRepository { get; }
        public IUserRepository UserRepository { get; }
        public ICustomerRepository CustomerRepository { get; }
        public IEmployeeRepository EmployeeRepository { get; }
        public IVehicleRepository VehicleRepository { get; }
        public IRentalRepository RentalRepository { get; }

        public IBaseRepository<Role> RoleRepository { get; }
        public IBaseRepository<Location> LocationRepository { get; }

        public async Task<bool> SaveChanges()
        {
            return await _context.SaveChangesAsync() > 0;
        }

        public UnitOfWork(CarRentalDbContext context)
        {
            _context = context;
            CategoryRepository = new CategoryRepository(context);
            UserRepository = new UserRepository(context);
            CustomerRepository = new CustomerRepository(context);
            EmployeeRepository = new EmployeeRepository(context);
            VehicleRepository = new VehicleRepository(context);
            RentalRepository = new RentalRepository(context);

            RoleRepository = new BaseRepository<Role>(context);
            LocationRepository = new BaseRepository<Location>(context);
        }
    }
}
