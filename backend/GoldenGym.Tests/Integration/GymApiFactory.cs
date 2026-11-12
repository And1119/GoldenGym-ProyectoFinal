using GoldenGym.Application.Interfaces;
using GoldenGym.Domain.Entities;
using GoldenGym.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GoldenGym.Tests.Integration;

/// <summary>
/// Levanta la API real en memoria reemplazando MySQL por EF Core InMemory,
/// para probar JWT, roles y respuestas HTTP sin depender de XAMPP.
/// </summary>
public class GymApiFactory : WebApplicationFactory<Program>
{
    private readonly string _nombreBd = "GoldenGymTests_" + Guid.NewGuid();

    public const string Password = "Clave1234";
    public const string CorreoAdmin = "admin@test.com";
    public const string CorreoEntrenador = "entrenador@test.com";
    public const string CorreoEntrenador2 = "entrenador2@test.com";
    public const string CorreoCliente = "cliente@test.com";
    public const string CorreoClienteSinPlan = "sinplan@test.com";
    public const string CorreoInactivo = "inactivo@test.com";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureServices(services =>
        {
            var descriptor = services.Single(d => d.ServiceType == typeof(DbContextOptions<GymDbContext>));
            services.Remove(descriptor);
            services.AddDbContext<GymDbContext>(o => o.UseInMemoryDatabase(_nombreBd));

            using var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<GymDbContext>();
            var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
            Sembrar(db, hasher);
        });
    }

    private static void Sembrar(GymDbContext db, IPasswordHasher hasher)
    {
        db.Database.EnsureCreated();
        if (db.Roles.Any()) return;

        var admin = new Rol { IdRol = 1, Nombre = RolNombres.Administrador };
        var entrenador = new Rol { IdRol = 2, Nombre = RolNombres.Entrenador };
        var cliente = new Rol { IdRol = 3, Nombre = RolNombres.Cliente };
        db.Roles.AddRange(admin, entrenador, cliente);

        var hash = hasher.Hash(Password);
        Usuario U(int id, Rol rol, string correo, bool activo = true) => new()
        {
            IdUsuario = id, IdRol = rol.IdRol, Nombres = "Test", Apellidos = rol.Nombre, Correo = correo,
            PasswordHash = hash, Activo = activo
        };
        db.Usuarios.AddRange(
            U(1, admin, CorreoAdmin),
            U(2, entrenador, CorreoEntrenador),
            U(3, entrenador, CorreoEntrenador2),
            U(4, cliente, CorreoCliente),
            U(5, cliente, CorreoClienteSinPlan),
            U(6, cliente, CorreoInactivo, activo: false));

        db.Planes.AddRange(
            new Plan { IdPlan = 1, Nombre = "Plan Semanal", DuracionDias = 7, Precio = 40000 },
            new Plan { IdPlan = 2, Nombre = "Plan Mensual", DuracionDias = 30, Precio = 65000 },
            new Plan { IdPlan = 3, Nombre = "Plan Antiguo", DuracionDias = 30, Precio = 50000, Activo = false });

        var hoy = DateOnly.FromDateTime(DateTime.Now);
        db.Membresias.Add(new Membresia
        {
            IdMembresia = 1, IdUsuario = 4, IdPlan = 2, FechaInicio = hoy.AddDays(-5),
            FechaFin = Membresia.CalcularFechaFin(hoy.AddDays(-5), 30), PrecioPagado = 65000
        });

        db.Ejercicios.AddRange(
            new Ejercicio { IdEjercicio = 1, Nombre = "Press de banca", GrupoMuscular = "Pecho" },
            new Ejercicio { IdEjercicio = 2, Nombre = "Sentadilla", GrupoMuscular = "Piernas" },
            new Ejercicio { IdEjercicio = 3, Nombre = "Ejercicio retirado", GrupoMuscular = "Pecho", Activo = false });

        db.SaveChanges();
    }
}
