using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

#nullable disable

namespace BusinessObjects;

public partial class FlowerBouquet
{
    public FlowerBouquet()
    {
        OrderDetails = new HashSet<OrderDetail>();
    }

    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int FlowerBouquetId { get; set; }
    public int CategoryId { get; set; }
    public string FlowerBouquetName { get; set; }
    public string Description { get; set; }
    public decimal UnitPrice { get; set; }
    public int UnitsInStock { get; set; }
    public byte? FlowerBouquetStatus { get; set; }
    public int? SupplierId { get; set; }

    public virtual Category Category { get; set; }
    public virtual Supplier Supplier { get; set; }
    [JsonIgnore]
    public virtual ICollection<OrderDetail> OrderDetails { get; set; }
}
