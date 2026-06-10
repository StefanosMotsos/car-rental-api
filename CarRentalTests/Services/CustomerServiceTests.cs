using AutoMapper;
using CarRentalApp.DTO.User;
using CarRentalApp.Exceptions;
using CarRentalApp.Models;
using CarRentalApp.Repositories;
using CarRentalApp.Security;
using CarRentalApp.Services.Customers;
using Microsoft.Extensions.Logging;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Text;

namespace CarRentalTests.Services
{
    public class CustomerServiceTests
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CustomerService> _logger;
        private readonly IMapper _mapper;
        private readonly IEncryptionUtil _encryptionUtil;
        private readonly CustomerService _service;

        public CustomerServiceTests()
        {
            _unitOfWork = Substitute.For<IUnitOfWork>();
            _mapper = Substitute.For<IMapper>();
            _encryptionUtil = Substitute.For<IEncryptionUtil>();
            _logger = Substitute.For<ILogger<CustomerService>>();

            _service = new CustomerService(_encryptionUtil, _unitOfWork, _mapper, _logger);
        }

        [Fact]
        public async Task SignupCustomerAsync_WhenCustomerIsNew_AddsUserAndSaves()
        {
            CustomerSignupDTO dto;
            Customer mappedCustomer;
            User mappedUser;
            dto = CreateValidSignupDTO("steve", "steve@gmail.com", "DL-12345");

            mappedCustomer = new Customer { DriverLicense = "DL-12345" };

            _unitOfWork.RoleRepository.GetByIdAsync(3).Returns(new Role { Id = 3, Name = "CUSTOMER" });
            mappedUser = new User { 
                Username = "steve", 
                Email = "steve@gmail.com", 
                Password = "PlainPass123!",
                RoleId = 3
            };

            _mapper.Map<Customer>(dto).Returns(mappedCustomer);
            _mapper.Map<User>(dto).Returns(mappedUser);
            _mapper.Map<CustomerReadOnlyDTO>(mappedUser).Returns(new CustomerReadOnlyDTO { Username = "steve" });

            _unitOfWork.UserRepository.GetUserByUsernameAsync(mappedUser.Username).Returns((User?)null);
            _unitOfWork.UserRepository.GetUserByEmailAsync("steve@gmail.com").Returns((User?)null);
            _unitOfWork.CustomerRepository.GetCustomerByDriverLicenseAsync("DL-12345").Returns((Customer?)null);

            _encryptionUtil.Encrypt("PlainPass123!").Returns("encrypted_password");

            CustomerReadOnlyDTO result = await _service.SignupCustomerAsync(dto);


            _encryptionUtil.Received(1).Encrypt("PlainPass123!");
            Assert.Equal("encrypted_password", mappedUser.Password);

            await _unitOfWork.UserRepository.Received(1).AddAsync(mappedUser);
            await _unitOfWork.Received(1).SaveChanges();

            Assert.NotNull(result);
        }

        [Fact]
        public async Task SignupCustomerAsync_WhenUsernameAlreadyExists_ThrowsEntityAlreadyExistsException()
        {
            CustomerSignupDTO dto;
            User existingUser;
            dto = CreateValidSignupDTO("steve", "steve@gmail.com", "DL-12345");

            existingUser = new User { Id = 1, Username = "steve" };

            _unitOfWork.UserRepository.GetUserByUsernameAsync("steve").Returns(existingUser);

            await Assert.ThrowsAsync<EntityAlreadyExistsException>(
                () => _service.SignupCustomerAsync(dto));

            await _unitOfWork.UserRepository.DidNotReceive().AddAsync(Arg.Any<User>());
            await _unitOfWork.DidNotReceive().SaveChanges();
        }

        [Fact]
        public async Task SignupCustomerAsync_WhenEmailAlreadyExists_ThrowsEntityAlreadyExistsException()
        {
            CustomerSignupDTO dto;
            User existingUser;
            dto = CreateValidSignupDTO("steve", "steve@gmail.com", "DL-12345");

            existingUser = new User { Id = 1, Username = "steve@gmail.com" };

            _unitOfWork.UserRepository.GetUserByUsernameAsync("steve").Returns((User?)null);
            _unitOfWork.UserRepository.GetUserByEmailAsync("steve@gmail.com").Returns(existingUser);

            await Assert.ThrowsAsync<EntityAlreadyExistsException>(
                () => _service.SignupCustomerAsync(dto));

            await _unitOfWork.UserRepository.DidNotReceive().AddAsync(Arg.Any<User>());
            await _unitOfWork.DidNotReceive().SaveChanges();
        }

        [Fact]
        public async Task SignupCustomerAsync_WhenDriverLicenseAlreadyExists_ThrowsEntityAlreadyExistsException()
        {
            CustomerSignupDTO dto;
            Customer existingCustomer;
            dto = CreateValidSignupDTO("steve", "steve@gmail.com", "DL-12345");

            existingCustomer = new Customer { DriverLicense = "DL-12345" };

            _unitOfWork.UserRepository.GetUserByUsernameAsync("steve").Returns((User?)null);
            _unitOfWork.UserRepository.GetUserByEmailAsync("steve@gmail.com").Returns((User?)null);
            _unitOfWork.CustomerRepository.GetCustomerByDriverLicenseAsync("DL-12345").Returns(existingCustomer);

            await Assert.ThrowsAsync<EntityAlreadyExistsException>(
                () => _service.SignupCustomerAsync(dto));

            await _unitOfWork.UserRepository.DidNotReceive().AddAsync(Arg.Any<User>());
            await _unitOfWork.DidNotReceive().SaveChanges();
        }

        private static CustomerSignupDTO CreateValidSignupDTO(string username, string email, string driverLicense)
        {
            CustomerSignupDTO dto;

            dto = new CustomerSignupDTO
            {
                Username = username,
                Email = email,
                Password = "PlainPass123!",
                Firstname = "Steve",
                Lastname = "Motsos",
                DateOfBirth = new DateOnly(2001, 9, 30),
                DriverLicense = driverLicense
            };

            return dto;
        }
    }
}
