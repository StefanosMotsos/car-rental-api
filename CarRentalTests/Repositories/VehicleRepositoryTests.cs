using CarRentalApp.Core;
using CarRentalApp.Data;
using CarRentalApp.Models;
using CarRentalApp.Models.Enums;
using CarRentalApp.Repositories.Vehicles;
using CarRentalTests.Helper;
using NSubstitute.Routing.Handlers;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace CarRentalTests.Repositories
{
    public class VehicleRepositoryTests
    {
        private readonly CarRentalDbContext _context;
        private readonly VehicleRepository _repository;
        private static CancellationToken Ct => TestContext.Current.CancellationToken;

        public VehicleRepositoryTests()
        {
            _context = TestDbContextFactory.Create();
            _repository = new VehicleRepository(_context);
        }

        // ==================== GetVehicleByLicensePlateAsync ====================

        [Fact]
        public async Task GetVehicleByLicensePlateAsync_WhenVehicleExists_ReturnsVehicle()
        {
            Vehicle? vehicle;
            vehicle = CreateVehicle("ABC_123");

            await _context.AddAsync(vehicle, Ct);
            await _context.SaveChangesAsync(Ct);

            vehicle = await _repository.GetVehicleByLicensePlateAsync("ABC_123");

            Assert.NotNull(vehicle);
            Assert.Equal("ABC_123", vehicle.LicensePlate);
        }

        [Fact]
        public async Task GetVehicleByLicensePlateAsync_WhenVehicleDoesNotExist_ReturnsNull()
        {

            Vehicle? vehicle = await _repository.GetVehicleByLicensePlateAsync("ABC_123");

            Assert.Null(vehicle);
        }

        // ==================== GetByUuidAsync ====================

        [Fact]
        public async Task GetByUuidAsync_WhenVehicleExists_ReturnVehicle()
        {
            Vehicle? vehicle = CreateVehicle("ABC_123");
            await _context.AddAsync(vehicle, Ct);
            await _context.SaveChangesAsync(Ct);

            vehicle = await _repository.GetByUuidAsync(vehicle.Uuid);

            Assert.NotNull(vehicle);
            Assert.Equal("ABC_123", vehicle.LicensePlate);
        }

        [Fact]
        public async Task GetByUuidAsync_WhenVehicleDoesNotExist_ReturnNull()
        {

            Vehicle? vehicle = await _repository.GetByUuidAsync(Guid.NewGuid());

            Assert.Null(vehicle);
        }


        // ==================== GetPaginatedFilteredVehiclesAsync ====================

        [Fact]
        public async Task GetPaginatedFilteredVehiclesAsync_WhenNoVehiclesExists_ReturnEmptyData()
        {
            PaginatedResult<Vehicle> vehicles = await _repository.GetPaginatedFilteredVehiclesAsync(1, 10, null!);

            Assert.Empty(vehicles.Data);
        }


        [Fact]
        public async Task GetPaginatedFilteredVehiclesAsync_WhenNoPredicates_ReturnAllVehicles()
        {
            Vehicle? vehicle1;
            Vehicle? vehicle2;
            vehicle1 = CreateVehicle("ABC_123");
            vehicle2 = CreateVehicle("ABC_124");

            await _context.AddAsync(vehicle1, Ct);
            await _context.AddAsync(vehicle2, Ct);
            await _context.SaveChangesAsync(Ct);

            PaginatedResult<Vehicle> vehicles = await _repository.GetPaginatedFilteredVehiclesAsync(1, 10, new List<Expression<Func<Vehicle, bool>>>());

            Assert.Equal(2, vehicles.Data.Count);
        }

        [Fact]
        public async Task GetPaginatedFilteredVehiclesAsync_WhenPredicateFilterByStatus_ReturnOnlyMatchingVehicles()
        {
            Vehicle? vehicle1;
            Vehicle? vehicle2;
            vehicle1 = CreateVehicle("ABC_123");
            vehicle2 = CreateVehicle("ABC_124");
            vehicle2.Status = VehicleStatus.Rented;

            await _context.AddAsync(vehicle1, Ct);
            await _context.AddAsync(vehicle2, Ct);
            await _context.SaveChangesAsync(Ct);

            List<Expression<Func<Vehicle, bool>>> predicates;
            predicates = new List<Expression<Func<Vehicle, bool>>>
                {
                    v => v.Status == VehicleStatus.Available
                };

            PaginatedResult<Vehicle> vehicles = await _repository.GetPaginatedFilteredVehiclesAsync(1, 10, predicates);

            Assert.Single(vehicles.Data);
        }

        [Fact]
        public async Task GetPaginatedFilteredVehiclesAsync_WhenPredicateMatchesNone_ReturnEmptyData()
        {
            Vehicle? vehicle1;
            Vehicle? vehicle2;
            vehicle1 = CreateVehicle("ABC_123");
            vehicle2 = CreateVehicle("ABC_124");
            vehicle2.Status = VehicleStatus.Rented;

            await _context.AddAsync(vehicle1, Ct);
            await _context.AddAsync(vehicle2, Ct);
            await _context.SaveChangesAsync(Ct);

            List<Expression<Func<Vehicle, bool>>> predicates = [];
            predicates = new List<Expression<Func<Vehicle, bool>>>
                {
                    v => v.Status == VehicleStatus.Maintenance
                };

            PaginatedResult<Vehicle> vehicles = await _repository.GetPaginatedFilteredVehiclesAsync(1, 10, predicates);

            Assert.Empty(vehicles.Data);
        }

        // ----- Helper Method -----

        private static Vehicle CreateVehicle(string licensePlate)
        {
            Vehicle vehicle;

            vehicle = new Vehicle
            {
                Make = "Toyota",
                Model = "Yaris",
                Year = 2020,
                LicensePlate = licensePlate,
                DailyRate = 50m,
                TierType = TierType.Economy,
                Status = VehicleStatus.Available,
                Category = new Category { Name = "SUV" },
                Photo = new VehiclePhoto
                {
                    OriginalName = "photo.jpg",
                    SavedName = "photo_saved.jpg",
                    FilePath = "/uploads/photo_saved.jpg",
                    ContentType = "image/jpeg",
                    Extension = ".jpg"
                }
            };

            return vehicle;
        }
    }
}
