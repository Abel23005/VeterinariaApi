using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Veterinaria.Datos.Data;
using Veterinaria.Datos.Repositories;
using Veterinaria.Negocio.Mapping;
using Veterinaria.Negocio.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDbContext<VeterinariaContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("Veterinaria")));
builder.Services.AddScoped<IDuenoRepository, DuenoRepository>();
builder.Services.AddScoped<IMascotaRepository, MascotaRepository>();
builder.Services.AddScoped<IDuenoService, DuenoService>();
builder.Services.AddScoped<IMascotaService, MascotaService>();
builder.Services.AddSingleton<IMapper>(sp => new MapperConfiguration(cfg => cfg.AddProfile<MappingProfile>(), sp.GetRequiredService<ILoggerFactory>()).CreateMapper());

var app = builder.Build();
if (app.Environment.IsDevelopment()) { app.UseSwagger(); app.UseSwaggerUI(); }
app.UseHttpsRedirection();
app.MapControllers();
app.Run();

public partial class Program { }
