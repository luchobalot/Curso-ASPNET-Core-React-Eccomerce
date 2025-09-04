using Ecommerce.Application.Contracts.Infraestructure;
using Ecommerce.Application.Models.Email;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class TestController : ControllerBase
{
    private readonly IEmailService _emailService;
    
    public TestController(IEmailService emailService)
    {
        _emailService = emailService;
    }

    [AllowAnonymous]
    [HttpGet]
    public async Task<IActionResult> SendEmail()
    {
        var message = new EmailMessage
        { 
            To = "balotluciano02@gmail.com",
            Body = "Prueba",
            Subject = "Cambiar el password",
        };
        
        var result = await _emailService.SendEmailAsync(message, "mi_token");
        return result ? Ok() : BadRequest();
    }
}