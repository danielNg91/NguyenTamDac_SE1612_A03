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
    private readonly IRepository<AspNetUser> _userRepository;
    private readonly UserManager<AspNetUser> _userManager;

    public AuthController(
        IOptions<AppSettings> appSettings,
        IRepository<AspNetUser> userRepository,
        UserManager<AspNetUser> userManager
    ) {
        _appSettings = appSettings.Value;
        _userRepository = userRepository;
        _userManager = userManager;
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

            return Ok(new {
                token = new JwtSecurityTokenHandler().WriteToken(token),
                expiration = token.ValidTo
            });
        }
        return Unauthorized();
    }

    private async Task<AspNetUser> ValidateNormalUser(LoginRequest credentials) {
        var user = await _userRepository.FoundOrThrow(
            u => u.Email.Equals(credentials.Email),
            new BadRequestException("User not exist")
        );
        var passwordHasher = new PasswordHasher<AspNetUser>();
        if (passwordHasher.VerifyHashedPassword(user, user.PasswordHash, credentials.Password) == PasswordVerificationResult.Success) {
            return user;
        }
        throw new BadRequestException("Incorrect username or password");
    }

    [HttpPost]
    [Route("register")]
    public async Task<IActionResult> Register([FromBody] RegisterAccountRequest req) {
        await ValidateRegisterFields(req);
        var user = Mapper.Map(req, new AspNetUser());
        user.SecurityStamp = Guid.NewGuid().ToString();
        var result = await _userManager.CreateAsync(user, req.Password);
        if (!result.Succeeded)
            throw new BadRequestException("User creation failed! Please check user details and try again.");

        return Ok();
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

    private LoginResponse ToLoginResponse(AspNetUser user, string token) {
        var resp = Mapper.Map(user, new LoginResponse());
        resp.Token = token;
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
