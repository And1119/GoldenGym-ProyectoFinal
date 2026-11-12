using GoldenGym.Domain.Entities;

namespace GoldenGym.Tests.Unit;

/// <summary>Pruebas unitarias de las reglas del dominio (sin base de datos).</summary>
public class DominioTests
{
    private static readonly DateOnly Hoy = new(2026, 11, 12);

    [Theory]
    [InlineData(1, "2026-11-12")]   // Rutina por sesión: solo el mismo día
    [InlineData(7, "2026-11-18")]   // Semanal
    [InlineData(30, "2026-12-11")]  // Mensual
    public void CalcularFechaFin_IncluyeElDiaDeInicio(int dias, string esperado)
    {
        Assert.Equal(DateOnly.Parse(esperado), Membresia.CalcularFechaFin(Hoy, dias));
    }

    [Fact]
    public void EstadoCalculado_SegunFechas()
    {
        var m = new Membresia { FechaInicio = Hoy, FechaFin = Hoy.AddDays(6) };
        Assert.Equal(EstadoMembresia.Pendiente, m.EstadoCalculado(Hoy.AddDays(-1)));
        Assert.Equal(EstadoMembresia.Vigente, m.EstadoCalculado(Hoy));
        Assert.Equal(EstadoMembresia.Vigente, m.EstadoCalculado(Hoy.AddDays(6)));
        Assert.Equal(EstadoMembresia.Vencida, m.EstadoCalculado(Hoy.AddDays(7)));

        m.Estado = EstadoMembresia.Cancelada;
        Assert.Equal(EstadoMembresia.Cancelada, m.EstadoCalculado(Hoy));
        Assert.False(m.EstaVigente(Hoy));
    }

    [Fact]
    public void SeCruzaCon_DetectaSolapamientos()
    {
        var m = new Membresia { FechaInicio = Hoy, FechaFin = Hoy.AddDays(29) };
        Assert.True(m.SeCruzaCon(Hoy.AddDays(29), Hoy.AddDays(35)));  // toca el último día
        Assert.True(m.SeCruzaCon(Hoy.AddDays(-5), Hoy));               // toca el primer día
        Assert.False(m.SeCruzaCon(Hoy.AddDays(30), Hoy.AddDays(36)));  // empieza al día siguiente
    }

    [Fact]
    public void Imc_SeCalculaConUnDecimal()
    {
        var p = new Progreso { PesoKg = 76.4m, EstaturaCm = 175 };
        Assert.Equal(24.9m, p.Imc);
        Assert.Null(new Progreso { PesoKg = 70 }.Imc);
    }

    [Fact]
    public void Asistencia_MinutosEnGimnasio()
    {
        var entrada = new DateTime(2026, 11, 12, 6, 0, 0);
        var a = new Asistencia { FechaEntrada = entrada };
        Assert.Null(a.MinutosEnGimnasio);
        a.FechaSalida = entrada.AddMinutes(95);
        Assert.Equal(95, a.MinutosEnGimnasio);
    }
}
