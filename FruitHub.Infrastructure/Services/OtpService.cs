using System.Security.Cryptography;
using FruitHub.Infrastructure.Interfaces;

namespace FruitHub.Infrastructure.Services;

public class OtpService : IOtpService
{
    public string GenerateOtp()
    {
        return RandomNumberGenerator.GetInt32(100000, 999999).ToString();
    }
}