using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

#nullable disable

namespace BusinessObjects;

public partial class AspNetUser
{
    public AspNetUser()
    {
        Orders = new HashSet<Order>();
    }

    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int CustomerId { get; set; }
    public string UserName { get; set; }
    public string NormalizedUserName { get; set; }
    public string Email { get; set; }
    public string NormalizedEmail { get; set; }
    public string EmailConfirmed { get; set; }
    public string PasswordHash { get; set; }
    public string SecirityStamp { get; set; }
    public string ConcurrencyStamp { get; set; }
    public string Phonenumber { get; set; }
    public string PhonenumberConfirmed { get; set; }
    public byte TwoFactorEnabled { get; set; }
    public string LockoutEnd { get; set; }
    public byte LockoutEnabled { get; set; }
    public int AccessFailCount { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }

    // TO BE REMOVE
    public string CustomerName { get; set; }
    public string City { get; set; }
    public string Country { get; set; }
    public string Password { get; set; }
    public DateTime? Birthday { get; set; }

    public virtual ICollection<Order> Orders { get; set; }
}
