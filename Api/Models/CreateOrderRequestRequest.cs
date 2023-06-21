using Api.Mappings;
using BusinessObjects;
using System.ComponentModel.DataAnnotations;

namespace Api.Models;

public class CreateOrderRequestRequest : IMapTo<Order>
{
    [Required]
    public DateTime OrderDate { get; set; }

    public DateTime? ShippedDate { get; set; }
    
    public string OrderStatus { get; set; }
    public decimal? Total { get; set; }
}
