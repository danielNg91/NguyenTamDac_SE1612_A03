using Api.Mappings;
using BusinessObjects;
using System.ComponentModel.DataAnnotations;

namespace Api.Models;

public class UpdateOrderDetail : IMapTo<OrderDetail>
{
    public decimal? UnitPrice { get; set; }
    public int? Quantity { get; set; }
    public double? Discount { get; set; }
}
