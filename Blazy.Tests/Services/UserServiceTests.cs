using Blazy.Core.DTOs;
using Blazy.Core.Entities;
using Blazy.Repository.Interfaces;
using Blazy.Services.Services;
using Microsoft.AspNetCore.Identity;
using Moq;
using Xunit;

namespace Blazy.Tests.Services;

/// <summary>
/// Unit tests for UserService
/// </summary>
public class UserServiceTests
{
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<UserManager<User>> _mockUserManager;
    private readonly UserService _userService;

    public UserServiceTests()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _mockUserManager = new Mock<UserManager<User>>(
            Mock.Of<IUserStore<User>>(),
            null!, null!, null!, null!, null!, null!, null!, null!);

        _userService = new UserService(_mockUserRepository.Object, _mockUserManager.Object);
    }

    [Fact]
    public async Task RegisterAsync_WithValidData_ReturnsSuccess()
    {
        // Arrange
        var model = new RegisterDto
        {
            Username = "testuser",
            Email = "test@example.com",
            Password = "Test123!",
            ConfirmPassword = "Test123!",
            FirstName = "Test",
            LastName = "User"
        };

        _mockUserRepository.Setup(r => r.GetByUsernameAsync("testuser"))
            .ReturnsAsync((User?)null);
        
        _mockUserRepository.Setup(r => r.GetByEmailAsync("test@example.com"))
            .ReturnsAsync((User?)null);

        _mockUserManager.Setup(m => m.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);

        _mockUserManager.Setup(m => m.AddToRoleAsync(It.IsAny<User>(), "User"))
            .ReturnsAsync(IdentityResult.Success);

        // Act
        var result = await _userService.RegisterAsync(model);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.User);
        Assert.Equal("testuser", result.User.Username);
    }

    [Fact]
    public async Task RegisterAsync_WithExistingUsername_ReturnsFailure()
    {
        // Arrange
        var model = new RegisterDto
        {
            Username = "existinguser",
            Email = "test@example.com",
            Password = "Test123!",
            ConfirmPassword = "Test123!"
        };

        var existingUser = new User { Username = "existinguser" };
        _mockUserRepository.Setup(r => r.GetByUsernameAsync("existinguser"))
            .ReturnsAsync(existingUser);

        // Act
        var result = await _userService.RegisterAsync(model);

        // Assert
        Assert.False(result.Success);
        Assert.Null(result.User);
        Assert.Contains("already taken", result.Message);
    }

    [Fact]
    public async Task LoginAsync_WithValidCredentials_ReturnsSuccess()
    {
        // Arrange
        var model = new LoginDto
        {
            UsernameOrEmail = "testuser",
            Password = "Test123!"
        };

        var user = new User
        {
            Id = 1,
            Username = "testuser",
            Email = "test@example.com"
        };

        _mockUserRepository.Setup(r => r.GetByUsernameAsync("testuser"))
            .ReturnsAsync(user);

        _mockUserManager.Setup(m => m.CheckPasswordAsync(user, "Test123!"))
            .ReturnsAsync(true);

        // Act
        var result = await _userService.LoginAsync(model);

        // Assert
        Assert.True(result.Success);
        Assert.NotNull(result.User);
        Assert.Equal("testuser", result.User.Username);
    }

    [Fact]
    public async Task LoginAsync_WithInvalidCredentials_ReturnsFailure()
    {
        // Arrange
        var model = new LoginDto
        {
            UsernameOrEmail = "testuser",
            Password = "WrongPassword"
        };

        var user = new User
        {
            Id = 1,
            Username = "testuser",
            Email = "test@example.com"
        };

        _mockUserRepository.Setup(r => r.GetByUsernameAsync("testuser"))
            .ReturnsAsync(user);

        _mockUserManager.Setup(m => m.CheckPasswordAsync(user, "WrongPassword"))
            .ReturnsAsync(false);

        // Act
        var result = await _userService.LoginAsync(model);

        // Assert
        Assert.False(result.Success);
        Assert.Null(result.User);
        Assert.Contains("Invalid", result.Message);
    }
}