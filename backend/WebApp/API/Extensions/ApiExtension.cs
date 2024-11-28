using System.Text;
using API.Contracts.Requests;
using API.Validation;
using Application.Interfaces.Auth;
using Application.Services;
using Core.Enums;
using FluentValidation;
using Infrastructure.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;


namespace API.Extensions;

public static class ApiExtensions
{
    public static void AddApiAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection(nameof(JwtOptions)));

        var jwtOptions = configuration.GetSection(nameof(JwtOptions)).Get<JwtOptions>();
        
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
            {
                options.TokenValidationParameters = new()
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtOptions!.SecretKey))
                };

                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        context.Token = context.Request.Cookies["tasty-cookies"];

                        return Task.CompletedTask;
                    }
                };
            });

        services.AddScoped<IPermissionService, PermissionService>();
        services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();

        services.AddAuthorization();
    }

    public static void AddAuthorizationPolicy(this IServiceCollection services, string policyName,
        Permission[] permissions)
    {
        services.AddAuthorization(options =>
        {
            options.AddPolicy(policyName, policy =>
                policy.Requirements.Add(new PermissionRequirement(permissions)));
        });
    }
    
    public static void AddValidators(this IServiceCollection services)
    {
        services.AddScoped<IValidator<CreateUserRequest>, CreateUserRequestValidator>();
    }
}