using Newtonsoft.Json;

namespace Ecommerce.Api.Errors;

public class CodeErrorResponse : CodeErrorResponse
{
    [JsonProperty(PropertyName = "details")]
    public string? Details { get; set; }
    public CodeErrorResponse(int statusCode, string[]? message = nullm, string? details = null) 
        : base(statusCode, message)
    {

    }
}