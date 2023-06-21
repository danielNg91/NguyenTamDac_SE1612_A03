using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

#nullable disable

namespace BusinessObjects;

public partial class AspNetUser : IdentityUser<int> {
    public AspNetUser() {
        Orders = new HashSet<Order>();
    }

    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public override int Id { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public virtual ICollection<Order> Orders { get; set; }
}
