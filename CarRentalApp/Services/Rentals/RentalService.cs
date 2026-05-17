using AutoMapper;
using CarRentalApp.Core;
using CarRentalApp.Core.Filters;
using CarRentalApp.DTO.Rental;
using CarRentalApp.Exceptions;
using CarRentalApp.Models;
using CarRentalApp.Models.Enums;
using CarRentalApp.Repositories;
using CarRentalApp.Resources;
using CarRentalApp.Security;
using CarRentalApp.Services.Customers;
using System.Linq.Expressions;

namespace CarRentalApp.Services.Rentals
{
    public class RentalService : IRentalService
    {
        private readonly IEncryptionUtil _encryptionUtil;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<RentalService> _logger;

        public RentalService(IEncryptionUtil encryptionUtil, IUnitOfWork unitOfWork,
            IMapper mapper, ILogger<RentalService> logger)
        {
            _encryptionUtil = encryptionUtil;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<RentalReadOnlyDTO> CreateRentalAsync(RentalCreateDTO dto, int callerUserId)
        {

            Vehicle? vehicle = await ValidateAndGetVehicleAsync(dto.VehicleUuid!.Value);
            Customer? customer = await ValidateAndGetCustomerAsync(callerUserId);
            Location? pickupLocation = await ValidateAndGetLocationAsync(dto.PickupLocationId!.Value);
            Location? dropoffLocation = await ValidateAndGetLocationAsync(dto.DropoffLocationId!.Value);
            await ValidateRentalDatesAsync(vehicle.Id, dto.StartDate!.Value, dto.EndDate!.Value);

            Rental rental = _mapper.Map<Rental>(dto);
            rental.Status = RentalStatus.Pending;
            rental.CustomerId = customer.Id;
            rental.Customer = customer;

            rental.VehicleId = vehicle.Id;
            rental.Vehicle = vehicle;
            vehicle.Status = VehicleStatus.Rented;

            rental.PickupLocation = pickupLocation;
            rental.DropoffLocation = dropoffLocation;

            await _unitOfWork.RentalRepository.AddAsync(rental);
            await _unitOfWork.SaveChanges();

            _logger.LogInformation("Rental with uuid {Uuid} was created successfully", rental.Uuid);
            return _mapper.Map<RentalReadOnlyDTO>(rental);
        }

        public async Task<RentalReadOnlyDTO> UpdateRentalAsync(RentalUpdateDTO dto, Guid uuid)
        {
            Rental? rental = await _unitOfWork.RentalRepository.GetByUuidAsync(uuid);
            if (rental == null)
            {
                _logger.LogWarning("Rental with uuid {Uuid} was not found", uuid);
                throw new EntityNotFoundException("Rental", ErrorMessages.NotFound);
            }

            Employee? employee = await _unitOfWork.EmployeeRepository.GetByUuidAsync(dto.EmployeeUuid!.Value);
            if (employee == null)
            {
                _logger.LogWarning("Employee with uuid {Uuid} was not found", uuid);
                throw new EntityNotFoundException("Employee", ErrorMessages.NotFound);
            }

            if (dto.Status == RentalStatus.Rejected || dto.Status == RentalStatus.Returned)
            {
                rental.Vehicle.Status = VehicleStatus.Available;
            }

            if (dto.Status == RentalStatus.Approved)
            {
                int days = rental.EndDate.DayNumber - rental.StartDate.DayNumber;
                rental.TotalCost = days * rental.Vehicle.DailyRate;
            }

            rental.Status = dto.Status!.Value;
            rental.Employee = employee;
            rental.EmployeeId = employee.Id;
            await _unitOfWork.SaveChanges();
            _logger.LogInformation($"Updated Rental is now {dto.Status}");
            return _mapper.Map<RentalReadOnlyDTO>(rental);
        }

        public async Task<PaginatedResult<RentalReadOnlyDTO>> CustomerRentalHistoryAsync(Guid uuid, int callerUserId, int pageNumber, 
            int pageSize, RentalFiltersDTO filters)
        {
            Customer? customer = await _unitOfWork.CustomerRepository.GetByUuidAsync(uuid);
            if (customer == null)
            {
                _logger.LogWarning("Customer with uuid {Uuid} was not found", uuid);
                throw new EntityNotFoundException("Customer", ErrorMessages.NotFound);
            }

            if (customer.UserId != callerUserId)
            {
                _logger.LogWarning("Customer with userId {UserId} is not allowed to access this resource", callerUserId);
                throw new EntityForbiddenException("Customer", ErrorMessages.Forbidden);
            }

            var predicates = BuildRentalPredicates(filters);
            predicates.Add(r => r.CustomerId == customer.Id);

            PaginatedResult<Rental> result = await _unitOfWork.RentalRepository.GetPaginatedFilteredRentalsAsync(pageNumber, pageSize, predicates);

            var dtoResult = new PaginatedResult<RentalReadOnlyDTO>()
            {
                Data = _mapper.Map<List<RentalReadOnlyDTO>>(result.Data),
                TotalRecords = result.TotalRecords,
                PageNumber = result.PageNumber,
                PageSize = result.PageSize,
            };

            _logger.LogInformation("Retrieved {Count} of the Customer's Rentals", result.Data.Count);
            return dtoResult;
        }

        public async Task<PaginatedResult<RentalReadOnlyDTO>> GetPaginatedFilteredRentalsAsync(int pageNumber, int pageSize, RentalFiltersDTO filters)
        {
            var predicates = BuildRentalPredicates(filters);
            if (filters.CustomerUuid.HasValue) predicates.Add(r => r.Customer.Uuid == filters.CustomerUuid.Value);
            if (filters.EmployeeUuid.HasValue) predicates.Add(r => r.Employee!.Uuid == filters.EmployeeUuid.Value);

            PaginatedResult<Rental> result = await _unitOfWork.RentalRepository.GetPaginatedFilteredRentalsAsync(pageNumber, pageSize, predicates);

            var dtoResult = new PaginatedResult<RentalReadOnlyDTO>()
            {
                Data = _mapper.Map<List<RentalReadOnlyDTO>>(result.Data),
                TotalRecords = result.TotalRecords,
                PageNumber = result.PageNumber,
                PageSize = result.PageSize,
            };

            _logger.LogInformation("Retrieved {Count} Rentals", result.Data.Count);
            return dtoResult;
        }

        private async Task<Vehicle> ValidateAndGetVehicleAsync(Guid vehicleUuid)
        {
            Vehicle? vehicle = await _unitOfWork.VehicleRepository.GetByUuidAsync(vehicleUuid);
            if (vehicle == null)
            {
                _logger.LogWarning("Vehicle with uuid {Uuid} was not found", vehicleUuid);
                throw new EntityNotFoundException("Vehicle", ErrorMessages.NotFound);
            }

            if (vehicle.Status != VehicleStatus.Available)
            {
                _logger.LogWarning("Vehicle with uuid {Uuid} not available for renting", vehicleUuid);
                throw new InvalidArgumentException("Vehicle", ErrorMessages.InvalidArgument);
            }

            return vehicle;
        }
        private async Task<Customer> ValidateAndGetCustomerAsync(int callerUserId)
        {
            User? user = await _unitOfWork.UserRepository.GetUserCustomerByIdAsync(callerUserId);
            if (user == null)
            {
                _logger.LogWarning("Customer with userId {UserId} was not found", callerUserId);
                throw new EntityNotFoundException("Customer", ErrorMessages.NotFound);
            }

            Customer? customer = user.Customer;
            return customer!;
        }

        private async Task<Location> ValidateAndGetLocationAsync(int locationId)
        {
            Location ? location = await _unitOfWork.LocationRepository.GetByIdAsync(locationId);
            if (location == null)
            {
                _logger.LogWarning("Location with id {PickupLocationId} was not found", locationId);
                throw new EntityNotFoundException("Location", ErrorMessages.NotFound);
            }

            return location;
        }

        private async Task ValidateRentalDatesAsync(int vehicleId, DateOnly startDate, DateOnly endDate)
        {
            if (startDate < DateOnly.FromDateTime(DateTime.UtcNow) || endDate <= startDate)
            {
                throw new InvalidArgumentException("Rental", ErrorMessages.InvalidArgument);
            }

            if (await _unitOfWork.RentalRepository.HasOverlappingRentalAsync(vehicleId, startDate, endDate))
            {
                _logger.LogWarning("Rental dates are invalid");
                throw new InvalidArgumentException("Rental", ErrorMessages.InvalidArgument);
            }
        }

        private List<Expression<Func<Rental, bool>>> BuildRentalPredicates(RentalFiltersDTO filters)
        {
            List<Expression<Func<Rental, bool>>> predicates = [];

            if (filters.Status.HasValue) predicates.Add(r => r.Status == filters.Status.Value);
            if (filters.VehicleUuid.HasValue) predicates.Add(r => r.Vehicle.Uuid == filters.VehicleUuid.Value);
            if (filters.StartDateFrom.HasValue) predicates.Add(r => r.StartDate >= filters.StartDateFrom.Value);
            if (filters.StartDateTo.HasValue) predicates.Add(r => r.StartDate <= filters.StartDateTo.Value);
            if (filters.MinTotalCost.HasValue) predicates.Add(r => r.TotalCost >= filters.MinTotalCost.Value);
            if (filters.MaxTotalCost.HasValue) predicates.Add(r => r.TotalCost <= filters.MaxTotalCost.Value);

            return predicates;
        }
    }
}
