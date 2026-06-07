using AutoMapper;
using CarRentalApp.Core;
using CarRentalApp.Core.Filters;
using CarRentalApp.DTO;
using CarRentalApp.DTO.User;
using CarRentalApp.Exceptions;
using CarRentalApp.Models;
using CarRentalApp.Repositories;
using CarRentalApp.Resources;
using CarRentalApp.Security;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Linq.Expressions;
using System.Security.Claims;
using System.Text;

namespace CarRentalApp.Services.Users
{
    public class UserService : IUserService
    {
        private readonly IEncryptionUtil _encryptionUtil;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<UserService> _logger;
        private readonly IConfiguration _configuration;

        public UserService(IEncryptionUtil encryptionUtil, IUnitOfWork unitOfWork, 
            IMapper mapper, ILogger<UserService> logger, IConfiguration configuration)
        {
            _encryptionUtil = encryptionUtil;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _configuration = configuration;
        }

        public async Task<UserReadOnlyDTO> GetUserByUuidAsync(Guid uuid)
        {
            User? user = await _unitOfWork.UserRepository.GetByUuidAsync(uuid);

            if (user is null)
            {
                _logger.LogWarning("User with uuid {Uuid} was not found", uuid);
                throw new EntityNotFoundException("User", ErrorMessages.NotFound);
            }

            _logger.LogInformation("User with uuid {Uuid} was retrieved successfully", uuid);
            return _mapper.Map<UserReadOnlyDTO>(user);
        }

        public async Task<PaginatedResult<UserReadOnlyDTO>> GetUsersPaginatedFilteredAsync(int pageNumber, int pageSize, 
            UserFiltersDTO filters)
        {
            List<Expression<Func<User, bool>>> predicates = [];

            if (!string.IsNullOrEmpty(filters.Username))
            {
                predicates.Add(u => u.Username == filters.Username);
            }
            if (!string.IsNullOrEmpty(filters.Email))
            {
                predicates.Add(u => u.Email == filters.Email);
            }
            if (!string.IsNullOrEmpty(filters.UserRole))
            {
                predicates.Add(u => u.Role.Name ==  filters.UserRole);
            }

            PaginatedResult<User> result = await _unitOfWork.UserRepository.GetPaginatedFilteredUsersAsync(pageNumber, pageSize, predicates);

            var dtoResult = new PaginatedResult<UserReadOnlyDTO>()
            {
                Data = _mapper.Map<List<UserReadOnlyDTO>>(result.Data),
                TotalRecords = result.TotalRecords,
                PageNumber = result.PageNumber,
                PageSize = result.PageSize
            };

            _logger.LogInformation("Retrieved {Count} Users", dtoResult.Data.Count);
            return dtoResult;
        }

        public async Task<User> VerifyAndGetUserAsync(UserLoginDTO credentials)
        {
            User? user = await _unitOfWork.UserRepository.GetUserByUsernameAsync(credentials.Username!);

            if (user is null || user.IsDeleted || !_encryptionUtil.IsValidPassword(credentials.Password!, user.Password))
            {
                _logger.LogWarning("Failed login attempt for username {Username}", credentials.Username);
                throw new EntityNotAuthorizedException("User", ErrorMessages.BadCredentials);
            }

            _logger.LogInformation("User with username {Username} verified for login", credentials.Username);
            return user;
        }

        public string CreateUserToken(User user)
        {
            var secretKey = _configuration["Jwt:Secret"]!;
            var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

            var claimsInfo = new List<Claim>
            {
                new Claim("nameid", user.Id.ToString()),
                new Claim("unique_name", user.Username),
                new Claim("email", user.Email),
                new Claim("role", user.Role.Name),
                new Claim("uuid", user.Uuid.ToString()),
            };

            var jwtSecurityToken = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claimsInfo,
                expires: DateTime.UtcNow.AddHours(3),
                signingCredentials: signingCredentials
            );

            return new JwtSecurityTokenHandler().WriteToken(jwtSecurityToken);
        }
    }
}
