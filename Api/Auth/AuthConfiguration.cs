using BusinessObjects;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Repository;
using System.Text;

namespace Api.Auth;

public static class AuthConfiguration {
    public static IServiceCollection AddAppAuthentication(this IServiceCollection services) {
        services.Configure<IdentityOptions>(opts => {
            opts.Lockout.AllowedForNewUsers = true;
            opts.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
            opts.Lockout.MaxFailedAccessAttempts = 5;
            opts.Password.RequireDigit = false;
            opts.Password.RequireNonAlphanumeric = false;
            opts.Password.RequireUppercase = false;
            opts.Password.RequireLowercase = false;
        });
        services.AddIdentity<AspNetUser, AspNetRole>()
            .AddEntityFrameworkStores<FUFlowerBouquetManagementContext>()
            .AddDefaultTokenProviders();
        
        services.AddAuthentication(options => {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options => {
            var appSettings = services.BuildServiceProvider().GetService<IOptions<AppSettings>>().Value;
            options.SaveToken = true;
            options.RequireHttpsMetadata = false;
            options.TokenValidationParameters = new TokenValidationParameters() {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidAudience = appSettings.JWTOptions.ValidAudience,
                ValidIssuer = appSettings.JWTOptions.ValidIssuer,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(appSettings.JWTOptions.Secret))
            };
        });
        return services;
    }

    public static IServiceCollection AddAppAuthorization(this IServiceCollection services) {
        services.AddAuthorization(options => {
            options.AddPolicy(PolicyName.ADMIN,
                policy => policy.RequireRole(PolicyName.ADMIN));
            options.AddPolicy(PolicyName.CUSTOMER,
                policy => policy.RequireRole(PolicyName.CUSTOMER));
        });
        return services;
    }
}
