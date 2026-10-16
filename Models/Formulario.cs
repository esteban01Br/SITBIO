using SITBIO_Desktop.Core;

namespace SITBIO_Desktop.Models;

/// <summary>
/// Formulario con estructura dinámica definida por el administrador del tenant
/// (OBJ-06). Permite crear categorías e inspecciones sin modificar el código.
/// </summary>
public class FormularioDinamico
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;
    public bool Activo { get; set; } = true;
    public DateTime FechaCreacion { get; set; }
    public string Autor { get; set; } = string.Empty;
    public int Version { get; set; } = 1;
    public int Aplicaciones { get; set; }
    public List<CampoDinamico> Campos { get; set; } = new();

    public int TotalCampos => Campos.Count;
    public int CamposRequeridos => Campos.Count(c => c.Requerido);

    public override string ToString() => Nombre;
}

public class CampoDinamico
{
    public int Id { get; set; }
    public string Etiqueta { get; set; } = string.Empty;
    public TipoCampoDinamico Tipo { get; set; }
    public bool Requerido { get; set; }
    public string Ayuda { get; set; } = string.Empty;
    public List<string> Opciones { get; set; } = new();
    public int Orden { get; set; }

    public string TipoTexto => Tipo switch
    {
        TipoCampoDinamico.Texto => "Texto corto",
        TipoCampoDinamico.TextoLargo => "Texto largo",
        TipoCampoDinamico.Numero => "Número",
        TipoCampoDinamico.Fecha => "Fecha",
        TipoCampoDinamico.Booleano => "Sí / No",
        TipoCampoDinamico.Seleccion => "Selección",
        _ => Tipo.ToString()
    };

    public string IconoTipo => Tipo switch
    {
        TipoCampoDinamico.Texto => "",
        TipoCampoDinamico.TextoLargo => "",
        TipoCampoDinamico.Numero => "",
        TipoCampoDinamico.Fecha => "",
        TipoCampoDinamico.Booleano => "",
        TipoCampoDinamico.Seleccion => "",
        _ => ""
    };
}
