using System.ComponentModel.DataAnnotations;

namespace Api.Models;

public class CartFlower
{
    [Required]
    public int FlowerBouquetId { get; set; }

    [Required]
    public int Quantity { get; set; }
}
