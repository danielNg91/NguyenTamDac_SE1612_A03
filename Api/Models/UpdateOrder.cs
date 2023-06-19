using Api.Mappings;
using BusinessObjects;

namespace Api.Models;

public class UpdateOrder : IMapTo<Order>
{
    public DateTime? OrderDate { get; set; }
    
    public DateTime? ShippedDate { get; set; }
    
    public decimal? Total { get; set; }
    
    public string? OrderStatus { get; set; }
}
