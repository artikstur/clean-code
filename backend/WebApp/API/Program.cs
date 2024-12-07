using API.Extensions;
using Application.Interfaces.Auth;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Application.Services;
using Core.Enums;
using Infrastructure.Auth;
using Microsoft.EntityFrameworkCore;
using Persistence;
using Persistence.Repositories;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);

builder.Configuration
    .AddJsonFile("appsettings.Secrets.json", optional: true, reloadOnChange: true);

var configuration = builder.Configuration;
var services = builder.Services;

services.AddControllers();
services.AddEndpointsApiExplorer();
services.AddSwaggerGen();

services.Configure<JwtOptions>(configuration.GetSection(nameof(JwtOptions)));
services.Configure<MinIoRequirement>(configuration.GetSection("Minio"));
services.Configure<AuthorizationOptions>(configuration.GetSection(nameof(AuthorizationOptions)));
services.AddApiAuthentication(configuration);

services.AddDbContext<WebDbContext>(options =>
{
    options.UseNpgsql(configuration.GetConnectionString(nameof(WebDbContext)));
});

services.AddScoped<IJwtProvider, JwtProvider>();
services.AddScoped<IPasswordHasher, PasswordHasher>();

services.AddScoped<IUsersRepository, UsersRepository>();
services.AddScoped<IDocumentsRepository, DocumentsRepository>();
services.AddScoped<IDocumentsAccessRepository, DocumentsAccessRepository>();

services.AddScoped<IUsersService, UsersService>();
services.AddScoped<IDocumentsService, DocumentsService>();
services.AddScoped<IMdService, MdService>();
services.AddScoped<IDocumentsAccessService, DocumentsAccessService>();

services.AddScoped<ErrorResponseFactory>();
services.AddAutoMapper(typeof(DataBaseMappings));

services.AddSingleton<MinioService>();

services.AddMdProcessor();

services.AddValidators();

services.AddFilters();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();

public partial class Program
{
}