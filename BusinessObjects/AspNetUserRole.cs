using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BusinessObjects;

[PrimaryKey(nameof(UserId), nameof(RoleId))]
public class AspNetUserRole
{
    [Key, Column(Order = 0)]
    public int UserId { get; set; }
    public virtual AspNetUser User { get; set; }
    
    [Key, Column(Order = 1)]
    public int RoleId { get; set; }
    public virtual AspNetRole Role { get; set; }
}
