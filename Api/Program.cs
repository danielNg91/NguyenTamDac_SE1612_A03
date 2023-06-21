using Api;
using Api.Utils;
using AutoWrapper;
using BusinessObjects;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Repository;
using System.Reflection;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
{
    var services = builder.Services;
    var configuration = builder.Configuration;

    services.AddControllers(options => {
        options.Filters.Add<ValidateModelStateFilter>();
    });
    services.AddEndpointsApiExplorer();
    services.Configure<AppSettings>(configuration.GetSection(nameof(AppSettings)));
    services.Configure<IdentityOptions>(opts => {
        opts.Lockout.AllowedForNewUsers = true;
        opts.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
        opts.Lockout.MaxFailedAccessAttempts = 3;
        opts.Password.RequireDigit = false;
        opts.Password.RequireNonAlphanumeric = false;
        opts.Password.RequireUppercase = false;
        opts.Password.RequireLowercase = false;
    });

    services.AddAutoMapper(Assembly.GetExecutingAssembly());
    services.AddDbContext<FUFlowerBouquetManagementContext>(options => {
        var appSettings = services.BuildServiceProvider().GetService<IOptions<AppSettings>>().Value;
        Console.WriteLine(appSettings);
        options.UseSqlServer(appSettings.ConnectionStrings.FUFlowerBouquetManagement);
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
    services.AddAuthorization(options => {
        options.AddPolicy(PolicyName.ADMIN,
            policy => policy.RequireRole(PolicyName.ADMIN));
        options.AddPolicy(PolicyName.CUSTOMER,
            policy => policy.RequireRole(PolicyName.CUSTOMER));
    });

    services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
    services.AddTransient<AuthenticationEvent>();

    services.AddCors(options => {
        options.AddPolicy("CorsPolicy", builder => {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
    });
    services.AddSwaggerGen(c => {
        c.SwaggerDoc("v1", new OpenApiInfo {
            Title = "FU Flower Bouquet Management",
            Version = "v1"
        });
        OpenApiSecurityScheme securityDefinition = new() {
            Name = "Bearer",
            BearerFormat = "JWT",
            Scheme = "bearer",
            Description = "Specify the authorization token.",
            In = ParameterLocation.Header,
            Type = SecuritySchemeType.Http,
        };
        c.AddSecurityDefinition("jwt_auth", securityDefinition);
        OpenApiSecurityScheme securityScheme = new OpenApiSecurityScheme() {
            Reference = new OpenApiReference() {
                Id = "jwt_auth",
                Type = ReferenceType.SecurityScheme
            }
        };
        OpenApiSecurityRequirement securityRequirements = new() {
            {
                securityScheme,
                new string[] { }
            },
        };
        c.AddSecurityRequirement(securityRequirements);
    });
}

// Build application
{
    var app = builder.Build();
    if (app.Environment.IsDevelopment()) {
        app.UseSwagger();
        app.UseSwaggerUI();
    }
    app.UseCors("CorsPolicy");
    app.UseApiResponseAndExceptionWrapper(
        new AutoWrapperOptions {
            IsApiOnly = false, ShowIsErrorFlagForSuccessfulResponse = true
        }
    );
    app.UseHttpsRedirection();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();
    app.Run();
}

