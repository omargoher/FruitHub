using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FruitHub.ApplicationCore.DTOs.Auth.Login;
using FruitHub.ApplicationCore.DTOs.Auth.Refresh;
using FruitHub.ApplicationCore.DTOs.Auth.Register;
using FruitHub.ApplicationCore.Exceptions;
using FruitHub.ApplicationCore.Interfaces;
using FruitHub.ApplicationCore.Models;
using FruitHub.Infrastructure.Models;
using FruitHub.Infrastructure.Identity;
using FruitHub.Infrastructure.Identity.Models;
using FruitHub.Infrastructure.Interfaces;
using FruitHub.Infrastructure.Interfaces.Repositories;
using FruitHub.Infrastructure.Interfaces.Services;
using FruitHub.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace FruitHub.Tests;

public class JwtAuthServiceTest
{
    private readonly  Mock<IIdentityUserRepository> _identityUserRepo;
    private readonly  Mock<ITokenService> _tokenService;
    private readonly Mock<IUnitOfWork> _uow;

    public JwtAuthServiceTest()
    {
        _identityUserRepo = new Mock<IIdentityUserRepository>();
        _uow = new Mock<IUnitOfWork>();
        _tokenService = new Mock<ITokenService>();
    }
    
    private JwtAuthService CreateSut()
    {
        return new JwtAuthService(
            _uow.Object,
            _tokenService.Object,
            _identityUserRepo.Object
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
    
    private RefreshTokenRequestDto ValidRefreshTokenRequestDto()
    {
        return new RefreshTokenRequestDto
        {
            RefreshToken = "valid-token"
        };
    }
    
    [Fact]
    public async Task RegisterAsync_WhenIdentityUserCreationFails_ShouldThrowsIdentityOperationException()
    {
        // Arrange
        _identityUserRepo.Setup(x =>
                x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed());
        
        var sut = CreateSut();
    
        // ACT & ASSERT
        await Assert.ThrowsAsync<IdentityOperationException>(() =>
            sut.RegisterAsync(ValidRegisterDto()));
    }
    
    [Fact]
    public async Task RegisterAsync_WhenAddToRoleFails_ShouldThrowsIdentityOperationException()
    {
        // Arrange
        _identityUserRepo.Setup(x =>
                x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);
        
        _identityUserRepo.Setup(x =>
                x.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed());
        
        var sut = CreateSut();
    
        // ACT & ASSERT
        await Assert.ThrowsAsync<IdentityOperationException>(() =>
            sut.RegisterAsync(ValidRegisterDto()));
    }

    [Fact]
    public async Task RegisterAsync_WhenAddToRoleFails_ShouldDeletesIdentityUser()
    {
        // Arrange
        _identityUserRepo.Setup(x =>
                x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);
        
        _identityUserRepo.Setup(x =>
                x.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed());
        
        _identityUserRepo
            .Setup(x =>
                x.DeleteAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(IdentityResult.Success);
        
        var sut = CreateSut();
    
        // ACT
        try
        {
            await sut.RegisterAsync(ValidRegisterDto());
        }
        catch
        {
            
        }
        
        // ASSERT
        _identityUserRepo.Verify(
            x => x.DeleteAsync(It.IsAny<ApplicationUser>()),
            Times.Once
        );
    }
    
    [Fact]
    public async Task RegisterAsync_WhenBusineesUserCreationThrowsException_ShouldRethrowsException()
    {
        // Arrange
        _identityUserRepo.Setup(x =>
                x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);
        
        _identityUserRepo.Setup(x =>
                x.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);
        
        _uow.Setup(x => x.User.Add(It.IsAny<User>()));
        
        _uow.Setup(x => x.SaveChangesAsync()).ThrowsAsync(new Exception());
        
        var sut = CreateSut();
    
        // ACT & ASSERT
        await Assert.ThrowsAsync<Exception>(() =>
            sut.RegisterAsync(ValidRegisterDto()));
    }
    
    [Fact]
    public async Task RegisterAsync_WhenBusineesUserCreationThrowsException_ShouldDeletesIdentityUser()
    {
        // Arrange
        _identityUserRepo.Setup(x =>
                x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);
        
        _identityUserRepo.Setup(x =>
                x.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);
        
        _identityUserRepo
            .Setup(x =>
                x.DeleteAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(IdentityResult.Success);
        
        _uow.Setup(x => x.User.Add(It.IsAny<User>()));
        
        _uow.Setup(x => x.SaveChangesAsync())
            .ThrowsAsync(new Exception());
        
        var sut = CreateSut();
    
        // ACT
        try
        {
            await sut.RegisterAsync(ValidRegisterDto());
        }
        catch
        {
            
        }
        
        // ASSERT
        _identityUserRepo.Verify(
            x => x.DeleteAsync(It.IsAny<ApplicationUser>()),
            Times.Once
        );
    }
    
    [Fact]
    public async Task RegisterAsync_WhenAddClaimFails_ShouldThrowsIdentityOperationException()
    {
        // Arrange
        _identityUserRepo.Setup(x =>
                x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);
        
        _identityUserRepo.Setup(x =>
                x.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);
        
        _identityUserRepo.Setup(x =>
                x.AddClaimAsync(It.IsAny<ApplicationUser>(), It.IsAny<Claim>()))
            .ReturnsAsync(IdentityResult.Failed());
        
        _uow.Setup(x => x.User.Add(It.IsAny<User>()));
        
        _uow.Setup(x => x.SaveChangesAsync());
        
        var sut = CreateSut();
    
        // ACT & ASSERT
        await Assert.ThrowsAsync<IdentityOperationException>(() =>
            sut.RegisterAsync(ValidRegisterDto()));
    }
    
    [Fact]
    public async Task RegisterAsync_WhenAddClaimFails_ShouldDeletesIdentityUser()
    {
        // Arrange
        _identityUserRepo.Setup(x =>
                x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);
        
        _identityUserRepo.Setup(x =>
                x.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);
        
        _identityUserRepo.Setup(x =>
                x.AddClaimAsync(It.IsAny<ApplicationUser>(), It.IsAny<Claim>()))
            .ReturnsAsync(IdentityResult.Failed());
        
        _identityUserRepo
            .Setup(x =>
                x.DeleteAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(IdentityResult.Success);
        
        _uow.Setup(x => x.User.Add(It.IsAny<User>()));
        
        _uow.Setup(x => x.SaveChangesAsync());
        
        var sut = CreateSut();
    
        // ACT 
        try
        {
            await sut.RegisterAsync(ValidRegisterDto());
        }
        catch
        {
        }
        
        // ASSERT
        _identityUserRepo.Verify(
            x => x.DeleteAsync(It.IsAny<ApplicationUser>()),
            Times.Once
        );
    }
    
    [Fact]
    public async Task RegisterAsync_WhenSuccessful_ShouldCreateUser()
    {
        // Arrange
        _identityUserRepo.Setup(x =>
                x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);
        
        _identityUserRepo.Setup(x =>
                x.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);
        
        _identityUserRepo.Setup(x =>
                x.AddClaimAsync(It.IsAny<ApplicationUser>(), It.IsAny<Claim>()))
            .ReturnsAsync(IdentityResult.Success);
                
        _uow.Setup(x => x.User.Add(It.IsAny<User>()));

        _uow
            .Setup(x => x.SaveChangesAsync());
        
        var sut = CreateSut();
    
        // ACT
        var user = await sut.RegisterAsync(ValidRegisterDto());
        
        // Assert
        Assert.Equal(ValidRegisterDto().Email, user.Email);
    }
    
    [Fact]
    public async Task LoginAsync_WhenEmailIsNotExist_ShouldThrowsInvalidCredentialsException()
    {
        // Arrange
        _identityUserRepo.Setup(x =>
                x.GetByEmail(It.IsAny<string>()))
            .ReturnsAsync((ApplicationUser?)null);
        
        var sut = CreateSut();
        
        // ACT & ASSERT
        await Assert.ThrowsAsync<InvalidCredentialsException>(() =>
            sut.LoginAsync(ValidLoginDto()));
    }
    
    [Fact]
    public async Task LoginAsync_WhenPasswordIsNotCorrect_ShouldThrowsInvalidCredentialsException()
    {
        // Arrange
        _identityUserRepo.Setup(x =>
                x.GetByEmail(It.IsAny<string>()))
            .ReturnsAsync(new ApplicationUser());
        
        _identityUserRepo.Setup(x =>
                x.CheckPasswordAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(false);
        
        var sut = CreateSut();
        
        // ACT & ASSERT
        await Assert.ThrowsAsync<InvalidCredentialsException>(() =>
            sut.LoginAsync(ValidLoginDto()));
    }
    
    [Fact]
    public async Task LoginAsync_WhenEmailIsNotConfirmed_ShouldThrowsEmailNotConfirmedException()
    {
        // Arrange
        _identityUserRepo.Setup(x =>
                x.GetByEmail(It.IsAny<string>()))
            .ReturnsAsync(new ApplicationUser
            {
                EmailConfirmed = false
            });
        
        _identityUserRepo.Setup(x =>
                x.CheckPasswordAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(true);
        
        var sut = CreateSut();
    
        // ACT & ASSERT
        await Assert.ThrowsAsync<EmailNotConfirmedException>(() =>
            sut.LoginAsync(ValidLoginDto()));
    }
    
    [Fact]
    public async Task LoginAsync_WhenJwtTokenCreationThrowsException_ShouldRethrowsException()
    {
        // Arrange
        _identityUserRepo.Setup(x =>
                x.GetByEmail(It.IsAny<string>()))
            .ReturnsAsync(new ApplicationUser
            {
                EmailConfirmed = true
            });
        
        _identityUserRepo.Setup(x =>
                x.CheckPasswordAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        _tokenService.Setup(t => 
                t.GenerateJwtAsync(It.IsAny<ApplicationUser>()))
            .ThrowsAsync(new Exception());
        
        var sut = CreateSut();
    
        // ACT & ASSERT
        await Assert.ThrowsAsync<Exception>(() =>
             sut.LoginAsync(ValidLoginDto()));
    }
    
    [Fact]
    public async Task LoginAsync_WhenRefreshTokenCreationThrowsException_ShouldRethrowsException()
    {
        // Arrange
        _identityUserRepo.Setup(x =>
                x.GetByEmail(It.IsAny<string>()))
            .ReturnsAsync(new ApplicationUser
            {
                EmailConfirmed = true
            });
        
        _identityUserRepo.Setup(x =>
                x.CheckPasswordAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(true);

        _tokenService.Setup(t => 
                t.CreateRefreshTokenAsync(It.IsAny<ApplicationUser>(), null))
            .ThrowsAsync(new Exception());
        
        var sut = CreateSut();
    
        // ACT & ASSERT
        await Assert.ThrowsAsync<Exception>(() =>
            sut.LoginAsync(ValidLoginDto()));
    }
    
    [Fact]
    public async Task LoginAsync_WhenSuccessful_ShouldReturnsValidJwtToken()
    {
        // Arrange
        _identityUserRepo.Setup(x =>
                x.GetByEmail(It.IsAny<string>()))
            .ReturnsAsync(new ApplicationUser
            {
                EmailConfirmed = true
            });
        
        _identityUserRepo.Setup(x =>
                x.CheckPasswordAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(true);
       
        _tokenService
            .Setup(x => x.GenerateJwtAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(new JwtSecurityToken());
        
        _tokenService.Setup(x => x.CreateRefreshTokenAsync(It.IsAny<ApplicationUser>(), null))
            .ReturnsAsync(new RefreshTokenModel {Token = "Valid"});
        
        var sut = CreateSut();
    
        // ACT
        var result = await sut.LoginAsync(ValidLoginDto());
    
        // ASSERT
        Assert.NotNull(result.AccessToken);
    }
    
    [Fact]
    public async Task LoginAsync_WhenSuccessful_ShouldReturnsValidRefreshToken()
    {
        // Arrange
        _identityUserRepo.Setup(x =>
                x.GetByEmail(It.IsAny<string>()))
            .ReturnsAsync(new ApplicationUser
            {
                EmailConfirmed = true
            });
        
        _identityUserRepo.Setup(x =>
                x.CheckPasswordAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(true);
       
        _tokenService
            .Setup(x => x.GenerateJwtAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(new JwtSecurityToken());
        
        _tokenService.Setup(x => x.CreateRefreshTokenAsync(It.IsAny<ApplicationUser>(), null))
            .ReturnsAsync(new RefreshTokenModel {Token = "Valid"});
        
        var sut = CreateSut();
        
        // ACT
        var result = await sut.LoginAsync(ValidLoginDto());
    
        // ASSERT
        Assert.NotNull(result.RefreshToken);
    }
    
    [Fact]
    public async Task RefreshAsync_WhenRefreshTokenNotForAnyUser_ShouldThrowsInvalidRefreshTokenException()
    {
        // Arrange
        _identityUserRepo.Setup(x =>
                x.GetByRefreshTokenWithRefreshTokens(It.IsAny<string>()))
            .ReturnsAsync((ApplicationUser?)null);
        
        var sut = CreateSut();
    
        // ACT & ASSERT
        await Assert.ThrowsAsync<InvalidRefreshTokenException>(() =>
            sut.RefreshAsync(ValidRefreshTokenRequestDto()));
    }
    
    [Fact]
    public async Task RefreshAsync_WhenRefreshTokenIsNotActive_ShouldThrowsInvalidRefreshTokenException()
    {
        // Arrange
        _identityUserRepo.Setup(x =>
                x.GetByRefreshTokenWithRefreshTokens(It.IsAny<string>()))
            .ReturnsAsync(new ApplicationUser());
       
        _tokenService.Setup(x => x.CreateRefreshTokenAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ThrowsAsync(new InvalidRefreshTokenException());
        
        var sut = CreateSut();
    
        // ACT & ASSERT
        await Assert.ThrowsAsync<InvalidRefreshTokenException>(() =>
            sut.RefreshAsync(ValidRefreshTokenRequestDto()));
    }
    
    [Fact]
    public async Task RefreshAsync_WhenRefreshTokenCreationThrowsException_ShouldRethrowsException()
    {
        // Arrange
        _identityUserRepo.Setup(x =>
                x.GetByRefreshTokenWithRefreshTokens(It.IsAny<string>()))
            .ReturnsAsync(new ApplicationUser());
       
        _tokenService.Setup(x => x.CreateRefreshTokenAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ThrowsAsync(new IdentityOperationException(new List<string>()));
        
        var sut = CreateSut();
    
        // ACT & ASSERT
        await Assert.ThrowsAsync<IdentityOperationException>(() =>
            sut.RefreshAsync(ValidRefreshTokenRequestDto()));
    }

    [Fact]
    public async Task RefreshAsync_WhenJwtTokenCreationThrowsException_ShouldRethrowsException()
    {
        // Arrange
        _identityUserRepo.Setup(x =>
                x.GetByRefreshTokenWithRefreshTokens(It.IsAny<string>()))
            .ReturnsAsync(new ApplicationUser());

        _tokenService.Setup(x => x.CreateRefreshTokenAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(new RefreshTokenModel { Token = "newToken" });
        
        _tokenService.Setup(x => x.GenerateJwtAsync(It.IsAny<ApplicationUser>()))
            .ThrowsAsync(new Exception());
        
        var sut = CreateSut();
    
        // ACT & ASSERT
        await Assert.ThrowsAsync<Exception>(() =>
             sut.RefreshAsync(ValidRefreshTokenRequestDto()));
    }
    
    [Fact]
    public async Task RefreshAsync_WhenSuccessful_ShouldReturnsValidJwtToken()
    {
        // Arrange
        _identityUserRepo.Setup(x =>
                x.GetByRefreshTokenWithRefreshTokens(It.IsAny<string>()))
            .ReturnsAsync(new ApplicationUser());

        _tokenService.Setup(x => x.CreateRefreshTokenAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(new RefreshTokenModel { Token = "newToken" });
        
        _tokenService
            .Setup(x => x.GenerateJwtAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(new JwtSecurityToken());
        
        var sut = CreateSut();
    
        // ACT
        var result = await sut.RefreshAsync(ValidRefreshTokenRequestDto());
    
        // ASSERT
        Assert.NotNull(result.AccessToken);
    }
    
    [Fact]
    public async Task RefreshAsync_WhenSuccessful_ShouldReturnsValidRefreshToken()
    {
        // Arrange
        _identityUserRepo.Setup(x =>
                x.GetByRefreshTokenWithRefreshTokens(It.IsAny<string>()))
            .ReturnsAsync(new ApplicationUser());

        _tokenService.Setup(x => x.CreateRefreshTokenAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(new RefreshTokenModel { Token = "newToken" });
        
        _tokenService
            .Setup(x => x.GenerateJwtAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(new JwtSecurityToken());
        
        var sut = CreateSut();
    
        // ACT
        var result = await sut.RefreshAsync(ValidRefreshTokenRequestDto());
    
        // ASSERT
        Assert.NotNull(result.RefreshToken);
    }
    
    [Fact]
    public async Task LogoutAsync_WhenUserNotFound_ShouldReturn()
    {
        // Arrange
        _identityUserRepo.Setup(x =>
                x.GetByIdWithRefreshTokens(It.IsAny<string>()))
            .ReturnsAsync((ApplicationUser?)null);
        
        var sut = CreateSut();
    
        // ACT
        await sut.LogoutAsync("000");
        
        // ASSERT
        _tokenService.Verify(t => t.RevokeAllAsync(It.IsAny<ApplicationUser>()), Times.Never);
    }
    
    [Fact]
    public async Task LogoutAsync_WhenSuccessful_ShouldRevokeAllRefreshTokens()
    {
        // Arrange
        _identityUserRepo.Setup(x =>
                x.GetByIdWithRefreshTokens(It.IsAny<string>()))
            .ReturnsAsync(new ApplicationUser());
        
        _identityUserRepo.Setup(x =>
                x.UpdateAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(IdentityResult.Success);
        
        _tokenService
            .Setup(x => x.RevokeAllAsync(It.IsAny<ApplicationUser>()));
        
        var sut = CreateSut();
        
        // ACT
        await sut.LogoutAsync("000");
        
        // ASSERT
        _tokenService.Verify(t => t.RevokeAllAsync(It.IsAny<ApplicationUser>()), Times.Once);
        _identityUserRepo.Verify(t => t.UpdateAsync(It.IsAny<ApplicationUser>()), Times.Once);
    }
}