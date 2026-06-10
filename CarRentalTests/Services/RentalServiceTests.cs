using AutoMapper;
using CarRentalApp.Core;
using CarRentalApp.Core.Filters;
using CarRentalApp.DTO.Rental;
using CarRentalApp.Exceptions;
using CarRentalApp.Models;
using CarRentalApp.Models.Enums;
using CarRentalApp.Repositories;
using CarRentalApp.Security;
using CarRentalApp.Services.Rentals;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;
using Location = CarRentalApp.Models.Location;
using User = CarRentalApp.Models.User;


namespace CarRentalTests.Services
{
    public class RentalServiceTests
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<RentalService> _logger;
        private readonly IMapper _mapper;
        private readonly RentalService _service;

        public RentalServiceTests()
        {
            _unitOfWork = Substitute.For<IUnitOfWork>();
            _mapper = Substitute.For<IMapper>();
            _logger = Substitute.For<ILogger<RentalService>>();

            _service = new RentalService(_unitOfWork, _mapper, _logger);
        }

        // ==================== CreateRentalAsync ====================

        [Fact]
        public async Task CreateRentalAsync_WhenAllDataIsValid_ReturnsRentalReadOnlyDTO()
        {
            RentalCreateDTO dto;
            Vehicle? vehicle;
            User? user;
            Customer? customer;
            Rental? mappedRental;

            dto = CreateValidRentalDTO();
            vehicle = new Vehicle { Uuid = dto.VehicleUuid!.Value, Make = "Toyota" };
            customer = new Customer { DriverLicense = "DL-12345" };
            user = new User { Id = 1, Username = "steve", Customer = customer };
            mappedRental = new Rental();

            _mapper.Map<Rental>(dto).Returns(mappedRental);
            _mapper.Map<RentalReadOnlyDTO>(mappedRental).Returns(new RentalReadOnlyDTO());

            _unitOfWork.VehicleRepository.GetByUuidAsync(dto.VehicleUuid!.Value).Returns(vehicle);
            _unitOfWork.UserRepository.GetUserCustomerByIdAsync(1).Returns(user);
            _unitOfWork.LocationRepository.GetByIdAsync(dto.PickupLocationId!.Value).Returns(new Location { Id = 1 });
            _unitOfWork.LocationRepository.GetByIdAsync(dto.DropoffLocationId!.Value).Returns(new Location { Id = 2 });
            _unitOfWork.RentalRepository.HasOverlappingRentalAsync(Arg.Any<int>(), Arg.Any<DateOnly>(), Arg.Any<DateOnly>()).Returns(false);

            RentalReadOnlyDTO result = await _service.CreateRentalAsync(dto, 1);

            await _unitOfWork.RentalRepository.Received(1).AddAsync(mappedRental);
            await _unitOfWork.Received(1).SaveChanges();

            Assert.NotNull(result);
        }

        [Fact]
        public async Task CreateRentalAsync_WhenVehicleNotFound_ThrowsEntityNotFoundException()
        {
            RentalCreateDTO dto;
            dto = CreateValidRentalDTO();
            _mapper.Map<Rental>(dto).Returns(new Rental());

            _unitOfWork.VehicleRepository.GetByUuidAsync(dto.VehicleUuid!.Value).Returns((Vehicle?)null);

            await Assert.ThrowsAsync<EntityNotFoundException>(
                () => _service.CreateRentalAsync(dto, 1));

            await _unitOfWork.RentalRepository.DidNotReceive().AddAsync(Arg.Any<Rental>());
            await _unitOfWork.DidNotReceive().SaveChanges();
        }

        [Fact]
        public async Task CreateRentalAsync_WhenVehicleStatusNotAvailable_ThrowsInvalidArgumentException()
        {
            RentalCreateDTO dto;
            Vehicle? vehicle;

            dto = CreateValidRentalDTO();
            vehicle = new Vehicle { Uuid = dto.VehicleUuid!.Value, Make = "Toyota", Status = VehicleStatus.Rented };

            _mapper.Map<Rental>(dto).Returns(new Rental());

            _unitOfWork.VehicleRepository.GetByUuidAsync(dto.VehicleUuid!.Value).Returns(vehicle);
            await Assert.ThrowsAsync<InvalidArgumentException>(
                () => _service.CreateRentalAsync(dto, 1));

            await _unitOfWork.RentalRepository.DidNotReceive().AddAsync(Arg.Any<Rental>());
            await _unitOfWork.DidNotReceive().SaveChanges();
        }

        [Fact]
        public async Task CreateRentalAsync_WhenVehicleIsDeleted_ThrowsInvalidArgumentException()
        {
            RentalCreateDTO dto;
            Vehicle? vehicle;

            dto = CreateValidRentalDTO();
            vehicle = new Vehicle { Uuid = dto.VehicleUuid!.Value, Make = "Toyota", IsDeleted = true };

            _mapper.Map<Rental>(dto).Returns(new Rental());

            _unitOfWork.VehicleRepository.GetByUuidAsync(dto.VehicleUuid!.Value).Returns(vehicle);

            await Assert.ThrowsAsync<InvalidArgumentException>(
                () => _service.CreateRentalAsync(dto, 1));

            await _unitOfWork.RentalRepository.DidNotReceive().AddAsync(Arg.Any<Rental>());
            await _unitOfWork.DidNotReceive().SaveChanges();
        }

        [Fact]
        public async Task CreateRentalAsync_WhenCustomerUserNotFound_ThrowsEntityNotFoundException()
        {
            RentalCreateDTO dto;
            Vehicle? vehicle;

            dto = CreateValidRentalDTO();
            vehicle = new Vehicle { Uuid = dto.VehicleUuid!.Value, Make = "Toyota" };

            _mapper.Map<Rental>(dto).Returns(new Rental());

            _unitOfWork.VehicleRepository.GetByUuidAsync(dto.VehicleUuid!.Value).Returns(vehicle);
            _unitOfWork.UserRepository.GetUserCustomerByIdAsync(1).Returns((User?)null);

            await Assert.ThrowsAsync<EntityNotFoundException>(
                () => _service.CreateRentalAsync(dto, 1));

            await _unitOfWork.RentalRepository.DidNotReceive().AddAsync(Arg.Any<Rental>());
            await _unitOfWork.DidNotReceive().SaveChanges();
        }

        [Fact]
        public async Task CreateRentalAsync_WhenCustomerNotLinked_ThrowsEntityNotFoundException()
        {
            RentalCreateDTO dto;
            Vehicle? vehicle;
            User? user;

            dto = CreateValidRentalDTO();
            vehicle = new Vehicle { Uuid = dto.VehicleUuid!.Value, Make = "Toyota" };
            user = new User { Id = 1, Username = "steve", Customer = null };

            _mapper.Map<Rental>(dto).Returns(new Rental());

            _unitOfWork.VehicleRepository.GetByUuidAsync(dto.VehicleUuid!.Value).Returns(vehicle);
            _unitOfWork.UserRepository.GetUserCustomerByIdAsync(1).Returns(user);

            await Assert.ThrowsAsync<EntityNotFoundException>(
                () => _service.CreateRentalAsync(dto, 1));

            await _unitOfWork.RentalRepository.DidNotReceive().AddAsync(Arg.Any<Rental>());
            await _unitOfWork.DidNotReceive().SaveChanges();
        }

        [Fact]
        public async Task CreateRentalAsync_WhenLocationNotFound_ThrowsEntityNotFoundException()
        {
            RentalCreateDTO dto;
            Vehicle? vehicle;
            User? user;
            Customer? customer;

            dto = CreateValidRentalDTO();
            vehicle = new Vehicle { Uuid = dto.VehicleUuid!.Value, Make = "Toyota" };
            customer = new Customer { DriverLicense = "DL-12345" };
            user = new User { Id = 1, Username = "steve", Customer = customer };

            _mapper.Map<Rental>(dto).Returns(new Rental());

            _unitOfWork.VehicleRepository.GetByUuidAsync(dto.VehicleUuid!.Value).Returns(vehicle);
            _unitOfWork.UserRepository.GetUserCustomerByIdAsync(1).Returns(user);
            _unitOfWork.LocationRepository.GetByIdAsync(dto.PickupLocationId!.Value).Returns((Location?)null);

            await Assert.ThrowsAsync<EntityNotFoundException>(
                () => _service.CreateRentalAsync(dto, 1));

            await _unitOfWork.RentalRepository.DidNotReceive().AddAsync(Arg.Any<Rental>());
            await _unitOfWork.DidNotReceive().SaveChanges();
        }

        [Fact]
        public async Task CreateRentalAsync_WhenDatesAreInvalid_ThrowsInvalidArgumentException()
        {
            RentalCreateDTO dto;
            Vehicle? vehicle;
            User? user;
            Customer? customer;

            dto = CreateValidRentalDTO();
            dto = dto with { StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-1)) };
            vehicle = new Vehicle { Uuid = dto.VehicleUuid!.Value, Make = "Toyota" };
            customer = new Customer { DriverLicense = "DL-12345" };
            user = new User { Id = 1, Username = "steve", Customer = customer };

            _mapper.Map<Rental>(dto).Returns(new Rental());

            _unitOfWork.VehicleRepository.GetByUuidAsync(dto.VehicleUuid!.Value).Returns(vehicle);
            _unitOfWork.UserRepository.GetUserCustomerByIdAsync(1).Returns(user);
            _unitOfWork.LocationRepository.GetByIdAsync(dto.PickupLocationId!.Value).Returns(new Location { Id = 1 });
            _unitOfWork.LocationRepository.GetByIdAsync(dto.DropoffLocationId!.Value).Returns(new Location { Id = 2 });

            await Assert.ThrowsAsync<InvalidArgumentException>(
                () => _service.CreateRentalAsync(dto, 1));

            await _unitOfWork.RentalRepository.DidNotReceive().AddAsync(Arg.Any<Rental>());
            await _unitOfWork.DidNotReceive().SaveChanges();
        }

        [Fact]
        public async Task CreateRentalAsync_WhenDatesOverlap_ThrowsInvalidArgumentException()
        {
            RentalCreateDTO dto;
            Vehicle? vehicle;
            User? user;
            Customer? customer;

            dto = CreateValidRentalDTO();
            vehicle = new Vehicle { Uuid = dto.VehicleUuid!.Value, Make = "Toyota" };
            customer = new Customer { DriverLicense = "DL-12345" };
            user = new User { Id = 1, Username = "steve", Customer = customer };

            _mapper.Map<Rental>(dto).Returns(new Rental());

            _unitOfWork.VehicleRepository.GetByUuidAsync(dto.VehicleUuid!.Value).Returns(vehicle);
            _unitOfWork.UserRepository.GetUserCustomerByIdAsync(1).Returns(user);
            _unitOfWork.LocationRepository.GetByIdAsync(dto.PickupLocationId!.Value).Returns(new Location { Id = 1 });
            _unitOfWork.LocationRepository.GetByIdAsync(dto.DropoffLocationId!.Value).Returns(new Location { Id = 2 });
            _unitOfWork.RentalRepository.HasOverlappingRentalAsync(Arg.Any<int>(), Arg.Any<DateOnly>(), Arg.Any<DateOnly>()).Returns(true);

            await Assert.ThrowsAsync<InvalidArgumentException>(
                () => _service.CreateRentalAsync(dto, 1));

            await _unitOfWork.RentalRepository.DidNotReceive().AddAsync(Arg.Any<Rental>());
            await _unitOfWork.DidNotReceive().SaveChanges();
        }

        // ==================== UpdateRentalAsync ====================

        [Fact]
        public async Task UpdateRentalAsync_WhenAllDataIsValid_ReturnsRentalReadOnlyDTO()
        {
            RentalUpdateDTO dto = new RentalUpdateDTO { Status = RentalStatus.Approved };
            Rental? rental;
            Vehicle? vehicle;
            Employee? employee;

            employee = new Employee { Id = 1 };
            vehicle = new Vehicle { Id = 1, DailyRate = 50m };
            rental = new Rental
            {
                Uuid = Guid.NewGuid(),
                Employee = employee,
                EmployeeId = employee.Id,
                Vehicle = vehicle,
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow),
                EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(3))
            };
            string callerRole = "EMPLOYEE";

            _mapper.Map<RentalReadOnlyDTO>(Arg.Any<Rental>()).Returns(new RentalReadOnlyDTO());

            _unitOfWork.RentalRepository.GetByUuidAsync(rental.Uuid).Returns(rental);
            _unitOfWork.EmployeeRepository.GetByUserIdAsync(1).Returns(employee);

            RentalReadOnlyDTO result = await _service.UpdateRentalAsync(dto, rental.Uuid, 1, callerRole);

            Assert.Equal(3 * 50m, rental.TotalCost);
            await _unitOfWork.Received(1).SaveChanges();
            Assert.NotNull(result);
        }

        [Fact]
        public async Task UpdateRentalAsync_WhenStatusIsRejected_SetsVehicleAvailable()
        {
            RentalUpdateDTO dto = new RentalUpdateDTO { Status = RentalStatus.Rejected };
            Vehicle? vehicle;
            Employee? employee;
            Rental? rental;

            vehicle = new Vehicle { Id = 1, DailyRate = 50m, Status = VehicleStatus.Rented };
            employee = new Employee { Id = 1 };
            rental = new Rental
            {
                Uuid = Guid.NewGuid(),
                Employee = employee,
                EmployeeId = employee.Id,
                Vehicle = vehicle,
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow),
                EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(3))
            };

            _mapper.Map<RentalReadOnlyDTO>(Arg.Any<Rental>()).Returns(new RentalReadOnlyDTO());
            _unitOfWork.RentalRepository.GetByUuidAsync(rental.Uuid).Returns(rental);
            _unitOfWork.EmployeeRepository.GetByUserIdAsync(1).Returns(employee);

            RentalReadOnlyDTO result = await _service.UpdateRentalAsync(dto, rental.Uuid, 1, "EMPLOYEE");

            Assert.Equal(VehicleStatus.Available, vehicle.Status);
            await _unitOfWork.Received(1).SaveChanges();
            Assert.NotNull(result);
        }

        [Fact]
        public async Task UpdateRentalAsync_WhenCallerIsAdmin_SkipsEmployeeLookup()
        {
            RentalUpdateDTO dto = new RentalUpdateDTO { Status = RentalStatus.Returned };
            Vehicle? vehicle;
            Rental? rental;

            vehicle = new Vehicle { Id = 1, DailyRate = 50m, Status = VehicleStatus.Rented };
            rental = new Rental
            {
                Uuid = Guid.NewGuid(),
                Vehicle = vehicle,
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow),
                EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(3))
            };

            _mapper.Map<RentalReadOnlyDTO>(Arg.Any<Rental>()).Returns(new RentalReadOnlyDTO());
            _unitOfWork.RentalRepository.GetByUuidAsync(rental.Uuid).Returns(rental);

            RentalReadOnlyDTO result = await _service.UpdateRentalAsync(dto, rental.Uuid, 1, "ADMIN");

            Assert.Equal(VehicleStatus.Available, vehicle.Status);
            await _unitOfWork.EmployeeRepository.DidNotReceive().GetByUserIdAsync(Arg.Any<int>());
            await _unitOfWork.Received(1).SaveChanges();
            Assert.NotNull(result);
        }

        [Fact]
        public async Task UpdateRentalAsync_WhenRentalNotFound_ThrowsEntityNotFoundException()
        {
            RentalUpdateDTO dto = new RentalUpdateDTO { Status = RentalStatus.Approved };

            _unitOfWork.RentalRepository.GetByUuidAsync(Arg.Any<Guid>()).Returns((Rental?)null);

            await Assert.ThrowsAsync<EntityNotFoundException>(
                () => _service.UpdateRentalAsync(dto, Guid.NewGuid(), 1, "ADMIN"));

            await _unitOfWork.DidNotReceive().SaveChanges();
        }

        [Fact]
        public async Task UpdateRentalAsync_WhenEmployeeNotFound_ThrowsEntityNotFoundException()
        {
            RentalUpdateDTO dto = new RentalUpdateDTO { Status = RentalStatus.Approved };
            Vehicle? vehicle;
            Rental? rental;

            vehicle = new Vehicle { Id = 1, DailyRate = 50m };
            rental = new Rental
            {
                Uuid = Guid.NewGuid(),
                Vehicle = vehicle,
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow),
                EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(3))
            };

            _unitOfWork.RentalRepository.GetByUuidAsync(rental.Uuid).Returns(rental);
            _unitOfWork.EmployeeRepository.GetByUserIdAsync(1).Returns((Employee?)null);

            await Assert.ThrowsAsync<EntityNotFoundException>(
                () => _service.UpdateRentalAsync(dto, rental.Uuid, 1, "EMPLOYEE"));

            await _unitOfWork.DidNotReceive().SaveChanges();
        }

        [Fact]
        public async Task UpdateRentalAsync_WhenEmployeeIsDeleted_ThrowsEntityNotFoundException()
        {
            RentalUpdateDTO dto = new RentalUpdateDTO { Status = RentalStatus.Approved };
            Vehicle? vehicle;
            Rental? rental;
            Employee? employee;

            vehicle = new Vehicle { Id = 1, DailyRate = 50m };
            employee = new Employee { Id = 1, IsDeleted = true };
            rental = new Rental
            {
                Uuid = Guid.NewGuid(),
                Vehicle = vehicle,
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow),
                EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(3))
            };

            _unitOfWork.RentalRepository.GetByUuidAsync(rental.Uuid).Returns(rental);
            _unitOfWork.EmployeeRepository.GetByUserIdAsync(1).Returns(employee);

            await Assert.ThrowsAsync<EntityNotFoundException>(
                () => _service.UpdateRentalAsync(dto, rental.Uuid, 1, "EMPLOYEE"));

            await _unitOfWork.DidNotReceive().SaveChanges();
        }

        // ==================== GetPaginatedFilteredRentalsAsync ====================

        [Fact]
        public async Task GetPaginatedFilteredRentalsAsync_WhenFiltersBySearch_ReturnsOnlyMatchingRentals()
        {
            RentalFiltersDTO filters;
            PaginatedResult<Rental> repoResult;

            filters = new RentalFiltersDTO { Search = "Yaris" };

            repoResult = new PaginatedResult<Rental>
            {
                Data = new List<Rental> { new Rental()},
                TotalRecords = 1,
                PageNumber = 1,
                PageSize = 10
            };

            _unitOfWork.RentalRepository
                .GetPaginatedFilteredRentalsAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<List<Expression<Func<Rental, bool>>>>())
                .Returns(repoResult);

            _mapper.Map<List<RentalReadOnlyDTO>>(repoResult.Data).Returns(new List<RentalReadOnlyDTO> { new RentalReadOnlyDTO()});

            PaginatedResult<RentalReadOnlyDTO> result = await _service.GetPaginatedFilteredRentalsAsync(1, 10, filters);

            Assert.NotNull(result);
            Assert.Equal(1, result.TotalRecords);
        }

        [Fact]
        public async Task GetPaginatedFilteredRentalsAsync_WhenNoFilters_ReturnsAllRentals()
        {
            RentalFiltersDTO filters;
            PaginatedResult<Rental> repoResult;

            filters = new RentalFiltersDTO();

            repoResult = new PaginatedResult<Rental>
            {
                Data = new List<Rental> { new Rental(), new Rental() },
                TotalRecords = 2,
                PageNumber = 1,
                PageSize = 10
            };

            _unitOfWork.RentalRepository
                .GetPaginatedFilteredRentalsAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<List<Expression<Func<Rental, bool>>>>())
                .Returns(repoResult);

            _mapper.Map<List<RentalReadOnlyDTO>>(repoResult.Data).Returns(new List<RentalReadOnlyDTO> { new RentalReadOnlyDTO(), new RentalReadOnlyDTO() });

            PaginatedResult<RentalReadOnlyDTO> result = await _service.GetPaginatedFilteredRentalsAsync(1, 10, filters);

            Assert.NotNull(result);
            Assert.Equal(2, result.TotalRecords);
        }

        [Fact]
        public async Task GetPaginatedFilteredRentalsAsync_WhenFiltersByEmployeeNameAndCustomerName_ReturnsMatchingRentals()
        {
            RentalFiltersDTO filters;
            PaginatedResult<Rental> repoResult;

            filters = new RentalFiltersDTO { EmployeeName = "John", CustomerName = "Steve" };

            repoResult = new PaginatedResult<Rental>
            {
                Data = new List<Rental> { new Rental() },
                TotalRecords = 1,
                PageNumber = 1,
                PageSize = 10
            };

            _unitOfWork.RentalRepository
                .GetPaginatedFilteredRentalsAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<List<Expression<Func<Rental, bool>>>>())
                .Returns(repoResult);

            _mapper.Map<List<RentalReadOnlyDTO>>(repoResult.Data).Returns(new List<RentalReadOnlyDTO> { new RentalReadOnlyDTO() });

            PaginatedResult<RentalReadOnlyDTO> result = await _service.GetPaginatedFilteredRentalsAsync(1, 10, filters);

            Assert.NotNull(result);
            Assert.Equal(1, result.TotalRecords);
        }

        [Fact]
        public async Task GetPaginatedFilteredRentalsAsync_WhenNoRentalsExist_ReturnsEmptyResult()
        {
            RentalFiltersDTO filters;
            PaginatedResult<Rental> repoResult;

            filters = new RentalFiltersDTO();

            repoResult = new PaginatedResult<Rental>
            {
                Data = new List<Rental>(),
                TotalRecords = 0,
                PageNumber = 1,
                PageSize = 10
            };

            _unitOfWork.RentalRepository
                .GetPaginatedFilteredRentalsAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<List<Expression<Func<Rental, bool>>>>())
                .Returns(repoResult);

            _mapper.Map<List<RentalReadOnlyDTO>>(repoResult.Data).Returns(new List<RentalReadOnlyDTO>());

            PaginatedResult<RentalReadOnlyDTO> result = await _service.GetPaginatedFilteredRentalsAsync(1, 10, filters);

            Assert.NotNull(result);
            Assert.Equal(0, result.TotalRecords);
            Assert.Empty(result.Data);
        }

        // ==================== CustomerRentalHistoryAsync ====================

        [Fact]
        public async Task CustomerRentalHistoryAsync_WhenNoFilters_ReturnsAllCustomerRentals()
        {
            User user;
            PaginatedResult<Rental> repoResult;

            user = new User { Id = 1, Customer = new Customer { Id = 1 } };

            repoResult = new PaginatedResult<Rental>
            {
                Data = new List<Rental> { new Rental(), new Rental() },
                TotalRecords = 2,
                PageNumber = 1,
                PageSize = 10
            };

            _unitOfWork.UserRepository.GetUserCustomerByIdAsync(1).Returns(user);

            _unitOfWork.RentalRepository
                .GetPaginatedFilteredRentalsAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<List<Expression<Func<Rental, bool>>>>())
                .Returns(repoResult);

            _mapper.Map<List<RentalReadOnlyDTO>>(repoResult.Data).Returns(new List<RentalReadOnlyDTO> { new RentalReadOnlyDTO(), new RentalReadOnlyDTO() });

            PaginatedResult<RentalReadOnlyDTO> result = await _service.CustomerRentalHistoryAsync(1, 1, 10, new RentalFiltersDTO());

            Assert.NotNull(result);
            Assert.Equal(2, result.TotalRecords);
        }

        [Fact]
        public async Task CustomerRentalHistoryAsync_WhenFiltersByStatus_ReturnsMatchingRentals()
        {
            User user;
            PaginatedResult<Rental> repoResult;
            RentalFiltersDTO filters;

            user = new User { Id = 1, Customer = new Customer { Id = 1 } };

            repoResult = new PaginatedResult<Rental>
            {
                Data = new List<Rental> { new Rental() },
                TotalRecords = 1,
                PageNumber = 1,
                PageSize = 10
            };

            filters = new RentalFiltersDTO { Status = RentalStatus.Approved };

            _unitOfWork.UserRepository.GetUserCustomerByIdAsync(1).Returns(user);

            _unitOfWork.RentalRepository
                .GetPaginatedFilteredRentalsAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<List<Expression<Func<Rental, bool>>>>())
                .Returns(repoResult);

            _mapper.Map<List<RentalReadOnlyDTO>>(repoResult.Data).Returns(new List<RentalReadOnlyDTO> { new RentalReadOnlyDTO() });

            PaginatedResult<RentalReadOnlyDTO> result = await _service.CustomerRentalHistoryAsync(1, 1, 10, filters);

            Assert.NotNull(result);
            Assert.Equal(1, result.TotalRecords);
        }

        [Fact]
        public async Task CustomerRentalHistoryAsync_WhenFiltersByStatusAndMinCost_ReturnsMatchingRentals()
        {
            User user;
            PaginatedResult<Rental> repoResult;
            RentalFiltersDTO filters;

            user = new User { Id = 1, Customer = new Customer { Id = 1 } };

            repoResult = new PaginatedResult<Rental>
            {
                Data = new List<Rental> { new Rental() },
                TotalRecords = 1,
                PageNumber = 1,
                PageSize = 10
            };

            filters = new RentalFiltersDTO { MinTotalCost = 50m, MaxTotalCost = 100m };

            _unitOfWork.UserRepository.GetUserCustomerByIdAsync(1).Returns(user);

            _unitOfWork.RentalRepository
                .GetPaginatedFilteredRentalsAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<List<Expression<Func<Rental, bool>>>>())
                .Returns(repoResult);

            _mapper.Map<List<RentalReadOnlyDTO>>(repoResult.Data).Returns(new List<RentalReadOnlyDTO> { new RentalReadOnlyDTO() });

            PaginatedResult<RentalReadOnlyDTO> result = await _service.CustomerRentalHistoryAsync(1, 1, 10, filters);

            Assert.NotNull(result);
            Assert.Equal(1, result.TotalRecords);
        }

        [Fact]
        public async Task CustomerRentalHistoryAsync_WhenNoRentalsExist_ReturnsEmptyResult()
        {
            User user;
            PaginatedResult<Rental> repoResult;

            user = new User { Id = 1, Customer = new Customer { Id = 1 } };

            repoResult = new PaginatedResult<Rental>
            {
                Data = new List<Rental>(),
                TotalRecords = 0,
                PageNumber = 1,
                PageSize = 10
            };

            _unitOfWork.UserRepository.GetUserCustomerByIdAsync(1).Returns(user);

            _unitOfWork.RentalRepository
                .GetPaginatedFilteredRentalsAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<List<Expression<Func<Rental, bool>>>>())
                .Returns(repoResult);

            _mapper.Map<List<RentalReadOnlyDTO>>(repoResult.Data).Returns(new List<RentalReadOnlyDTO>());

            PaginatedResult<RentalReadOnlyDTO> result = await _service.CustomerRentalHistoryAsync(1, 1, 10, new RentalFiltersDTO());

            Assert.NotNull(result);
            Assert.Equal(0, result.TotalRecords);
            Assert.Empty(result.Data);
        }

        [Fact]
        public async Task CustomerRentalHistoryAsync_WhenUserNotFound_ThrowsEntityNotFoundException()
        {
            _unitOfWork.UserRepository.GetUserCustomerByIdAsync(1).Returns((User?)null);

            await Assert.ThrowsAsync<EntityNotFoundException>(
                () => _service.CustomerRentalHistoryAsync(1, 1, 10, new RentalFiltersDTO()));

            await _unitOfWork.RentalRepository.DidNotReceive()
                .GetPaginatedFilteredRentalsAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<List<Expression<Func<Rental, bool>>>>());
        }

        [Fact]
        public async Task CustomerRentalHistoryAsync_WhenCustomerNotLinked_ThrowsEntityNotFoundException()
        {
            User user;
            user = new User { Id = 1, Customer = null };

            _unitOfWork.UserRepository.GetUserCustomerByIdAsync(1).Returns(user);

            await Assert.ThrowsAsync<EntityNotFoundException>(
                () => _service.CustomerRentalHistoryAsync(1, 1, 10, new RentalFiltersDTO()));

            await _unitOfWork.RentalRepository.DidNotReceive()
                .GetPaginatedFilteredRentalsAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<List<Expression<Func<Rental, bool>>>>());
        }

        private static RentalCreateDTO CreateValidRentalDTO()
        {
            RentalCreateDTO dto;

            dto = new RentalCreateDTO
            {
                VehicleUuid = Guid.NewGuid(),
                PickupLocationId = 1,
                DropoffLocationId = 2,
                StartDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(1)),
                EndDate = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(3))
            };

            return dto;
        }
    }
}
