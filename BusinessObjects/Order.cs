using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

#nullable disable

namespace BusinessObjects;

public partial class Order
{
    public Order()
    {
        OrderDetails = new HashSet<OrderDetail>();
    }

    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int OrderId { get; set; }
    public int? CustomerId { get; set; }
    public DateTime OrderDate { get; set; }
    public DateTime? ShippedDate { get; set; }
    public string Freight  { get; set; }
    public decimal? Total { get; set; }
    public string OrderStatus { get; set; }

    public virtual AspNetUser Customer { get; set; }
    public virtual ICollection<OrderDetail> OrderDetails { get; set; }
}
