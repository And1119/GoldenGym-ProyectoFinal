using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using GoldenGym.Application.DTOs;

namespace GoldenGym.Tests.Integration;

/// <summary>Pruebas de JWT y roles exigidas en la sustentación (login, 401, 403, CRUD por API).</summary>
public class SeguridadApiTests : IClassFixture<GymApiFactory>
{
    private readonly GymApiFactory _factory;

    public SeguridadApiTests(GymApiFactory factory) => _factory = factory;

    private async Task<HttpClient> ClienteConTokenAsync(string correo)
    {
        var http = _factory.CreateClient();
        var resp = await http.PostAsJsonAsync("/api/auth/login", new { correo, password = GymApiFactory.Password });
        resp.EnsureSuccessStatusCode();
        var auth = await resp.Content.ReadFromJsonAsync<AuthResponse>();
        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", auth!.Token);
        return http;
    }

    // ------------------------- Autenticación -------------------------

    [Fact]
    public async Task Login_ConCredencialesValidas_DevuelveTokenYRol()
    {
        var http = _factory.CreateClient();
        var resp = await http.PostAsJsonAsync("/api/auth/login", new { correo = GymApiFactory.CorreoAdmin, password = GymApiFactory.Password });

        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);
        var auth = await resp.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.False(string.IsNullOrWhiteSpace(auth!.Token));
        Assert.Equal("Administrador", auth.Usuario.Rol);
        Assert.Equal(3, auth.Token.Split('.').Length); // header.payload.firma
    }

    [Fact]
    public async Task Login_ConPasswordIncorrecta_Devuelve401()
    {
        var http = _factory.CreateClient();
        var resp = await http.PostAsJsonAsync("/api/auth/login", new { correo = GymApiFactory.CorreoAdmin, password = "Incorrecta1" });

        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
        var error = await resp.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.Equal("Correo o contraseña incorrectos.", error!.Mensaje);
    }

    [Fact]
    public async Task Login_UsuarioInactivo_Devuelve401()
    {
        var http = _factory.CreateClient();
        var resp = await http.PostAsJsonAsync("/api/auth/login", new { correo = GymApiFactory.CorreoInactivo, password = GymApiFactory.Password });
        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    [Fact]
    public async Task EndpointProtegido_SinToken_Devuelve401()
    {
        var resp = await _factory.CreateClient().GetAsync("/api/dashboard");
        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    [Fact]
    public async Task EndpointProtegido_ConTokenAlterado_Devuelve401()
    {
        var http = await ClienteConTokenAsync(GymApiFactory.CorreoCliente);
        var token = http.DefaultRequestHeaders.Authorization!.Parameter!;
        http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token[..^4] + "abcd");

        var resp = await http.GetAsync("/api/dashboard");
        Assert.Equal(HttpStatusCode.Unauthorized, resp.StatusCode);
    }

    [Fact]
    public async Task Registro_CorreoDuplicado_Devuelve409()
    {
        var resp = await _factory.CreateClient().PostAsJsonAsync("/api/auth/registro", new
        {
            nombres = "Otro", apellidos = "Usuario", correo = GymApiFactory.CorreoCliente.ToUpper(), password = "Clave1234"
        });
        Assert.Equal(HttpStatusCode.Conflict, resp.StatusCode);
    }

    [Fact]
    public async Task Registro_DatosInvalidos_Devuelve400ConErroresPorCampo()
    {
        var resp = await _factory.CreateClient().PostAsJsonAsync("/api/auth/registro", new
        {
            nombres = "", apellidos = "X", correo = "no-es-correo", password = "123"
        });
        Assert.Equal(HttpStatusCode.BadRequest, resp.StatusCode);
        var error = await resp.Content.ReadFromJsonAsync<ErrorResponse>();
        Assert.NotNull(error!.Errores);
        Assert.Contains("correo", error.Errores!.Keys);
        Assert.Contains("password", error.Errores.Keys);
    }

    [Fact]
    public async Task Registro_Valido_CreaClienteYDevuelveToken()
    {
        var resp = await _factory.CreateClient().PostAsJsonAsync("/api/auth/registro", new
        {
            nombres = "Nuevo", apellidos = "Afiliado", correo = "nuevo@test.com", password = "Clave1234", telefono = "3001234567"
        });
        Assert.Equal(HttpStatusCode.Created, resp.StatusCode);
        var auth = await resp.Content.ReadFromJsonAsync<AuthResponse>();
        Assert.Equal("Cliente", auth!.Usuario.Rol); // el registro público nunca crea administradores
    }

    // ------------------------- Autorización por roles -------------------------

    [Fact]
    public async Task Cliente_ListarUsuarios_Devuelve403()
    {
        var http = await ClienteConTokenAsync(GymApiFactory.CorreoCliente);
        var resp = await http.GetAsync("/api/usuarios");
        Assert.Equal(HttpStatusCode.Forbidden, resp.StatusCode);
    }

    [Fact]
    public async Task Cliente_CrearEjercicio_Devuelve403()
    {
        var http = await ClienteConTokenAsync(GymApiFactory.CorreoCliente);
        var resp = await http.PostAsJsonAsync("/api/ejercicios", new { nombre = "Hack", grupoMuscular = "Pecho" });
        Assert.Equal(HttpStatusCode.Forbidden, resp.StatusCode);
    }

    [Fact]
    public async Task Entrenador_CrearPlan_Devuelve403()
    {
        var http = await ClienteConTokenAsync(GymApiFactory.CorreoEntrenador);
        var resp = await http.PostAsJsonAsync("/api/planes", new { nombre = "Plan Pirata", duracionDias = 10, precio = 1000 });
        Assert.Equal(HttpStatusCode.Forbidden, resp.StatusCode);
    }

    [Fact]
    public async Task Entrenador_ListarUsuarios_SoloVeClientes()
    {
        var http = await ClienteConTokenAsync(GymApiFactory.CorreoEntrenador);
        var usuarios = await http.GetFromJsonAsync<List<UsuarioDto>>("/api/usuarios?rol=Administrador");
        Assert.NotNull(usuarios);
        Assert.All(usuarios!, u => Assert.Equal("Cliente", u.Rol));
    }

    [Fact]
    public async Task Admin_ListarUsuariosFiltrados_Devuelve200()
    {
        var http = await ClienteConTokenAsync(GymApiFactory.CorreoAdmin);
        var usuarios = await http.GetFromJsonAsync<List<UsuarioDto>>("/api/usuarios?rol=Entrenador&activo=true");
        Assert.Equal(2, usuarios!.Count);
    }

    [Fact]
    public async Task Cliente_VerRutinaDeOtroCliente_Devuelve403()
    {
        var entrenador = await ClienteConTokenAsync(GymApiFactory.CorreoEntrenador);
        var crear = await entrenador.PostAsJsonAsync("/api/rutinas", new
        {
            nombre = "Rutina privada", nivel = "Principiante", idCliente = 5,
            ejercicios = new[] { new { idEjercicio = 1, series = 3, repeticiones = 10, descansoSeg = 60, orden = 1 } }
        });
        Assert.Equal(HttpStatusCode.Created, crear.StatusCode);
        var rutina = await crear.Content.ReadFromJsonAsync<RutinaDto>();

        var cliente = await ClienteConTokenAsync(GymApiFactory.CorreoCliente); // es el cliente 4, no el 5
        var resp = await cliente.GetAsync($"/api/rutinas/{rutina!.IdRutina}");
        Assert.Equal(HttpStatusCode.Forbidden, resp.StatusCode);
    }

    [Fact]
    public async Task Entrenador_EditarRutinaDeOtroEntrenador_Devuelve403()
    {
        var entrenador1 = await ClienteConTokenAsync(GymApiFactory.CorreoEntrenador);
        var crear = await entrenador1.PostAsJsonAsync("/api/rutinas", new
        {
            nombre = "Rutina de E1", nivel = "Intermedio", idCliente = 4,
            ejercicios = new[] { new { idEjercicio = 2, series = 4, repeticiones = 12, descansoSeg = 90, orden = 1 } }
        });
        var rutina = await crear.Content.ReadFromJsonAsync<RutinaDto>();

        var entrenador2 = await ClienteConTokenAsync(GymApiFactory.CorreoEntrenador2);
        var resp = await entrenador2.DeleteAsync($"/api/rutinas/{rutina!.IdRutina}");
        Assert.Equal(HttpStatusCode.Forbidden, resp.StatusCode);
    }

    [Fact]
    public async Task PlanesPublicos_SinToken_Devuelve200SoloActivos()
    {
        var planes = await _factory.CreateClient().GetFromJsonAsync<List<PlanDto>>("/api/planes?incluirInactivos=true");
        Assert.All(planes!, p => Assert.True(p.Activo));
    }
}
