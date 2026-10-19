using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SITBIO.Api.Domain;

namespace SITBIO.Api.Data;

/// <summary>
/// Contexto EF Core contra PostgreSQL (fase 2 — ADSO-ARQ-01).
/// Aislamiento lógico por tenant_id en todas las tablas (RSK-03 / SEC-003).
/// </summary>
public class SITBIOContext : DbContext
{
    public SITBIOContext(DbContextOptions<SITBIOContext> opciones) : base(opciones) { }

    public DbSet<Institucion> Instituciones => Set<Institucion>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Equipo> Equipos => Set<Equipo>();
    public DbSet<OrdenTrabajo> Ordenes => Set<OrdenTrabajo>();
    public DbSet<Tecnico> Tecnicos => Set<Tecnico>();
    public DbSet<Intervencion> Intervenciones => Set<Intervencion>();
    public DbSet<FormularioDinamico> Formularios => Set<FormularioDinamico>();
    public DbSet<EventoAuditoria> Auditoria => Set<EventoAuditoria>();
    public DbSet<Notificacion> Notificaciones => Set<Notificacion>();

    protected override void OnModelCreating(ModelBuilder mb)
    {
        base.OnModelCreating(mb);

        mb.Entity<Institucion>(e =>
        {
            e.ToTable("instituciones");
            e.HasKey(x => x.Id);
            e.Property(x => x.NIT).HasMaxLength(20).IsRequired();
            e.Property(x => x.Nombre).HasMaxLength(200).IsRequired();
            e.Property(x => x.Ciudad).HasMaxLength(100);
            e.Property(x => x.Direccion).HasMaxLength(200);
            e.Property(x => x.Telefono).HasMaxLength(50);
            e.Property(x => x.ResolucionSanitaria).HasMaxLength(200);
            e.HasIndex(x => x.NIT);
        });

        mb.Entity<Usuario>(e =>
        {
            e.ToTable("usuarios");
            e.HasKey(x => x.Id);
            e.Property(x => x.Nombres).HasMaxLength(100).IsRequired();
            e.Property(x => x.Apellidos).HasMaxLength(100).IsRequired();
            e.Property(x => x.Email).HasMaxLength(160).IsRequired();
            e.Property(x => x.Telefono).HasMaxLength(50);
            e.Property(x => x.Cargo).HasMaxLength(200);
            e.Property(x => x.PasswordHash).HasMaxLength(200);
            // Correo único por institución (ADSO-ARQ-01 §5.1).
            e.HasIndex(x => new { x.TenantId, x.Email }).IsUnique();
        });

        mb.Entity<Equipo>(e =>
        {
            e.ToTable("equipos");
            e.HasKey(x => x.Id);
            e.Property(x => x.CodigoInventario).HasMaxLength(50).IsRequired();
            e.Property(x => x.Nombre).HasMaxLength(200).IsRequired();
            e.Property(x => x.Categoria).HasMaxLength(120);
            e.Property(x => x.Marca).HasMaxLength(100);
            e.Property(x => x.Modelo).HasMaxLength(100);
            e.Property(x => x.NumeroSerie).HasMaxLength(100);
            e.Property(x => x.Ubicacion).HasMaxLength(160);
            e.Property(x => x.Responsable).HasMaxLength(160);
            e.Property(x => x.ValorAdquisicion).HasPrecision(18, 2);
            e.Property(x => x.Fabricante).HasMaxLength(100);
            e.Property(x => x.ClaseRiesgo).HasMaxLength(10);
            e.Property(x => x.ResolucionAplicable).HasMaxLength(100);
            e.Property(x => x.CamposDinamicos).HasColumnType("jsonb")
                .HasConversion(v => AJson(v), s => DeJson<Dictionary<string, string>>(s));
            // Número de serie único por institución (ADSO-ARQ-01 §5.1).
            e.HasIndex(x => new { x.TenantId, x.NumeroSerie }).IsUnique();
            e.HasIndex(x => new { x.TenantId, x.CodigoInventario }).IsUnique();
        });

        mb.Entity<OrdenTrabajo>(e =>
        {
            e.ToTable("ordenes_trabajo");
            e.HasKey(x => x.Id);
            e.Property(x => x.Codigo).HasMaxLength(30).IsRequired();
            e.Property(x => x.Titulo).HasMaxLength(300).IsRequired();
            e.Property(x => x.Descripcion).HasMaxLength(2000);
            e.Property(x => x.EquipoNombre).HasMaxLength(200);
            e.Property(x => x.EquipoUbicacion).HasMaxLength(160);
            e.Property(x => x.SolicitanteNombre).HasMaxLength(200);
            e.Property(x => x.TecnicoNombre).HasMaxLength(200);
            e.Property(x => x.CostoEstimado).HasPrecision(18, 2);
            e.Property(x => x.CostoReal).HasPrecision(18, 2);
            e.Property(x => x.ReporteSala).HasMaxLength(300);
            e.Property(x => x.Historial).HasColumnType("jsonb")
                .HasConversion(v => AJson(v), s => DeJson<List<EventoOrden>>(s));
            e.Property(x => x.Comentarios).HasColumnType("jsonb")
                .HasConversion(v => AJson(v), s => DeJson<List<ComentarioOrden>>(s));
            // Código único de seguimiento por institución (§3.1 metodología).
            e.HasIndex(x => new { x.TenantId, x.Codigo }).IsUnique();
            e.HasIndex(x => x.TenantId);
            e.HasIndex(x => x.EquipoId);
        });

        mb.Entity<Tecnico>(e =>
        {
            e.ToTable("tecnicos");
            e.HasKey(x => x.Id);
            e.Property(x => x.Nombre).HasMaxLength(200).IsRequired();
            e.Property(x => x.Iniciales).HasMaxLength(10);
            e.Property(x => x.Telefono).HasMaxLength(50);
            e.Property(x => x.Correo).HasMaxLength(160);
            e.Property(x => x.Turno).HasMaxLength(50);
            e.Property(x => x.Especialidades).HasColumnType("jsonb")
                .HasConversion(v => AJson(v), s => DeJson<List<EspecialidadTecnica>>(s));
            e.HasIndex(x => x.TenantId);
        });

        mb.Entity<Intervencion>(e =>
        {
            e.ToTable("intervenciones");
            e.HasKey(x => x.Id);
            e.Property(x => x.OrdenCodigo).HasMaxLength(30);
            e.Property(x => x.EquipoNombre).HasMaxLength(200);
            e.Property(x => x.TecnicoNombre).HasMaxLength(200);
            e.Property(x => x.Diagnostico).HasMaxLength(2000);
            e.Property(x => x.CausaRaiz).HasMaxLength(2000);
            e.Property(x => x.AccionesRealizadas).HasMaxLength(2000);
            e.Property(x => x.PruebasRealizadas).HasMaxLength(2000);
            e.Property(x => x.ResultadoPrueba).HasMaxLength(500);
            e.Property(x => x.NotasCampo).HasMaxLength(2000);
            e.Property(x => x.Recomendaciones).HasMaxLength(2000);
            e.Property(x => x.CostoRepuestos).HasPrecision(18, 2);
            e.Property(x => x.ManoObra).HasPrecision(18, 2);
            e.Property(x => x.Repuestos).HasColumnType("jsonb")
                .HasConversion(v => AJson(v), s => DeJson<List<RepuestoConsumido>>(s));
            e.Property(x => x.Evidencias).HasColumnType("jsonb")
                .HasConversion(v => AJson(v), s => DeJson<List<string>>(s));
            e.HasIndex(x => new { x.TenantId, x.OrdenId });
        });

        mb.Entity<FormularioDinamico>(e =>
        {
            e.ToTable("formularios");
            e.HasKey(x => x.Id);
            e.Property(x => x.Nombre).HasMaxLength(200).IsRequired();
            e.Property(x => x.Descripcion).HasMaxLength(1000);
            e.Property(x => x.Categoria).HasMaxLength(100);
            e.Property(x => x.Autor).HasMaxLength(200);
            e.Property(x => x.Campos).HasColumnType("jsonb")
                .HasConversion(v => AJson(v), s => DeJson<List<CampoDinamico>>(s));
            e.HasIndex(x => x.TenantId);
        });

        mb.Entity<EventoAuditoria>(e =>
        {
            e.ToTable("eventos_auditoria");
            e.HasKey(x => x.Id);
            e.Property(x => x.Usuario).HasMaxLength(200);
            e.Property(x => x.Rol).HasMaxLength(10);
            e.Property(x => x.Entidad).HasMaxLength(100);
            e.Property(x => x.Referencia).HasMaxLength(200);
            e.Property(x => x.Detalle).HasMaxLength(1000);
            e.Property(x => x.Dispositivo).HasMaxLength(200);
            e.HasIndex(x => x.TenantId);
            e.HasIndex(x => x.Fecha);
        });

        mb.Entity<Notificacion>(e =>
        {
            e.ToTable("notificaciones");
            e.HasKey(x => x.Id);
            e.Property(x => x.Titulo).HasMaxLength(200);
            e.Property(x => x.Mensaje).HasMaxLength(1000);
            e.Property(x => x.Tipo).HasMaxLength(20);
            e.Property(x => x.Icono).HasMaxLength(10);
            e.HasIndex(x => x.TenantId);
        });

        // La aplicación opera con hora local de Bogotá: se guardan como
        // "timestamp without time zone" para aceptar DateTime.Today (Kind=Unspecified)
        // sin conversiones de zona horaria (decisión registrada en ADR-006).
        foreach (var entidad in mb.Model.GetEntityTypes())
            foreach (var propiedad in entidad.GetProperties())
                if (propiedad.ClrType == typeof(DateTime) || propiedad.ClrType == typeof(DateTime?))
                    propiedad.SetColumnType("timestamp without time zone");
    }

    /// <summary>Serializa listas/diccionarios a jsonb con opciones por defecto (sin camelCase).</summary>
    private static string AJson<T>(T valor) => JsonSerializer.Serialize(valor);

    private static T DeJson<T>(string json) => JsonSerializer.Deserialize<T>(json)!;
}
