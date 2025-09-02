using System.ComponentModel.DataAnnotations.Schema;
using Ecommerce.Domain.Common;

namespace Ecommerce.Domain;

public class OrderItem : BaseDomainModel
{
    public Product? Product { get; set; }

    public Guid ProductId { get; set; }

    [Column(TypeName = "DECIMAL(10,2)")]
    public decimal Precio { get; set; }

    public int Cantidad { get; set; }

    public Order? Order { get; set; }

    public Guid OrderId { get; set; }

    public Guid ProductItemId { get; set; }

    public string? ProductName { get; set; }

    public string? ProductImage { get; set; }
}