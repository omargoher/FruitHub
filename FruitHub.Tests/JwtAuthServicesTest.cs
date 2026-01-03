using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using FruitHub.ApplicationCore.DTOs.Auth.Login;
using FruitHub.ApplicationCore.DTOs.Auth.Refresh;
using FruitHub.ApplicationCore.DTOs.Auth.Register;
using FruitHub.ApplicationCore.Exceptions;
using FruitHub.ApplicationCore.Interfaces;
using FruitHub.ApplicationCore.Models;
using FruitHub.Infrastructure.Identity;
using FruitHub.Infrastructure.Interfaces;
using FruitHub.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Moq;

namespace FruitHub.Tests;

// TODO Review the names
public class JwtAuthServicesTest
{
    private readonly  Mock<IIdentityUserRepository> _identityUserRepo;
    private readonly  Mock<ITokenService> _tokenService;
    private readonly Mock<IUnitOfWork> _uow;

    public JwtAuthServicesTest()
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
    public async Task RegisterAsync_WhenIdentityUserCreationFails_ThrowsIdentityOperationException()
    {
        // Arrange
        _identityUserRepo.Setup(x =>
                x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed(
                new IdentityError { Code = "Password", Description = "Password does not meet security requirements" }
            ));
        var sut = CreateSut();
    
        // ACT & ASSERT
        await Assert.ThrowsAsync<IdentityOperationException>(() =>
            sut.RegisterAsync(ValidRegisterDto()));
    }
    
    [Fact]
    public async Task RegisterAsync_WhenAddToRoleFails_ThrowsIdentityOperationExceptionAndDeletesIdentityUser()
    {
        // Arrange
        _identityUserRepo.Setup(x =>
                x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);
        _identityUserRepo.Setup(x =>
                x.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed(
                new IdentityError { Code = "Role", Description = "" }
            ));
        _identityUserRepo
            .Setup(x =>
                x.DeleteAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(IdentityResult.Success);
        
        var sut = CreateSut();
    
        // ACT & ASSERT
        await Assert.ThrowsAsync<IdentityOperationException>(() =>
            sut.RegisterAsync(ValidRegisterDto()));
        
        _identityUserRepo.Verify(
            x => x.DeleteAsync(It.IsAny<ApplicationUser>()),
            Times.Once
        );
    }
    
    [Fact]
    public async Task RegisterAsync_WhenDomainUserSaveFails_ThrowsIdentityOperationExceptionAndDelesteIdentityUser()
    {
        // Arrange
        _identityUserRepo.Setup(x =>
                x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);
        
        _identityUserRepo.Setup(x =>
                x.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);
        
        _identityUserRepo
            .Setup(x => x.DeleteAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(IdentityResult.Success);
        
        _uow.Setup(x => x.User.Add(It.IsAny<User>()));
        
        _uow.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(0);
        
        var sut = CreateSut();
    
        // ACT & ASSERT
        await Assert.ThrowsAsync<RegistrationFailedException>(() =>
            sut.RegisterAsync(ValidRegisterDto()));
        
        _identityUserRepo.Verify(
            x => x.DeleteAsync(It.IsAny<ApplicationUser>()),
            Times.Once
        );
    }
    
    [Fact]
    public async Task RegisterAsync_WhenAddClaimFails_ThrowsIdentityOperationExceptionAndDeletesIdentityUser()
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
            .ReturnsAsync(IdentityResult.Failed(
                new IdentityError { Code = "fail", Description = "" }
            ));
        
        _identityUserRepo
            .Setup(x =>
                x.DeleteAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(IdentityResult.Success);
        
        _uow.Setup(x => x.User.Add(It.IsAny<User>()));
        
        _uow.Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);
        
        var sut = CreateSut();
    
        // ACT & ASSERT
        await Assert.ThrowsAsync<IdentityOperationException>(() =>
            sut.RegisterAsync(ValidRegisterDto()));
        
        _identityUserRepo.Verify(
            x => x.DeleteAsync(It.IsAny<ApplicationUser>()),
            Times.Once
        );
    }
    
    [Fact]
    public async Task RegisterAsync_WhenUnexpectedExceptionOccurs_RethrowsExceptionAndDeletesIdentityUser()
    {
        // Arrange
        _identityUserRepo.Setup(x =>
                x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);
        _identityUserRepo.Setup(x =>
                x.AddToRoleAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);
        _identityUserRepo
            .Setup(x => x.DeleteAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(IdentityResult.Success);
        
        _uow.Setup(x => x.User.Add(It.IsAny<User>()));
        _uow
            .Setup(x => x.SaveChangesAsync())
            .ThrowsAsync(new InvalidOperationException("DB is down"));
        
        var sut = CreateSut();
        
        // ACT & ASSERT
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            sut.RegisterAsync(ValidRegisterDto()));
        
        _identityUserRepo.Verify(
            x => x.DeleteAsync(It.IsAny<ApplicationUser>()),
            Times.Once
        );
    }
    
    [Fact]
    public async Task RegisterAsync_WhenSuccessful_CreateUser()
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
            .Setup(x => x.SaveChangesAsync())
            .ReturnsAsync(1);
        
        var sut = CreateSut();
    
        // ACT
        await sut.RegisterAsync(ValidRegisterDto());
        
        // ASSERT
        _identityUserRepo.Verify(
            x => x.DeleteAsync(It.IsAny<ApplicationUser>()),
            Times.Never
        );
    }
    
    [Fact]
    public async Task LoginAsync_WhenLoginDtoIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        var sut = CreateSut();
    
        // ACT & ASSERT
        await Assert.ThrowsAsync<ArgumentNullException>(() =>  sut.LoginAsync(null));
    }
    
    [Fact]
    public async Task LoginAsync_WhenEmailIsNotExist_ThrowsInvalidCredentialsException()
    {
        // Arrange
        _identityUserRepo.Setup(x =>
                x.GetByEmailWithRefreshTokens(It.IsAny<string>()))
            .ReturnsAsync((ApplicationUser?)null);
        
        var sut = CreateSut();
        
        // ACT & ASSERT
        await Assert.ThrowsAsync<InvalidCredentialsException>(() =>
            sut.LoginAsync(ValidLoginDto()));
    }
    
    [Fact]
    public async Task LoginAsync_WhenPasswordIsNotCorrect_ThrowsInvalidCredentialsException()
    {
        // Arrange
        _identityUserRepo.Setup(x =>
                x.GetByEmailWithRefreshTokens(It.IsAny<string>()))
            .ReturnsAsync(new ApplicationUser { Email = "ex@g.com"});
        
        _identityUserRepo.Setup(x =>
                x.CheckPasswordAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(false);
        
        var sut = CreateSut();
        
        // ACT & ASSERT
        await Assert.ThrowsAsync<InvalidCredentialsException>(() =>
            sut.LoginAsync(ValidLoginDto()));
    }
   
    [Fact]
    public async Task LoginAsync_WhenEmailIsNotConfirmed_ReturnsErrorResponse()
    {
        // Arrange
        _identityUserRepo.Setup(x =>
                x.GetByEmailWithRefreshTokens(It.IsAny<string>()))
            .ReturnsAsync(new ApplicationUser
            {
                Email = "test@test.com",
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
    public async Task LoginAsync_WhenUnexpectedExceptionOccurs_RethrowsException()
    {
        // Arrange
        _identityUserRepo.Setup(x =>
                x.GetByEmailWithRefreshTokens(It.IsAny<string>()))
            .ThrowsAsync(new InvalidOperationException("DB is down"));
        
        var sut = CreateSut();
    
        // ACT & ASSERT
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
             sut.LoginAsync(ValidLoginDto()));
    }
    
    [Fact]
    public async Task LoginAsync_WhenSuccessful_ReturnsJwtAndRefreshToken()
    {
        // Arrange
        _identityUserRepo.Setup(x =>
                x.GetByEmailWithRefreshTokens(It.IsAny<string>()))
            .ReturnsAsync(new ApplicationUser
            {
                Email = "test@test.com",
                EmailConfirmed = true
            });
        
        _identityUserRepo.Setup(x =>
                x.CheckPasswordAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
            .ReturnsAsync(true);
       
        _tokenService
            .Setup(x => x.GenerateJwtAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(new JwtSecurityToken());
        
        _tokenService.Setup(x => x.CreateRefreshTokenAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(new RefreshTokenDto {Token = "ddd"});
        
        var sut = CreateSut();
    
    
        var result = await sut.LoginAsync(ValidLoginDto());

        Assert.NotNull(result.Email);
        Assert.NotNull(result.RefreshToken);
        Assert.NotNull(result.Token);
    }
    
    
    [Fact]
    public async Task RefreshAsync_WhenLoginDtoIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        var sut = CreateSut();
    
        // ACT & ASSERT
        await Assert.ThrowsAsync<ArgumentNullException>(() =>  sut.RefreshAsync(null));
    }

    [Fact]
    public async Task RefreshAsync_WhenInvalidToken_ThrowsInvalidRefreshTokenException()
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
    public async Task RefreshAsync_WhenValidToken_ReturnsNewToken()
    {
        // Arrange
        _identityUserRepo.Setup(x =>
                x.GetByRefreshTokenWithRefreshTokens(It.IsAny<string>()))
            .ReturnsAsync((new ApplicationUser
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
            }));
        
        _tokenService
            .Setup(x => x.GenerateJwtAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(new JwtSecurityToken());
        
        _tokenService.Setup(x => x.CreateRefreshTokenAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(new RefreshTokenDto {Token = "new-refresh-token"});

        var sut = CreateSut();
    
        // ACT
        var result = await sut.RefreshAsync(ValidRefreshTokenRequestDto());
        
        // ASSERT
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
    public async Task LogoutAsync_WhenIdentityUserIdIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        var sut = CreateSut();
    
        // ACT & ASSERT
        await Assert.ThrowsAsync<ArgumentNullException>(() =>  sut.LogoutAsync(null));
    }

    [Fact]
    public async Task LogoutAsync_WhenUserNotFound_Return()
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
    public async Task LogoutAsync_WhenUpdateUserFails_ThrowsIdentityOperationException()
    {
        // Arrange
        _identityUserRepo.Setup(x =>
                x.GetByIdWithRefreshTokens(It.IsAny<string>()))
            .ReturnsAsync(new ApplicationUser());
        
        _identityUserRepo.Setup(x =>
                x.UpdateAsync(It.IsAny<ApplicationUser>()))
            .ReturnsAsync(IdentityResult.Failed());
        
        _tokenService
            .Setup(x => x.RevokeAllAsync(It.IsAny<ApplicationUser>()));
        

        var sut = CreateSut();
    
        // ACT & ASSERT
        await Assert.ThrowsAsync<IdentityOperationException>(() =>  sut.LogoutAsync("000"));
    }
    
    [Fact]
    public async Task LogoutAsync_WhenSuccessful_RevokeAllRefreshTokens()
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