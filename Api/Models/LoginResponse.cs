using Api.Mappings;
using BusinessObjects;

namespace Api.Models;

public class LoginResponse : IMapFrom<AspNetUser>
{
    public int Id { get; set; }
    public string Email { get; set; }
    public string Role { get; set; }
    public string UserName { get; set; }
    public string Token { get; set; }
}
