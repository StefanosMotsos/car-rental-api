using AutoMapper;
using CarRentalApp.Core;
using CarRentalApp.Core.Filters;
using CarRentalApp.DTO.User;
using CarRentalApp.Exceptions;
using CarRentalApp.Models;
using CarRentalApp.Repositories;
using CarRentalApp.Resources;
using CarRentalApp.Security;
using CarRentalApp.Services.Users;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Security.Claims;

namespace CarRentalApp.Services.Customers
{
    public class CustomerService : ICustomerService
    {

        private readonly IEncryptionUtil _encryptionUtil;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<CustomerService> _logger;

        public CustomerService(IEncryptionUtil encryptionUtil, IUnitOfWork unitOfWork,
            IMapper mapper, ILogger<CustomerService> logger)
        {
            _encryptionUtil = encryptionUtil;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<CustomerReadOnlyDTO> SignupCustomerAsync(CustomerSignupDTO dto)
        {
            await ValidateCustomerSignupAsync(dto);

            Customer customer = _mapper.Map<Customer>(dto);
            User user = _mapper.Map<User>(dto);

            Role? role = await _unitOfWork.RoleRepository.GetByIdAsync(3);
            user.RoleId = role!.Id;
            user.Role = role;

            user.Customer = customer;
            user.Password = _encryptionUtil.Encrypt(user.Password);
            user.ModifiedAt = DateTime.UtcNow;
            customer.ModifiedAt = DateTime.UtcNow;

            await _unitOfWork.UserRepository.AddAsync(user);
            await _unitOfWork.SaveChanges();

            _logger.LogInformation("Customer {Username} signed up successfully!", user.Username);
            return _mapper.Map<CustomerReadOnlyDTO>(user);
        }

        public async Task<CustomerReadOnlyDTO> UpdateCustomerAsync(Guid uuid, CustomerUpdateDTO dto, int callerUserId)
        {
            Customer? customer = await _unitOfWork.CustomerRepository.GetByUuidAsync(uuid);
            if (customer is null || customer.IsDeleted)
            {
                _logger.LogWarning("Customer with uuid {Uuid} was not found", uuid);
                throw new EntityNotFoundException("Customer", ErrorMessages.NotFound);
            }

            if (callerUserId != customer.UserId)
            {
                _logger.LogWarning("Customer with id {CallerUserId} is not allowed to access this service", callerUserId);
                throw new EntityForbiddenException("Customer", ErrorMessages.Forbidden);
            }

            await ValidateCustomerUpdateAsync(dto, customer);

            _mapper.Map(dto, customer);
            _mapper.Map(dto, customer.User);
            customer.User.Password = _encryptionUtil.Encrypt(dto.Password!);

            customer.User.ModifiedAt = DateTime.UtcNow;
            customer.ModifiedAt = DateTime.UtcNow;
            await _unitOfWork.SaveChanges();

            _logger.LogInformation("Customer with uuid {Uuid} was updated successfully!", uuid);
            return _mapper.Map<CustomerReadOnlyDTO>(customer.User);
        }

        public async Task<bool> DeleteCustomerByUuidAsync(Guid uuid)
        {
            Customer? customer = await _unitOfWork.CustomerRepository.GetByUuidAsync(uuid);
            if (customer == null || customer.IsDeleted)
            {
                _logger.LogWarning("Customer with uuid {Uuid} was not found", uuid);
                throw new EntityNotFoundException("Customer", ErrorMessages.NotFound);
            }

            customer.IsDeleted = true;
            customer.DeletedAt = DateTime.UtcNow;
            customer.User.IsDeleted = true;
            customer.User.DeletedAt = DateTime.UtcNow;

            await _unitOfWork.SaveChanges();
            return true;
        }

        public async Task<CustomerReadOnlyDTO> GetActiveCustomerByUuidAsync(Guid uuid)
        {
            Customer? customer = await _unitOfWork.CustomerRepository.GetByUuidAsync(uuid);
            if (customer is null || customer.IsDeleted)
            {
                _logger.LogWarning("Customer with uuid {Uuid} was not found", uuid);
                throw new EntityNotFoundException("Customer", ErrorMessages.NotFound);
            }

            _logger.LogInformation("Active Customer with uuid {Uuid} was fetched successfully!", uuid);
            return _mapper.Map<CustomerReadOnlyDTO>(customer.User);
        }

        public async Task<CustomerReadOnlyDTO> GetCustomerByUuidAsync(Guid uuid)
        {
            Customer? customer = await _unitOfWork.CustomerRepository.GetByUuidAsync(uuid);
            if (customer is null)
            {
                _logger.LogWarning("Customer with uuid {Uuid} was not found", uuid);
                throw new EntityNotFoundException("Customer", ErrorMessages.NotFound);
            }

            _logger.LogInformation("Customer with uuid {Uuid} was fetched successfully!", uuid);
            return _mapper.Map<CustomerReadOnlyDTO>(customer.User);
        }

        public async Task<PaginatedResult<CustomerReadOnlyDTO>> GetPaginatedFilteredActiveCustomersAsync(int pageNumber, int pageSize,
            CustomerFiltersDTO dto)
        {
            var predicates = BuildCustomerPredicates(dto);
            predicates.Add(c => !c.IsDeleted);

            PaginatedResult<Customer> result = await _unitOfWork.CustomerRepository.GetPaginatedFilteredCustomersAsync(pageNumber, pageSize, predicates);

            var dtoResult = new PaginatedResult<CustomerReadOnlyDTO>()
            {
                Data = _mapper.Map<List<CustomerReadOnlyDTO>>(result.Data),
                TotalRecords = result.TotalRecords,
                PageNumber = result.PageNumber,
                PageSize = result.PageSize,
            };

            _logger.LogInformation("Retrieved {Count} Active Customers", dtoResult.Data.Count);
            return dtoResult;
        }

        public async Task<PaginatedResult<CustomerReadOnlyDTO>> GetPaginatedFilteredCustomersAsync(int pageNumber, int pageSize, 
            CustomerFiltersDTO dto)
        {
            var predicates = BuildCustomerPredicates(dto);

            PaginatedResult<Customer> result = await _unitOfWork.CustomerRepository.GetPaginatedFilteredCustomersAsync(pageNumber, pageSize, predicates);

            var dtoResult = new PaginatedResult<CustomerReadOnlyDTO>()
            {
                Data = _mapper.Map<List<CustomerReadOnlyDTO>>(result.Data),
                TotalRecords = result.TotalRecords,
                PageNumber = result.PageNumber,
                PageSize = result.PageSize,
            };

            _logger.LogInformation("Retrieved {Count} Customers", dtoResult.Data.Count);
            return dtoResult;
        }

        private static void ValidateAge(DateOnly dateOfBirth)
        {
            var today = DateOnly.FromDateTime(DateTime.UtcNow);
            var age = today.Year - dateOfBirth.Year;
            if (dateOfBirth > today.AddYears(-age)) age--;
            if (age < 18)
                throw new InvalidArgumentException("Customer", ErrorMessages.InvalidArgument);
        }

        private async Task ValidateCustomerSignupAsync(CustomerSignupDTO dto)
        {
            User? existingUser = await _unitOfWork.UserRepository.GetUserByUsernameAsync(dto.Username!);
            if (existingUser is not null)
            {
                _logger.LogWarning("User with username {Username} already exists", dto.Username);
                throw new EntityAlreadyExistsException("User", ErrorMessages.AlreadyExists);
            }

            User? existingEmail = await _unitOfWork.UserRepository.GetUserByEmailAsync(dto.Email!);
            if (existingEmail is not null)
            {
                _logger.LogWarning("User with email {Email} already exists", dto.Email);
                throw new EntityAlreadyExistsException("User", ErrorMessages.AlreadyExists);
            }

            Customer? existingLicense = await _unitOfWork.CustomerRepository.GetCustomerByDriverLicenseAsync(dto.DriverLicense!);
            if (existingLicense is not null)
            {
                _logger.LogWarning("Customer with driver license {DriverLicense} already exists", dto.DriverLicense);
                throw new EntityAlreadyExistsException("Customer", ErrorMessages.AlreadyExists);
            }

            ValidateAge(dto.DateOfBirth!.Value);
        }

        private async Task ValidateCustomerUpdateAsync(CustomerUpdateDTO dto, Customer existing)
        {
            if (dto.Username != existing.User.Username)
            {
                User? existingUser = await _unitOfWork.UserRepository.GetUserByUsernameAsync(dto.Username!);
                if (existingUser is not null)
                {
                    _logger.LogWarning("User with username {Username} already exists", dto.Username);
                    throw new EntityAlreadyExistsException("User", ErrorMessages.AlreadyExists);
                }
            }

            if (dto.Email != existing.User.Email)
            {
                User? existingEmail = await _unitOfWork.UserRepository.GetUserByEmailAsync(dto.Email!);
                if (existingEmail is not null)
                {
                    _logger.LogWarning("User with email {Email} already exists", dto.Email);
                    throw new EntityAlreadyExistsException("User", ErrorMessages.AlreadyExists);
                }
            }

            if (dto.DriverLicense != existing.DriverLicense)
            {
                Customer? existingLicense = await _unitOfWork.CustomerRepository.GetCustomerByDriverLicenseAsync(dto.DriverLicense!);
                if (existingLicense is not null)
                {
                    _logger.LogWarning("Customer with driver license {DriverLicense} already exists", dto.DriverLicense);
                    throw new EntityAlreadyExistsException("Customer", ErrorMessages.AlreadyExists);
                }
            }

            ValidateAge(dto.DateOfBirth!.Value);
        }

        private List<Expression<Func<Customer, bool>>> BuildCustomerPredicates(CustomerFiltersDTO filters)
        {
            List<Expression<Func<Customer, bool>>> predicates = [];

            if(!string.IsNullOrEmpty(filters.Firstname)) predicates.Add(c => c.User.Firstname == filters.Firstname);
            if(!string.IsNullOrEmpty(filters.Lastname)) predicates.Add(c => c.User.Lastname == filters.Lastname);
            if(!string.IsNullOrEmpty(filters.Email)) predicates.Add(c => c.User.Email == filters.Email);
            if(!string.IsNullOrEmpty(filters.DriverLicenceNumber)) predicates.Add(c => c.DriverLicense == filters.DriverLicenceNumber);

            return predicates;

        }
    }
}
