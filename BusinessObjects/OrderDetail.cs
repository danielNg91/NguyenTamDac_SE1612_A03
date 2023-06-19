using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

#nullable disable

namespace BusinessObjects;

[PrimaryKey(nameof(OrderId), nameof(FlowerBouquetId))]
public partial class OrderDetail
{
    [Key, Column(Order = 0)]
    public int OrderId { get; set; }
    
    [Key, Column(Order = 1)]
    public int FlowerBouquetId { get; set; }
    public decimal UnitPrice { get; set; }
    public int Quantity { get; set; }
    public double Discount { get; set; }

    public virtual FlowerBouquet FlowerBouquet { get; set; }
    [JsonIgnore]
    public virtual Order Order { get; set; }
}
