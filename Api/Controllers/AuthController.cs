using Api.Models;
using Api.Utils;
using Application.Exceptions;
using BusinessObjects;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Repository;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Api.Controllers;


[Route("api/v1/auth")]
public class AuthController : BaseController {
    private readonly IRepository<AspNetUser> _userRepository;
    private readonly AppSettings _appSettings;

    public AuthController(IOptions<AppSettings> appSettings, IRepository<AspNetUser> userRepository) {
        _appSettings = appSettings.Value;
        _userRepository = userRepository;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest credentials) {
        if (credentials.Email.Equals(_appSettings.AdminAccount.Email) &&
            credentials.Password.Equals(_appSettings.AdminAccount.Password)) {
            var admin = new AspNetUser {
                CustomerId = int.MaxValue,
                Email = credentials.Email
            };
            var adminToken = GenerateJwtToken(admin, PolicyName.ADMIN);
            return Ok(ToLoginResponse(admin, adminToken));
        }

        var user = await _userRepository.FoundOrThrow(
            u => u.Email.Equals(credentials.Email) && u.Password.Equals(credentials.Password),
            new BadRequestException("User not exist"));
        var userToken = GenerateJwtToken(user, PolicyName.CUSTOMER);
        return Ok(ToLoginResponse(user, userToken));
    }

    private async Task SetIdentity(string userId, string email, string role) {
        var claims = new List<Claim>
                    {
                        new Claim("id", userId),
                        new Claim(ClaimTypes.Email, email),
                        new Claim(ClaimTypes.Role, role)
                    };
        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity), new AuthenticationProperties { IsPersistent = true });
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterAccount req) {
        await ValidateRegisterFields(req);
        var user = Mapper.Map(req, new AspNetUser());
        user.CustomerId = await GetUserId();
        await _userRepository.CreateAsync(user);
        return Ok();
    }

    private async Task<int> GetUserId() {
        var user = (await _userRepository.ToListAsync()).OrderByDescending(u => u.CustomerId).FirstOrDefault();
        return user == null ? 1 : (user.CustomerId + 1);
    }

    private async Task ValidateRegisterFields(RegisterAccount req) {
        if (req.Email.Equals(_appSettings.AdminAccount.Email)) {
            throw new BadRequestException("Email already existed");
        }

        var isEmailExisted = (await _userRepository.FirstOrDefaultAsync(u => u.Email == req.Email)) != null;
        if (isEmailExisted) {
            throw new BadRequestException("Email already existed");
        }
    }

    private string GenerateJwtToken(AspNetUser user, string role) {
        var symmetricKey = Encoding.ASCII.GetBytes(_appSettings.Secret);
        var tokenHandler = new JwtSecurityTokenHandler();
        var now = DateTime.UtcNow;
        var claims = new List<Claim> {
            new Claim("id", user.CustomerId.ToString()),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, role)
        };
        var tokenDescriptor = new SecurityTokenDescriptor {
            Subject = new ClaimsIdentity(claims),
            Expires = now.AddDays(7),
            SigningCredentials = new SigningCredentials(
                new SymmetricSecurityKey(symmetricKey),
                SecurityAlgorithms.HmacSha256Signature
            )
        };
        var stoken = tokenHandler.CreateToken(tokenDescriptor);
        var token = tokenHandler.WriteToken(stoken);
        return token;
    }

    private LoginResponse ToLoginResponse(AspNetUser user, string token) {
        var resp = Mapper.Map(user, new LoginResponse());
        resp.Token = token;
        return resp;
    }
}
