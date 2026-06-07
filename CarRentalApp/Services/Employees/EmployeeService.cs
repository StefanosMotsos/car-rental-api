using AutoMapper;
using CarRentalApp.Core;
using CarRentalApp.Core.Filters;
using CarRentalApp.DTO.User;
using CarRentalApp.Exceptions;
using CarRentalApp.Models;
using CarRentalApp.Repositories;
using CarRentalApp.Resources;
using CarRentalApp.Security;
using System.Linq.Expressions;

namespace CarRentalApp.Services.Employees
{
    public class EmployeeService : IEmployeeService
    {

        private readonly IMapper _mapper;
        private readonly ILogger<EmployeeService> _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEncryptionUtil _encryptionUtil;

        public EmployeeService(IMapper mapper, ILogger<EmployeeService> logger,
            IUnitOfWork unitOfWork, IEncryptionUtil encryptionUtil)
        {
            _mapper = mapper;
            _logger = logger;
            _unitOfWork = unitOfWork;
            _encryptionUtil = encryptionUtil;
        }

        public async Task<EmployeeReadOnlyDTO> SignUpEmployeeAsync(EmployeeSignupDTO dto)
        {
            await ValidateEmployeeSignUpAsync(dto);

            Employee employee = _mapper.Map<Employee>(dto);
            User user = _mapper.Map<User>(dto);
            Role? role = await _unitOfWork.RoleRepository.GetByIdAsync(2); //EMPLOYEE role, seeded constant
            user.RoleId = role!.Id;
            user.Role = role;

            user.Employee = employee;
            user.Password = _encryptionUtil.Encrypt(user.Password);

            await _unitOfWork.UserRepository.AddAsync(user);
            await _unitOfWork.SaveChanges();

            _logger.LogInformation("Employee {Username} was registered successfully!", user.Username);
            return _mapper.Map<EmployeeReadOnlyDTO>(user);
        }

        public async Task<EmployeeReadOnlyDTO> UpdateEmployeeAsync(Guid uuid, EmployeeUpdateDTO dto)
        {
            Employee? employee = await _unitOfWork.EmployeeRepository.GetByUuidAsync(uuid);
            if (employee == null || employee.IsDeleted)
            {
                _logger.LogWarning("Employee with uuid {Uuid} was not found", uuid);
                throw new EntityNotFoundException("Employee", ErrorMessages.NotFound);
            }

            await ValidateEmployeeUpdateAsync(dto, employee);

            _mapper.Map(dto, employee);
            _mapper.Map(dto, employee.User);
            employee.User.Password = _encryptionUtil.Encrypt(dto.Password!);

            employee.User.ModifiedAt = DateTime.UtcNow;
            employee.ModifiedAt = DateTime.UtcNow;
            await _unitOfWork.SaveChanges();

            _logger.LogInformation("Employee with uuid {Uuid} was updated successfully!", uuid);
            return _mapper.Map<EmployeeReadOnlyDTO>(employee.User);
        }

        public async Task<bool> DeleteEmployeeByUuidAsync(Guid uuid)
        {
            Employee? employee = await _unitOfWork.EmployeeRepository.GetByUuidAsync(uuid);
            if (employee == null || employee.IsDeleted)
            {
                _logger.LogWarning("Employee with uuid {Uuid} was not found", uuid);
                throw new EntityNotFoundException("Employee", ErrorMessages.NotFound);
            }

            employee.IsDeleted = true;
            employee.DeletedAt = DateTime.UtcNow;
            employee.User.IsDeleted = true;
            employee.User.DeletedAt = DateTime.UtcNow;
            employee.ModifiedAt = DateTime.UtcNow;
            employee.User.ModifiedAt = DateTime.UtcNow;

            await _unitOfWork.SaveChanges();
            _logger.LogInformation("Employee with uuid {Uuid} was soft deleted successfully", uuid);
            return true;
        }

        public async Task<EmployeeReadOnlyDTO> GetActiveEmployeeByUuidAsync(Guid uuid)
        {
            Employee? employee = await _unitOfWork.EmployeeRepository.GetByUuidAsync(uuid);
            if (employee == null || employee.IsDeleted)
            {
                _logger.LogWarning("Employee with uuid {Uuid} was not found", uuid);
                throw new EntityNotFoundException("Employee", ErrorMessages.NotFound);
            }

            _logger.LogInformation("Active Employee with uuid {Uuid} was fetched successfully!", uuid);
            return _mapper.Map<EmployeeReadOnlyDTO>(employee.User);
        }

        public async Task<EmployeeReadOnlyDTO> GetEmployeeByUuidAsync(Guid uuid)
        {
            Employee? employee = await _unitOfWork.EmployeeRepository.GetByUuidAsync(uuid);
            if (employee == null)
            {
                _logger.LogWarning("Employee with uuid {Uuid} was not found", uuid);
                throw new EntityNotFoundException("Employee", ErrorMessages.NotFound);
            }

            _logger.LogInformation("Employee with uuid {Uuid} was fetched successfully!", uuid);
            return _mapper.Map<EmployeeReadOnlyDTO>(employee.User);
        }

        public async Task<PaginatedResult<EmployeeReadOnlyDTO>> GetPaginatedFilteredActiveEmployeesAsync(int pageNumber, int pageSize, EmployeeFiltersDTO filters)
        {
            var predicates = BuildEmployeePredicates(filters);
            predicates.Add(c => !c.IsDeleted);

            PaginatedResult<Employee> result = await _unitOfWork.EmployeeRepository.GetPaginatedFilteredEmployeesAsync(pageNumber, pageSize, predicates);

            var dtoResult = new PaginatedResult<EmployeeReadOnlyDTO>()
            {
                Data = _mapper.Map<List<EmployeeReadOnlyDTO>>(result.Data.Select(c => c.User).ToList()),
                TotalRecords = result.TotalRecords,
                PageNumber = result.PageNumber,
                PageSize = result.PageSize,
            };

            _logger.LogInformation("Retrieved {Count} Active Employees", dtoResult.Data.Count);
            return dtoResult;
        }

        public async Task<PaginatedResult<EmployeeReadOnlyDTO>> GetPaginatedFilteredEmployeesAsync(int pageNumber, int pageSize, EmployeeFiltersDTO filters)
        {
            var predicates = BuildEmployeePredicates(filters);

            PaginatedResult<Employee> result = await _unitOfWork.EmployeeRepository.GetPaginatedFilteredEmployeesAsync(pageNumber, pageSize, predicates);

            var dtoResult = new PaginatedResult<EmployeeReadOnlyDTO>()
            {
                Data = _mapper.Map<List<EmployeeReadOnlyDTO>>(result.Data.Select(c => c.User).ToList()),
                TotalRecords = result.TotalRecords,
                PageNumber = result.PageNumber,
                PageSize = result.PageSize,
            };

            _logger.LogInformation("Retrieved {Count} Employees", dtoResult.Data.Count);
            return dtoResult;
        }

        private async Task ValidateEmployeeSignUpAsync(EmployeeSignupDTO dto)
        {
            User? existingUser = await _unitOfWork.UserRepository.GetUserByUsernameAsync(dto.Username!);
            if (existingUser != null)
            {
                _logger.LogWarning("Employee with username {Username} already exists", dto.Username);
                throw new EntityAlreadyExistsException("Employee", ErrorMessages.AlreadyExists);
            }

            User? existingEmail = await _unitOfWork.UserRepository.GetUserByEmailAsync(dto.Email!);
            if (existingEmail != null)
            {
                _logger.LogWarning("Employee with email {Email} already exists", dto.Email);
                throw new EntityAlreadyExistsException("Email", ErrorMessages.AlreadyExists);
            }
        }

        private async Task ValidateEmployeeUpdateAsync(EmployeeUpdateDTO dto, Employee existing)
        {

            if (dto.Username != existing.User.Username)
            {
                User? existingUser = await _unitOfWork.UserRepository.GetUserByUsernameAsync(dto.Username!);
                if (existingUser != null)
                {
                    _logger.LogWarning("Employee with username {Username} already exists", dto.Username);
                    throw new EntityAlreadyExistsException("User", ErrorMessages.AlreadyExists);
                }
            }

            if (dto.Email != existing.User.Email)
            {
                User? existingEmail = await _unitOfWork.UserRepository.GetUserByEmailAsync(dto.Email!);
                if (existingEmail != null)
                {
                    _logger.LogWarning("Employee with email {Email} already exists", dto.Email);
                    throw new EntityAlreadyExistsException("Email", ErrorMessages.AlreadyExists);
                }
            }
        }

        private List<Expression<Func<Employee, bool>>> BuildEmployeePredicates(EmployeeFiltersDTO filters)
        {
            List<Expression<Func<Employee, bool>>> predicates = [];

            if (!string.IsNullOrEmpty(filters.Username)) predicates.Add(c => c.User.Username == filters.Username);
            if (!string.IsNullOrEmpty(filters.Firstname)) predicates.Add(c => c.User.Firstname == filters.Firstname);
            if (!string.IsNullOrEmpty(filters.Lastname)) predicates.Add(c => c.User.Lastname.Contains(filters.Lastname));
            if (!string.IsNullOrEmpty(filters.Email)) predicates.Add(c => c.User.Email == filters.Email);

            return predicates;
        }
    }
}
