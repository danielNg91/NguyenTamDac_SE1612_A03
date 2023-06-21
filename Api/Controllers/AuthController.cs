using Api.Models;
using Api.Utils;
using Application.Exceptions;
using BusinessObjects;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
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
    private readonly AppSettings _appSettings;
    private readonly UserManager<AspNetUser> _userManager;
    private readonly RoleManager<AspNetRole> _roleManager;

    public AuthController(
        IOptions<AppSettings> appSettings,
        UserManager<AspNetUser> userManager,
        RoleManager<AspNetRole> roleManager
    ) {
        _appSettings = appSettings.Value;
        _userManager = userManager;
        _roleManager = roleManager;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest credentials) {
        //if (credentials.Email.Equals(_appSettings.AdminAccount.Email) &&
        //    credentials.Password.Equals(_appSettings.AdminAccount.Password)) {
        //    var admin = new AspNetUser {
        //        CustomerId = int.MaxValue,
        //        Email = credentials.Email
        //    };
        //    var adminToken = GenerateJwtToken(admin, PolicyName.ADMIN);
        //    return Ok(ToLoginResponse(admin, adminToken));
        //}

        //var user = await ValidateNormalUser(credentials);
        //var userToken = GenerateJwtToken(user, PolicyName.CUSTOMER);
        //return Ok(ToLoginResponse(user, userToken));

        var user = await _userManager.FindByEmailAsync(credentials.Email);
        if (user != null && await _userManager.CheckPasswordAsync(user, credentials.Password)) {
            var userRoles = await _userManager.GetRolesAsync(user);

            var authClaims = new List<Claim>
            {
                    new Claim(ClaimTypes.Name, user.UserName),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                };

            foreach (var userRole in userRoles) {
                authClaims.Add(new Claim(ClaimTypes.Role, userRole));
            }

            var token = GetToken(authClaims);
            return Ok(ToLoginResponse(user, new JwtSecurityTokenHandler().WriteToken(token), token.ValidTo));
        }
        return Unauthorized();
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
        await AddUserRoles(user);
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

    private LoginResponse ToLoginResponse(AspNetUser user, string token, DateTime expiration) {
        var resp = Mapper.Map(user, new LoginResponse());
        resp.Token = token;
        resp.Expiration = expiration;
        return resp;
    }

    private JwtSecurityToken GetToken(List<Claim> authClaims) {
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
