using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
    .AddJsonFile("ocelot.json", optional: false, reloadOnChange: true)
    .AddJsonFile($"ocelot.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true);

var jwt = builder.Configuration.GetSection("Jwt");
var secret = jwt["Clave"] ?? throw new InvalidOperationException("Falta Jwt:Clave en secretos o variables de entorno.");
if (Encoding.UTF8.GetByteCount(secret) < 32)
    throw new InvalidOperationException("Jwt:Clave debe tener al menos 32 bytes.");

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = "Bearer";
        options.DefaultChallengeScheme = "Bearer";
    })
    .AddJwtBearer("Bearer", options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt["Emisor"],
            ValidateAudience = true,
            ValidAudience = jwt["Audiencia"],
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
            RoleClaimType = "rol",
            ClockSkew = TimeSpan.FromSeconds(30)
        };
    });
builder.Services.AddAuthorization();
builder.Services.AddOcelot(builder.Configuration);

var app = builder.Build();
app.UseAuthentication();
app.UseAuthorization();
await app.UseOcelot();

public partial class Program { }
