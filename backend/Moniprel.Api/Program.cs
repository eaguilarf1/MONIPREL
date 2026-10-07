using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;
using Moniprel.Api.Datos;
using Moniprel.Api.Interfaces;
using Moniprel.Api.Servicios;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(opciones =>
{
    opciones.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "MONIPREL API",
        Version = "v1"
    });

    opciones.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Ingrese el token JWT."
    });

    opciones.AddSecurityRequirement(documento =>
        new OpenApiSecurityRequirement
        {
            [new OpenApiSecuritySchemeReference("Bearer", documento)] = []
        });
});

var cadenaConexion =
    builder.Configuration.GetConnectionString("PostgreSQL")
    ?? throw new InvalidOperationException(
        "No se encontró la cadena de conexión 'PostgreSQL'.");

builder.Services.AddDbContext<ContextoMoniprel>(opciones =>
    opciones.UseNpgsql(cadenaConexion));

var claveJwt = builder.Configuration["Jwt:Clave"]
    ?? throw new InvalidOperationException(
        "No se encontró la configuración Jwt:Clave.");

var emisorJwt = builder.Configuration["Jwt:Emisor"]
    ?? throw new InvalidOperationException(
        "No se encontró la configuración Jwt:Emisor.");

var audienciaJwt = builder.Configuration["Jwt:Audiencia"]
    ?? throw new InvalidOperationException(
        "No se encontró la configuración Jwt:Audiencia.");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opciones =>
    {
        opciones.TokenValidationParameters =
            new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,

                ValidIssuer = emisorJwt,
                ValidAudience = audienciaJwt,

                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(claveJwt)),

                ClockSkew = TimeSpan.Zero
            };
    });

builder.Services.AddAuthorization();

builder.Services.AddScoped<IServicioToken, ServicioToken>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

await InicializadorDatos.InicializarAsync(
    app.Services,
    app.Configuration);

app.Run();