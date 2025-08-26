using System.Runtime.Serialization;

namespace Ecommerce.Domain;

public enum ProductStatus
{
    [EnumMember(Value = "Producto Activo")]
    Active,
    [EnumMember(Value = "Producto Inactvo")]
    Inactive
}