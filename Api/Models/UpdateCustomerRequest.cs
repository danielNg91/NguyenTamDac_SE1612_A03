using Api.Mappings;
using BusinessObjects;
using System.ComponentModel.DataAnnotations;

namespace Api.Models;

public class UpdateCustomerRequest : IMapTo<AspNetUser>
{
    public string? Password { get; set; }
    
    [Phone]
    public string? Phonenumber { get; set; }
    
    public string? FirstName { get; set; }
    
    public string? LastName { get; set; }
}
