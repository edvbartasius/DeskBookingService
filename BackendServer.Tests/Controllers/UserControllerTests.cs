using BackendServer.Tests.Helpers;
using DeskBookingService;
using DeskBookingService.Controllers;
using DeskBookingService.Models;
using DeskBookingService.Models.DTOs;
using DeskBookingService.Services.Validators;
using FluentValidation;
using Mapster;
using MapsterMapper;
using Microsoft.AspNetCore.Mvc;
using Xunit;

namespace BackendServer.Tests.Controllers;

public class UserControllerTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly UserController _controller;
    private readonly IMapper _mapper;

    public UserControllerTests()
    {
        // Use a unique database name for each test instance
        var databaseName = $"UserControllerTests_{Guid.NewGuid()}_{DateTime.UtcNow.Ticks}";
        _context = TestDbContextFactory.CreateInMemoryContext(databaseName);
        TestDbContextFactory.SeedTestData(_context);

        var config = TypeAdapterConfig.GlobalSettings;
        _mapper = new Mapper(config);

        var validator = new UserValidator(_context);
        _controller = new UserController(_context, _mapper, validator);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }

    #region GetUsers Tests

    [Fact]
    public async Task GetUsers_ReturnsAllUsers()
    {
        // Act
        var result = await _controller.GetUsers();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var users = Assert.IsAssignableFrom<List<UserDto>>(okResult.Value);
        Assert.Equal(2, users.Count);
    }

    #endregion

    #region AddUser Tests

    [Fact]
    public async Task AddUser_ValidUser_ReturnsOk()
    {
        // Arrange
        var userDto = new UserDto
        {
            Name = "Test",
            Surname = "User",
            Email = "test.user@test.com",
            Password = "password123",
            Role = UserRole.User
        };

        // Act
        var result = await _controller.AddUser(userDto);

        // Assert
        Assert.IsType<OkObjectResult>(result);
        // User should be created (ID is auto-generated)
        Assert.Contains(_context.Users, u => u.Email == "test.user@test.com");
    }

    [Fact]
    public async Task AddUser_DuplicateEmail_ReturnsBadRequest()
    {
        // Arrange
        var userDto = new UserDto
        {
            Name = "John",
            Surname = "Doe",
            Email = "john.doe@test.com", // Already exists
            Password = "password123",
            Role = UserRole.User
        };

        // Act
        var result = await _controller.AddUser(userDto);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    #endregion

    #region DeleteUser Tests

    [Fact]
    public async Task Delete_ExistingUser_ReturnsOk()
    {
        // Arrange
        var userId = "user1";

        // Act
        var result = await _controller.Delete(userId);

        // Assert
        Assert.IsType<OkResult>(result);
        var userInDb = await _context.Users.FindAsync(userId);
        Assert.Null(userInDb);
    }

    [Fact]
    public async Task Delete_NonExistentUser_ReturnsNotFound()
    {
        // Arrange
        var userId = "nonexistent";

        // Act
        var result = await _controller.Delete(userId);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }

    #endregion

    #region UpdateUser Tests

    [Fact]
    public async Task UpdateUser_ExistingUser_ReturnsOk()
    {
        // Arrange
        var userId = "user1";
        var userDto = new UserDto
        {
            Name = "Updated",
            Surname = "Name",
            Email = "john.doe@test.com",
            Password = "newpassword123",
            Role = UserRole.User
        };

        // Act
        var result = await _controller.UpdateUser(userId, userDto);

        // Assert
        Assert.IsType<OkResult>(result);
        var userInDb = await _context.Users.FindAsync(userId);
        Assert.NotNull(userInDb);
        Assert.Equal("Updated", userInDb!.Name);
    }

    [Fact]
    public async Task UpdateUser_NonExistentUser_ReturnsNotFound()
    {
        // Arrange
        var userId = "nonexistent";
        var userDto = new UserDto
        {
            Name = "Test",
            Surname = "User",
            Email = "test@test.com",
            Password = "password123",
            Role = UserRole.User
        };

        // Act
        var result = await _controller.UpdateUser(userId, userDto);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result);
    }

    #endregion

    #region RegisterUser Tests

    [Fact]
    public async Task Register_ValidUser_ReturnsOk()
    {
        // Arrange
        var userDto = new UserDto
        {
            Name = "New",
            Surname = "User",
            Email = "new.user@test.com",
            Password = "password123",
            Role = UserRole.User
        };

        // Act
        var result = await _controller.Register(userDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedUser = Assert.IsType<UserDto>(okResult.Value);
        Assert.Equal("new.user@test.com", returnedUser.Email);
        Assert.Equal(UserRole.User, returnedUser.Role);
    }

    [Fact]
    public async Task Register_DuplicateEmail_ReturnsConflict()
    {
        // Arrange
        var userDto = new UserDto
        {
            Name = "John",
            Surname = "Doe",
            Email = "john.doe@test.com", // Already exists
            Password = "password123",
            Role = UserRole.User
        };

        // Act
        var result = await _controller.Register(userDto);

        // Assert
        Assert.IsType<ConflictObjectResult>(result);
    }

    [Fact]
    public async Task Register_InvalidPassword_ReturnsBadRequest()
    {
        // Arrange
        var userDto = new UserDto
        {
            Name = "Test",
            Surname = "User",
            Email = "test@test.com",
            Password = "short", // Too short, no digit
            Role = UserRole.User
        };

        // Act
        var result = await _controller.Register(userDto);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task Register_InvalidEmail_ReturnsBadRequest()
    {
        // Arrange
        var userDto = new UserDto
        {
            Name = "Test",
            Surname = "User",
            Email = "invalid-email", // Invalid format
            Password = "password123",
            Role = UserRole.User
        };

        // Act
        var result = await _controller.Register(userDto);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    #endregion

    #region LoginUser Tests

    [Fact]
    public async Task LoginUser_ValidCredentials_ReturnsOk()
    {
        // Arrange
        var loginDto = new LoginDto
        {
            Email = "john.doe@test.com",
            Password = "password123"
        };

        // Act
        var result = await _controller.LoginUser(loginDto);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var user = Assert.IsType<UserDto>(okResult.Value);
        Assert.Equal("john.doe@test.com", user.Email);
    }

    [Fact]
    public async Task LoginUser_InvalidPassword_ReturnsBadRequest()
    {
        // Arrange
        var loginDto = new LoginDto
        {
            Email = "john.doe@test.com",
            Password = "wrongpassword"
        };

        // Act
        var result = await _controller.LoginUser(loginDto);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task LoginUser_NonExistentEmail_ReturnsBadRequest()
    {
        // Arrange
        var loginDto = new LoginDto
        {
            Email = "nonexistent@test.com",
            Password = "password123"
        };

        // Act
        var result = await _controller.LoginUser(loginDto);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task LoginUser_InvalidEmailFormat_ReturnsBadRequest()
    {
        // Arrange
        var loginDto = new LoginDto
        {
            Email = "invalid-email",
            Password = "password123"
        };

        // Act
        var result = await _controller.LoginUser(loginDto);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    #endregion
}

