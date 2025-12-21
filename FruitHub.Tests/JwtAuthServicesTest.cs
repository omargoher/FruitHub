using System.IdentityModel.Tokens.Jwt;
using FruitHub.ApplicationCore.DTOs;
using FruitHub.ApplicationCore.DTOs.Auth.Login;
using FruitHub.ApplicationCore.DTOs.Auth.Refresh;
using FruitHub.ApplicationCore.DTOs.Auth.Register;
using FruitHub.ApplicationCore.Interfaces;
using FruitHub.ApplicationCore.Models;
using FruitHub.ApplicationCore.Options;
using FruitHub.Infrastructure.Identity;
using FruitHub.Infrastructure.Interfaces;
using FruitHub.Infrastructure.Persistence;
using FruitHub.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Moq;
using Microsoft.EntityFrameworkCore.InMemory;

namespace FruitHub.Tests;

// TODO => Refactoring it (Create DBContext, Sut)

public class JwtAuthServicesTest
{
    private readonly Mock<UserManager<ApplicationUser>> _userManager;
    private readonly  Mock<ITokenService> _tokenService;
    private readonly Mock<IUnitOfWork> _repositories;

    public JwtAuthServicesTest()
    {
        var userStore = new Mock<IUserStore<ApplicationUser>>();

        _userManager = new Mock<UserManager<ApplicationUser>>(
            userStore.Object,
            null, null, null, null, null, null, null, null
        );
        
        _repositories = new Mock<IUnitOfWork>();
        
        _tokenService = new Mock<ITokenService>();
    }
    
    private JwtAuthService CreateSut()
    {
        return new JwtAuthService(
            _userManager.Object,
            _repositories.Object,
            _tokenService.Object
        );
    }

    private static AppIdentityDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppIdentityDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppIdentityDbContext(options);
    }
    
    private static UserManager<ApplicationUser> CreateUserManager(
        AppIdentityDbContext context)
    {
        var store = new UserStore<ApplicationUser>(context);

        return new UserManager<ApplicationUser>(
            store,
            null,
            new PasswordHasher<ApplicationUser>(),
            null,
            null,
            null,
            null,
            null,
            null
        );
    }
    
    private RegisterDto ValidRegisterDto()
    {
        return new RegisterDto
        {
            UserName = "omar",
            Email = "omar@test.com",
            Password = "StrongPass1!",
            FullName = "Omar Ahmed"
        };
    }

    private LoginDto ValidLoginDto()
    {
        return new LoginDto
        {
            Email = "omar@test.com",
            Password = "StrongPass1!",
        };
    }
    
    [Fact]
    public async Task RegisterAsync_WhenRegisterDtoIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        var jwtAuth = CreateSut();

        // ACT
        var result = () =>  jwtAuth.RegisterAsync(null);

        // ASSERT
        await Assert.ThrowsAsync<ArgumentNullException>(result);
    }
    
    [Fact]
    public async Task RegisterAsync_WhenIdentityUserCreationFails_ReturnsErrorResponse()
    {
        // Arrange
        _userManager.Setup(x =>
                x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed(
                new IdentityError { Code = "Password", Description = "Password does not meet security requirements" }
            ));
        var jwtAuth = CreateSut();

        // ACT
        var result = await jwtAuth.RegisterAsync(ValidRegisterDto());

        // ASSERT
        Assert.False(result.IsRegistered);
        Assert.Equal("Password does not meet security requirements", result.Errors[0]);
    }
    
    [Fact]
    public async Task RegisterAsync_WhenAddToRoleFails_ReturnsErrorAndDeletesIdentityUser()
    {
        // Arrange
        _userManager.Setup(x =>
                x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);
        _userManager.Setup(x =>
                x.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed(
                new IdentityError { Code = "Role", Description = "" }
            ));
        _userManager
            .Setup(x => x.DeleteAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(IdentityResult.Success);
        
        var jwtAuth = CreateSut();

        // ACT
        var result = await jwtAuth.RegisterAsync(ValidRegisterDto());

        // ASSERT
        Assert.False(result.IsRegistered);
        Assert.Equal("Registration failed", result.Errors[0]);
        
        _userManager.Verify(
            x => x.DeleteAsync(It.IsAny<ApplicationUser>()),
            Times.Once
        );
    }
    
    [Fact]
    public async Task RegisterAsync_WhenDomainUserSaveFails_ReturnsErrorAndDeletesIdentityUser()
    {
        // Arrange
        _userManager.Setup(x =>
                x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);
        _userManager.Setup(x =>
                x.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);
        _userManager
            .Setup(x => x.DeleteAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(IdentityResult.Success);
        
        _repositories.Setup(x => x.Repository<User, int>().Insert(It.IsAny<User>()));
        _repositories.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(0);
        
        var jwtAuth = CreateSut();

        // ACT
        var result = await jwtAuth.RegisterAsync(ValidRegisterDto());

        // ASSERT

        Assert.False(result.IsRegistered);
        Assert.Equal("Registration failed", result.Errors[0]);
        
        // ASSERT
        _userManager.Verify(
            x => x.DeleteAsync(It.IsAny<ApplicationUser>()),
            Times.Once
        );
    }
    
    [Fact]
    public async Task RegisterAsync_WhenUnexpectedExceptionOccurs_RethrowsExceptionAndDeletesIdentityUser()
    {
        // Arrange
        _userManager.Setup(x =>
                x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);
        _userManager.Setup(x =>
                x.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);
        _userManager
            .Setup(x => x.DeleteAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(IdentityResult.Success);
        
        _repositories.Setup(x => x.Repository<User, int>().Insert(It.IsAny<User>()));
        _repositories
            .Setup(x => x.SaveChangesAsync())
            .ThrowsAsync(new InvalidOperationException("DB is down"));
        
        var jwtAuth = CreateSut();

        // ACT
        var result = () => jwtAuth.RegisterAsync(ValidRegisterDto());

        // ASSERT
        await Assert.ThrowsAsync<InvalidOperationException>(result);
        
        _userManager.Verify(
            x => x.DeleteAsync(It.IsAny<ApplicationUser>()),
            Times.Once
        );
    }
    
    [Fact]
    public async Task RegisterAsync_WhenSuccessful_ReturnsRegisteredUserAndDoesNotDeleteIdentityUser()
    {
        // Arrange
        _userManager.Setup(x =>
                x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);
        _userManager.Setup(x =>
                x.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);
        _userManager
            .Setup(x => x.DeleteAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(IdentityResult.Success);
        
        _repositories.Setup(x => x.Repository<User, int>().Insert(It.IsAny<User>()));
        _repositories
            .Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);
        
        var jwtAuth = CreateSut();


        var result = await jwtAuth.RegisterAsync(ValidRegisterDto());

        Assert.True(result.IsRegistered);

        _userManager.Verify(
            x => x.DeleteAsync(It.IsAny<ApplicationUser>()),
            Times.Never
        );
    }
    
    [Fact]
    public async Task LoginAsync_WhenLoginDtoIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        var jwtAuth = CreateSut();

        // ACT
        var result = () =>  jwtAuth.LoginAsync(null);

        // ASSERT
        await Assert.ThrowsAsync<ArgumentNullException>(result);
    }

    [Fact]
    public async Task LoginAsync_WhenEmailIsNotExist_ReturnsErrorResponse()
    {
        // Arrange
        _userManager.Setup(x =>
                x.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((ApplicationUser?)null);
        
        var jwtAuth = CreateSut();
        
        // ACT
        var result = await jwtAuth.LoginAsync(ValidLoginDto());

        // ASSERT
        Assert.Equal("This email not for any user", result.Errors[0]);
        Assert.False(result.IsAuthenticated);
    }
    
    [Fact]
    public async Task LoginAsync_WhenPasswordIsIncorrect_ReturnsErrorResponse()
    {
        // Arrange
        _userManager.Setup(x =>
                x.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(
                new ApplicationUser
                {
                    Email = "omar@gmail.com",
                });
        _userManager.Setup(x =>
                x.CheckPasswordAsync(It.IsAny<ApplicationUser>(),It.IsAny<string>()))
            .ReturnsAsync(false);
        
        var jwtAuth = CreateSut();
        
        // ACT
        var result = await jwtAuth.LoginAsync(ValidLoginDto());

        // ASSERT
        Assert.Equal("Password Is Incorrect", result.Errors[0]);
        Assert.False(result.IsAuthenticated);
    }
    
    [Fact]
    public async Task LoginAsync_WhenEmailIsNotConfirmed_ReturnsErrorResponse()
    {
        // Arrange
        _userManager.Setup(x =>
                x.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(
                new ApplicationUser
                {
                    Email = "test@test.com",
                    EmailConfirmed = false
                });
        _userManager.Setup(x =>
                x.CheckPasswordAsync(It.IsAny<ApplicationUser>(),It.IsAny<string>()))
            .ReturnsAsync(true);
        var jwtAuth = CreateSut();

        // ACT
        var result = await jwtAuth.LoginAsync(ValidLoginDto());

        // ASSERT
        Assert.Equal("Email is not verify", result.Errors[0]);
        Assert.False(result.IsAuthenticated);
    }
    
    [Fact]
    public async Task LoginAsync_WhenUnexpectedExceptionOccurs_RethrowsException()
    {
        // Arrange
        _userManager.Setup(x =>
                x.FindByEmailAsync(It.IsAny<string>()))
            .ThrowsAsync(new InvalidOperationException("DB is down"));
        
        var jwtAuth = CreateSut();

        // ACT
        var result = () => jwtAuth.LoginAsync(ValidLoginDto());

        // ASSERT
        await Assert.ThrowsAsync<InvalidOperationException>(result);
    }
    
    [Fact]
    public async Task LoginAsync_WhenSuccessful_ReturnsJwtTokenAndRefreshToken()
    {
        // Arrange
        _userManager.Setup(x =>
                x.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(
                new ApplicationUser
                {
                    UserName = "omar",
                    Email = "test@test.com",
                    EmailConfirmed = true
                });
        _userManager.Setup(x =>
                x.CheckPasswordAsync(It.IsAny<ApplicationUser>(),It.IsAny<string>()))
            .ReturnsAsync(true);
        _userManager
            .Setup(x => x.GetRolesAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(new List<string> { "User" });
        
        _tokenService
            .Setup(x => x.GenerateJwtAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(new JwtSecurityToken());
        _tokenService.Setup(x => x.CreateRefreshTokenAsync(It.IsAny<ApplicationUser>())).ReturnsAsync(new RefreshTokenDto {Token = "ddd"});
        
        var jwtAuth = CreateSut();


        var result = await jwtAuth.LoginAsync(ValidLoginDto());
        
        Assert.True(result.IsAuthenticated);
    }

    [Fact]
    public async Task RefreshAsync_WhenRefreshTokenIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        var jwtAuth = CreateSut();

        // ACT
        var result = () =>  jwtAuth.RefreshAsync(null);

        // ASSERT
        await Assert.ThrowsAsync<ArgumentNullException>(result);
    }
    
    [Fact]
    public async Task RefreshAsync_WhenInvalidToken_ReturnsErrorResponse()
    {
        // Arrange
        var context = CreateDbContext();
        var userManager = CreateUserManager(context);

        var jwtAuth = new JwtAuthService(
            userManager,
            _repositories.Object,
            _tokenService.Object
        );
        
        // ACT
        var result = await jwtAuth.RefreshAsync(new RefreshTokenRequestDto {RefreshToken = "pla"});
    
        // ASSERT
        Assert.Contains("INVALID_TOKEN", result.Errors);
        Assert.False(result.IsAuthenticated);
    }
    
    [Fact]
    public async Task RefreshAsync_WhenBeValidToken_ReturnsNewToken()
    {
        // Arrange
        var context = CreateDbContext();
        var userManager = CreateUserManager(context);

        var user = new ApplicationUser
        {
            UserName = "test",
            Email = "test@test.com",
            RefreshTokens = new List<RefreshToken>
            {
                new RefreshToken
                {
                    Token = "valid-token",
                    ExpiresAt = DateTime.UtcNow.AddDays(1)
                }
            }
        };

        context.Users.Add(user);
        context.SaveChanges();

        _tokenService
            .Setup(x => x.GenerateJwtAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(new JwtSecurityToken());

        _tokenService
            .Setup(x => x.CreateRefreshTokenAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(new RefreshTokenDto
            {
                Token = "new-refresh-token",
                ExpiresAt = DateTime.UtcNow.AddDays(14)
            });

        var authService = new JwtAuthService(
            userManager,
            _repositories.Object,
            _tokenService.Object
        );

        // Act
        var result = await authService.RefreshAsync(new RefreshTokenRequestDto {RefreshToken = "valid-token"});

        // Assert
        Assert.True(result.IsAuthenticated);
        Assert.Equal("test@test.com", result.Email);
        Assert.Equal("new-refresh-token", result.RefreshToken);

        _tokenService.Verify(
            x => x.GenerateJwtAsync(It.IsAny<ApplicationUser>()),
            Times.Once);

        _tokenService.Verify(
            x => x.CreateRefreshTokenAsync(It.IsAny<ApplicationUser>()),
            Times.Once);
    }
    
    [Fact]
    public async Task LogoutAsync_WhenEmailIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        var jwtAuth = CreateSut();

        // ACT
        var result = () =>  jwtAuth.LogoutAsync(null);

        // ASSERT
        await Assert.ThrowsAsync<ArgumentNullException>(result);
    }

    [Fact]
    public async Task LogoutAsync_WhenEmailIsNotExist_ReturnsWithoutRevokeAnyThing()
    {
        // Arrange
        _userManager.Setup(x =>
                x.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((ApplicationUser?)null);
        
        var jwtAuth = CreateSut();
        
        // ACT
        await jwtAuth.LogoutAsync("o@g.com");

        // ASSERT
        _tokenService.Verify(
            x => x.RevokeAllAsync(It.IsAny<ApplicationUser>()),
            Times.Never
        );
    }

    [Fact]
    public async Task LogoutAsync_WhenSuccessful_RevokeAllUserToken()
    {
        // Arrange
        _userManager.Setup(x =>
                x.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(new ApplicationUser { });
        
        var jwtAuth = CreateSut();
        
        // ACT
        await jwtAuth.LogoutAsync("o@g.com");

        // ASSERT
        _tokenService.Verify(
            x => x.RevokeAllAsync(It.IsAny<ApplicationUser>()),
            Times.Once
        );
    }
}