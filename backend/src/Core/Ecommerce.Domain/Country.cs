namespace Ecommerce.Domain.Common;

public class Country : BaseDomainModel
{
    public string? Nombre { get; set; }
    public string? Iso2 { get; set; }
    public string? Iso3 { get; set; }
}