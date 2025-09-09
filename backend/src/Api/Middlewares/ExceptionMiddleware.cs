using System.Net;
using Newtonsoft.Json;
using Ecommerce.Api.Errors;

namespace Ecommerce.Api.Middlewares;

public class ExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionMiddleware> _logger;

    public ExceptionMiddleware(RequestDelegate next, ILogger<ExceptionMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, ex.Message);
            context.Response.ContentType = "application/json";
            var statusCode = (int)HttpStatusCode.InternalServerError;
            var result = string.Empty;

            switch (ex)
            {
                case NotFoundException notFoundException:
                    statusCode = (int)HttpStatusCode.NotFound;
                    result = JsonConvert.SerializeObject(new CodeErrorException(statusCode, new[] { ex.Message }, ex.StackTrace));
                    break;

                case FluentValidation.ValidationException validationException:
                    statusCode = (int)HttpStatusCode.BadRequest;
                    var errors = validationException.Errors.Select(err => err.ErrorMessage).ToArray();
                    result = JsonConvert.SerializeObject(new CodeErrorException(statusCode, errors, ex.StackTrace));
                    break;

                case BadRequestException badRequestException:
                    statusCode = (int)HttpStatusCode.BadRequest;
                    result = JsonConvert.SerializeObject(new CodeErrorException(statusCode, new[] { ex.Message }, ex.StackTrace));
                    break;

                default:
                    statusCode = (int)HttpStatusCode.InternalServerError;
                    result = JsonConvert.SerializeObject(new CodeErrorException(statusCode, new[] { ex.Message }, ex.StackTrace));
                    break;
            }

            context.Response.StatusCode = statusCode;
            await context.Response.WriteAsync(result);
        }
    }
}
