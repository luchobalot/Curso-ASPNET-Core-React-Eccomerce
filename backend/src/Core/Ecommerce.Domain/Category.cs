using System.ComponentModel.DataAnnotations.Schema;

namespace Ecommerce.Domain.Common;

public class Category : BaseDomainModel
{
    [Column(TypeName = "nvarchar(100)")]
    public string? Nombre { get; set; }
    public virtual ICollection<Product>? Products { get; set; }
}