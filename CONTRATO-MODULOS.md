# SITBIO-Desktop — Contrato de construcción de módulos

Proyecto WPF .NET 10, MVVM con `CommunityToolkit.Mvvm 8.4.2`.
Raíz: `C:\Users\Sena\Downloads\Enterprise Healthcare SaaS UI\SITBIO-Desktop`
Namespaces raíz: `SITBIO_Desktop.*` (la raíz del proyecto es `SITBIO_Desktop`, p. ej. `namespace SITBIO_Desktop.ViewModels;`)

---

## 1. Archivos que debes crear

| Ruta | ViewModel | Vista | Namespace de la vista |
|---|---|---|---|
| `ViewModels/EquiposViewModel.cs` | `EquiposViewModel` | `Views/EquiposView.xaml` + `.cs` | `SITBIO_Desktop.Views` |
| `ViewModels/EquipoDetalleViewModel.cs` | `EquipoDetalleViewModel` | `Views/EquipoDetalleView.xaml` + `.cs` | `SITBIO_Desktop.Views` |
| `ViewModels/OrdenesViewModel.cs` | `OrdenesViewModel` | `Views/OrdenesView.xaml` + `.cs` | `SITBIO_Desktop.Views` |
| `ViewModels/OrdenDetalleViewModel.cs` | `OrdenDetalleViewModel` | `Views/OrdenDetalleView.xaml` + `.cs` | `SITBIO_Desktop.Views` |
| `ViewModels/IntervencionViewModel.cs` | `IntervencionViewModel` | `Views/IntervencionView.xaml` + `.cs` | `SITBIO_Desktop.Views` |
| `ViewModels/TecnicosViewModel.cs` | `TecnicosViewModel` | `Views/TecnicosView.xaml` + `.cs` | `SITBIO_Desktop.Views` |
| `ViewModels/FormulariosViewModel.cs` | `FormulariosViewModel` | `Views/FormulariosView.xaml` + `.cs` | `SITBIO_Desktop.Views` |
| `ViewModels/FormBuilderViewModel.cs` | `FormBuilderViewModel` | `Views/FormBuilderView.xaml` + `.cs` | `SITBIO_Desktop.Views` |
| `ViewModels/ReportesViewModel.cs` | `ReportesViewModel` | `Views/ReportesView.xaml` + `.cs` | `SITBIO_Desktop.Views` |
| `ViewModels/UsuariosViewModel.cs` | `UsuariosViewModel` | `Views/UsuariosView.xaml` + `.cs` | `SITBIO_Desktop.Views` |
| `ViewModels/UsuarioDetalleViewModel.cs` | `UsuarioDetalleViewModel` | `Views/UsuarioDetalleView.xaml` + `.cs` | `SITBIO_Desktop.Views` |
| `ViewModels/ConfiguracionViewModel.cs` | `ConfiguracionViewModel` | `Views/ConfiguracionView.xaml` + `.cs` | `SITBIO_Desktop.Views` |
| `ViewModels/AuditoriaViewModel.cs` | `AuditoriaViewModel` | `Views/AuditoriaView.xaml` + `.cs` | `SITBIO_Desktop.Views` |
| `ViewModels/ProximamenteViewModel.cs` | `ProximamenteViewModel` | `Views/ProximamenteView.xaml` + `.cs` | `SITBIO_Desktop.Views` |

La vista XAML SIEMPRE declara `x:Class="SITBIO_Desktop.Views.<Nombre>Vista"` y la clase code-behind es `public partial class <Nombre>Vista : UserControl` con `InitializeComponent()` en el constructor.
Referencia obligatoria de estilo: leer `Views/DashboardView.xaml`, `Views/DashboardView.xaml.cs` y `ViewModels/DashboardViewModel.cs` antes de empezar.

---

## 2. Firmas de constructor EXACTAS (las registra App.xaml.cs)

**Módulos de lista** (`Equipos`, `Ordenes`, `Tecnicos`, `Formularios`, `Reportes`, `Usuarios`, `Configuracion`, `Auditoria`):

```csharp
public EquiposViewModel(ISesionService sesion, IRepositorioDatos datos,
    IAuditoriaService auditoria, IToastService toast,
    INavegacionService navegacion, IExportacionService exportacion)
```

**Módulos de detalle** (`EquipoDetalle`, `OrdenDetalle`, `Intervencion`, `UsuarioDetalle`, `FormBuilder`): primer parámetro `int id` y después los mismos seis servicios, en el mismo orden:

```csharp
public EquipoDetalleViewModel(int id, ISesionService sesion, IRepositorioDatos datos,
    IAuditoriaService auditoria, IToastService toast,
    INavegacionService navegacion, IExportacionService exportacion)
```

`ProximamenteViewModel` solo recibe `string clave` (`"calibraciones"` o `"inventario"`) y arma su propio título, icono y mensaje.

---

## 3. Reglas obligatorias

1. **NO** modificar `App.xaml`, `App.xaml.cs`, `MainWindow.*`, `Themes/*`, `Controls/*`, `Core/*`, `Services/*`, `Models/*` ni ningún archivo que no te hayan asignado. Ellos añaden los `DataTemplate` y el registro de rutas.
2. **NO** ejecutar `dotnet build` (hay compilaciones concurrentes). Se compila al final.
3. **NO** agregar paquetes NuGet.
4. Textos de interfaz **en español**; comentarios en español, en serio y con acentos.
5. MVVM: lógica en el ViewModel con `[ObservableProperty]` y `[RelayCommand]` de CommunityToolkit. El code-behind solo para eventos de la vista (doble clic, cambio de selector, foco).
6. **RBAC**: oculta o deshabilita acciones que el rol no puede hacer usando `_sesion.PuedeEditar(AreaAplicacion.X)` / `_sesion.IntentarAcceso(AreaAplicacion.X)`. Expón la propiedad como `public bool PuedeX => ...`.
7. **Auditoría**: registra toda escritura con `_auditoria.Registrar(AccionAuditoria.Creacion|Actualizacion|Eliminacion|Asignacion|CambioEstado|Consulta|Exportacion, "<Entidad>", "<referencia>", "<detalle>")`.
8. **Toast**: confirma operaciones con `_toast.Exito(...)`, `_toast.Informacion(...)`, `_toast.Advertencia(...)`.
9. **Navegación**: `_navegacion.Navegar(Rutas.X, parametro)`; los detalles exponen `VolverCommand` que regresa al módulo contenedor. Rutas disponibles: `Dashboard, Equipos, EquipoDetalle, Ordenes, OrdenDetalle, Intervencion, Tecnicos, Formularios, FormBuilder, Reportes, Usuarios, UsuarioDetalle, Configuracion, Auditoria, Calibraciones, Inventario`.
10. **Aislamiento por tenant**: SIEMPRE filtra con `_datos.XxxDe(tenantId)` donde `tenantId = _sesion.TenantActual?.Id ?? 0`.
11. Datos demo en memoria (no crear nuevos servicios de datos). Ediciones en memoria: modificar el objeto devuelto por `_datos.Xxx(id)` y avisar con toast; `AgregarX`/`EliminarX` existen para Equipo/Usuario/Orden/Formulario.

---

## 4. Recursos XAML disponibles (claves)

**Colores/pinceles** (`Themes/Colors.xaml`):
`PrimaryBrush, PrimaryDarkBrush, PrimaryDeepBrush, PrimaryLightBrush, PrimarySoftBrush, PrimarySofterBrush, AccentBrush, AccentDarkBrush, AccentSoftBrush, BackgroundBrush, SurfaceBrush, SurfaceAltBrush, SurfaceHoverBrush, SidebarBrush, TopBarBrush, BorderBrush, BorderStrongBrush, DividerBrush, TextPrimaryBrush, TextSecondaryBrush, TextMutedBrush, TextOnPrimaryBrush, SuccessBrush, SuccessSoftBrush, WarningBrush, WarningSoftBrush, DangerBrush, DangerSoftBrush, InfoBrush, InfoSoftBrush, PurpleBrush, PurpleSoftBrush, Chart1Brush…Chart7Brush`

**Tipografía** (`Themes/Typography.xaml`): `Texto.Titulo, Texto.Seccion, Texto.Tarjeta, Texto.Cuerpo, Texto.Etiqueta, Texto.Auxiliar, Texto.Dato, Texto.Numero, Texto.Ruta, Texto.CabeceraTabla`, fuente `Font.Ui`.

**Botones/controles** (`Themes/Controls.xaml`): `Boton, Boton.Secundario, Boton.Contorno, Boton.Fantasma, Boton.Peligro, Boton.Exito, Boton.Acento, Boton.Pequeno, Boton.Icono, Boton.Icono.Contorno` (el `Tag`/`Content` es un glifo: botones con `<TextBlock Text="" FontFamily="{StaticResource IconFont}"/>`), `Tarjeta, Tarjeta.Sencilla, Campo, AreaTexto, Campo.Contrasena, Selector, Casilla, Alternador, Progreso, IconoTexto`, y `ElementoSelector`.

**Chips y tablas** (`Themes/Lists.xaml`): `Chip`, `Chip.Texto`, `Tabla` (implícito en todo DataGrid), `Tabla.Cabecera`, `Tabla.Fila`, `Tabla.Celda`, `Lista.Elemento`, `Pestana`, `Pestanas`.

**Convertidores** (claves en `Themes/Controls.xaml`, clases en `Core/Converters.cs`):

| Clave | Entrada | Salida |
|---|---|---|
| `CvtEstadoEquipoColor` / `CvtEstadoEquipoFondo` | `EstadoEquipo` | color / fondo del chip |
| `CvtOrdenColor` / `CvtOrdenFondo` | `EstadoOrden` | color / fondo del chip |
| `CvtPrioridadColor` / `CvtPrioridadFondo` | `Prioridad` | color / fondo del chip |
| `CvtTipoColor` | `TipoMantenimiento` | color |
| `CvtCargaColor` | número 0-100 | verde/ámbar/rojo |
| `CvtSlaColor` | número ≥90/≥75/<75 | verde/ámbar/rojo |
| `CvtTextoVisible` | string | Visible/Collapsed si tiene texto |
| `CvtNuloVisible` | object/string | Visible si tiene valor |
| `CvtInverso` | bool | bool invertido |
| `CvtBoolVisible` | bool | Visible si true |
| `CvtNoBoolVisible` | bool | Visible si false |
| `CvtCero` | número | Visible si > 0 |
| `CvtHex` / `CvtHexColor` | `#RRGGBB` | Brush / Color |
| `CvtColeccionVisible` | colección | Visible si **tiene** ítems |
| `CvtColeccionVacia` | colección | Visible si **está vacía** (estado vacío) |
| `CvtNivelColor` / `CvtNivelTexto` | `NivelAcceso` | color / "Lectura y escritura" |
| `CvtMoneda` | decimal | `$ 1.234` compacto |
| `CvtFechaRelativa` | DateTime | "hace 5 min" |

**Iconografía**: `FontFamily="{StaticResource IconFont}"` con glifos (ustedes usan `&#xExxx;` en XAML o `"\uExxx"` en C#). Verificados: Inicio `E80F`, Equipos `E912`, Ordenes `E90F`, Intervención `E77F`, Técnicos `E716`, Formularios `E729`, Reportes `E9F9`, Usuarios `E902`, Configuración `E713`, Auditoría `E81C`, Agregar `E710`, Editar `E70F`, Eliminar `E74D`, Buscar `E721`, Filtrar `E71C`, Cerrar `E711`, Aceptar `E73E`, Exportar `E8E5`, Asignar `E748`, Advertencia `E7BA`, Información `E946`, Error `E783`, Calendario `E787`, Reloj `E823`, Ubicación `E819`, Chevron derecha `E76A`, Chevron izquierda `E76B`, Flecha arriba `E74A`, Flecha abajo `E74B`, Descargar `E896`, Detalle `E8A7`, Ficha `E81D`, Escudo `E72E`, Sincronizar `E895`, Volver `E72A`.

---

## 5. Controles reutilizables (`xmlns:ctrl="clr-namespace:SITBIO_Desktop.Controls"`)

```xml
<!-- Encabezado estándar de módulo -->
<ctrl:PageHeader Titulo="..." Subtitulo="..." Icono="&#xE912;"
                 Contador="12" NotaContador="registros">
    <ctrl:PageHeader.Acciones>
        <StackPanel Orientation="Horizontal"> ...botones... </StackPanel>
    </ctrl:PageHeader.Acciones>
</ctrl:PageHeader>

<!-- Tarjeta KPI (o use ItemsSource de ObservableCollection<TarjetaKpi>) -->
<ctrl:KpiCard Titulo="..." Valor="..." Unidad="%" Icono="&#xE9F9;" Ayuda="..."
               DeltaTexto="..." DeltaIcono="&#xE74A;" Leyenda="..." />

<!-- Estado vacío: SOLO visible si la colección está vacía -->
<ctrl:EmptyState Icono="&#xE721;" Titulo="Sin resultados"
   Mensaje="..." TextoAccion="Nuevo registro" AccionCommand="{Binding NuevoCommand}"/>
   Visibility="{Binding Items, Converter={StaticResource CvtColeccionVacia}}"/>

<!-- Diálogo modal: colóquelo como ÚLTIMO hijo del Grid raíz de la vista -->
<ctrl:Dialogo Visible="{Binding DialogoAbierto}" Titulo="..." Subtitulo="..."
   Icono="&#xE710;" Ancho="620" Contenido="{Binding ContenidoDialogo}"
   TextoConfirmar="Guardar" TextoCancelar="Cancelar"
   ConfirmarCommand="{Binding GuardarCommand}"
   CancelarCommand="{Binding CancelarCommand}"/>

<!-- Gráficas -->
<ctrl:GraficaBarras ItemsSource="{Binding Serie}" Altura="180" AnchoBarra="30"
                    Formato="0.#" ColorBarra="{StaticResource PrimaryBrush}"/>
<ctrl:GraficaLinea ItemsSource="{Binding Serie}" Altura="150" Formato="0.0"/>
<ctrl:GraficaDona ItemsSource="{Binding Distribucion}" Tamanio="170"
                  TextoCentro="58" SubtextoCentro="equipos" MostrarLeyenda="True"/>
```

`ItemsSource` de gráficas acepta `IEnumerable<PuntoSerie>` (`Etiqueta`, `Valor`) y `IEnumerable<SegmentoDistribucion>` (`Etiqueta`, `Valor`, `Color` = `"#RRGGBB"`).

---

## 6. Servicios (inyección por constructor, ya definidos en `Core/AppServicios.cs`)

```csharp
AppServicios.Datos        // IRepositorioDatos (leer §7)
AppServicios.Sesion       // ISesionService: UsuarioActual, TenantActual, Tenants, IntentarAcceso(area), Nivel(area), PuedeEditar(area)
AppServicios.Auditoria    // IAuditoriaService.Registrar(accion, entidad, referencia, detalle)
AppServicios.Navegacion   // INavegacionService.Navegar(clave, param), PuedeNavegar(clave)
AppServicios.Toast        // IToastService: Exito/Informacion/Advertencia/Error (mensaje, titulo)
AppServicios.Exportacion  // IExportacionService: ExportarIndicadoresExcel/Pdf(tenantId), ExportarOrdenesExcel/Pdf(tenantId, ordenes), ExportarInventarioExcel(tenantId)
```

Formateo: `Formateo.Num(double)`, `Formateo.Entero(int)`, `Formateo.Porcentaje(double)`, `Formateo.Dinero(decimal)`, `Formateo.Fecha(DateTime)`, `Formateo.FechaHora(DateTime)` (clase `SITBIO_Desktop.Core.Formateo`).
Paleta: `Paleta.Severidad("exito|advertencia|peligro|info|acento|neutro")` → `(Brush Color, Brush Fondo)`; `Paleta.Pincel("#hex")`, `Paleta.ColorFromString`. Clase `SITBIO_Desktop.Core.Paleta`.
KPI reutilizable: `ViewModels.TarjetaKpi.Crear(titulo, valor, icono, severidad, delta, leyenda, ayuda, deltaIcono, unidad)`.

---

## 7. Datos (leer `Services/IRepositorioDatos.cs`, `Services/DemoDataProvider.cs` y `Models/*.cs`)

Métodos por tenant: `EquiposDe, OrdenesDe, TecnicosDe, IntervencionesDe, FormulariosDe, AuditoriaDe, NotificacionesDe, UsuariosDe`.
Búsquedas por id: `Equipo(id), Orden(id), OrdenPorCodigo(codigo), Tecnico(id), Usuario(id), Formulario(id), IntervencionDeOrden(ordenId)`.
Escritura: `AgregarOrden, AgregarEquipo, AgregarUsuario, AgregarFormulario, EliminarEquipo, EliminarFormulario, SiguienteCodigoOrden(tenantId)`.
Estadísticas: `Indicadores(tenantId)` → `IndicadoresDashboard` (EquiposRegistrados, EquiposOperativos, OrdenesAbiertas, OrdenesCriticas, PreventivosVencidos, CalibracionesProximas, DisponibilidadPorcentaje, CumplimientoSLA, MtbfHoras, MttrHoras, CostoMes, CostoMesAnterior, VariacionCosto), `SerieMantenimientos(tenantId, meses)`, `DistribucionPorEstado(tenantId)`, `DistribucionPorTipo(tenantId)`, `DistribucionEquiposPorCategoria(tenantId)`.

Modelos clave y sus miembros calculados (ver `Models/`): `Equipo` (`EstadoTexto`, `PreventivoVencido`, `PorcentajeVida`, `OrdenesAbiertas`, `CamposDinamicos`), `OrdenTrabajo` (`Codigo` tipo `SIT-2026-000123`, `EstadoTexto`, `PrioridadTexto`, `TipoTexto`, `EstaAbierta`, `FechaLimite`, `HorasRestantes`, `ProgresoSLA`, `SLAVencido`, `Columna` (`ColumnaKanban`), `Historial` (`List<EventoOrden>`), `Comentarios`, `CostoEstimado/Real`), `Tecnico` (carga por técnico), `Intervencion`, `FormularioDinamico` + `CampoFormulario` (campos dinámicos por tenant), `Usuario` (`NombreCompleto`, `Iniciales`, `NombreRol`, `SiglaRol`, `Rol`), `EventoAuditoria` (`AccionTexto`, `Icono`, `Usuario`, `Rol`, `Entidad`, `Referencia`, `Detalle`, `Fecha`), `Notificacion`.

Enums en `Core/Enums.cs`: `EstadoEquipo, EstadoOrden, Prioridad, TipoMantenimiento, ColumnaKanban, RolSistema, AreaAplicacion, NivelAcceso, AccionAuditoria, TipoCampo`.

---

## 8. Patrones visuales (copiar y adaptar)

**Fila de filtros sobre tarjeta**
```xml
<Border Style="{StaticResource Tarjeta}" Padding="16,14" Margin="0,0,0,16">
    <Grid>
        <Grid.ColumnDefinitions>
            <ColumnDefinition Width="2*"/>
            <ColumnDefinition Width="*"/>
            <ColumnDefinition Width="*"/>
            <ColumnDefinition Width="Auto"/>
        </Grid.ColumnDefinitions>
        <TextBox Style="{StaticResource Campo}" Tag="&#xE721;"
                 Text="{Binding Busqueda, UpdateSourceTrigger=PropertyChanged}"
                 ToolTip="Buscar por código, nombre o ubicación"/>
        <ComboBox Grid.Column="1" Style="{StaticResource Selector}" Margin="10,0"
                  ItemsSource="{Binding Estados}" SelectedItem="{Binding EstadoSeleccionado}"/>
        <ComboBox Grid.Column="2" Style="{StaticResource Selector}" Margin="0,0,10,0"
                  ItemsSource="{Binding Tipos}" SelectedItem="{Binding TipoSeleccionado}"/>
        <Button Grid.Column="3" Style="{StaticResource Boton.Contorno}"
                Command="{Binding LimpiarFiltrosCommand}" Content="Limpiar"/>
    </Grid>
</Border>
```
El `Tag` del `Campo` muestra un glifo a la izquierda del texto.

**Chip de estado**
```xml
<Border Style="{StaticResource Chip}" BorderThickness="1"
        Background="{Binding Estado, Converter={StaticResource CvtOrdenFondo}}"
        BorderBrush="{Binding Estado, Converter={StaticResource CvtOrdenColor}}">
    <TextBlock Text="{Binding EstadoTexto}" Style="{StaticResource Chip.Texto}"
               Foreground="{Binding Estado, Converter={StaticResource CvtOrdenColor}}"/>
</Border>
```

**Tarjeta con encabezado y acción a la derecha**
```xml
<Border Style="{StaticResource Tarjeta}" Padding="20,16">
    <DockPanel>
        <Grid DockPanel.Dock="Top" Margin="0,0,0,12">
            <StackPanel>
                <TextBlock Text="Título" Style="{StaticResource Texto.Tarjeta}"/>
                <TextBlock Text="Subtítulo" Style="{StaticResource Texto.Auxiliar}" Margin="0,4,0,0"/>
            </StackPanel>
            <Button Style="{StaticResource Boton.Fantasma}" Padding="8,5"
                    HorizontalAlignment="Right" Command="{Binding AlgoCommand}"
                    Content="Ver todo "/>
        </StackPanel>
        ...contenido...
    </DockPanel>
</Border>
```

**DataGrid** (el estilo es implícito; solo agrégale columnas) y para filas con estado vacío, superponer `ctrl:EmptyState` en un `Grid` con `Visibility="{Binding Items, Converter={StaticResource CvtColeccionVacia}}"`.

**Columna de acciones**
```xml
<DataGridTemplateColumn Width="60">
    <DataGridTemplateColumn.CellTemplate>
        <DataTemplate>
            <Button Style="{StaticResource Boton.Icono.Contorno}" Padding="6" Content=""
                    ToolTip="Abrir"
                    Command="{Binding DataContext.VerCommand, RelativeSource={RelativeSource AncestorType=UserControl}}"
                    CommandParameter="{Binding}"/>
        </DataTemplate>
    </DataGridTemplateColumn.CellTemplate>
</DataGridTemplateColumn>
```

**Vista base**: `ScrollViewer` vertical → `StackPanel Margin="28,22,28,34"` → `PageHeader` → contenido en `Grid` de 1 o 2 columnas con tarjetas separadas por `Margin="0,18,0,0"`. Nunca scrollbars horizontales.

---

## 9. Checklist mínimo por módulo

- **Equipos**: búsqueda + filtros (categoría, estado), tabla (código, equipo, categoría, ubicación, estado, próximo mantenimiento, acciones), fila de KPIs, alta con `Dialogo`, exportar `ExportarInventarioExcel`, clic → `EquipoDetalle`.
- **EquipoDetalle**: cabecera con estado y código, tarjeta de datos generales + hoja de vida (fechas, vida útil con `Progreso`), campos dinámicos del tenant, listado de órdenes del equipo, gráfica de intervenciones, botón editar y `VolverCommand`.
- **Ordenes**: **vista de tabla y kanban conmutables** (botón que alterna `VistaTabla`), filtros (búsqueda, estado, prioridad, tipo), KPIs, botón "Nueva orden" con `Dialogo` (título, equipo, tipo, prioridad, descripción; usa `SiguienteCodigoOrden`), exportación Excel/PDF de la selección, clic → `OrdenDetalle`.
- **OrdenDetalle**: cabecera con código único, estado, prioridad y SLA (barra `Progreso`), datos de la orden y del equipo, **línea de tiempo** `Historial`, comentarios con envío, acciones de cambio de estado (asignar técnico, iniciar, verificar, cerrar) visibles según RBAC, botón a `Intervencion`.
- **Intervencion**: ficha de la intervención asociada a la orden (`IntervencionDeOrden`): checklist de intervención, repuestos/costos, campos de firma y observaciones, guardar con auditoría `CambioEstado`.
- **Tecnicos**: tarjetas de técnicos con carga actual (barra con `CvtCargaColor`), especialidades, disponibilidad, asignación de orden a técnico mediante `Dialogo`, tabla de órdenes asignadas.
- **Formularios**: lista de formularios dinámicos (nombre, tipo, campos, versiones, estado), alta/edición con `Dialogo`, botón al `FormBuilder`, exportación.
- **FormBuilder**: constructor de campos dinámicos: lista de campos con tipo, etiqueta, obligatorio, ordenar (subir/bajar), agregar/eliminar campo, vista previa del formulario, guardar con auditoría.
- **Reportes**: selector de periodo + KPIs (`TarjetaKpi.Crear`), gráficas (barras de mantenimiento, dona por estado/tipo, línea de costos), tabla resumen, 4 botones de exportación (indicadores Excel/PDF, órdenes Excel/PDF).
- **Usuarios**: búsqueda y filtro por rol/estado, tabla, alta con `Dialogo` (rol y tenant), clic → `UsuarioDetalle`.
- **UsuarioDetalle**: perfil con iniciales, datos, rol, último acceso, **matriz RBAC** de su rol vs. `AreaAplicacion` usando `PermisosRbac.Nivel(rol, area)` con `CvtNivelColor`/`CvtNivelTexto`, historial de auditoría del usuario.
- **Configuracion**: pestañas (`Pestanas`) con: institución activa (multi-tenant), parámetros operativos (SLA, preventivos, vigencia), campos dinámicos por tenant, y matriz completa de permisos de los 6 roles.
- **Auditoria**: filtros (acción, usuario, fecha, búsqueda), tabla inmutable (fecha, usuario, rol, acción, entidad, referencia, detalle, dispositivo), KPIs de eventos, exportación, aviso de que la bitácora no se puede modificar.
- **Proximamente**: pantalla informativa con `EmptyState`, icono, descripción del módulo y qué entregará la próxima fase (usa `ctrl:EmptyState` dentro de una tarjeta).
