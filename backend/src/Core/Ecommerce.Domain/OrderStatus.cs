using System.Runtime.Serialization;

namespace Ecommerce.Domain;

public enum OrderStatus
{
    [EnumMember(Value = "Pendiente")]
    Pending,
    [EnumMember(Value = "El pago fue recibido")]
    Completed,
    [EnumMember(Value = "El pago fue rechazado")]
    Error,
    [EnumMember(Value = "El pago fue enviado")]
    Enviado

}