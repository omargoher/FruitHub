using FruitHub.ApplicationCore.DTOs;
using FruitHub.ApplicationCore.DTOs.Auth.PasswordRecovery;
using FruitHub.ApplicationCore.Interfaces;
using FruitHub.ApplicationCore.Models;
using FruitHub.ApplicationCore.Options;
using FruitHub.Infrastructure.Identity;
using FruitHub.Infrastructure.Interfaces;
using FruitHub.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Moq;

namespace FruitHub.Tests;

public class PasswordRecoveryServiceTest
{
    private readonly Mock<UserManager<ApplicationUser>> _userManager;
    private readonly Mock<IEmailService> _email;
    private readonly Mock<IAppCache> _cache;
    private readonly Mock<IOtpService> _otp;
    
    public PasswordRecoveryServiceTest()
    {
        var userStore = new Mock<IUserStore<ApplicationUser>>();

        _userManager = new Mock<UserManager<ApplicationUser>>(
            userStore.Object,
            null, null, null, null, null, null, null, null
        );
        
        _email = new Mock<IEmailService>();
        _cache = new Mock<IAppCache>();
        _otp = new Mock<IOtpService>();
    }
    
    private PasswordRecoveryService CreateSut()
    {
        return new PasswordRecoveryService(
            _userManager.Object,
            _email.Object,
            _cache.Object,
            _otp.Object
        );
    }

    private VerifyForgetPasswordCodeDto ValidVerifyForgetPasswordCodeDto()
    {
        return new VerifyForgetPasswordCodeDto
        {
            Email = "omar@test.com",
            Otp = "123555"
        };
    }

    private SendForgetPasswordCodeDto ValidSendForgetPasswordCodeDto()
    {
        return new SendForgetPasswordCodeDto
        {
            Email = "omar@test.com",
         };
    }
    
    private ResetPasswordDto ValidResetPasswordDto()
    {
        return new ResetPasswordDto
        {
            ResetToken = "omar@test.com",
            NewPassword = "123555"
        };
    }
    
    [Fact]
    public async Task SendCodeAsync_WhenSendForgetPasswordCodeDtoIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        var sut = CreateSut();

        // ACT
        var result = () =>  sut.SendCodeAsync(null);

        // ASSERT
        await Assert.ThrowsAsync<ArgumentNullException>(result);
    }
    
    [Fact]
    public async Task SendCodeAsync_WhenUserIsNull_Return()
    {
        // Arrange
        _userManager.Setup(x =>
                x.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((ApplicationUser?) null);
        
        var stu = CreateSut();

        // ACT
        await stu.SendCodeAsync(ValidSendForgetPasswordCodeDto());
        
        // ASSERT
   
    }
    
    [Fact]
    public async Task SendCodeAsync_WhenEmailAlreadyConfirmed_Return()
    {
        // Arrange
        _userManager.Setup(x =>
                x.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(new ApplicationUser
            {
                Email = "o@g.com",
                EmailConfirmed = true
            });
        
        var stu = CreateSut();

        // ACT
        await stu.SendCodeAsync(ValidSendForgetPasswordCodeDto());

        // ASSERT
        
    }
    
    [Fact]
    public async Task SendCodeAsync_WhenEmailIsNotConfirmed_SendOTPUsingEmail()
    {
        // Arrange
        _userManager.Setup(x =>
                x.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(new ApplicationUser
            {
                Email = "o@g.com",
                EmailConfirmed = false
            });

        _otp.Setup(x => 
            x.GenerateOtp()).Returns("111111");
        
        _cache.Setup(x => 
            x.SetAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<TimeSpan>()));
        
        _email.Setup(x => 
            x.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>()));
        
        var stu = CreateSut();

        // ACT
        await stu.SendCodeAsync(ValidSendForgetPasswordCodeDto());

        // ASSERT
        _email.Verify(
            x => x.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())
            ,Times.Once);
    }
    
    [Fact]
    public async Task VerifyCodeAsync_WhenConfirmEmailDtoIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        var sut = CreateSut();

        // ACT
        var result = () =>  sut.VerifyCodeAsync(null);

        // ASSERT
        await Assert.ThrowsAsync<ArgumentNullException>(result);
    }
    
    [Fact]
    public async Task VerifyCodeAsync_WhenCacheIsNull_ReturnsOtpExpired()
    {
        _cache.Setup(x =>
                x.GetAsync<EmailOtpCacheModel>(It.IsAny<string>()))
            .ReturnsAsync((EmailOtpCacheModel?)null);
    
        var sut = CreateSut();
    
        var result = await sut.VerifyCodeAsync(ValidVerifyForgetPasswordCodeDto());
    
        Assert.Contains("OTP_EXPIRED", result.Errors);
        Assert.False(result.IsVerify);
    }
    
    [Fact]
    public async Task VerifyCodeAsync_WhenAttemptsZero_ReturnsOtpLocked()
    {
        _cache.Setup(x =>
                x.GetAsync<EmailOtpCacheModel>(It.IsAny<string>()))
            .ReturnsAsync(new EmailOtpCacheModel
            {
                AttemptsLeft = 0,
                Otp = "1234"
            });
    
        var sut = CreateSut();
    
        var result = await sut.VerifyCodeAsync(ValidVerifyForgetPasswordCodeDto());
    
        Assert.Contains("OTP_LOCKED", result.Errors);
    }
    
    [Fact]
    public async Task VerifyCodeAsync_WhenOtpInvalid_DecrementsAttempts()
    {
        var cache = new EmailOtpCacheModel
        {
            AttemptsLeft = 3,
            Otp = "CORRECT"
        };
        _cache.Setup(x =>
                x.GetAsync<EmailOtpCacheModel>(It.IsAny<string>()))
            .ReturnsAsync(cache);
    
        var sut = CreateSut();
    
        var result = await sut.VerifyCodeAsync(ValidVerifyForgetPasswordCodeDto());
    
        Assert.Contains("OTP_INVALID", result.Errors);
        Assert.Equal(2, cache.AttemptsLeft);
    
        _cache.Verify(x =>
                x.SetAsync(
                    It.IsAny<string>(),
                    It.IsAny<EmailOtpCacheModel>(),
                    It.IsAny<TimeSpan>()),
            Times.Once);
    }
    
    [Fact]
    public async Task VerifyCodeAsync_WhenUserNotFound_ReturnsInvalidRequest()
    {
        _cache.Setup(x =>
                x.GetAsync<EmailOtpCacheModel>(It.IsAny<string>()))
            .ReturnsAsync(new EmailOtpCacheModel
            {
                AttemptsLeft = 3,
                Otp = "1234"
            });
    
        _userManager.Setup(x =>
                x.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((ApplicationUser?)null);
    
        var sut = CreateSut();
    
        var result = await sut.VerifyCodeAsync(
            new VerifyForgetPasswordCodeDto()
            {
                Email = "test@test.com",
                Otp = "1234"
            });
    
        Assert.Contains("INVALID_REQUEST", result.Errors);
    }

    [Fact]
    public async Task VerifyCodeAsync_WhenValid_SendResetToken()
    {
        var cache = new EmailOtpCacheModel
        {
            AttemptsLeft = 3,
            Otp = "1234"
        };

        var user = new ApplicationUser
        {
            Email = "test@test.com",
            EmailConfirmed = true
        };

        _cache.Setup(x =>
                x.GetAsync<EmailOtpCacheModel>(It.IsAny<string>()))
            .ReturnsAsync(cache);

        _userManager.Setup(x =>
                x.FindByEmailAsync(user.Email))
            .ReturnsAsync(user);

        _userManager.Setup(x =>
                x.GeneratePasswordResetTokenAsync(user))
            .ReturnsAsync("5555");
        
        _userManager.Setup(x =>
                x.UpdateAsync(user))
            .ReturnsAsync(IdentityResult.Success);

        var sut = CreateSut();

        var result = await sut.VerifyCodeAsync(
            new VerifyForgetPasswordCodeDto
            {
                Email = user.Email,
                Otp = "1234"
            });

        Assert.True(result.IsVerify);
        Assert.Empty(result.Errors);
        Assert.Equal("5555", result.ResetToken);

        _cache.Verify(x =>
                x.RemoveAsync(It.IsAny<string>()),
            Times.Once);
    }

    [Fact]
    public async Task ResetAsync_WhenResetPasswordDtoIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        var sut = CreateSut();

        // ACT
        var result = () =>  sut.ResetAsync(null);

        // ASSERT
        await Assert.ThrowsAsync<ArgumentNullException>(result);
    }
    
    [Fact]
    public async Task ResetAsync_WhenEmailIsNull_ReturnsInvalidResetToken()
    {
        // Arrange
        _cache.Setup(x =>
                x.GetAsync<string>(It.IsAny<string>()))
            .ReturnsAsync((string?)null);
    
        var sut = CreateSut();
    
        // ACT
        var result = await sut.ResetAsync(ValidResetPasswordDto());
    
        // ASSERT
        Assert.Contains("INVALID_RESET_TOKEN", result.Errors);
        Assert.False(result.IsReset);
    }
    
    [Fact]
    public async Task ResetAsync_WhenUserNotFound_ReturnsInvalidRequest()
    {
        _cache.Setup(x =>
                x.GetAsync<string>(It.IsAny<string>()))
            .ReturnsAsync("Token");
    
        _userManager.Setup(x =>
                x.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((ApplicationUser?)null);
    
        var sut = CreateSut();
    
        // ACT
        var result = await sut.ResetAsync(ValidResetPasswordDto());
    
        // ASSERT
        Assert.Contains("INVALID_REQUEST", result.Errors);
        Assert.False(result.IsReset);
    }

    [Fact]
    public async Task ResetAsync_WhenResetPasswordFailed_ReturnsError()
    {
        _cache.Setup(x =>
                x.GetAsync<string>(It.IsAny<string>()))
            .ReturnsAsync("Token");
    
        _userManager.Setup(x =>
                x.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(new ApplicationUser { });
    
        _userManager.Setup(x =>
                x.ResetPasswordAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Failed());
        
        var sut = CreateSut();
    
        // ACT
        var result = await sut.ResetAsync(ValidResetPasswordDto());
    
        // ASSERT
        Assert.NotEmpty(result.Errors);
        Assert.False(result.IsReset);
    }

    [Fact]
    public async Task ResetAsync_WhenValid_ResetToken()
    {
        _cache.Setup(x =>
                x.GetAsync<string>(It.IsAny<string>()))
            .ReturnsAsync("Token");
    
        _userManager.Setup(x =>
                x.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync(new ApplicationUser { });
    
        _userManager.Setup(x =>
                x.ResetPasswordAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>(), It.IsAny<string>()))
            .ReturnsAsync(IdentityResult.Success);
        
        var sut = CreateSut();
    
        // ACT
        var result = await sut.ResetAsync(ValidResetPasswordDto());
    
        // ASSERT
        Assert.Empty(result.Errors);
        Assert.True(result.IsReset);

        _cache.Verify(x =>
            x.RemoveAsync(
                It.IsAny<string>()
            ));
    }

}