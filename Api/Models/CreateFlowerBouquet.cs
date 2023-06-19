using Api.Mappings;
using BusinessObjects;
using System.ComponentModel.DataAnnotations;

namespace Api.Models;

public class CreateFlowerBouquet : IMapTo<FlowerBouquet>
{
    [Required]
    public int CategoryId { get; set; }

    [Required, MaxLength(40)]
    public string FlowerBouquetName { get; set; }

    [Required, MaxLength(220)]
    public string Description { get; set; }

    [Required]
    [Range(0, (double)decimal.MaxValue)]
    public decimal UnitPrice { get; set; }

    [Required]
    [Range(0, int.MaxValue)]
    public int UnitsInStock { get; set; }
    
    [Range(0, 255)]
    public byte? FlowerBouquetStatus { get; set; }
    public int? SupplierId { get; set; }

}
