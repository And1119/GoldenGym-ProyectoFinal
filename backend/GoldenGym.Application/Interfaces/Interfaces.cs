using GoldenGym.Domain.Entities;

namespace GoldenGym.Application.Interfaces;

// ---------------- Puertos de seguridad (implementados en Infrastructure) ----------------

public interface IPasswordHasher
{
    string Hash(string password);
    bool Verificar(string password, string hash);
}

public interface ITokenService
{
    (string Token, DateTime ExpiraEnUtc) GenerarToken(Usuario usuario);
}

// ---------------- Puertos de persistencia (repositorios) ----------------

public interface IUnitOfWork
{
    Task GuardarCambiosAsync(CancellationToken ct = default);
}

public interface IUsuarioRepository
{
    Task<Usuario?> ObtenerPorIdAsync(int id, CancellationToken ct = default);
    Task<Usuario?> ObtenerPorCorreoAsync(string correo, CancellationToken ct = default);
    Task<bool> ExisteCorreoAsync(string correo, int? excluirId = null, CancellationToken ct = default);
    Task<bool> ExisteDocumentoAsync(string documento, int? excluirId = null, CancellationToken ct = default);
    Task<List<Usuario>> ListarAsync(string? buscar, string? rol, bool? activo, CancellationToken ct = default);
    Task<Rol?> ObtenerRolPorNombreAsync(string nombre, CancellationToken ct = default);
    Task AgregarAsync(Usuario usuario, CancellationToken ct = default);
}

public interface IPlanRepository
{
    Task<List<Plan>> ListarAsync(string? buscar, bool soloActivos, CancellationToken ct = default);
    Task<Plan?> ObtenerPorIdAsync(int id, CancellationToken ct = default);
    Task<bool> ExisteNombreAsync(string nombre, int? excluirId = null, CancellationToken ct = default);
    Task AgregarAsync(Plan plan, CancellationToken ct = default);
}

public interface IMembresiaRepository
{
    Task<List<Membresia>> ListarAsync(int? idUsuario, int? idPlan, string? buscar, CancellationToken ct = default);
    Task<Membresia?> ObtenerPorIdAsync(int id, CancellationToken ct = default);
    /// <summary>Membresías con Estado = Activa del usuario (para validar cruces de fechas y vigencia).</summary>
    Task<List<Membresia>> ListarActivasDeUsuarioAsync(int idUsuario, CancellationToken ct = default);
    Task AgregarAsync(Membresia membresia, CancellationToken ct = default);
}

public interface IEjercicioRepository
{
    Task<List<Ejercicio>> ListarAsync(string? buscar, string? grupoMuscular, bool soloActivos, CancellationToken ct = default);
    Task<Ejercicio?> ObtenerPorIdAsync(int id, CancellationToken ct = default);
    Task<List<Ejercicio>> ObtenerPorIdsAsync(IEnumerable<int> ids, CancellationToken ct = default);
    Task<bool> ExisteNombreAsync(string nombre, int? excluirId = null, CancellationToken ct = default);
    Task AgregarAsync(Ejercicio ejercicio, CancellationToken ct = default);
}

public interface IRutinaRepository
{
    Task<List<Rutina>> ListarAsync(int? idCliente, int? idEntrenador, string? buscar, string? nivel, bool soloActivas, CancellationToken ct = default);
    Task<Rutina?> ObtenerPorIdAsync(int id, CancellationToken ct = default);
    Task AgregarAsync(Rutina rutina, CancellationToken ct = default);
}

public interface IAsistenciaRepository
{
    Task<List<Asistencia>> ListarAsync(int? idUsuario, DateTime? desde, DateTime? hasta, CancellationToken ct = default);
    Task<Asistencia?> ObtenerPorIdAsync(int id, CancellationToken ct = default);
    Task<Asistencia?> ObtenerAbiertaDelDiaAsync(int idUsuario, DateOnly dia, CancellationToken ct = default);
    Task AgregarAsync(Asistencia asistencia, CancellationToken ct = default);
    void Eliminar(Asistencia asistencia);
}

public interface IProgresoRepository
{
    Task<List<Progreso>> ListarAsync(int? idUsuario, DateOnly? desde, DateOnly? hasta, CancellationToken ct = default);
    Task<Progreso?> ObtenerPorIdAsync(int id, CancellationToken ct = default);
    Task AgregarAsync(Progreso progreso, CancellationToken ct = default);
    void Eliminar(Progreso progreso);
}
