using SITBIO_Desktop.Core;

namespace SITBIO_Desktop.Models;

/// <summary>Perfil técnico extendido del usuario con rol Técnico.</summary>
public class Tecnico
{
    public int Id { get; set; }
    public int UsuarioId { get; set; }
    public int TenantId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Iniciales { get; set; } = string.Empty;
    public List<EspecialidadTecnica> Especialidades { get; set; } = new();
    public DisponibilidadTecnico Disponibilidad { get; set; } = DisponibilidadTecnico.Disponible;
    public double CapacidadHorasSemana { get; set; } = 40;
    public double HorasAsignadas { get; set; }
    public double HorasSemanaAnterior { get; set; }
    public int OrdenesCompletadas { get; set; }
    public int OrdenesActivas { get; set; }
    public double Calificacion { get; set; } = 4.5;
    public string Telefono { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public string Turno { get; set; } = "Diurno";

    public double Carga => CapacidadHorasSemana <= 0 ? 0 : HorasAsignadas / CapacidadHorasSemana * 100;
    public bool Sobrecargado => Carga > 90;

    public string EspecialidadesTexto => Especialidades.Count == 0
        ? "Sin asignar"
        : string.Join(", ", Especialidades.Select(EspecieTexto));

    public static string EspecieTexto(EspecialidadTecnica e) => e switch
    {
        EspecialidadTecnica.ElectronicaBiomedica => "Electrónica biomédica",
        EspecialidadTecnica.Mecanica => "Mecánica",
        EspecialidadTecnica.Electroquimica => "Electroquímica",
        EspecialidadTecnica.Imagenologia => "Imagenología",
        EspecialidadTecnica.Laboratorio => "Laboratorio",
        EspecialidadTecnica.Infusion => "Infusión",
        EspecialidadTecnica.AireMedico => "Aire médico",
        _ => e.ToString()
    };

    public string DisponibilidadTexto => Disponibilidad switch
    {
        DisponibilidadTecnico.Disponible => "Disponible",
        DisponibilidadTecnico.EnCampo => "En campo",
        DisponibilidadTecnico.EnDescanso => "En descanso",
        DisponibilidadTecnico.NoDisponible => "No disponible",
        _ => Disponibilidad.ToString()
    };
}
