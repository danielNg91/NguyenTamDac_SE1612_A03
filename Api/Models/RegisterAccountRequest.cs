using Api.Mappings;
using BusinessObjects;
using System.ComponentModel.DataAnnotations;

namespace Api.Models;

public class RegisterAccountRequest : IMapTo<AspNetUser>
{
    [Required, EmailAddress, MaxLength(100)]
    public string Email { get; set; }

    [Required, MaxLength(180)]
    public string UserName { get; set; }

    [Required, MaxLength(30)]
    public string Password { get; set; }

    [Required, Phone]
    public string Phonenumber { get; set; }

    [Required]
    public string FirstName { get; set; }

    [Required]
    public string LastName { get; set; }

}
