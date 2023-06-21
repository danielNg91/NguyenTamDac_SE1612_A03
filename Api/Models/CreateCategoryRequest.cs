using Api.Mappings;
using BusinessObjects;
using System.ComponentModel.DataAnnotations;

namespace Api.Models;

public class CreateCategoryRequest : IMapTo<Category> {
    [Required]
    public string CategoryName { get; set; }
    [Required]
    public string CategoryDescription { get; set; }
}
