using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects;
public class AspNetRoleClaim
{
    [Key, DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int Id { get; set; }
    public int RoleId { get; set; }
    public virtual AspNetRole Role { get; set; }
    public string ClaimType { get; set; }
    public string ClaimVale { get; set; }
}
