using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

#nullable disable

namespace BusinessObjects;

public partial class Supplier
{
    public Supplier()
    {
        FlowerBouquets = new HashSet<FlowerBouquet>();
    }

    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int SupplierId { get; set; }
    public string SupplierName { get; set; }
    public string SupplierAddress { get; set; }
    public string Telephone { get; set; }

    public virtual ICollection<FlowerBouquet> FlowerBouquets { get; set; }
}
