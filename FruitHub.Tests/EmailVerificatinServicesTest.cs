using FruitHub.ApplicationCore.DTOs;
using FruitHub.ApplicationCore.DTOs.Auth.EmailVerification;
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

public class EmailConfirmationServiceTest
{
    private readonly Mock<UserManager<ApplicationUser>> _userManager;
    private readonly Mock<IEmailService> _email;
    private readonly Mock<IAppCache> _cache;
    private readonly Mock<IOtpService> _otp;
    
    public EmailConfirmationServiceTest()
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
    
    private EmailConfirmationService CreateSut()
    {
        return new EmailConfirmationService(
            _userManager.Object,
            _email.Object,
            _cache.Object,
            _otp.Object
        );
    }

    private ConfirmEmailCodeDto ValidConfirmEmailCodeDto()
    {
        return new ConfirmEmailCodeDto
        {
            Email = "omar@test.com",
            Otp = "123555"
        };
    }

    private SendEmailConfirmationCodeDto ValidSendEmailConfirmationCodeDto()
    {
        return new SendEmailConfirmationCodeDto
        {
            Email = "omar@test.com",
         };
    }
    
    [Fact]
    public async Task SendConfirmationCodeAsync_WhenSendConfirmationCodeDtoIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        var sut = CreateSut();

        // ACT
        var result = () =>  sut.SendConfirmationCodeAsync(null);

        // ASSERT
        await Assert.ThrowsAsync<ArgumentNullException>(result);
    }
    
    [Fact]
    public async Task SendConfirmationCodeAsync_WhenUserIsNull_Return()
    {
        // Arrange
        _userManager.Setup(x =>
                x.FindByEmailAsync(It.IsAny<string>()))
            .ReturnsAsync((ApplicationUser?) null);
        
        var stu = CreateSut();

        // ACT
        await stu.SendConfirmationCodeAsync(ValidSendEmailConfirmationCodeDto());
        
        // ASSERT
   
    }
    
    [Fact]
    public async Task SendConfirmationCodeAsync_WhenEmailAlreadyConfirmed_Return()
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
        await stu.SendConfirmationCodeAsync(ValidSendEmailConfirmationCodeDto());

        // ASSERT
        
    }
    
    [Fact]
    public async Task SendConfirmationCodeAsync_WhenEmailIsNotConfirmed_SendOTPUsingEmail()
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
        await stu.SendConfirmationCodeAsync(ValidSendEmailConfirmationCodeDto());

        // ASSERT
        _email.Verify(
            x => x.SendAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>())
            ,Times.Once);
    }
    
    [Fact]
    public async Task ConfirmAsync_WhenConfirmEmailCodeDtoIsNull_ThrowsArgumentNullException()
    {
        // Arrange
        var sut = CreateSut();

        // ACT
        var result = () =>  sut.ConfirmAsync(null);

        // ASSERT
        await Assert.ThrowsAsync<ArgumentNullException>(result);
    }
    
    [Fact]
    public async Task ConfirmAsync_WhenCacheIsNull_ReturnsOtpExpired()
    {
        _cache.Setup(x =>
                x.GetAsync<EmailOtpCacheModel>(It.IsAny<string>()))
            .ReturnsAsync((EmailOtpCacheModel?)null);

        var sut = CreateSut();

        var result = await sut.ConfirmAsync(ValidConfirmEmailCodeDto());

        Assert.Contains("OTP_EXPIRED", result.Errors);
        Assert.False(result.IsConfirmed);
    }
    
    [Fact]
    public async Task ConfirmAsync_WhenAttemptsZero_ReturnsOtpLocked()
    {
        _cache.Setup(x =>
                x.GetAsync<EmailOtpCacheModel>(It.IsAny<string>()))
            .ReturnsAsync(new EmailOtpCacheModel
            {
                AttemptsLeft = 0,
                Otp = "1234"
            });

        var sut = CreateSut();

        var result = await sut.ConfirmAsync(ValidConfirmEmailCodeDto());

        Assert.Contains("OTP_LOCKED", result.Errors);
    }
    
    [Fact]
    public async Task ConfirmAsync_WhenOtpInvalid_DecrementsAttempts()
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

        var result = await sut.ConfirmAsync(ValidConfirmEmailCodeDto());

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
    public async Task ConfirmAsync_WhenUserNotFound_ReturnsInvalidRequest()
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

        var result = await sut.ConfirmAsync(
            new ConfirmEmailCodeDto
            {
                Email = "test@test.com",
                Otp = "1234"
            });

        Assert.Contains("INVALID_REQUEST", result.Errors);
    }
    
    [Fact]
    public async Task ConfirmAsync_WhenValid_ConfirmsEmail()
    {
        var cache = new EmailOtpCacheModel
        {
            AttemptsLeft = 3,
            Otp = "1234"
        };

        var user = new ApplicationUser
        {
            Email = "test@test.com",
            EmailConfirmed = false
        };

        _cache.Setup(x =>
                x.GetAsync<EmailOtpCacheModel>(It.IsAny<string>()))
            .ReturnsAsync(cache);

        _userManager.Setup(x =>
                x.FindByEmailAsync(user.Email))
            .ReturnsAsync(user);

        _userManager.Setup(x =>
                x.UpdateAsync(user))
            .ReturnsAsync(IdentityResult.Success);

        var sut = CreateSut();

        var result = await sut.ConfirmAsync(
            new ConfirmEmailCodeDto
            {
                Email = user.Email,
                Otp = "1234"
            });

        Assert.True(result.IsConfirmed);
        Assert.Empty(result.Errors);
        Assert.True(user.EmailConfirmed);

        _cache.Verify(x =>
                x.RemoveAsync(It.IsAny<string>()),
            Times.Once);
    }

}