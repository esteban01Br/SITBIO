using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SITBIO.Api.Migrations
{
    /// <inheritdoc />
    public partial class Inicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "equipos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TenantId = table.Column<int>(type: "integer", nullable: false),
                    CodigoInventario = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Categoria = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                    Marca = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Modelo = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    NumeroSerie = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Ubicacion = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    Responsable = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    Estado = table.Column<int>(type: "integer", nullable: false),
                    FechaInstalacion = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UltimoMantenimiento = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ProximoMantenimiento = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    UltimaCalibracion = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    ProximaCalibracion = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    VidaUtilMeses = table.Column<int>(type: "integer", nullable: false),
                    ValorAdquisicion = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Fabricante = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ClaseRiesgo = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    ResolucionAplicable = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    CamposDinamicos = table.Column<string>(type: "jsonb", nullable: false),
                    OrdenesAbiertas = table.Column<int>(type: "integer", nullable: false),
                    TotalIntervenciones = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_equipos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "eventos_auditoria",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TenantId = table.Column<int>(type: "integer", nullable: false),
                    Fecha = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Usuario = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Rol = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Accion = table.Column<int>(type: "integer", nullable: false),
                    Entidad = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Referencia = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Detalle = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Dispositivo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_eventos_auditoria", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "formularios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TenantId = table.Column<int>(type: "integer", nullable: false),
                    Nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Descripcion = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Categoria = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Activo = table.Column<bool>(type: "boolean", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Autor = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Version = table.Column<int>(type: "integer", nullable: false),
                    Aplicaciones = table.Column<int>(type: "integer", nullable: false),
                    Campos = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_formularios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "instituciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NIT = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Ciudad = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Direccion = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Telefono = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    ResolucionSanitaria = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Plan = table.Column<int>(type: "integer", nullable: false),
                    Estado = table.Column<int>(type: "integer", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    TotalCamas = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_instituciones", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "intervenciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TenantId = table.Column<int>(type: "integer", nullable: false),
                    OrdenId = table.Column<int>(type: "integer", nullable: false),
                    OrdenCodigo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    EquipoId = table.Column<int>(type: "integer", nullable: false),
                    EquipoNombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    TecnicoId = table.Column<int>(type: "integer", nullable: false),
                    TecnicoNombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Estado = table.Column<int>(type: "integer", nullable: false),
                    FechaInicio = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    FechaFin = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    Diagnostico = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    CausaRaiz = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    AccionesRealizadas = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    PruebasRealizadas = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    ResultadoPrueba = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    NotasCampo = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    Recomendaciones = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    HorasInvertidas = table.Column<double>(type: "double precision", nullable: false),
                    CostoRepuestos = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ManoObra = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Repuestos = table.Column<string>(type: "jsonb", nullable: false),
                    Evidencias = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_intervenciones", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "notificaciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TenantId = table.Column<int>(type: "integer", nullable: false),
                    Titulo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Mensaje = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Fecha = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    Leida = table.Column<bool>(type: "boolean", nullable: false),
                    Tipo = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Icono = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_notificaciones", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ordenes_trabajo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TenantId = table.Column<int>(type: "integer", nullable: false),
                    Codigo = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Titulo = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Descripcion = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    EquipoId = table.Column<int>(type: "integer", nullable: false),
                    EquipoNombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    EquipoUbicacion = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    Tipo = table.Column<int>(type: "integer", nullable: false),
                    Prioridad = table.Column<int>(type: "integer", nullable: false),
                    Estado = table.Column<int>(type: "integer", nullable: false),
                    SolicitanteId = table.Column<int>(type: "integer", nullable: false),
                    SolicitanteNombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    TecnicoId = table.Column<int>(type: "integer", nullable: true),
                    TecnicoNombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    FechaSolicitud = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    FechaAsignacion = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    FechaInicio = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    FechaCierre = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    SLAHoras = table.Column<int>(type: "integer", nullable: false),
                    CostoEstimado = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    CostoReal = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ReporteSala = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Historial = table.Column<string>(type: "jsonb", nullable: false),
                    Comentarios = table.Column<string>(type: "jsonb", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ordenes_trabajo", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "tecnicos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UsuarioId = table.Column<int>(type: "integer", nullable: false),
                    TenantId = table.Column<int>(type: "integer", nullable: false),
                    Nombre = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Iniciales = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Especialidades = table.Column<string>(type: "jsonb", nullable: false),
                    Disponibilidad = table.Column<int>(type: "integer", nullable: false),
                    CapacidadHorasSemana = table.Column<double>(type: "double precision", nullable: false),
                    HorasAsignadas = table.Column<double>(type: "double precision", nullable: false),
                    HorasSemanaAnterior = table.Column<double>(type: "double precision", nullable: false),
                    OrdenesCompletadas = table.Column<int>(type: "integer", nullable: false),
                    OrdenesActivas = table.Column<int>(type: "integer", nullable: false),
                    Calificacion = table.Column<double>(type: "double precision", nullable: false),
                    Telefono = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Correo = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    Turno = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tecnicos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "usuarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    TenantId = table.Column<int>(type: "integer", nullable: false),
                    Nombres = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Apellidos = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                    Telefono = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Cargo = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    Rol = table.Column<int>(type: "integer", nullable: false),
                    Activo = table.Column<bool>(type: "boolean", nullable: false),
                    PrimerIngreso = table.Column<bool>(type: "boolean", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "timestamp without time zone", nullable: false),
                    UltimoAcceso = table.Column<DateTime>(type: "timestamp without time zone", nullable: true),
                    PasswordHash = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuarios", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_equipos_TenantId_CodigoInventario",
                table: "equipos",
                columns: new[] { "TenantId", "CodigoInventario" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_equipos_TenantId_NumeroSerie",
                table: "equipos",
                columns: new[] { "TenantId", "NumeroSerie" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_eventos_auditoria_Fecha",
                table: "eventos_auditoria",
                column: "Fecha");

            migrationBuilder.CreateIndex(
                name: "IX_eventos_auditoria_TenantId",
                table: "eventos_auditoria",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_formularios_TenantId",
                table: "formularios",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_instituciones_NIT",
                table: "instituciones",
                column: "NIT");

            migrationBuilder.CreateIndex(
                name: "IX_intervenciones_TenantId_OrdenId",
                table: "intervenciones",
                columns: new[] { "TenantId", "OrdenId" });

            migrationBuilder.CreateIndex(
                name: "IX_notificaciones_TenantId",
                table: "notificaciones",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ordenes_trabajo_EquipoId",
                table: "ordenes_trabajo",
                column: "EquipoId");

            migrationBuilder.CreateIndex(
                name: "IX_ordenes_trabajo_TenantId",
                table: "ordenes_trabajo",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_ordenes_trabajo_TenantId_Codigo",
                table: "ordenes_trabajo",
                columns: new[] { "TenantId", "Codigo" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_tecnicos_TenantId",
                table: "tecnicos",
                column: "TenantId");

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_TenantId_Email",
                table: "usuarios",
                columns: new[] { "TenantId", "Email" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "equipos");

            migrationBuilder.DropTable(
                name: "eventos_auditoria");

            migrationBuilder.DropTable(
                name: "formularios");

            migrationBuilder.DropTable(
                name: "instituciones");

            migrationBuilder.DropTable(
                name: "intervenciones");

            migrationBuilder.DropTable(
                name: "notificaciones");

            migrationBuilder.DropTable(
                name: "ordenes_trabajo");

            migrationBuilder.DropTable(
                name: "tecnicos");

            migrationBuilder.DropTable(
                name: "usuarios");
        }
    }
}
