using Api.Mappings;
using BusinessObjects;
using System.ComponentModel.DataAnnotations;

namespace Api.Models;

public class UpdateCustomer : IMapTo<AspNetUser>
{
    [MaxLength(180)]
    public string? CustomerName { get; set; }

    [MaxLength(15)]
    public string? City { get; set; }

    [MaxLength(15)]
    public string? Country { get; set; }

    [MaxLength(30)]
    public string? Password { get; set; }

    public DateTime? Birthday { get; set; }
}
