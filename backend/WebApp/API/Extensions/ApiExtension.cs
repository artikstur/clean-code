using System.Text;
using API.Contracts.Requests;
using API.Filters;
using API.Validation;
using FluentValidation;
using Infrastructure.Auth;
using MarkdownRenderer;
using MarkdownRenderer.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
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

        services.AddAuthorization();
    }

    public static void AddValidators(this IServiceCollection services)
    {
        services.AddScoped<IValidator<CreateUserRequest>, CreateUserRequestValidator>();
    }

    public static void AddMdProcessor(this IServiceCollection services)
    {
        services.AddSingleton<ITokensParser, TokensParser>();
        services.AddSingleton<IMarkdownConverter, MarkdownConverter>();
    }

    // можно использовать НЕ сервис фильтры
    public static void AddFilters(this IServiceCollection services)
    {
        services.AddScoped<ValidateDocumentAuthorFilter>();
        services.AddScoped<DocumentExistsFilter>();
        services.AddScoped<ValidateDocumentEditorsFilter>();
        services.AddScoped<ValidateDocumentReadersFilter>();
        services.AddScoped<ValidateAuthorOrEditorFilter>();
        services.AddScoped<ValidateAuthorOrReaderFilter>();
    }
}