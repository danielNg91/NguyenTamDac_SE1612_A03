using BusinessObjects;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Repository;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Principal;
using System.Text;

namespace Api.Auth;

public class JwtMiddleware : IMiddleware {
    private readonly AppSettings _appSettings;
    private readonly IRepository<AspNetUser> _userRepo;

    public JwtMiddleware(
        IOptions<AppSettings> appSettings,
        IRepository<AspNetUser> userRepo
    ) {
        _appSettings = appSettings.Value;
        _userRepo = userRepo;
    }

    public async Task InvokeAsync(HttpContext context, RequestDelegate next) {
        var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

        if (token != null) {
            await AttachUserToContext(context, token);
        }
        await next(context);
    }

    private async Task AttachUserToContext(HttpContext context, string token) {
        try {
            var tokenHandler = new JwtSecurityTokenHandler();
            var jwtToken = tokenHandler.ReadToken(token) as JwtSecurityToken;

            if (jwtToken != null) {
                var symmetricKey = Encoding.ASCII.GetBytes(_appSettings.Secret);
                var validationParameters = new TokenValidationParameters() {
                    RequireExpirationTime = true,
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    IssuerSigningKey = new SymmetricSecurityKey(symmetricKey)
                };
                SecurityToken securityToken;
                tokenHandler.ValidateToken(token, validationParameters, out securityToken);
                await HandleAdminLogin(context, jwtToken);
            }
        } catch {
            // do nothing ==> AuthorizeAttribute handler
        }
    }

    private async Task HandleAdminLogin(HttpContext context, JwtSecurityToken jwtToken) {
        var email = jwtToken.Claims.First(x => x.Type == "email").Value;
        if (email.Equals(_appSettings.AdminAccount.Email)) {
            var admin = new AspNetUser {
                CustomerId = int.MaxValue,
                Email = email
            };
            context.Items["User"] = admin;
            return;
        }
        var userId = int.Parse(jwtToken.Claims.First(x => x.Type == "id").Value);
        context.Items["User"] = await _userRepo.FirstOrDefaultAsync(u => u.CustomerId == userId);
    }
}
