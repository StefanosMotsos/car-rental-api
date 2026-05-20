using AutoMapper;
using CarRentalApp.Core;
using CarRentalApp.Core.Filters;
using CarRentalApp.DTO.Vehicle;
using CarRentalApp.Exceptions;
using CarRentalApp.Models;
using CarRentalApp.Models.Enums;
using CarRentalApp.Repositories;
using CarRentalApp.Resources;
using System.Linq.Expressions;

namespace CarRentalApp.Services.Vehicles
{
    public class VehicleService : IVehicleService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<VehicleService> _logger;
        private readonly IConfiguration _configuration;
        private readonly string _uploadDir;

        public VehicleService(IUnitOfWork unitOfWork, IMapper mapper, 
            ILogger<VehicleService> logger, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _configuration = configuration;
            _uploadDir = _configuration["FileStorage:UploadDir"]!;
        }

        public async Task<VehicleReadOnlyDTO> AddVehicleAsync(VehicleCreateDTO dto)
        {
            Vehicle? existingVehicle = await _unitOfWork.VehicleRepository.GetVehicleByLicensePlateAsync(dto.LicensePlate!);
            if (existingVehicle != null)
            {
                _logger.LogWarning("Vehicle with license plate {LicensePlate} already exists", dto.LicensePlate);
                throw new EntityAlreadyExistsException("Vehicle", ErrorMessages.AlreadyExists);
            }

            Category? category = await _unitOfWork.CategoryRepository.GetByIdAsync(dto.CategoryId!.Value);
            if (category == null)
            {
                _logger.LogWarning("Vehicle's Category {CategoryId} is invalid", dto.CategoryId);
                throw new InvalidArgumentException("Category", ErrorMessages.InvalidArgument);
            }

            Vehicle vehicle = _mapper.Map<Vehicle>(dto);
            vehicle.Category = category;
            vehicle.Status = VehicleStatus.Available;

            await _unitOfWork.VehicleRepository.AddAsync(vehicle);
            await _unitOfWork.SaveChanges();

            _logger.LogInformation("Vehicle with license plate {LicensePlate} was added successfully", vehicle.LicensePlate);
            return _mapper.Map<VehicleReadOnlyDTO>(vehicle);
        }

        public async Task SaveVehiclePhoto(Guid uuid, IFormFile photo)
        {
            Vehicle? vehicle = await _unitOfWork.VehicleRepository.GetByUuidAsync(uuid);
            if (vehicle is null)
            {
                _logger.LogWarning("Vehicle with uuid {Uuid} was not found", uuid);
                throw new EntityNotFoundException("Vehicle", ErrorMessages.NotFound);
            }

            try
            {
                string originalName = photo.FileName;
                string extension = Path.GetExtension(originalName);
                string savedName = Guid.NewGuid() + extension;
                string filePath = Path.Combine(_uploadDir, savedName);
                string contentType = photo.ContentType;
                Directory.CreateDirectory(_uploadDir);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await photo.CopyToAsync(stream);
                }


                if (vehicle.Photo != null)
                {
                    if (File.Exists(vehicle.Photo.FilePath)) File.Delete(vehicle.Photo.FilePath);

                    vehicle.Photo = null;
                }

                VehiclePhoto vehiclePhoto = new VehiclePhoto
                {
                    OriginalName = originalName,
                    SavedName = savedName,
                    FilePath = filePath,
                    ContentType = contentType,
                    Extension = extension,
                    VehicleId = vehicle.Id
                };

                vehicle.Photo = vehiclePhoto;
                await _unitOfWork.SaveChanges();

                _logger.LogInformation("Photo for vehicle with uuid {Uuid} saved successfully", uuid);
            } catch (IOException ex)
            {
                _logger.LogError(ex, "Error saving photo for vehicle with uuid {Uuid}", uuid);
                throw new ServerException("VehiclePhoto", ErrorMessages.InternalServerError);
            }
        }

        public async Task<VehicleReadOnlyDTO> UpdateVehicleAsync(Guid uuid, VehicleUpdateDTO dto)
        {
            Vehicle? existingVehicle = await _unitOfWork.VehicleRepository.GetByUuidAsync(uuid);
            if (existingVehicle == null)
            {
                _logger.LogWarning("Vehicle with uuid {Uuid} was not found", uuid);
                throw new EntityNotFoundException("Vehicle", ErrorMessages.NotFound);
            }

            if (!dto.LicensePlate!.Equals(existingVehicle.LicensePlate))
            {
                Vehicle? existingPlate = await _unitOfWork.VehicleRepository.GetVehicleByLicensePlateAsync(dto.LicensePlate!);
                if (existingPlate != null)
                {
                    _logger.LogWarning("Vehicle with license plate {LicensePlate} already exists", dto.LicensePlate);
                    throw new EntityAlreadyExistsException("Vehicle", ErrorMessages.AlreadyExists);
                }
            }

            Category? category = await _unitOfWork.CategoryRepository.GetByIdAsync(dto.CategoryId!.Value);
            if (category == null)
            {
                _logger.LogWarning("Vehicle's Category {CategoryId} is invalid", dto.CategoryId);
                throw new InvalidArgumentException("Category", ErrorMessages.InvalidArgument);
            }

            _mapper.Map(dto, existingVehicle);
            existingVehicle!.Category = category;
            existingVehicle.ModifiedAt = DateTime.UtcNow;

            await _unitOfWork.SaveChanges();

            _logger.LogInformation("Vehicle with license plate {LicensePlate} was updated successfully", existingVehicle.LicensePlate);
            return _mapper.Map<VehicleReadOnlyDTO>(existingVehicle);
        }

        public async Task<bool> DeleteVehicleByUuidAsync(Guid uuid)
        {
            Vehicle? existingVehicle = await _unitOfWork.VehicleRepository.GetByUuidAsync(uuid);
            if (existingVehicle == null)
            {
                _logger.LogWarning("Vehicle with uuid {Uuid} was not found", uuid);
                throw new EntityNotFoundException("Vehicle", ErrorMessages.NotFound);
            }

            if (await _unitOfWork.RentalRepository.HasActiveRentalsForVehicleAsync(existingVehicle.Id))
            {
                _logger.LogWarning("Vehicle with uuid {Uuid} has active rentals and cannot be deleted", uuid);
                throw new InvalidArgumentException("Vehicle", ErrorMessages.InvalidArgument);
            }

            existingVehicle.IsDeleted = true;
            existingVehicle.DeletedAt = DateTime.UtcNow;
            existingVehicle.ModifiedAt = DateTime.UtcNow;

            await _unitOfWork.SaveChanges();
            _logger.LogInformation("Vehicle with uuid {Uuid} was soft deleted successfully", uuid);
            return true;
        }

        public async Task<VehicleReadOnlyDTO> GetVehicleByUuidAsync(Guid uuid)
        {
            Vehicle? existingVehicle = await _unitOfWork.VehicleRepository.GetByUuidAsync(uuid);
            if (existingVehicle == null)
            {
                _logger.LogWarning("Vehicle with uuid {Uuid} was not found", uuid);
                throw new EntityNotFoundException("Vehicle", ErrorMessages.NotFound);
            }

            _logger.LogInformation("Vehicle with uuid {Uuid} was fetched successfully", uuid);
            return _mapper.Map<VehicleReadOnlyDTO>(existingVehicle);
        }

        public async Task<PaginatedResult<VehicleReadOnlyDTO>> GetPaginatedFilteredAvailableVehiclesAsync(int pageNumber, int pageSize, 
            VehicleFiltersDTO filters)
        {
            var predicates = BuildVehiclePredicates(filters);
            predicates.Add(v => v.Status == VehicleStatus.Available);

            PaginatedResult<Vehicle> result = await _unitOfWork.VehicleRepository.GetPaginatedFilteredVehiclesAsync(pageNumber, pageSize, predicates);

            var dtoResult = new PaginatedResult<VehicleReadOnlyDTO>()
            {
                Data = _mapper.Map<List<VehicleReadOnlyDTO>>(result.Data),
                TotalRecords = result.TotalRecords,
                PageNumber = result.PageNumber,
                PageSize = result.PageSize
            };

            _logger.LogInformation("Retrieved {Count} Available Vehicles", dtoResult.Data.Count);
            return dtoResult;
        }

        public async Task<PaginatedResult<VehicleReadOnlyDTO>> GetPaginatedFilteredVehiclesAsync(int pageNumber, int pageSize, 
            VehicleFiltersDTO filters)
        {
            var predicates = BuildVehiclePredicates(filters);
            if (filters.Status.HasValue) predicates.Add(v => v.Status == filters.Status.Value);

            PaginatedResult<Vehicle> result = await _unitOfWork.VehicleRepository.GetPaginatedFilteredVehiclesAsync(pageNumber, pageSize, predicates);

            var dtoResult = new PaginatedResult<VehicleReadOnlyDTO>()
            {
                Data = _mapper.Map<List<VehicleReadOnlyDTO>>(result.Data),
                TotalRecords = result.TotalRecords,
                PageNumber = result.PageNumber,
                PageSize = result.PageSize
            };

            _logger.LogInformation("Retrieved {Count} Vehicles", dtoResult.Data.Count);
            return dtoResult;
        }

        private List<Expression<Func<Vehicle, bool>>> BuildVehiclePredicates(VehicleFiltersDTO filters)
        {
            List<Expression<Func<Vehicle, bool>>> predicates = [];

            if (!string.IsNullOrEmpty(filters.Make)) predicates.Add(v => v.Make == filters.Make);
            if (!string.IsNullOrEmpty(filters.Model)) predicates.Add(v => v.Model == filters.Model);
            if (filters.MinYear.HasValue) predicates.Add(v => v.Year >= filters.MinYear.Value);
            if (filters.MaxYear.HasValue) predicates.Add(v => v.Year <= filters.MaxYear.Value);
            if (filters.MinDailyRate.HasValue) predicates.Add(v => v.DailyRate >= filters.MinDailyRate.Value);
            if (filters.MaxDailyRate.HasValue) predicates.Add(v => v.DailyRate <= filters.MaxDailyRate.Value);
            if (filters.TierType.HasValue) predicates.Add(v => v.TierType == filters.TierType.Value);
            if (filters.CategoryId.HasValue) predicates.Add(v => v.CategoryId == filters.CategoryId.Value);

            return predicates;
        }
    }
}
