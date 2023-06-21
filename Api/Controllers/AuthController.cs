using Api.Auth;
using Api.Models;
using Application.Exceptions;
using BusinessObjects;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Api.Controllers;


[Route("api/v1/auth")]
public class AuthController : BaseController {
    private readonly AppSettings _appSettings;
    private readonly UserManager<AspNetUser> _userManager;
    private readonly RoleManager<AspNetRole> _roleManager;
    private readonly SignInManager<AspNetUser> _signInManager;

    public AuthController(
        IOptions<AppSettings> appSettings,
        UserManager<AspNetUser> userManager,
        RoleManager<AspNetRole> roleManager,
        SignInManager<AspNetUser> signInManager
    ) {
        _appSettings = appSettings.Value;
        _userManager = userManager;
        _roleManager = roleManager;
        _signInManager = signInManager;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest credentials) {
        if (
            credentials.Email.Equals(_appSettings.AdminAccount.Email) &&
            credentials.Password.Equals(_appSettings.AdminAccount.Password)
        ) {
            var user = (await _userManager.FindByEmailAsync(credentials.Email));
            if (user == null) {
                var admin = new AspNetUser {
                    Email = credentials.Email,
                    SecurityStamp = Guid.NewGuid().ToString(),
                    UserName = "admin"
                };
                var result = await _userManager.CreateAsync(admin, credentials.Password);
                if (!result.Succeeded)
                    throw new BadRequestException(result);
                await AddUserRoles(admin, PolicyName.ADMIN);
                await AddUserRoles(admin, PolicyName.CUSTOMER);
            }
        }
        return await ToLoginResponse(credentials);
    }

    private async Task<IActionResult> ToLoginResponse(LoginRequest credentials) {
        var token = await GetUserToken(credentials);
        return Ok(new {
            token = new JwtSecurityTokenHandler().WriteToken(token),
            expiration = token.ValidTo
        });
    }

    private async Task<JwtSecurityToken> GetUserToken(LoginRequest credentials) {
        var user = await _userManager.FindByEmailAsync(credentials.Email);
        var result = await _signInManager.PasswordSignInAsync(
            user, 
            credentials.Password, 
            isPersistent: false, 
            lockoutOnFailure: true
        );
        if (!result.Succeeded) {
            if (user == null) {
                throw new UnauthorizedException();
            }
            if (result.IsLockedOut) {
                throw new BadRequestException("This account is lockout");
            }
            await _userManager.AccessFailedAsync(user);
            throw new UnauthorizedException();
        }

        await _userManager.ResetAccessFailedCountAsync(user);
        var userRoles = await _userManager.GetRolesAsync(user);
        var authClaims = new List<Claim> {
            new Claim("id", user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };
        foreach (var userRole in userRoles) {
            authClaims.Add(new Claim(ClaimTypes.Role, userRole));
        }
        var token = GenerateToken(authClaims);
        return token;
    }

    [HttpPost]
    [Route("register")]
    public async Task<IActionResult> Register([FromBody] RegisterAccountRequest req) {
        await ValidateRegisterFields(req);
        var user = Mapper.Map(req, new AspNetUser());
        user.SecurityStamp = Guid.NewGuid().ToString();
        var result = await _userManager.CreateAsync(user, req.Password);
        if (!result.Succeeded)
            throw new BadRequestException(result);
        await AddUserRoles(user, PolicyName.CUSTOMER);
        return Ok();
    }

    private async Task AddUserRoles(AspNetUser user, string role = PolicyName.CUSTOMER) {
        if (!await _roleManager.RoleExistsAsync(PolicyName.ADMIN)) {
            await _roleManager.CreateAsync(new AspNetRole(PolicyName.ADMIN));
        }
        if (!await _roleManager.RoleExistsAsync(PolicyName.CUSTOMER)) {
            await _roleManager.CreateAsync(new AspNetRole(PolicyName.CUSTOMER));
        }
        if (await _roleManager.RoleExistsAsync(role)) {
            await _userManager.AddToRoleAsync(user, role);
        }
    }

    private async Task ValidateRegisterFields(RegisterAccountRequest req) {
        if (req.Email.Equals(_appSettings.AdminAccount.Email)) {
            throw new BadRequestException("Email already existed");
        }

        var isEmailExisted = (await _userManager.FindByEmailAsync(req.Email)) != null;
        if (isEmailExisted) {
            throw new BadRequestException("Email already existed");
        }
    }

    private JwtSecurityToken GenerateToken(List<Claim> authClaims) {
        var authSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_appSettings.JWTOptions.Secret));
        var token = new JwtSecurityToken(
            issuer: _appSettings.JWTOptions.ValidIssuer,
            audience: _appSettings.JWTOptions.ValidAudience,
            expires: DateTime.Now.AddHours(3),
            claims: authClaims,
            signingCredentials: new SigningCredentials(authSigningKey, SecurityAlgorithms.HmacSha256)
        );
        return token;
    }
}
