using System.ComponentModel.DataAnnotations.Schema;

namespace Ecommerce.Domain.Common;

public class Order : BaseDomainModel
{
    public Order() 
    {
        OrderItems = new List<OrderItem>();
    }

    public Order(
        string compradorNombre,
        string compradorEmail,
        OrderAddress orderAddress,
        decimal subtotal,
        decimal total,
        decimal impuesto,
        decimal precioEnvio
    )
    {
        CompradorNombre = compradorNombre;
        CompradorUserName = compradorEmail;
        OrderAddress = orderAddress;
        Subtotal = subtotal;
        Total = total;
        Impuesto = impuesto;
        PrecioEnvio = precioEnvio;
        OrderItems = new List<OrderItem>();
    }

    public string? CompradorNombre { get; set; }
    public string? CompradorUserName { get; set; }
    public OrderAddress? OrderAddress { get; set; }
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    [Column(TypeName = "decimal(10,2)")]
    public decimal Subtotal { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    [Column(TypeName = "decimal(10,2)")]
    public decimal Total { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal Impuesto { get; set; }

    [Column(TypeName = "decimal(10,2)")]
    public decimal PrecioEnvio { get; set; }
    public string? PaymentIntentId { get; set; }
    public string? ClientSecret { get; set; }
    public string? StripeApiKey { get; set; }
}