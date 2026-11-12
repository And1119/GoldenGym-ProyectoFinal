using GoldenGym.Application.Interfaces;
using GoldenGym.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GoldenGym.Infrastructure.Persistence;

/// <summary>Contexto EF Core. Mapea exactamente las tablas creadas por database/schema.sql.</summary>
public class GymDbContext : DbContext, IUnitOfWork
{
    public GymDbContext(DbContextOptions<GymDbContext> options) : base(options) { }

    public DbSet<Rol> Roles => Set<Rol>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Plan> Planes => Set<Plan>();
    public DbSet<Membresia> Membresias => Set<Membresia>();
    public DbSet<Ejercicio> Ejercicios => Set<Ejercicio>();
    public DbSet<Rutina> Rutinas => Set<Rutina>();
    public DbSet<RutinaEjercicio> RutinaEjercicios => Set<RutinaEjercicio>();
    public DbSet<Asistencia> Asistencias => Set<Asistencia>();
    public DbSet<Progreso> Progresos => Set<Progreso>();

    public Task GuardarCambiosAsync(CancellationToken ct = default) => SaveChangesAsync(ct);

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(GymDbContext).Assembly);
    }
}
