using FastEndpoints;
using FastEndpoints.Swagger;
using LeaveMgmt.Api.Auth;
using LeaveMgmt.Application;
using LeaveMgmt.Application.Abstractions.Identity;
using LeaveMgmt.Infrastructure;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
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
        services.SwaggerDocument(o =>
        {
            o.DocumentSettings = s =>
            {
                s.Title = "LeaveMgmt API";
                s.Version = "v1";
            };
            o.ShortSchemaNames = true;
        });

        return services;
    }

    public static WebApplication UseApi(this WebApplication app)
    {
        app.UseAuthentication();
        app.UseAuthorization();

        app.UseFastEndpoints(c => c.Endpoints.RoutePrefix = "api");

        app.UseSwaggerGen(); // FastEndpoints.Swagger

        return app;
    }
}
