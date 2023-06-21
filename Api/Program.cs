using Api;
using Api.Auth;
using Api.Utils;
using AutoWrapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
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
        var appSettings = services.BuildServiceProvider().GetService<IOptions<AppSettings>>().Value;
        Console.WriteLine(appSettings);
        options.UseSqlServer(appSettings.ConnectionStrings.FUFlowerBouquetManagement);
    });
    services.AddAppAuthentication();
    services.AddAppAuthorization();
    services.AddCors(options => {
        options.AddPolicy("CorsPolicy", builder => {
            builder.AllowAnyOrigin()
                   .AllowAnyMethod()
                   .AllowAnyHeader();
        });
    });
    services.AddSwagger();
    services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
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

