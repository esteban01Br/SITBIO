using SITBIO_Desktop.Core;

namespace SITBIO_Desktop.Core;

/// <summary>Nivel de acceso concedido a un área del sistema.</summary>
public enum NivelAcceso
{
    Ninguno,
    Lectura,
    Total
}

/// <summary>
/// Matriz RBAC: qué rol accede a qué área y con qué nivel.
/// Derivada de la matriz de actores del DOC-01-FCP §6.1 y de los CU definidos
/// en la documentación de gobernanza (§2). Centraliza el control de acceso para
/// que la UI y las operaciones consulten el mismo criterio (OBJ-02).
/// </summary>
public static class PermisosRbac
{
    private static readonly Dictionary<RolSistema, Dictionary<AreaAplicacion, NivelAcceso>> Matriz = new()
    {
        // AC-01 — privilegios totales sobre todas las áreas
        [RolSistema.Administrador] = new()
        {
            [AreaAplicacion.Dashboard] = NivelAcceso.Total,
            [AreaAplicacion.Equipos] = NivelAcceso.Total,
            [AreaAplicacion.Ordenes] = NivelAcceso.Total,
            [AreaAplicacion.Intervencion] = NivelAcceso.Total,
            [AreaAplicacion.Tecnicos] = NivelAcceso.Total,
            [AreaAplicacion.Formularios] = NivelAcceso.Total,
            [AreaAplicacion.Reportes] = NivelAcceso.Total,
            [AreaAplicacion.Usuarios] = NivelAcceso.Total,
            [AreaAplicacion.Configuracion] = NivelAcceso.Total,
            [AreaAplicacion.Auditoria] = NivelAcceso.Total,
        },

        // AC-02 — supervisa activos, asigna y cierra órdenes; consulta usuarios
        [RolSistema.CoordinadorBiomedico] = new()
        {
            [AreaAplicacion.Dashboard] = NivelAcceso.Total,
            [AreaAplicacion.Equipos] = NivelAcceso.Total,
            [AreaAplicacion.Ordenes] = NivelAcceso.Total,
            [AreaAplicacion.Intervencion] = NivelAcceso.Total,
            [AreaAplicacion.Tecnicos] = NivelAcceso.Total,
            [AreaAplicacion.Formularios] = NivelAcceso.Total,
            [AreaAplicacion.Reportes] = NivelAcceso.Total,
            [AreaAplicacion.Usuarios] = NivelAcceso.Lectura,
            [AreaAplicacion.Configuracion] = NivelAcceso.Ninguno,
            [AreaAplicacion.Auditoria] = NivelAcceso.Lectura,
        },

        // AC-03 — ejecuta intervenciones y actualiza sus órdenes asignadas
        [RolSistema.Tecnico] = new()
        {
            [AreaAplicacion.Dashboard] = NivelAcceso.Lectura,
            [AreaAplicacion.Equipos] = NivelAcceso.Lectura,
            [AreaAplicacion.Ordenes] = NivelAcceso.Total,
            [AreaAplicacion.Intervencion] = NivelAcceso.Total,
            [AreaAplicacion.Tecnicos] = NivelAcceso.Ninguno,
            [AreaAplicacion.Formularios] = NivelAcceso.Lectura,
            [AreaAplicacion.Reportes] = NivelAcceso.Ninguno,
            [AreaAplicacion.Usuarios] = NivelAcceso.Ninguno,
            [AreaAplicacion.Configuracion] = NivelAcceso.Ninguno,
            [AreaAplicacion.Auditoria] = NivelAcceso.Ninguno,
        },

        // AC-04 — reporta fallas y consulta por código de rastreo
        [RolSistema.PersonalClinico] = new()
        {
            [AreaAplicacion.Dashboard] = NivelAcceso.Lectura,
            [AreaAplicacion.Equipos] = NivelAcceso.Lectura,
            [AreaAplicacion.Ordenes] = NivelAcceso.Total,
            [AreaAplicacion.Intervencion] = NivelAcceso.Ninguno,
            [AreaAplicacion.Tecnicos] = NivelAcceso.Ninguno,
            [AreaAplicacion.Formularios] = NivelAcceso.Lectura,
            [AreaAplicacion.Reportes] = NivelAcceso.Ninguno,
            [AreaAplicacion.Usuarios] = NivelAcceso.Ninguno,
            [AreaAplicacion.Configuracion] = NivelAcceso.Ninguno,
            [AreaAplicacion.Auditoria] = NivelAcceso.Ninguno,
        },

        // AC-05 — dirección: indicadores y costos
        [RolSistema.Director] = new()
        {
            [AreaAplicacion.Dashboard] = NivelAcceso.Total,
            [AreaAplicacion.Equipos] = NivelAcceso.Lectura,
            [AreaAplicacion.Ordenes] = NivelAcceso.Lectura,
            [AreaAplicacion.Intervencion] = NivelAcceso.Ninguno,
            [AreaAplicacion.Tecnicos] = NivelAcceso.Lectura,
            [AreaAplicacion.Formularios] = NivelAcceso.Ninguno,
            [AreaAplicacion.Reportes] = NivelAcceso.Total,
            [AreaAplicacion.Usuarios] = NivelAcceso.Lectura,
            [AreaAplicacion.Configuracion] = NivelAcceso.Ninguno,
            [AreaAplicacion.Auditoria] = NivelAcceso.Lectura,
        },

        // AC-07 — auditor externo: solo consulta trazabilidad
        [RolSistema.Regulador] = new()
        {
            [AreaAplicacion.Dashboard] = NivelAcceso.Lectura,
            [AreaAplicacion.Equipos] = NivelAcceso.Lectura,
            [AreaAplicacion.Ordenes] = NivelAcceso.Lectura,
            [AreaAplicacion.Intervencion] = NivelAcceso.Ninguno,
            [AreaAplicacion.Tecnicos] = NivelAcceso.Ninguno,
            [AreaAplicacion.Formularios] = NivelAcceso.Ninguno,
            [AreaAplicacion.Reportes] = NivelAcceso.Lectura,
            [AreaAplicacion.Usuarios] = NivelAcceso.Ninguno,
            [AreaAplicacion.Configuracion] = NivelAcceso.Ninguno,
            [AreaAplicacion.Auditoria] = NivelAcceso.Lectura,
        },
    };

    /// <summary>Nivel efectivo de un rol sobre un área (Ninguno si no está definido).</summary>
    public static NivelAcceso Nivel(RolSistema rol, AreaAplicacion area)
        => Matriz.TryGetValue(rol, out var areas) && areas.TryGetValue(area, out var nivel)
            ? nivel
            : NivelAcceso.Ninguno;

    public static bool PuedeVer(RolSistema rol, AreaAplicacion area) => Nivel(rol, area) != NivelAcceso.Ninguno;

    public static bool PuedeEditar(RolSistema rol, AreaAplicacion area) => Nivel(rol, area) == NivelAcceso.Total;

    /// <summary>Áreas visibles en la navegación para un rol (oculta las denegadas).</summary>
    public static IReadOnlyList<AreaAplicacion> AreasVisibles(RolSistema rol)
        => Enum.GetValues<AreaAplicacion>().Where(a => PuedeVer(rol, a)).ToList();
}
