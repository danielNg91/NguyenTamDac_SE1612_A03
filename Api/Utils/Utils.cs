using Microsoft.OpenApi.Models;

namespace Api.Utils;

public static class Utils {
    public static IServiceCollection AddSwagger(this IServiceCollection services) {
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
        return services;
    }
}
