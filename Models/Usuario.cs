using SITBIO_Desktop.Core;

namespace SITBIO_Desktop.Models;

/// <summary>
/// Usuario del sistema. La autenticación maneja BCrypt + JWT según OBJ-02;
/// en esta fase los credenciales son demo y viven en memoria.
/// </summary>
public class Usuario
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public string Nombres { get; set; } = string.Empty;
    public string Apellidos { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Telefono { get; set; } = string.Empty;
    public string Cargo { get; set; } = string.Empty;
    public RolSistema Rol { get; set; }
    public bool Activo { get; set; } = true;
    public bool PrimerIngreso { get; set; }
    public DateTime FechaRegistro { get; set; }
    public DateTime? UltimoAcceso { get; set; }
    public string PasswordHash { get; set; } = string.Empty;

    public string NombreCompleto => $"{Nombres} {Apellidos}".Trim();

    public string Iniciales
    {
        get
        {
            var n = string.IsNullOrWhiteSpace(Nombres) ? "?" : Nombres.Trim()[..1];
            var a = string.IsNullOrWhiteSpace(Apellidos) ? "" : Apellidos.Trim()[..1];
            return (n + a).ToUpperInvariant();
        }
    }

    public string NombreRol => Rol switch
    {
        RolSistema.Administrador => "Administrador de Sistema",
        RolSistema.CoordinadorBiomedico => "Ingeniero / Coordinador Biomédico",
        RolSistema.Tecnico => "Técnico de Mantenimiento",
        RolSistema.PersonalClinico => "Personal Clínico / Asistencial",
        RolSistema.Director => "Director / Gerente Hospitalario",
        RolSistema.Regulador => "Ente Regulador / Auditor",
        _ => Rol.ToString()
    };

    public string SiglaRol => Rol switch
    {
        RolSistema.Administrador => "ADM",
        RolSistema.CoordinadorBiomedico => "CBM",
        RolSistema.Tecnico => "TEC",
        RolSistema.PersonalClinico => "PCL",
        RolSistema.Director => "DIR",
        RolSistema.Regulador => "REG",
        _ => "---"
    };

    public override string ToString() => NombreCompleto;
}
