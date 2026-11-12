using System.Reflection;
using System.Text;
using GoldenGym.Api.Extensions;
using GoldenGym.Api.Middleware;
using GoldenGym.Application.DTOs;
using GoldenGym.Infrastructure;
using GoldenGym.Infrastructure.Security;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ---------------- Capas Application + Infrastructure (inyección de dependencias) ----------------
builder.Services.AddGoldenGym(builder.Configuration);

// ---------------- Autenticación JWT ----------------
var jwt = builder.Configuration.GetSection(JwtOptions.Seccion).Get<JwtOptions>() ?? new JwtOptions();
if (string.IsNullOrWhiteSpace(jwt.Key) || jwt.Key.Length < 32)
    throw new InvalidOperationException("Configura 'Jwt:Key' en appsettings.json con una clave de al menos 32 caracteres.");

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(o =>
    {
        o.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = jwt.Issuer,
            ValidateAudience = true,
            ValidAudience = jwt.Audience,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
            ValidateLifetime = true,
            ClockSkew = TimeSpan.FromMinutes(1)
        };
        // Respuestas 401 / 403 con el mismo formato JSON que el resto de errores.
        o.Events = new JwtBearerEvents
        {
            OnChallenge = async ctx =>
            {
                ctx.HandleResponse();
                var expirado = ctx.AuthenticateFailure is SecurityTokenExpiredException;
                await ErrorHandlingMiddleware.EscribirErrorAsync(ctx.HttpContext, StatusCodes.Status401Unauthorized,
                    expirado ? "Tu sesión expiró. Inicia sesión nuevamente." : "Debes iniciar sesión para acceder a este recurso.");
            },
            OnForbidden = ctx => ErrorHandlingMiddleware.EscribirErrorAsync(ctx.HttpContext, StatusCodes.Status403Forbidden,
                "Acceso denegado: tu rol no tiene permiso para realizar esta acción.")
        };
    });

builder.Services.AddAuthorization(Politicas.Registrar);

// ---------------- Controladores + validación de modelos ----------------
builder.Services.AddControllers();
builder.Services.Configure<ApiBehaviorOptions>(o =>
{
    // Errores de validación (DataAnnotations) con el formato ErrorResponse (RNF-07).
    o.InvalidModelStateResponseFactory = ctx =>
    {
        var errores = ctx.ModelState
            .Where(e => e.Value?.Errors.Count > 0)
            .ToDictionary(
                e => string.IsNullOrEmpty(e.Key) ? "general" : char.ToLowerInvariant(e.Key[0]) + e.Key[1..],
                e => e.Value!.Errors.Select(x => string.IsNullOrEmpty(x.ErrorMessage) ? "Valor inválido." : x.ErrorMessage).ToArray());
        return new BadRequestObjectResult(new ErrorResponse(400, "Hay datos inválidos en la solicitud.", errores));
    };
});

builder.Services.AddCors(o => o.AddDefaultPolicy(p => p.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

// ---------------- Swagger / OpenAPI con soporte de JWT ----------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Golden Gym Ibagué API",
        Version = "v1",
        Description = "API REST del proyecto final integrador (Grupo 9 · Aplicación GYM). " +
                      "Usa POST /api/auth/login, copia el token y pulsa 'Authorize'."
    });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Pega solo el token JWT (sin la palabra Bearer)."
    });
    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } },
            Array.Empty<string>()
        }
    });
    var xml = Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml");
    if (File.Exists(xml)) c.IncludeXmlComments(xml);
});

var app = builder.Build();

// ---------------- Pipeline HTTP ----------------
app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Golden Gym API v1"));
app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Comprobación rápida de conexión (la usa la app Android).
app.MapGet("/api/health", () => Results.Ok(new { estado = "ok", servicio = "Golden Gym API", hora = DateTime.Now }))
   .AllowAnonymous();

app.Run();

/// <summary>Necesario para las pruebas de integración con WebApplicationFactory.</summary>
public partial class Program { }
