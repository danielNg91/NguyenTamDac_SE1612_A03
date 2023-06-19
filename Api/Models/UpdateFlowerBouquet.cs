using Api.Mappings;
using BusinessObjects;
using System.ComponentModel.DataAnnotations;

namespace Api.Models;

public class UpdateFlowerBouquet : IMapTo<FlowerBouquet>
{
    public int? CategoryId { get; set; }
    
    [MaxLength(40)]
    public string? FlowerBouquetName { get; set; }

    [MaxLength(220)]
    public string? Description { get; set; }

    [Range(0, (double)decimal.MaxValue)]
    public decimal? UnitPrice { get; set; }

    [Range(0, int.MaxValue)]
    public int? UnitsInStock { get; set; }

    [Range(0, 255)]
    public byte? FlowerBouquetStatus { get; set; }
    public int? SupplierId { get; set; }

}
