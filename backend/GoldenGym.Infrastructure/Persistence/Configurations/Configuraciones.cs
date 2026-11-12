using GoldenGym.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace GoldenGym.Infrastructure.Persistence.Configurations;

public class RolConfiguration : IEntityTypeConfiguration<Rol>
{
    public void Configure(EntityTypeBuilder<Rol> b)
    {
        b.ToTable("Roles");
        b.HasKey(x => x.IdRol);
        b.Property(x => x.Nombre).HasMaxLength(30).IsRequired();
        b.Property(x => x.Descripcion).HasMaxLength(150);
        b.HasIndex(x => x.Nombre).IsUnique();
    }
}

public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> b)
    {
        b.ToTable("Usuarios");
        b.HasKey(x => x.IdUsuario);
        b.Property(x => x.Nombres).HasMaxLength(80).IsRequired();
        b.Property(x => x.Apellidos).HasMaxLength(80).IsRequired();
        b.Property(x => x.Correo).HasMaxLength(120).IsRequired();
        b.Property(x => x.PasswordHash).HasMaxLength(255).IsRequired();
        b.Property(x => x.Telefono).HasMaxLength(20);
        b.Property(x => x.Documento).HasMaxLength(20);
        b.HasIndex(x => x.Correo).IsUnique();
        b.HasIndex(x => x.Documento).IsUnique();
        b.Ignore(x => x.NombreCompleto);
        b.HasOne(x => x.Rol).WithMany(r => r.Usuarios).HasForeignKey(x => x.IdRol).OnDelete(DeleteBehavior.Restrict);
    }
}

public class PlanConfiguration : IEntityTypeConfiguration<Plan>
{
    public void Configure(EntityTypeBuilder<Plan> b)
    {
        b.ToTable("Planes");
        b.HasKey(x => x.IdPlan);
        b.Property(x => x.Nombre).HasMaxLength(60).IsRequired();
        b.Property(x => x.Descripcion).HasMaxLength(300);
        b.Property(x => x.Precio).HasPrecision(10, 2);
        b.HasIndex(x => x.Nombre).IsUnique();
    }
}

public class MembresiaConfiguration : IEntityTypeConfiguration<Membresia>
{
    public void Configure(EntityTypeBuilder<Membresia> b)
    {
        b.ToTable("Membresias");
        b.HasKey(x => x.IdMembresia);
        b.Property(x => x.PrecioPagado).HasPrecision(10, 2);
        b.Property(x => x.Estado).HasMaxLength(15).IsRequired();
        b.HasOne(x => x.Usuario).WithMany(u => u.Membresias).HasForeignKey(x => x.IdUsuario).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Plan).WithMany().HasForeignKey(x => x.IdPlan).OnDelete(DeleteBehavior.Restrict);
    }
}

public class EjercicioConfiguration : IEntityTypeConfiguration<Ejercicio>
{
    public void Configure(EntityTypeBuilder<Ejercicio> b)
    {
        b.ToTable("Ejercicios");
        b.HasKey(x => x.IdEjercicio);
        b.Property(x => x.Nombre).HasMaxLength(80).IsRequired();
        b.Property(x => x.GrupoMuscular).HasMaxLength(40).IsRequired();
        b.Property(x => x.Descripcion).HasMaxLength(300);
        b.HasIndex(x => x.Nombre).IsUnique();
    }
}

public class RutinaConfiguration : IEntityTypeConfiguration<Rutina>
{
    public void Configure(EntityTypeBuilder<Rutina> b)
    {
        b.ToTable("Rutinas");
        b.HasKey(x => x.IdRutina);
        b.Property(x => x.Nombre).HasMaxLength(80).IsRequired();
        b.Property(x => x.Objetivo).HasMaxLength(200);
        b.Property(x => x.Nivel).HasMaxLength(15).IsRequired();
        b.HasOne(x => x.Cliente).WithMany().HasForeignKey(x => x.IdCliente).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Entrenador).WithMany().HasForeignKey(x => x.IdEntrenador).OnDelete(DeleteBehavior.Restrict);
    }
}

public class RutinaEjercicioConfiguration : IEntityTypeConfiguration<RutinaEjercicio>
{
    public void Configure(EntityTypeBuilder<RutinaEjercicio> b)
    {
        b.ToTable("RutinaEjercicio");
        b.HasKey(x => new { x.IdRutina, x.IdEjercicio });
        b.HasOne(x => x.Rutina).WithMany(r => r.Ejercicios).HasForeignKey(x => x.IdRutina).OnDelete(DeleteBehavior.Cascade);
        b.HasOne(x => x.Ejercicio).WithMany().HasForeignKey(x => x.IdEjercicio).OnDelete(DeleteBehavior.Restrict);
    }
}

public class AsistenciaConfiguration : IEntityTypeConfiguration<Asistencia>
{
    public void Configure(EntityTypeBuilder<Asistencia> b)
    {
        b.ToTable("Asistencias");
        b.HasKey(x => x.IdAsistencia);
        b.Ignore(x => x.MinutosEnGimnasio);
        b.HasOne(x => x.Usuario).WithMany().HasForeignKey(x => x.IdUsuario).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.RegistradoPor).WithMany().HasForeignKey(x => x.IdRegistradoPor).OnDelete(DeleteBehavior.SetNull);
    }
}

public class ProgresoConfiguration : IEntityTypeConfiguration<Progreso>
{
    public void Configure(EntityTypeBuilder<Progreso> b)
    {
        b.ToTable("Progresos");
        b.HasKey(x => x.IdProgreso);
        b.Property(x => x.PesoKg).HasPrecision(5, 2);
        b.Property(x => x.EstaturaCm).HasPrecision(5, 2);
        b.Property(x => x.GrasaCorporal).HasPrecision(4, 1);
        b.Property(x => x.Observaciones).HasMaxLength(300);
        b.Ignore(x => x.Imc);
        b.HasOne(x => x.Usuario).WithMany().HasForeignKey(x => x.IdUsuario).OnDelete(DeleteBehavior.Restrict);
        b.HasOne(x => x.Entrenador).WithMany().HasForeignKey(x => x.IdEntrenador).OnDelete(DeleteBehavior.SetNull);
    }
}
