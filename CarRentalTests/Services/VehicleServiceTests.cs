using AutoMapper;
using CarRentalApp.Core;
using CarRentalApp.Core.Filters;
using CarRentalApp.DTO.Vehicle;
using CarRentalApp.Exceptions;
using CarRentalApp.Models;
using CarRentalApp.Models.Enums;
using CarRentalApp.Repositories;
using CarRentalApp.Security;
using CarRentalApp.Services.Rentals;
using CarRentalApp.Services.Vehicles;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace CarRentalTests.Services
{
    public class VehicleServiceTests
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<VehicleService> _logger;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly VehicleService _service;

        public VehicleServiceTests()
        {
            _unitOfWork = Substitute.For<IUnitOfWork>();
            _mapper = Substitute.For<IMapper>();
            _logger = Substitute.For<ILogger<VehicleService>>();
            _configuration = Substitute.For<IConfiguration>();

            _service = new VehicleService(_unitOfWork, _mapper, _logger, _configuration);
        }


        // ==================== AddVehicleAsync ====================

        [Fact]
        public async Task AddVehicleAsync_WhenVehicleIsNew_AddsVehicleAndSaves()
        {
            VehicleCreateDTO dto;
            Category category;
            Vehicle mappedVehicle;

            dto = CreateValidVehicleCreateDTO("ABC_123");
            category = new Category { Id = 1, Name = "SUV" };
            mappedVehicle = new Vehicle { LicensePlate = "ABC_123", Category = category };

            _unitOfWork.VehicleRepository.GetVehicleByLicensePlateAsync("ABC_123").Returns((Vehicle?)null);
            _unitOfWork.CategoryRepository.GetByIdAsync(1).Returns(category);

            _mapper.Map<Vehicle>(dto).Returns(mappedVehicle);
            _mapper.Map<VehicleReadOnlyDTO>(mappedVehicle).Returns(new VehicleReadOnlyDTO { LicensePlate = "ABC_123" });

            VehicleReadOnlyDTO result = await _service.AddVehicleAsync(dto);

            await _unitOfWork.VehicleRepository.Received(1).AddAsync(mappedVehicle);
            await _unitOfWork.Received(1).SaveChanges();

            Assert.NotNull(result);
        }

        [Fact]
        public async Task AddVehicleAsync_WhenLicensePlateAlreadyExists_ThrowsEntityAlreadyExistsException()
        {
            VehicleCreateDTO dto;
            Vehicle existingVehicle;

            dto = CreateValidVehicleCreateDTO("ABC_123");
            existingVehicle = new Vehicle { LicensePlate = "ABC_123" };

            _unitOfWork.VehicleRepository.GetVehicleByLicensePlateAsync("ABC_123").Returns(existingVehicle);

            await Assert.ThrowsAsync<EntityAlreadyExistsException>(
                () => _service.AddVehicleAsync(dto));

            await _unitOfWork.VehicleRepository.DidNotReceive().AddAsync(Arg.Any<Vehicle>());
            await _unitOfWork.DidNotReceive().SaveChanges();
        }

        [Fact]
        public async Task AddVehicleAsync_WhenCategoryNotFound_ThrowsInvalidArgumentException()
        {
            VehicleCreateDTO dto;

            dto = CreateValidVehicleCreateDTO("ABC_123");

            _unitOfWork.VehicleRepository.GetVehicleByLicensePlateAsync("ABC_123").Returns((Vehicle?)null);
            _unitOfWork.CategoryRepository.GetByIdAsync(1).Returns((Category?)null);

            await Assert.ThrowsAsync<InvalidArgumentException>(
                () => _service.AddVehicleAsync(dto));

            await _unitOfWork.VehicleRepository.DidNotReceive().AddAsync(Arg.Any<Vehicle>());
            await _unitOfWork.DidNotReceive().SaveChanges();
        }

        // ==================== UpdateVehicleAsync ====================

        [Fact]
        public async Task UpdateVehicleAsync_WhenVehicleIsValid_UpdatesAndSaves()
        {
            Guid uuid;
            VehicleUpdateDTO dto;
            Vehicle existingVehicle;
            Category category;

            uuid = Guid.NewGuid();
            dto = CreateValidVehicleUpdateDTO("ABC_123");
            category = new Category { Id = 1, Name = "SUV" };
            existingVehicle = new Vehicle { LicensePlate = "ABC_123", Category = category };

            _unitOfWork.VehicleRepository.GetByUuidAsync(uuid).Returns(existingVehicle);
            _unitOfWork.CategoryRepository.GetByIdAsync(1).Returns(category);

            _mapper.Map<VehicleReadOnlyDTO>(existingVehicle).Returns(new VehicleReadOnlyDTO { LicensePlate = "ABC_123" });

            VehicleReadOnlyDTO result = await _service.UpdateVehicleAsync(uuid, dto);

            await _unitOfWork.Received(1).SaveChanges();
            Assert.NotNull(result);
        }

        [Fact]
        public async Task UpdateVehicleAsync_WhenVehicleNotFound_ThrowsEntityNotFoundException()
        {
            Guid uuid;
            VehicleUpdateDTO dto;

            uuid = Guid.NewGuid();
            dto = CreateValidVehicleUpdateDTO("ABC_123");

            _unitOfWork.VehicleRepository.GetByUuidAsync(uuid).Returns((Vehicle?)null);

            await Assert.ThrowsAsync<EntityNotFoundException>(
                () => _service.UpdateVehicleAsync(uuid, dto));

            await _unitOfWork.DidNotReceive().SaveChanges();
        }

        [Fact]
        public async Task UpdateVehicleAsync_WhenLicensePlateAlreadyExists_ThrowsEntityAlreadyExistsException()
        {
            Guid uuid;
            VehicleUpdateDTO dto;
            Vehicle existingVehicle;
            Vehicle conflictingVehicle;

            uuid = Guid.NewGuid();
            dto = CreateValidVehicleUpdateDTO("ABC_999");
            existingVehicle = new Vehicle { LicensePlate = "ABC_123" };
            conflictingVehicle = new Vehicle { LicensePlate = "ABC_999" };

            _unitOfWork.VehicleRepository.GetByUuidAsync(uuid).Returns(existingVehicle);
            _unitOfWork.VehicleRepository.GetVehicleByLicensePlateAsync("ABC_999").Returns(conflictingVehicle);

            await Assert.ThrowsAsync<EntityAlreadyExistsException>(
                () => _service.UpdateVehicleAsync(uuid, dto));

            await _unitOfWork.DidNotReceive().SaveChanges();
        }

        [Fact]
        public async Task UpdateVehicleAsync_WhenCategoryNotFound_ThrowsInvalidArgumentException()
        {
            Guid uuid;
            VehicleUpdateDTO dto;
            Vehicle existingVehicle;

            uuid = Guid.NewGuid();
            dto = CreateValidVehicleUpdateDTO("ABC_123");
            existingVehicle = new Vehicle { LicensePlate = "ABC_123" };

            _unitOfWork.VehicleRepository.GetByUuidAsync(uuid).Returns(existingVehicle);
            _unitOfWork.CategoryRepository.GetByIdAsync(1).Returns((Category?)null);

            await Assert.ThrowsAsync<InvalidArgumentException>(
                () => _service.UpdateVehicleAsync(uuid, dto));

            await _unitOfWork.DidNotReceive().SaveChanges();
        }

        // ==================== DeleteVehicleByUuidAsync ====================

        [Fact]
        public async Task DeleteVehicleByUuidAsync_WhenVehicleExists_SoftDeletesAndSaves()
        {
            Guid uuid;
            Vehicle existingVehicle;

            uuid = Guid.NewGuid();
            existingVehicle = new Vehicle { LicensePlate = "ABC_123" };

            _unitOfWork.VehicleRepository.GetByUuidAsync(uuid).Returns(existingVehicle);
            _unitOfWork.RentalRepository.HasActiveRentalsForVehicleAsync(existingVehicle.Id).Returns(false);

            bool result = await _service.DeleteVehicleByUuidAsync(uuid);

            await _unitOfWork.Received(1).SaveChanges();
            Assert.True(result);
        }

        [Fact]
        public async Task DeleteVehicleByUuidAsync_WhenVehicleNotFound_ThrowsEntityNotFoundException()
        {
            Guid uuid;

            uuid = Guid.NewGuid();

            _unitOfWork.VehicleRepository.GetByUuidAsync(uuid).Returns((Vehicle?)null);

            await Assert.ThrowsAsync<EntityNotFoundException>(
                () => _service.DeleteVehicleByUuidAsync(uuid));

            await _unitOfWork.DidNotReceive().SaveChanges();
        }

        [Fact]
        public async Task DeleteVehicleByUuidAsync_WhenVehicleHasActiveRentals_ThrowsInvalidArgumentException()
        {
            Guid uuid;
            Vehicle existingVehicle;

            uuid = Guid.NewGuid();
            existingVehicle = new Vehicle { LicensePlate = "ABC_123" };

            _unitOfWork.VehicleRepository.GetByUuidAsync(uuid).Returns(existingVehicle);
            _unitOfWork.RentalRepository.HasActiveRentalsForVehicleAsync(existingVehicle.Id).Returns(true);

            await Assert.ThrowsAsync<InvalidArgumentException>(
                () => _service.DeleteVehicleByUuidAsync(uuid));

            await _unitOfWork.DidNotReceive().SaveChanges();
        }

        // ==================== GetVehicleByUuidAsync ====================

        [Fact]
        public async Task GetVehicleByUuidAsync_WhenVehicleExists_ReturnsVehicle()
        {
            Guid uuid;
            Vehicle existingVehicle;

            uuid = Guid.NewGuid();
            existingVehicle = new Vehicle { LicensePlate = "ABC_123" };

            _unitOfWork.VehicleRepository.GetByUuidAsync(uuid).Returns(existingVehicle);
            _mapper.Map<VehicleReadOnlyDTO>(existingVehicle).Returns(new VehicleReadOnlyDTO { LicensePlate = "ABC_123" });

            VehicleReadOnlyDTO result = await _service.GetVehicleByUuidAsync(uuid);

            Assert.NotNull(result);
        }

        [Fact]
        public async Task GetVehicleByUuidAsync_WhenVehicleNotFound_ThrowsEntityNotFoundException()
        {
            Guid uuid;

            uuid = Guid.NewGuid();

            _unitOfWork.VehicleRepository.GetByUuidAsync(uuid).Returns((Vehicle?)null);

            await Assert.ThrowsAsync<EntityNotFoundException>(
                () => _service.GetVehicleByUuidAsync(uuid));
        }

        // ==================== GetPaginatedFilteredVehiclesAsync ====================

        [Fact]
        public async Task GetPaginatedFilteredVehiclesAsync_WhenFiltersByStatus_ReturnOnlyMatchingVehicles()
        {
            VehicleFiltersDTO filters;
            PaginatedResult<Vehicle> repoResult;

            filters = new VehicleFiltersDTO { Status = VehicleStatus.Available};
            repoResult = new PaginatedResult<Vehicle>
            {
                Data = new List<Vehicle> { new Vehicle(), new Vehicle() },
                TotalRecords = 2,
                PageNumber = 1,
                PageSize = 10
            };

            _mapper.Map<List<VehicleReadOnlyDTO>>(repoResult.Data).Returns(new List<VehicleReadOnlyDTO> { new VehicleReadOnlyDTO(), new VehicleReadOnlyDTO()});

            _unitOfWork.VehicleRepository
                .GetPaginatedFilteredVehiclesAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<List<Expression<Func<Vehicle, bool>>>>())
                .Returns(repoResult);

            PaginatedResult<VehicleReadOnlyDTO> result = await _service.GetPaginatedFilteredVehiclesAsync(1, 10, filters);

            Assert.NotNull(result);
            Assert.Equal(2, result.TotalRecords);
        }

        private static VehicleUpdateDTO CreateValidVehicleUpdateDTO(string licensePlate)
        {
            VehicleUpdateDTO dto;

            dto = new VehicleUpdateDTO
            {
                Make = "Toyota",
                Model = "Yaris",
                Year = 2020,
                LicensePlate = licensePlate,
                DailyRate = 50m,
                TierType = TierType.Economy,
                CategoryId = 1
            };

            return dto;
        }

        [Fact]
        public async Task GetPaginatedFilteredVehiclesAsync_WhenNoFilters_ReturnsAllVehicles()
        {
            VehicleFiltersDTO filters;
            PaginatedResult<Vehicle> repoResult;

            filters = new VehicleFiltersDTO();
            repoResult = new PaginatedResult<Vehicle>
            {
                Data = new List<Vehicle> { new Vehicle(), new Vehicle() },
                TotalRecords = 2,
                PageNumber = 1,
                PageSize = 10
            };

            _unitOfWork.VehicleRepository
                .GetPaginatedFilteredVehiclesAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<List<Expression<Func<Vehicle, bool>>>>())
                .Returns(repoResult);

            _mapper.Map<List<VehicleReadOnlyDTO>>(repoResult.Data).Returns(new List<VehicleReadOnlyDTO> { new VehicleReadOnlyDTO(), new VehicleReadOnlyDTO() });

            PaginatedResult<VehicleReadOnlyDTO> result = await _service.GetPaginatedFilteredVehiclesAsync(1, 10, filters);

            Assert.NotNull(result);
            Assert.Equal(2, result.TotalRecords);
        }

        [Fact]
        public async Task GetPaginatedFilteredVehiclesAsync_WhenFiltersByStatusAndTierType_ReturnsMatchingVehicles()
        {
            VehicleFiltersDTO filters;
            PaginatedResult<Vehicle> repoResult;

            filters = new VehicleFiltersDTO { Status = VehicleStatus.Available, TierType = TierType.Economy };
            repoResult = new PaginatedResult<Vehicle>
            {
                Data = new List<Vehicle> { new Vehicle() },
                TotalRecords = 1,
                PageNumber = 1,
                PageSize = 10
            };

            _unitOfWork.VehicleRepository
                .GetPaginatedFilteredVehiclesAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<List<Expression<Func<Vehicle, bool>>>>())
                .Returns(repoResult);

            _mapper.Map<List<VehicleReadOnlyDTO>>(repoResult.Data).Returns(new List<VehicleReadOnlyDTO> { new VehicleReadOnlyDTO() });

            PaginatedResult<VehicleReadOnlyDTO> result = await _service.GetPaginatedFilteredVehiclesAsync(1, 10, filters);

            Assert.NotNull(result);
            Assert.Equal(1, result.TotalRecords);
        }

        [Fact]
        public async Task GetPaginatedFilteredVehiclesAsync_WhenNoVehiclesExist_ReturnsEmptyResult()
        {
            VehicleFiltersDTO filters;
            PaginatedResult<Vehicle> repoResult;

            filters = new VehicleFiltersDTO();
            repoResult = new PaginatedResult<Vehicle>
            {
                Data = new List<Vehicle>(),
                TotalRecords = 0,
                PageNumber = 1,
                PageSize = 10
            };

            _unitOfWork.VehicleRepository
                .GetPaginatedFilteredVehiclesAsync(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<List<Expression<Func<Vehicle, bool>>>>())
                .Returns(repoResult);

            _mapper.Map<List<VehicleReadOnlyDTO>>(repoResult.Data).Returns(new List<VehicleReadOnlyDTO>());

            PaginatedResult<VehicleReadOnlyDTO> result = await _service.GetPaginatedFilteredVehiclesAsync(1, 10, filters);

            Assert.NotNull(result);
            Assert.Equal(0, result.TotalRecords);
            Assert.Empty(result.Data);
        }

        private static VehicleCreateDTO CreateValidVehicleCreateDTO(string licensePlate)
        {
            VehicleCreateDTO dto;

            dto = new VehicleCreateDTO
            {
                Make = "Toyota",
                Model = "Yaris",
                Year = 2020,
                LicensePlate = licensePlate,
                DailyRate = 50m,
                TierType = TierType.Economy,
                CategoryId = 1
            };

            return dto;
        }
    }
}
