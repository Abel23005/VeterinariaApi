using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Propietarios.Api.Models;

var builder = WebApplication.CreateBuilder(args);
var jwt = builder.Configuration.GetSection("Jwt");
var secret = jwt["Clave"] ?? throw new InvalidOperationException("Falta Jwt:Clave en secretos o variables de entorno.");
if (Encoding.UTF8.GetByteCount(secret) < 32)
    throw new InvalidOperationException("Jwt:Clave debe tener al menos 32 bytes.");

builder.Services.AddControllers();
builder.Services.AddSingleton<IPropietarioStore, InMemoryPropietarioStore>();
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => options.TokenValidationParameters = new TokenValidationParameters
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
    });
builder.Services.AddAuthorization();

var app = builder.Build();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();

public partial class Program { }
