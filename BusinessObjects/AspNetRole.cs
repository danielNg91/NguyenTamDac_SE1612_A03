using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessObjects;
public class AspNetRole : IdentityRole<int> {
    public AspNetRole() : base() {}
    public AspNetRole(string roleName) : base(roleName) { }
}
