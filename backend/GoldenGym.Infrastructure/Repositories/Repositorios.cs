using GoldenGym.Application.Interfaces;
using GoldenGym.Domain.Entities;
using GoldenGym.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GoldenGym.Infrastructure.Repositories;

public class UsuarioRepository : IUsuarioRepository
{
    private readonly GymDbContext _db;
    public UsuarioRepository(GymDbContext db) => _db = db;

    public Task<Usuario?> ObtenerPorIdAsync(int id, CancellationToken ct = default) =>
        _db.Usuarios.Include(u => u.Rol).FirstOrDefaultAsync(u => u.IdUsuario == id, ct);

    public Task<Usuario?> ObtenerPorCorreoAsync(string correo, CancellationToken ct = default) =>
        _db.Usuarios.Include(u => u.Rol).FirstOrDefaultAsync(u => u.Correo == correo, ct);

    public Task<bool> ExisteCorreoAsync(string correo, int? excluirId = null, CancellationToken ct = default) =>
        _db.Usuarios.AnyAsync(u => u.Correo == correo && (excluirId == null || u.IdUsuario != excluirId), ct);

    public Task<bool> ExisteDocumentoAsync(string documento, int? excluirId = null, CancellationToken ct = default) =>
        _db.Usuarios.AnyAsync(u => u.Documento == documento && (excluirId == null || u.IdUsuario != excluirId), ct);

    public async Task<List<Usuario>> ListarAsync(string? buscar, string? rol, bool? activo, CancellationToken ct = default)
    {
        var q = _db.Usuarios.Include(u => u.Rol).AsQueryable();
        if (buscar is not null)
            q = q.Where(u => u.Nombres.Contains(buscar) || u.Apellidos.Contains(buscar) || u.Correo.Contains(buscar)
                             || (u.Documento != null && u.Documento.Contains(buscar)));
        if (rol is not null) q = q.Where(u => u.Rol!.Nombre == rol);
        if (activo.HasValue) q = q.Where(u => u.Activo == activo.Value);
        return await q.OrderBy(u => u.Nombres).ThenBy(u => u.Apellidos).ToListAsync(ct);
    }

    public Task<Rol?> ObtenerRolPorNombreAsync(string nombre, CancellationToken ct = default) =>
        _db.Roles.FirstOrDefaultAsync(r => r.Nombre == nombre, ct);

    public async Task AgregarAsync(Usuario usuario, CancellationToken ct = default) => await _db.Usuarios.AddAsync(usuario, ct);
}

public class PlanRepository : IPlanRepository
{
    private readonly GymDbContext _db;
    public PlanRepository(GymDbContext db) => _db = db;

    public async Task<List<Plan>> ListarAsync(string? buscar, bool soloActivos, CancellationToken ct = default)
    {
        var q = _db.Planes.AsQueryable();
        if (soloActivos) q = q.Where(p => p.Activo);
        if (buscar is not null) q = q.Where(p => p.Nombre.Contains(buscar));
        return await q.OrderBy(p => p.DuracionDias).ToListAsync(ct);
    }

    public Task<Plan?> ObtenerPorIdAsync(int id, CancellationToken ct = default) =>
        _db.Planes.FirstOrDefaultAsync(p => p.IdPlan == id, ct);

    public Task<bool> ExisteNombreAsync(string nombre, int? excluirId = null, CancellationToken ct = default) =>
        _db.Planes.AnyAsync(p => p.Nombre == nombre && (excluirId == null || p.IdPlan != excluirId), ct);

    public async Task AgregarAsync(Plan plan, CancellationToken ct = default) => await _db.Planes.AddAsync(plan, ct);
}

public class MembresiaRepository : IMembresiaRepository
{
    private readonly GymDbContext _db;
    public MembresiaRepository(GymDbContext db) => _db = db;

    private IQueryable<Membresia> ConDetalle() => _db.Membresias.Include(m => m.Usuario).Include(m => m.Plan);

    public async Task<List<Membresia>> ListarAsync(int? idUsuario, int? idPlan, string? buscar, CancellationToken ct = default)
    {
        var q = ConDetalle();
        if (idUsuario.HasValue) q = q.Where(m => m.IdUsuario == idUsuario.Value);
        if (idPlan.HasValue) q = q.Where(m => m.IdPlan == idPlan.Value);
        if (buscar is not null)
            q = q.Where(m => m.Usuario!.Nombres.Contains(buscar) || m.Usuario.Apellidos.Contains(buscar)
                             || m.Usuario.Correo.Contains(buscar) || m.Plan!.Nombre.Contains(buscar));
        return await q.OrderByDescending(m => m.FechaInicio).ToListAsync(ct);
    }

    public Task<Membresia?> ObtenerPorIdAsync(int id, CancellationToken ct = default) =>
        ConDetalle().FirstOrDefaultAsync(m => m.IdMembresia == id, ct);

    public Task<List<Membresia>> ListarActivasDeUsuarioAsync(int idUsuario, CancellationToken ct = default) =>
        ConDetalle().Where(m => m.IdUsuario == idUsuario && m.Estado == EstadoMembresia.Activa).ToListAsync(ct);

    public async Task AgregarAsync(Membresia membresia, CancellationToken ct = default) => await _db.Membresias.AddAsync(membresia, ct);
}

public class EjercicioRepository : IEjercicioRepository
{
    private readonly GymDbContext _db;
    public EjercicioRepository(GymDbContext db) => _db = db;

    public async Task<List<Ejercicio>> ListarAsync(string? buscar, string? grupoMuscular, bool soloActivos, CancellationToken ct = default)
    {
        var q = _db.Ejercicios.AsQueryable();
        if (soloActivos) q = q.Where(e => e.Activo);
        if (buscar is not null) q = q.Where(e => e.Nombre.Contains(buscar) || (e.Descripcion != null && e.Descripcion.Contains(buscar)));
        if (grupoMuscular is not null) q = q.Where(e => e.GrupoMuscular == grupoMuscular);
        return await q.OrderBy(e => e.GrupoMuscular).ThenBy(e => e.Nombre).ToListAsync(ct);
    }

    public Task<Ejercicio?> ObtenerPorIdAsync(int id, CancellationToken ct = default) =>
        _db.Ejercicios.FirstOrDefaultAsync(e => e.IdEjercicio == id, ct);

    public Task<List<Ejercicio>> ObtenerPorIdsAsync(IEnumerable<int> ids, CancellationToken ct = default)
    {
        var lista = ids.Distinct().ToList();
        return _db.Ejercicios.Where(e => lista.Contains(e.IdEjercicio)).ToListAsync(ct);
    }

    public Task<bool> ExisteNombreAsync(string nombre, int? excluirId = null, CancellationToken ct = default) =>
        _db.Ejercicios.AnyAsync(e => e.Nombre == nombre && (excluirId == null || e.IdEjercicio != excluirId), ct);

    public async Task AgregarAsync(Ejercicio ejercicio, CancellationToken ct = default) => await _db.Ejercicios.AddAsync(ejercicio, ct);
}

public class RutinaRepository : IRutinaRepository
{
    private readonly GymDbContext _db;
    public RutinaRepository(GymDbContext db) => _db = db;

    public async Task<List<Rutina>> ListarAsync(int? idCliente, int? idEntrenador, string? buscar, string? nivel, bool soloActivas, CancellationToken ct = default)
    {
        var q = _db.Rutinas.Include(r => r.Cliente).Include(r => r.Entrenador).Include(r => r.Ejercicios).AsQueryable();
        if (idCliente.HasValue) q = q.Where(r => r.IdCliente == idCliente.Value);
        if (idEntrenador.HasValue) q = q.Where(r => r.IdEntrenador == idEntrenador.Value);
        if (soloActivas) q = q.Where(r => r.Activa);
        if (nivel is not null) q = q.Where(r => r.Nivel == nivel);
        if (buscar is not null)
            q = q.Where(r => r.Nombre.Contains(buscar) || (r.Objetivo != null && r.Objetivo.Contains(buscar))
                             || r.Cliente!.Nombres.Contains(buscar) || r.Cliente.Apellidos.Contains(buscar));
        return await q.OrderByDescending(r => r.FechaCreacion).ToListAsync(ct);
    }

    public Task<Rutina?> ObtenerPorIdAsync(int id, CancellationToken ct = default) =>
        _db.Rutinas
            .Include(r => r.Cliente)
            .Include(r => r.Entrenador)
            .Include(r => r.Ejercicios).ThenInclude(re => re.Ejercicio)
            .FirstOrDefaultAsync(r => r.IdRutina == id, ct);

    public async Task AgregarAsync(Rutina rutina, CancellationToken ct = default) => await _db.Rutinas.AddAsync(rutina, ct);
}

public class AsistenciaRepository : IAsistenciaRepository
{
    private readonly GymDbContext _db;
    public AsistenciaRepository(GymDbContext db) => _db = db;

    private IQueryable<Asistencia> ConDetalle() => _db.Asistencias.Include(a => a.Usuario).Include(a => a.RegistradoPor);

    public async Task<List<Asistencia>> ListarAsync(int? idUsuario, DateTime? desde, DateTime? hasta, CancellationToken ct = default)
    {
        var q = ConDetalle();
        if (idUsuario.HasValue) q = q.Where(a => a.IdUsuario == idUsuario.Value);
        if (desde.HasValue) q = q.Where(a => a.FechaEntrada >= desde.Value);
        if (hasta.HasValue) q = q.Where(a => a.FechaEntrada <= hasta.Value);
        return await q.OrderByDescending(a => a.FechaEntrada).ToListAsync(ct);
    }

    public Task<Asistencia?> ObtenerPorIdAsync(int id, CancellationToken ct = default) =>
        ConDetalle().FirstOrDefaultAsync(a => a.IdAsistencia == id, ct);

    public Task<Asistencia?> ObtenerAbiertaDelDiaAsync(int idUsuario, DateOnly dia, CancellationToken ct = default)
    {
        var inicio = dia.ToDateTime(TimeOnly.MinValue);
        var fin = inicio.AddDays(1);
        return _db.Asistencias.FirstOrDefaultAsync(a => a.IdUsuario == idUsuario && a.FechaSalida == null
                                                        && a.FechaEntrada >= inicio && a.FechaEntrada < fin, ct);
    }

    public async Task AgregarAsync(Asistencia asistencia, CancellationToken ct = default) => await _db.Asistencias.AddAsync(asistencia, ct);

    public void Eliminar(Asistencia asistencia) => _db.Asistencias.Remove(asistencia);
}

public class ProgresoRepository : IProgresoRepository
{
    private readonly GymDbContext _db;
    public ProgresoRepository(GymDbContext db) => _db = db;

    private IQueryable<Progreso> ConDetalle() => _db.Progresos.Include(p => p.Usuario).Include(p => p.Entrenador);

    public async Task<List<Progreso>> ListarAsync(int? idUsuario, DateOnly? desde, DateOnly? hasta, CancellationToken ct = default)
    {
        var q = ConDetalle();
        if (idUsuario.HasValue) q = q.Where(p => p.IdUsuario == idUsuario.Value);
        if (desde.HasValue) q = q.Where(p => p.Fecha >= desde.Value);
        if (hasta.HasValue) q = q.Where(p => p.Fecha <= hasta.Value);
        return await q.OrderByDescending(p => p.Fecha).ThenByDescending(p => p.IdProgreso).ToListAsync(ct);
    }

    public Task<Progreso?> ObtenerPorIdAsync(int id, CancellationToken ct = default) =>
        ConDetalle().FirstOrDefaultAsync(p => p.IdProgreso == id, ct);

    public async Task AgregarAsync(Progreso progreso, CancellationToken ct = default) => await _db.Progresos.AddAsync(progreso, ct);

    public void Eliminar(Progreso progreso) => _db.Progresos.Remove(progreso);
}
