using BCrypt.Net;
using Microsoft.EntityFrameworkCore;
using Moniprel.Api.Entidades;

namespace Moniprel.Api.Datos;

public static class InicializadorDatos
{
    public static async Task InicializarAsync(
        IServiceProvider servicios,
        IConfiguration configuracion)
    {
        using var alcance = servicios.CreateScope();

        var contexto = alcance.ServiceProvider
            .GetRequiredService<ContextoMoniprel>();

        var correo = configuracion["AdministradorInicial:Correo"];
        var contrasena = configuracion["AdministradorInicial:Contrasena"];

        if (string.IsNullOrWhiteSpace(correo) ||
            string.IsNullOrWhiteSpace(contrasena))
        {
            return;
        }

        var existeAdministrador = await contexto.Usuarios
            .AnyAsync(x => x.Correo == correo);

        if (existeAdministrador)
        {
            return;
        }

        var rolAdministrador = await contexto.Roles
            .SingleAsync(x => x.Nombre == "Administrador");

        var administrador = new Usuario
        {
            Nombres = "Administrador",
            Apellidos = "MONIPREL",
            Correo = correo.Trim().ToLowerInvariant(),
            ContrasenaHash = BCrypt.Net.BCrypt.HashPassword(contrasena),
            Activo = true,
            RolId = rolAdministrador.Id,
            FechaCreacion = DateTime.UtcNow
        };

        contexto.Usuarios.Add(administrador);

        await contexto.SaveChangesAsync();
    }
}