using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

#nullable disable

namespace BusinessObjects;

public partial class Category
{
    public Category()
    {
        FlowerBouquets = new HashSet<FlowerBouquet>();
    }

    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int CategoryId { get; set; }
    public string CategoryName { get; set; }
    public string CategoryDescription { get; set; }

    public virtual ICollection<FlowerBouquet> FlowerBouquets { get; set; }
}
