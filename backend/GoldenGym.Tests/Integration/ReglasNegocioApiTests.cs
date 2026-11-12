using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using GoldenGym.Application.DTOs;

namespace GoldenGym.Tests.Integration;

/// <summary>Flujos completos Android → API → BD con reglas de negocio (CRUD y validaciones).</summary>
public class ReglasNegocioApiTests : IClassFixture<GymApiFactory>
{
    private readonly GymApiFactory _factory;
    private static readonly string Hoy = DateTime.Now.ToString("yyyy-MM-dd");

    public ReglasNegocioApiTests(GymApiFactory factory) => _factory = factory;

    private async Task<HttpClient> ConTokenAsync(string correo)
    {
        var http = _factory.CreateClient();
        var resp = await http.PostAsJsonAsync("/api/auth/login", new { correo, password = GymApiFactory.Password });
        var auth = await resp.Content.ReadFromJsonAsync<AuthResponse>();
        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth!.Token);
        return http;
    }

    [Fact]
    public async Task Ejercicio_CrudCompleto_PorEntrenador()
    {
        var http = await ConTokenAsync(GymApiFactory.CorreoEntrenador);

        // Crear
        var crear = await http.PostAsJsonAsync("/api/ejercicios", new { nombre = "Remo con barra", grupoMuscular = "Espalda", descripcion = "Agarre prono" });
        Assert.Equal(HttpStatusCode.Created, crear.StatusCode);
        var e = await crear.Content.ReadFromJsonAsync<EjercicioDto>();

        // Duplicado -> 409
        var dup = await http.PostAsJsonAsync("/api/ejercicios", new { nombre = "Remo con barra", grupoMuscular = "Espalda" });
        Assert.Equal(HttpStatusCode.Conflict, dup.StatusCode);

        // Editar
        var editar = await http.PutAsJsonAsync($"/api/ejercicios/{e!.IdEjercicio}", new { nombre = "Remo con barra T", grupoMuscular = "Espalda", activo = true });
        Assert.Equal(HttpStatusCode.OK, editar.StatusCode);

        // Buscar por dos criterios
        var lista = await http.GetFromJsonAsync<List<EjercicioDto>>("/api/ejercicios?buscar=Remo&grupoMuscular=Espalda");
        Assert.Contains(lista!, x => x.Nombre == "Remo con barra T");

        // Desactivar
        var borrar = await http.DeleteAsync($"/api/ejercicios/{e.IdEjercicio}");
        Assert.Equal(HttpStatusCode.NoContent, borrar.StatusCode);
        var detalle = await http.GetFromJsonAsync<EjercicioDto>($"/api/ejercicios/{e.IdEjercicio}");
        Assert.False(detalle!.Activo);
    }

    [Fact]
    public async Task Membresia_CruceDeFechas_Devuelve422()
    {
        var admin = await ConTokenAsync(GymApiFactory.CorreoAdmin);
        // El cliente 4 ya tiene una membresía mensual vigente.
        var resp = await admin.PostAsJsonAsync("/api/membresias", new { idUsuario = 4, idPlan = 1, fechaInicio = Hoy });
        Assert.Equal(HttpStatusCode.UnprocessableEntity, resp.StatusCode);
    }

    [Fact]
    public async Task Membresia_PlanInactivo_Devuelve422()
    {
        var admin = await ConTokenAsync(GymApiFactory.CorreoAdmin);
        var resp = await admin.PostAsJsonAsync("/api/membresias", new { idUsuario = 5, idPlan = 3, fechaInicio = Hoy });
        Assert.Equal(HttpStatusCode.UnprocessableEntity, resp.StatusCode);
    }

    [Fact]
    public async Task Membresia_ClienteCompraPlan_CalculaFechaFinYQuedaVigente()
    {
        var http = _factory.CreateClient();
        var reg = await http.PostAsJsonAsync("/api/auth/registro", new { nombres = "Compra", apellidos = "Plan", correo = "compra@test.com", password = "Clave1234" });
        var auth = await reg.Content.ReadFromJsonAsync<AuthResponse>();
        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth!.Token);

        // Aunque envíe otro IdUsuario, la API usa el del token.
        var resp = await http.PostAsJsonAsync("/api/membresias", new { idUsuario = 1, idPlan = 1, fechaInicio = Hoy });
        Assert.Equal(HttpStatusCode.Created, resp.StatusCode);
        var m = await resp.Content.ReadFromJsonAsync<MembresiaDto>();
        Assert.Equal(auth.Usuario.IdUsuario, m!.IdUsuario);
        Assert.Equal("Vigente", m.Estado);
        Assert.Equal(m.FechaInicio.AddDays(6), m.FechaFin);
        Assert.Equal(7, m.DiasRestantes);
    }

    [Fact]
    public async Task Asistencia_SinMembresiaVigente_Devuelve422()
    {
        var http = await ConTokenAsync(GymApiFactory.CorreoClienteSinPlan);
        var resp = await http.PostAsJsonAsync("/api/asistencias/entrada", new { });
        Assert.Equal(HttpStatusCode.UnprocessableEntity, resp.StatusCode);
        var error = await resp.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.Contains("membresía vigente", error!.Mensaje);
    }

    [Fact]
    public async Task Asistencia_EntradaDobleYSalida()
    {
        var http = await ConTokenAsync(GymApiFactory.CorreoCliente);

        var entrada = await http.PostAsJsonAsync("/api/asistencias/entrada", new { });
        Assert.Equal(HttpStatusCode.Created, entrada.StatusCode);
        var a = await entrada.Content.ReadFromJsonAsync<AsistenciaDto>();

        var doble = await http.PostAsJsonAsync("/api/asistencias/entrada", new { });
        Assert.Equal(HttpStatusCode.UnprocessableEntity, doble.StatusCode);

        var salida = await http.PutAsync($"/api/asistencias/{a!.IdAsistencia}/salida", null);
        Assert.Equal(HttpStatusCode.OK, salida.StatusCode);
        var cerrada = await salida.Content.ReadFromJsonAsync<AsistenciaDto>();
        Assert.NotNull(cerrada!.FechaSalida);
    }

    [Fact]
    public async Task Rutina_ConEjercicioInactivoODuplicado_Devuelve422()
    {
        var http = await ConTokenAsync(GymApiFactory.CorreoEntrenador);
        var inactivo = await http.PostAsJsonAsync("/api/rutinas", new
        {
            nombre = "Mala", nivel = "Principiante", idCliente = 4,
            ejercicios = new[] { new { idEjercicio = 3, series = 3, repeticiones = 10, descansoSeg = 60, orden = 1 } }
        });
        Assert.Equal(HttpStatusCode.UnprocessableEntity, inactivo.StatusCode);

        var duplicado = await http.PostAsJsonAsync("/api/rutinas", new
        {
            nombre = "Mala 2", nivel = "Principiante", idCliente = 4,
            ejercicios = new[]
            {
                new { idEjercicio = 1, series = 3, repeticiones = 10, descansoSeg = 60, orden = 1 },
                new { idEjercicio = 1, series = 3, repeticiones = 10, descansoSeg = 60, orden = 2 }
            }
        });
        Assert.Equal(HttpStatusCode.UnprocessableEntity, duplicado.StatusCode);
    }

    [Fact]
    public async Task Rutina_ActualizarDetalle_SincronizaEjercicios()
    {
        var http = await ConTokenAsync(GymApiFactory.CorreoEntrenador);
        var crear = await http.PostAsJsonAsync("/api/rutinas", new
        {
            nombre = "Full body", nivel = "Principiante", idCliente = 4,
            ejercicios = new[] { new { idEjercicio = 1, series = 3, repeticiones = 10, descansoSeg = 60, orden = 1 } }
        });
        var r = await crear.Content.ReadFromJsonAsync<RutinaDto>();

        var editar = await http.PutAsJsonAsync($"/api/rutinas/{r!.IdRutina}", new
        {
            nombre = "Full body v2", nivel = "Intermedio", idCliente = 4, activa = true,
            ejercicios = new[] { new { idEjercicio = 2, series = 5, repeticiones = 8, descansoSeg = 120, orden = 1 } }
        });
        Assert.Equal(HttpStatusCode.OK, editar.StatusCode);
        var actualizada = await editar.Content.ReadFromJsonAsync<RutinaDto>();
        Assert.Single(actualizada!.Ejercicios);
        Assert.Equal(2, actualizada.Ejercicios[0].IdEjercicio);
        Assert.Equal("Intermedio", actualizada.Nivel);
    }

    [Fact]
    public async Task Progreso_FechaFutura_Devuelve422_YCalculaImc()
    {
        var http = await ConTokenAsync(GymApiFactory.CorreoEntrenador);
        var futura = DateTime.Now.AddDays(2).ToString("yyyy-MM-dd");
        var mala = await http.PostAsJsonAsync("/api/progresos", new { idUsuario = 4, fecha = futura, pesoKg = 70 });
        Assert.Equal(HttpStatusCode.UnprocessableEntity, mala.StatusCode);

        var ok = await http.PostAsJsonAsync("/api/progresos", new { idUsuario = 4, fecha = Hoy, pesoKg = 80, estaturaCm = 200 });
        Assert.Equal(HttpStatusCode.Created, ok.StatusCode);
        var p = await ok.Content.ReadFromJsonAsync<ProgresoDto>();
        Assert.Equal(20.0m, p!.Imc);
    }

    [Fact]
    public async Task Admin_NoPuedeDesactivarseASiMismo()
    {
        var http = await ConTokenAsync(GymApiFactory.CorreoAdmin);
        var resp = await http.DeleteAsync("/api/usuarios/1");
        Assert.Equal(HttpStatusCode.UnprocessableEntity, resp.StatusCode);
    }

    [Fact]
    public async Task Dashboard_Cliente_MuestraMembresiaVigente()
    {
        var http = await ConTokenAsync(GymApiFactory.CorreoCliente);
        var d = await http.GetFromJsonAsync<DashboardDto>("/api/dashboard");
        Assert.NotNull(d!.MembresiaVigente);
        Assert.Equal("Plan Mensual", d.MembresiaVigente!.NombrePlan);
        Assert.Equal("Cliente", d.Usuario.Rol);
    }

    [Fact]
    public async Task RecursoInexistente_Devuelve404()
    {
        var http = await ConTokenAsync(GymApiFactory.CorreoAdmin);
        var resp = await http.GetAsync("/api/membresias/9999");
        Assert.Equal(HttpStatusCode.NotFound, resp.StatusCode);
    }
}
