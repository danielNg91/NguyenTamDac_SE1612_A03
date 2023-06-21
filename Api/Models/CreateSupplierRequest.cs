using Api.Mappings;
using BusinessObjects;
using System.ComponentModel.DataAnnotations;

namespace Api.Models;

public class CreateSupplierRequest : IMapTo<Supplier> {
    [Required]
    public string SupplierName { get; set; }
    
    [Required]
    public string SupplierAddress { get; set; }

    [Required, Phone]
    public string Telephone { get; set; }
}
