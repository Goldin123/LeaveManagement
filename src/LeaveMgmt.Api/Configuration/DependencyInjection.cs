using FastEndpoints;
using FastEndpoints.Swagger;
using LeaveMgmt.Api.Auth;
using LeaveMgmt.Application;
using LeaveMgmt.Application.Abstractions.Identity;
using LeaveMgmt.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.IdentityModel.Tokens;
using NJsonSchema;
using NJsonSchema.Generation; 
using NJsonSchema.Generation.TypeMappers;
using System.Text;

namespace LeaveMgmt.Api.Configuration;

public static class DependencyInjection
{
    public static IServiceCollection AddApi(this IServiceCollection services, IConfiguration config)
    {
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUser, CurrentUser>();

        services.AddInfrastructure(config);
        services.AddApplication();

        // JWT
        var key = config["Jwt:Key"] ?? "dev-only-key-change-me";
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(o =>
                {
                    o.TokenValidationParameters = new()
                    {
                        ValidateIssuer = false,
                        ValidateAudience = false,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
                        ClockSkew = TimeSpan.FromMinutes(1)
                    };
                });
        services.AddAuthorization();

        services.AddFastEndpoints();

        // Older API:
        services.AddSwaggerDocument(s =>
        {
            // DateOnly mapping to prevent generator 500
            s.SchemaSettings.TypeMappers.Add(new PrimitiveTypeMapper(typeof(DateOnly), schema =>
            {
                schema.Type = JsonObjectType.String;
                schema.Format = "date";
            }));
            s.SchemaSettings.TypeMappers.Add(new PrimitiveTypeMapper(typeof(DateOnly?), schema =>
            {
                schema.Type = JsonObjectType.String;
                //schema.IsNullable = true;
                schema.Format = "date";
            }));

            s.Title = "LeaveMgmt API";
            s.Version = "v1";
        });

        return services;
    }

    public static WebApplication UseApi(this WebApplication app)
    {
        app.UseAuthentication();
        app.UseAuthorization();

        app.UseFastEndpoints(c => c.Endpoints.RoutePrefix = "api");

        // Older middleware pair:
        app.UseOpenApi();
        app.UseSwaggerUi(s => s.ConfigureDefaults()); // UI at /swagger

        return app;
    }
}
