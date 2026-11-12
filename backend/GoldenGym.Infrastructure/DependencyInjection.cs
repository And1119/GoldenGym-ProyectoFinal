using GoldenGym.Application.Common;
using GoldenGym.Application.Interfaces;
using GoldenGym.Application.Services;
using GoldenGym.Infrastructure.Persistence;
using GoldenGym.Infrastructure.Repositories;
using GoldenGym.Infrastructure.Security;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace GoldenGym.Infrastructure;

/// <summary>Registro de dependencias (inyección de dependencias) de las capas Application e Infrastructure.</summary>
public static class DependencyInjection
{
    public static IServiceCollection AddGoldenGym(this IServiceCollection services, IConfiguration config)
    {
        // ---- Persistencia: EF Core + MySQL/MariaDB (XAMPP) ----
        var cadena = config.GetConnectionString("GoldenGymDb")
                     ?? throw new InvalidOperationException("Falta la cadena de conexión 'GoldenGymDb' en appsettings.json.");
        var versionServidor = config["Database:ServerVersion"] ?? "10.4.32-mariadb";
        services.AddDbContext<GymDbContext>(o => o.UseMySql(cadena, ServerVersion.Parse(versionServidor)));
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<GymDbContext>());

        // ---- Repositorios ----
        services.AddScoped<IUsuarioRepository, UsuarioRepository>();
        services.AddScoped<IPlanRepository, PlanRepository>();
        services.AddScoped<IMembresiaRepository, MembresiaRepository>();
        services.AddScoped<IEjercicioRepository, EjercicioRepository>();
        services.AddScoped<IRutinaRepository, RutinaRepository>();
        services.AddScoped<IAsistenciaRepository, AsistenciaRepository>();
        services.AddScoped<IProgresoRepository, ProgresoRepository>();

        // ---- Seguridad ----
        services.Configure<JwtOptions>(config.GetSection(JwtOptions.Seccion));
        services.AddSingleton<IPasswordHasher, BCryptPasswordHasher>();
        services.AddSingleton<ITokenService, JwtTokenService>();
        services.AddSingleton<IFechaProvider, FechaProviderSistema>();

        // ---- Servicios de aplicación (casos de uso) ----
        services.AddScoped<AuthService>();
        services.AddScoped<UsuarioService>();
        services.AddScoped<PlanService>();
        services.AddScoped<MembresiaService>();
        services.AddScoped<EjercicioService>();
        services.AddScoped<RutinaService>();
        services.AddScoped<AsistenciaService>();
        services.AddScoped<ProgresoService>();
        services.AddScoped<DashboardService>();

        return services;
    }
}
