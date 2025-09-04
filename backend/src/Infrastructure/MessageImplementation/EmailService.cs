using Ecommerce.Application.Contracts.Infraestructure;
using Ecommerce.Application.Models.Email;
using FluentEmail.Core;
using Microsoft.Extensions.Options;
namespace Ecommerce.Infrastructure.MessageImplementation;
public class EmailService : IEmailService
{
    private readonly IFluentEmail _fluentEmail;
    private readonly EmailFluentSettings _emailFluentSettings;
    public EmailService(IFluentEmail fluentEmail, IOptions<EmailFluentSettings> emailFluentSettings)    
    {
        _fluentEmail = fluentEmail;
        _emailFluentSettings = emailFluentSettings.Value;
    }
    public async Task<bool> SendEmailAsync(EmailMessage email, string token)
    {
        try
        {
            var htmlContent = $"{email.Body} {_emailFluentSettings.BaseUrl}/password/reset/{token}";    
            var result = await _fluentEmail
                .To(email.To)
                .Subject(email.Subject)
                .Body(htmlContent, true)
                .SendAsync();
            return result.Successful;
        }
        catch (Exception)
        {
            return false;
        }
    }
}

