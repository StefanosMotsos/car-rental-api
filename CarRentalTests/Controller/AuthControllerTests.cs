using CarRentalApp.Controllers;
using CarRentalApp.DTO;
using CarRentalApp.DTO.Auth;
using CarRentalApp.DTO.User;
using CarRentalApp.Models;
using CarRentalApp.Services;
using CarRentalApp.Services.Customers;
using CarRentalApp.Services.Employees;
using CarRentalApp.Services.Users;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Text;

namespace CarRentalTests.Controller
{
    public class AuthControllerTests
    {
        private readonly IApplicationService _applicationService;
        private readonly IUserService _userService;
        private readonly IEmployeeService _employeeService;
        private readonly ICustomerService _customerService;
        private readonly AuthController _authController;

        public AuthControllerTests()
        {
            _applicationService = Substitute.For<IApplicationService>();
            _employeeService = Substitute.For<IEmployeeService>();
            _customerService = Substitute.For<ICustomerService>();
            _userService = Substitute.For<IUserService>();


            _applicationService.EmployeeService.Returns(_employeeService);
            _applicationService.CustomerService.Returns(_customerService);
            _applicationService.UserService.Returns(_userService);

            _authController = new AuthController(_applicationService);
        }

        [Fact]
        public async Task Login_ReturnsOk()
        {
            UserLoginDTO credentials;
            User user;

            user = new User { Id = 1, Username = "steve"};
            credentials = new UserLoginDTO { Username = "steve", Password = "PlainPass123!" };
            
            _applicationService.UserService.VerifyAndGetUserAsync(credentials).Returns(user);
            _applicationService.UserService.CreateUserToken(user).Returns("fake_token");

            var result = await _authController.Login(credentials);

            OkObjectResult ok = Assert.IsType<OkObjectResult>(result.Result);
            Assert.IsType<JwtTokenDTO>(ok.Value);
        }

        [Fact]
        public async Task RegisterCustomer_ReturnsCreated()
        {
            CustomerSignupDTO dto;
            CustomerReadOnlyDTO customer;

            customer = new CustomerReadOnlyDTO {Username = "steve" };
            dto = new CustomerSignupDTO { Username = "steve", DriverLicense = "DL-12345" };

            _applicationService.CustomerService.SignupCustomerAsync(dto).Returns(customer);

            var result = await _authController.RegisterCustomer(dto);

            CreatedResult created = Assert.IsType<CreatedResult>(result.Result);
        }

        [Fact]
        public async Task RegisterEmployee_ReturnsCreated()
        {
            EmployeeSignupDTO dto;
            EmployeeReadOnlyDTO employee;

            employee = new EmployeeReadOnlyDTO { Username = "steve" };
            dto = new EmployeeSignupDTO { Username = "steve", PhoneNumber = "690709119"};

            _applicationService.EmployeeService.SignUpEmployeeAsync(dto).Returns(employee);

            var result = await _authController.RegisterEmployee(dto);

            CreatedResult created = Assert.IsType<CreatedResult>(result.Result);
        }
    }
}
