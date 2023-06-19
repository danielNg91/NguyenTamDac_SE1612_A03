using Api.Mappings;
using BusinessObjects;
using System.ComponentModel.DataAnnotations;

namespace Api.Models;

public class CreateOrder : IMapTo<Order>
{
    [Required, Range(0, int.MaxValue)]
    public int CustomerId { get; set; }

    [Required]
    public DateTime OrderDate { get; set; }

    public DateTime? ShippedDate { get; set; }
    
    public string OrderStatus { get; set; }
    public decimal? Total { get; set; }
}
