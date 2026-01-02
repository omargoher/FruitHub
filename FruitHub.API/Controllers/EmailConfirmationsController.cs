using FruitHub.ApplicationCore.DTOs.Auth.EmailVerification;
using FruitHub.ApplicationCore.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;

namespace FruitHub.API.Controllers;

[ApiController]
[Route("api/email-confirmations")]
public class EmailConfirmationsController : ControllerBase
{
   private readonly IEmailConfirmationService _emailConfirmationService;

   public EmailConfirmationsController(IEmailConfirmationService emailConfirmationService)
   {
      _emailConfirmationService = emailConfirmationService;
   }

   [HttpPost]
   public async Task<IActionResult> SendAsync(SendEmailConfirmationCodeDto dto)
   {
      await _emailConfirmationService.SendAsync(dto);
      // I Choose 202 becuse this is async operation 
      return Accepted();
   }
 
   [HttpPut]
   public async Task<IActionResult> ConfirmAsync(ConfirmEmailCodeDto dto)
   {
      await _emailConfirmationService.ConfirmAsync(dto);
      return NoContent();
   }
}