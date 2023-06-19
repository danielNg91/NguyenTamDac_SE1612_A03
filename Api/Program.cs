using Api;
using Api.Auth;
using Api.Utils;
using AutoWrapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Repository;
using System.Reflection;

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
    services.AddAutoMapper(Assembly.GetExecutingAssembly());
    services.AddDbContext<FUFlowerBouquetManagementContext>(options => {
        {
            var settings = services.BuildServiceProvider().GetService<IOptions<AppSettings>>();
            Console.WriteLine(settings);
            options.UseSqlServer(settings.Value.ConnectionStrings.FUFlowerBouquetManagement);
        }
    });

    services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
    services.AddTransient<JwtMiddleware>();
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
    app.UseMiddleware<JwtMiddleware>();
    app.UseApiResponseAndExceptionWrapper(new AutoWrapperOptions { IsApiOnly = false, ShowIsErrorFlagForSuccessfulResponse = true });
    app.UseHttpsRedirection();
    app.UseAuthentication();
    app.UseAuthorization();
    app.MapControllers();
    app.Run();
}

