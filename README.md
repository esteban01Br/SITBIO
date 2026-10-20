# SITBIO — Sistema de Información Técnica y Biomédica

Aplicación de **escritorio** para la gestión de mantenimiento de equipos biomédicos en
instituciones de salud. Construida en **C# / .NET 10 / WPF** con el patrón **MVVM**.

> **Estado actual (fase 2, completada)**: backend **API REST ASP.NET Core 8** + **PostgreSQL 16**
> en Docker (`src/backend/SITBIO.Api/`, JWT + BCrypt + RBAC + aislamiento por tenant) y cliente
> WPF conectado mediante un interruptor **Demo/Api** (`RepositorioApi` + `sitbio.config.json`).
> Fase 3 (MongoDB de intervenciones/formularios/auditoría) según la arquitectura oficial
> ADSO-ARQ-01 (plan en la sección 4; documento fuente en `docs/arquitectura/`).

---

## Índice

1. [Qué es SITBIO](#1-qué-es-sitbio)
2. [Cómo ejecutarla](#2-cómo-ejecutarla)
3. [Stack tecnológico](#3-stack-tecnológico)
4. [Base de datos: qué se usa hoy y qué viene](#4-base-de-datos-qué-se-usa-hoy-y-qué-viene)
5. [Arquitectura MVVM (cómo está organizado el código)](#5-arquitectura-mvvm-cómo-está-organizado-el-código)
6. [Los módulos de la aplicación](#6-los-módulos-de-la-aplicación)
7. [Reglas del negocio (metodología)](#7-reglas-del-negocio-metodología)
8. [Diseño y tema visual](#8-diseño-y-tema-visual)
9. [Bugs corregidos durante el desarrollo](#9-bugs-corregidos-durante-el-desarrollo)
10. [Git: flujo GitFlow y buenas prácticas](#10-git-flujo-gitflow-y-buenas-prácticas)
11. [Siguientes pasos](#11-siguientes-pasos)

---

## 1. Qué es SITBIO

SITBIO es una plataforma SaaS de escritorio para que un hospital o clínica administre:

- **Hoja de vida de equipos biomédicos**: dónde están, en qué estado, cuándo el próximo mantenimiento.
- **Órdenes de trabajo**: solicitud → asignación → intervención → verificación → cierre, con
  **código único de seguimiento** (`SIT-2026-000123`), prioridades, SLA y vista **kanban**.
- **Intervenciones**: checklist técnica, repuestos, costos y firma.
- **Técnicos**: especialidades, disponibilidad y carga de trabajo.
- **Formularios dinámicos**: cada institución define sus propios campos (tipo de mantenimiento, etc.).
- **Reportes e indicadores**: disponibilidad, MTBF, MTTR, cumplimiento de SLA, costos; exportables a **PDF y Excel**.
- **Bitácora de auditoría**: registro inmutable de quién hizo qué y cuándo.
- **Control de acceso por rol (RBAC)**: 6 roles con permisos distintos por área.
- **Multi-institución (multi-tenant)**: cada consulta está aislada por institución activa.

Los requisitos formales provienen de la documentación del proyecto (metodología de los dos `.docx`):
RBAC multi-tenant, código único, kanban, auditoría inmutable y exportación PDF/Excel.

---

## 2. Cómo ejecutarla

**Requisitos**: Windows 10/11 y el **.NET 10 SDK** instalado (o Visual Studio con soporte .NET 10).
Para el **modo Api** se necesita además **Docker Desktop** (PostgreSQL corre en contenedor).

### Modo Demo (por defecto) — sin servidor ni base de datos

```powershell
cd "C:\Users\Sena\Downloads\Enterprise Healthcare SaaS UI\SITBIO-Desktop"
dotnet run --project src/client/SITBIO-Desktop.csproj
```

### En Visual Studio

Abrir `SITBIO.sln` (o `src/client/SITBIO-Desktop.csproj`) y presionar **F5**.

> Con la configuración por defecto (`"ModoDatos": "Demo"` en `src/client/sitbio.config.json`)
> arranca directo con datos demo en memoria: no necesita internet, servidor ni base de datos.

### Modo Api — API REST + PostgreSQL (fase 2)

```powershell
# 1) Base de datos (una sola vez por equipo):
docker run -d --name sitbio-pg -p 5433:5432 `
  -e POSTGRES_USER=sitbio_dev -e POSTGRES_PASSWORD=sitbio_dev -e POSTGRES_DB=sitbio `
  -v sitbio-pgdata:/var/lib/postgresql/data postgres:16
# (si el contenedor ya existe: docker start sitbio-pg)

# 2) API REST — http://localhost:5088 (Swagger en /swagger):
dotnet run --project src/backend/SITBIO.Api

# 3) Cliente en modo Api, sin tocar archivos:
$env:SITBIO_MODO = "Api"
dotnet run --project src/client/SITBIO-Desktop.csproj
```

Otro camino (persistente): editar `src/client/sitbio.config.json` y poner `"ModoDatos": "Api"`.
La variable `SITBIO_URL_API` cambia la URL del backend (por defecto `http://localhost:5088`).

> En modo Api los datos **persisten** en PostgreSQL, el login usa **BCrypt + JWT**, cada
> escritura se sirve desde la API (PUT/POST/DELETE) y la sesión queda atada a su
> institución: para operar sobre otra hay que cerrar sesión y volver a entrar.
> Si la API no responde, la app sigue operativa y muestra un aviso en lugar de cerrarse.

### Credenciales de demostración

En la pantalla de inicio hay botones de **"Accesos de demostración por rol"** que completan los
campos automáticamente. También se pueden digitar:

| Rol | Correo | Contraseña |
|---|---|---|
| Administrador | `admin@sitbio.com` | `Admin123!` |
| Coordinador Biomédico | `coordinador@sitbio.com` | `Coord123!` |
| Técnico | `tecnico@sitbio.com` | `Tec123!` |
| Personal Clínico | `enfermera@sitbio.com` | `Clini123!` |
| Director | `director@sitbio.com` | `Dir123!` |
| Regulador / Auditor | `auditor@sitbio.com` | `Audit123!` |

La contraseña depende del rol (`DemoDataProvider.ClaveDemo`). La institución activa por defecto es la
**Clínica San José de la Montaña**; hay usuarios de otras dos instituciones para probar el aislamiento
multi-tenant (`admin2@sitbio.com`, `admin3@sitbio.com`, etc. con la misma lógica de contraseñas).

---

## 3. Stack tecnológico

| Tecnología | Versión | Para qué sirve |
|---|---|---|
| C# / .NET | 10 (`net10.0-windows`) | lenguaje y runtime |
| WPF (XAML) | incluido en .NET | ventanas, estilos y vistas |
| CommunityToolkit.Mvvm | 8.4.2 | MVVM: `[ObservableProperty]`, `[RelayCommand]` |
| ClosedXML | 0.105.1 | exportación a **Excel** |
| QuestPDF | 2026.9.1 | exportación a **PDF** |
| ASP.NET Core Web API | 8 (`net8.0`) | backend REST: JWT, BCrypt, Swagger, RBAC |
| EF Core + Npgsql | 8.0.x | ORM y migraciones contra PostgreSQL |
| PostgreSQL | 16 (Docker, puerto **5433**) | base de datos del modo Api (BD `sitbio`) |
| Segoe MDL2 Assets | fuente del sistema | iconografía (`&#xE710;`, ``, …) |

No hay React, Node, npm ni web: es **solo C#/.NET**, como se pidió para esta fase.

---

## 4. Base de datos: qué se usa hoy y qué viene

### 4.1 ¿Qué base de datos se está utilizando?

**Depende del interruptor `ModoDatos` (fase 2): `Demo` por defecto, `Api` para PostgreSQL.**

En **modo Demo** los datos viven **en memoria RAM**:

- `src/client/Services/DemoDataProvider.cs` implementa la interfaz `IRepositorioDatos` usando
  `List<Equipo>`, `List<OrdenTrabajo>`, `List<Usuario>`, etc.
- Al arrancar la app, el `DemoDataProvider` **siembra** los datos de demostración
  (métodos `SembrarEquipos`, `SembrarOrdenes`, `SembrarUsuarios`, `SembrarTecnicos`, …):
  3 instituciones, ~35 equipos, ~120 órdenes, técnicos, formularios, usuarios y bitácora.
- Al cerrar la app **todo se pierde**: los altas, ediciones y borrados hechos durante la sesión no persisten.
- No hay `connection string` ni PostgreSQL: es el modo que arranca en cualquier equipo sin instalar nada.

En **modo Api** (fase 2b) los mismos datos viven en **PostgreSQL 16** y viajan por HTTP:

- `src/client/Services/RepositorioApi.cs` implementa la misma interfaz con `HttpClient` +
  el JWT de `POST /api/v1/auth/login`; las escrituras hacen PUT/POST/DELETE contra la API
  y las lecturas se cachean por sesión (se invalidan en cada escritura).
- El backend (`src/backend/SITBIO.Api/`) usa EF Core + `SITBIOContext`, **migraciones
  automáticas al arrancar** (`MigrateAsync`) y una siembra equivalente (`Data/SemillaDatos.cs`)
  que solo se ejecuta si las tablas están vacías.
- La conexión apunta a `localhost:5433` (contenedor Docker `sitbio-pg`); los usuarios,
  contraseñas (BCrypt) y roles son los mismos de la tabla de la sección 2.

### 4.2 ¿Por qué se hizo así?

1. **Fase 1 = interfaz**: el objetivo de esta entrega era tener los 15 módulos completos y verificados.
2. **Desacoplamiento**: toda la lógica habla con la *interfaz* `IRepositorioDatos`, nunca con la clase
   concreta. Cambiar la base de datos es cambiar **una línea** (ver 4.4).
3. **Arranque cero**: el proyecto compila y corre en cualquier computador sin instalar nada extra.

### 4.3 Cómo quedó preparado el cambio (y ya aplicado en la fase 2)

```
ViewModels  ──usan──▶  IRepositorioDatos (interfaz)  ◀──implementa──  DemoDataProvider  (Demo: memoria)
                                                    ◀──implementa──  RepositorioApi    (Api: HTTP + JWT → API REST → PostgreSQL)
```

Puntos clave del diseño:

- **Un único punto de inyección**: `src/client/Core/AppServicios.cs`, método `Inicializar()`
  lee `sitbio.config.json` y elige el implementante:

  ```csharp
  var cfg = ConfiguracionCliente.Cargar();
  Datos = cfg.Modo == ModoDatos.Api
      ? new RepositorioApi(cfg.UrlApi)   // fase 2: API REST + JWT
      : new DemoDataProvider();          // modo por defecto
  ```

- **Aislamiento por tenant en la interfaz**: todos los métodos de consulta filtran por institución
  (`EquiposDe(tenantId)`, `OrdenesDe(tenantId)`, `AuditoriaDe(tenantId)`, …). Ese `tenant_id`
  será la columna que aísle los datos en cada tabla (requisito RSK-03).
- **Código único centralizado**: `SiguienteCodigoOrden(tenantId)` genera `SIT-2026-000123`;
  en la BD será un contador o secuencia con **índice único por tenant**.
- **Auditoría por evento**: toda escritura pasa por `AgregarAuditoria`; en la BD será una tabla
  **append-only** (solo INSERT, nunca UPDATE ni DELETE).
- **Estadísticas del panel** (`Indicadores`, `SerieMantenimientos`, `DistribucionPorEstado`, …)
  se calcularán en el backend (consultas en PostgreSQL) y el cliente solo las consume.
- El propio `AppServicios` está comentado para indicar que allí se sustituirá por un contenedor DI real cuando
  llegue EF Core, **sin tocar los ViewModels**.

### 4.4 Plan de datos según la arquitectura oficial (ADSO-ARQ-01)

> Documento fuente: `docs/arquitectura/Arquitectura_Datos_y_Diseno_Tecnico_ADSO_SENA_APA7_v1.docx`
> (equipo ADSO, aprobado el 19/10/2026). Incluye diagramas C4, las decisiones ADR-001..003 y
> los lineamientos de seguridad SEC-001..004.

La arquitectura aprobada es **cliente–servidor con persistencia híbrida**:

| Capa | Tecnología | Responsabilidad |
|---|---|---|
| Cliente | WPF + MVVM (este proyecto) | solo presentación; **prohibido** escribir SQL o acceder a la BD desde las vistas |
| API REST | ASP.NET Core 8 Web API | autenticación **JWT** (HS256, 30 min, claims `id_usuario`/`tenant_id`/`rol`), contraseñas con **BCrypt** (cost ≥ 11), RBAC y filtrado obligatorio por `tenant_id` |
| SQL | **PostgreSQL 16** + EF Core | `TENANT`, `USUARIO`, `EQUIPO_BIOMEDICO`, `ORDEN_TRABAJO`: datos rígidos y transaccionales, llaves UUID, índices únicos compuestos (`tenant_id,numero_serie`, `tenant_id,correo`) y borrado lógico |
| NoSQL | **MongoDB 7** (BSON) | `intervenciones` (con `camposDinamicos{}`), `configuracion_campos` y `bitacora_auditoria`: estructura flexible, validación con JSON Schema e índices (p. ej. `{tenant_id, equipo_id}`) |
| Analítica | Python 3.12 (PyMongo, Pandas, Scikit-learn) | pipeline **K-Means** de agrupación de mantenimientos (ADR-003) expuesto en `/api/v1/analytics/kmeans` |

**¿Por qué híbrido (ADR-002)?** Las entidades rígidas y transaccionales van a SQL con ACID y
aislamiento multi-tenant; las intervenciones, cuyos campos varían según el tipo de equipo, van a
documentos MongoDB **sin alterar el esquema relacional**.

**Cómo llega esto al cliente WPF sin reescribir la aplicación:**

1. El cliente **no** hablará con la base de datos: consumirá la API REST (`/api/v1/...`)
   con el token JWT en la cabecera `Authorization: Bearer ...`.
2. `AppServicios.Inicializar()` reemplazará `DemoDataProvider` por un
   `RepositorioApi : IRepositorioDatos` (HttpClient + JSON) → **los ViewModels no se modifican**
   porque siguen hablando con la misma interfaz.
3. Del lado del backend (carpeta futura `/src/backend`): `SITBIOContext` con EF Core y
   migraciones contra PostgreSQL, siembra equivalente a `DemoDataProvider`, middleware JWT/RBAC
   y servicio de intervenciones sobre MongoDB.
4. Seguridad (SEC-003): el filtro `tenant_id` se aplica **siempre en el servidor**, errores en
   formato ProblemDetails (RFC 7807) y CORS/HTTPS en cada ambiente (DEV/QA/PROD con sus
   `appsettings`).
5. Auditoría inmutable: colección `bitacora_auditoria` de MongoDB con retención de 1 año; en
   PostgreSQL, borrado lógico en las tablas transaccionales.
6. Respaldos: `pg_dump` + `mongodump` diarios (RPO 24 h / RTO 30 min) y cumplimiento de la
   Ley 1581 de 2012 (Habeas Data) para datos de pacientes.

**Secuencia de fases:** **(2)** backend API + PostgreSQL con EF Core y JWT → **(3)** MongoDB de
intervenciones, formularios y bitácora → **(4)** pipeline Python con K-Means → **(5)**
endurecimiento (rate limiting, refresh tokens, índices de volumen y retención de datos).

**Volumen de diseño:** 20 usuarios concurrentes por tenant, hasta 5.000 equipos y más de 500
órdenes de trabajo al mes por institución.

---

## 5. Arquitectura MVVM (cómo está organizado el código)

**MVVM** separa tres capas: la **Vista** (XAML, solo apariencia), el **ViewModel** (estado y comandos)
y el **modelo/servicios** (datos y reglas). La vista nunca "sabe" de dónde vienen los datos.

```
SITBIO-Desktop/                     (raíz del repositorio, sección 14.1 de la arquitectura)
├── SITBIO.sln                      → solución: cliente + backend
├── README.md                       → este archivo
├── docs/arquitectura/              → documento oficial ADSO-ARQ-01 (.docx)
├── src/backend/SITBIO.Api/         → API REST fase 2 (net8.0): Controllers/, Domain/,
│                                   Data/ (SITBIOContext + SemillaDatos), Auth/ (JWT), Program.cs
└── src/client/                     → cliente WPF (net10.0-windows)
    ├── SITBIO-Desktop.csproj       → proyecto (copia sitbio.config.json al bin)
    ├── sitbio.config.json          → interruptor ModoDatos (Demo/Api) y UrlApi
    ├── App.xaml / App.xaml.cs      → arranque: cultura es-CO, servicios, DataTemplates y rutas
    ├── MainWindow.xaml             → contenedor de la ventana (aloja el Shell)
    ├── Core/
    │   ├── AppServicios.cs         → localizador de servicios (elige Demo/Api al arrancar)
    │   ├── ConfiguracionCliente.cs → lee sitbio.config.json + variables SITBIO_MODO/URL_API
    │   ├── Enums.cs                → EstadoEquipo, EstadoOrden, Prioridad, RolSistema…
    │   ├── PermisosRbac.cs         → matriz de permisos: rol × área → nivel de acceso
    │   ├── Converters.cs           → convertidores XAML (colores de chip, visibilidad…)
    │   ├── Formateo.cs             → números, moneda, fechas en formato es-CO
    │   └── Paleta.cs               → colores por severidad (éxito, advertencia…)
    ├── Models/                     → entidades: Equipo, OrdenTrabajo, Intervencion, Usuario,
    │                                   Tecnico, Formulario, Auditoria, Institucion, Dashboard
    ├── Services/
    │   ├── IRepositorioDatos.cs    → CONTRATO de datos (lo comparten Demo y Api)
    │   ├── DemoDataProvider.cs     → implementación en memoria + siembra de datos demo
    │   ├── RepositorioApi.cs       → implementación HTTP+JWT contra la API (fase 2b)
    │   ├── SesionService.cs        → usuario actual, tenant actual, validación de acceso (RBAC)
    │   ├── AuditoriaService.cs     → registra eventos de auditoría en toda escritura
    │   ├── NavegacionService.cs    → rutas de navegación y control de permisos por ruta
    │   ├── ExportacionService.cs   → Excel (ClosedXML) y PDF (QuestPDF)
    │   └── ToastService.cs         → notificaciones flotantes de confirmación
    ├── ViewModels/                 → un ViewModel por módulo (estado + comandos)
    ├── Views/                      → una vista XAML (+ code-behind mínimo) por módulo
    ├── Controls/                   → controles reutilizables (PageHeader, KpiCard, Dialogo,
    │                                   EmptyState, gráficas de barras/línea/dona…)
    ├── Themes/                     → sistema de diseño (colores, tipografía, estilos)
    └── CONTRATO-MODULOS.md         → contrato interno de construcción de módulos
```

### Servicios disponibles (localizador `AppServicios`)

| Servicio | Interfaz | Función |
|---|---|---|
| `AppServicios.Datos` | `IRepositorioDatos` | leer/escribir datos (hoy memoria, mañana BD) |
| `AppServicios.Sesion` | `ISesionService` | usuario y tenant activos, `PuedeEditar(area)` |
| `AppServicios.Auditoria` | `IAuditoriaService` | `Registrar(accion, entidad, referencia, detalle)` |
| `AppServicios.Navegacion` | `INavegacionService` | `Navegar(Rutas.X, parametro)` |
| `AppServicios.Toast` | `IToastService` | `Exito/Informacion/Advertencia/Error` |
| `AppServicios.Exportacion` | `IExportacionService` | exportar indicadores/órdenes/inventario a Excel/PDF |

Los ViewModels reciben los servicios por **constructor** (6 parámetros estándar; los de detalle
agregan `int id` al inicio). Eso permite probarlos y sustituirlos (p. ej. la BD) sin reescribirlos.

### Navegación

- `NavegacionService` expone las rutas (`Rutas.Dashboard`, `Rutas.Ordenes`, …).
- `App.xaml` registra un `DataTemplate` por ViewModel → la vista que se muestra.
- `ShellViewModel` arma el menú lateral (grupos: OPERACIÓN, ACTIVOS, ANÁLISIS Y GESTIÓN, SISTEMA).

---

## 6. Los módulos de la aplicación

| Módulo | Vista / ViewModel | Qué hace |
|---|---|---|
| Inicio de sesión | `LoginView` | validación multi-tenant + accesos demo por rol |
| Shell | `ShellView` | menú lateral, topbar, cambio de institución, sesión |
| Panel de control | `DashboardView` | KPIs, gráficas de carga/dona, órdenes prioritarias, alertas |
| Órdenes de trabajo | `OrdenesView` | tabla **y kanban** conmutables, filtros, nueva orden, exportación |
| Detalle de orden | `OrdenDetalleView` | código único, SLA, línea de tiempo, cambios de estado RBAC |
| Intervención | `IntervencionView` | checklist, repuestos y costos, firma y observaciones |
| Equipos biomédicos | `EquiposView` | inventario con filtros, KPIs, alta, exportación |
| Detalle de equipo | `EquipoDetailView` | hoja de vida, campos dinámicos, órdenes y gráfica del equipo |
| Asignación de técnicos | `TecnicosView` | carga de trabajo, especialidades, asignación de órdenes |
| Reportes | `ReportesView` | periodo, KPIs, gráficas y 4 exportaciones (Excel/PDF) |
| Formularios | `FormulariosView` | CRUD de formularios dinámicos por tenant |
| Constructor de formularios | `FormBuilderView` | crear/editar campos (tipo, etiqueta, obligatorio, orden) |
| Usuarios y permisos | `UsuariosView` | alta, filtro por rol/estado, acceso al detalle |
| Detalle de usuario | `UsuarioDetailView` | perfil y **matriz RBAC** del rol, historial de auditoría |
| Bitácora de auditoría | `AuditoriaView` | filtros, tabla inmutable, KPIs y exportación |
| Configuración | `ConfiguracionView` | 4 pestañas: institución, parámetros, campos dinámicos, matriz de permisos |
| Calibraciones / Inventario | `ProximamenteView` | módulos marcados "próximamente" (fase 2/3) |

---

## 7. Reglas del negocio (metodología)

### Control de acceso por rol (RBAC)

| Rol | Contraseña | Acceso típico |
|---|---|---|
| Administrador | `Admin123!` | todo: usuarios, configuración, auditoría |
| Coordinador Biomédico | `Coord123!` | órdenes, equipos, técnicos, reportes |
| Técnico | `Tec123!` | sus órdenes e intervenciones |
| Personal Clínico | `Clini123!` | solicitar órdenes, consultar equipos |
| Director | `Dir123!` | indicadores y costos (lectura) |
| Regulador / Auditor | `Audit123!` | bitácora y consulta (solo lectura) |

- La matriz `rol × área` vive en `Core/PermisosRbac.cs` y se muestra en **UsuarioDetalle** y **Configuración**.
- En la interfaz, las acciones que el rol no puede hacer se ocultan/deshabilitan con
  `PuedeEditar(area)` / `IntentarAcceso(area)`.

### Multi-tenant (3 instituciones demo)

| # | Institución | Ciudad | Plan |
|---|---|---|---|
| 1 | Clínica San José de la Montaña | Medellín | Profesional |
| 2 | Hospital Universitario del Valle de Aburrá | Bello | Institucional |
| 3 | IPS Salud Integral Envigado | Envigado | Básico (en prueba) |

Todas las consultas pasan por `XxxDe(tenantId)`: un usuario nunca ve datos de otra institución.

### Código único de seguimiento

Formato **`SIT-2026-000123`** (prefijo-año-consecutivo), generado por
`IRepositorioDatos.SiguienteCodigoOrden(tenantId)`; es único por institución y es la llave de
búsqueda (`OrdenPorCodigo`).

### Kanban

La orden pasa por columnas (`ColumnaKanban`): Nueva → Asignada → En proceso → Verificación →
Cerrada. `OrdenTrabajo.Columna` alimenta la vista kanban de **Órdenes**; cada cambio de estado
genera un evento en la línea de tiempo **y** en la bitácora.

### Auditoría inmutable

- Toda escritura registra: acción, entidad, referencia, detalle, usuario, rol, fecha y dispositivo.
- La bitácora **no se puede editar ni borrar** desde la interfaz (y en fase 2 será una tabla
  *append-only* en PostgreSQL).

### Exportación

- **Excel** con ClosedXML y **PDF** con QuestPDF (`ExportacionService`).
- Cada exportación también se registra en auditoría (`AccionAuditoria.Exportacion`).

### Indicadores

Disponibilidad, MTBF, MTTR, cumplimiento de SLA, preventivos vencidos y costos mensuales
(`IRepositorioDatos.Indicadores(tenantId)` → panel e informes).

---

## 8. Diseño y tema visual

- **Paleta clínica azul turquesa**: `PrimaryBrush` #0B6FA4 → `AccentBrush` turquesa; estados
  éxito/advertencia/peligro/info. Todo centralizado en `Themes/Colors.xaml`.
- **Tipografía**: `Themes/Typography.xaml` (estilos `Texto.Titulo`, `Texto.Tarjeta`, `Texto.Auxiliar`…).
- **Estilos**: `Themes/Controls.xaml` (botones `Boton*`, campos `Campo`, `Selector`, `Chip`,
  `Tarjeta`) y `Themes/Lists.xaml` (DataGrid `Tabla`, pestañas `Pestanas`, listas).
- **Iconografía**: fuente Segoe MDL2 Assets; en XAML `&#xEExxx;` y en C# `"\uEExxx"`.
- **Cultura `es-CO`**: números, moneda y fechas en formato colombiano (fijada en `App.OnStartup`).
- **Menú lateral**: filas con plantilla propia `BotonMenu` (alineación a la izquierda y recorte de
  textos largos), píldora azul en el módulo activo, hover gris y badge "próximamente" para módulos
  en desarrollo. Ver sección 9.

---

## 9. Bugs corregidos durante el desarrollo

1. **Crash aleatorio al arrancar** (~37 % de las ejecuciones): en `DemoDataProvider.SembrarOrdenes`
   se usaba `idsEquipos[rnd.Next(...)]` dentro del lambda de `.First(...)`, que se ejecutaba varias
   veces → índice fuera de rango. Se calculó el índice fuera del lambda.
2. **`FormBuilderView.xaml` no compilaba**: comentario XML con `--` y falta de
   `using System.Windows.Input;` en el code-behind.
3. **Pestañas vacías en Configuración**: el estilo `Pestanas` tenía `<ContentPresenter/>` sin
   `ContentSource="SelectedContent"`.
4. **Selectores mostraban el `ToString()` de los `records`**: se agregó
   `public override string ToString() => Nombre;` a los 6 records de opciones.
5. **Rutas sin vista**: se registraron los `DataTemplate` y rutas de los 14 módulos en
   `App.xaml`/`App.xaml.cs`.
6. **Espaciado etiqueta→valor** en OrdenDetalle (10 labels) y `TargetNullValue` para técnico nulo.
7. **Colisión de clases** CS0102: `FormularioAsignacion` renombrada a `FormularioAsignacionOrden`.
8. **Repositorio Git contaminado**: `bin/` y `obj/` estaban trackeados; se creó `.gitignore` y se
   limpiaron del índice antes del primer push.
9. **Menú lateral desorganizado (arreglo grande)**:
   - *Síntoma*: íconos desalineados, el badge "próximamente" cortado en "Inventario de repuestos"
     y filas que se desbordaban contra el borde del panel.
   - *Causa raíz 1*: la plantilla compartida `BaseBoton` envolvía el contenido en un
     `StackPanel Orientation="Horizontal" HorizontalAlignment="Center"`. Un StackPanel horizontal
     **mide sus hijos con ancho infinito** (por eso el texto nunca se recortaba) y además
     **centraba cada fila según su propio ancho** (por eso los íconos empezaban en x distintas:
     66, 44, 16…).
   - *Solución 1*: plantilla propia `PlantillaMenu` con `ContentPresenter` en `Stretch` → todas las
     filas se miden con el ancho disponible, quedan alineadas a la izquierda y el texto largo
     recorta con "…" dejando espacio al badge.
   - *Causa raíz 2*: la píldora azul de la fila activa nunca se pintaba porque el borde tenía
     `Background="Transparent"` como **valor local**, y un valor local le gana a los triggers de un
     `Style` (verificado con una prueba en frío). Se quitaron los valores locales y ahora el
     `DataTrigger` de "activo" pinta el fondo y la barra lateral.
   - *Extras*: se agregó hover gris (`SurfaceAltBrush`) a las filas, se separaron mejor los grupos
     del menú y se cambió el ícono de Inventario (`E896` flecha de descarga → `E8EC` etiqueta).

---

## 10. Git: flujo GitFlow y buenas prácticas

- **Repositorio**: `https://github.com/esteban01Br/SITBIO.git`
- **Ramificación** (según la sección 14.1 de la arquitectura oficial):

| Rama | Para qué sirve | Regla |
|---|---|---|
| `main` | entregas / producción | **protegida: no se le hace push directo**; solo recibe cambios desde `develop` mediante Pull Request |
| `develop` | integración del trabajo diario | aquí convergen todas las tareas |
| `feature/<tarea>` | una tarea a la vez | nace de `develop` y se mergea a `develop` con PR |
| `release/*`, `hotfix/*` | preparar entrega / arreglo urgente | futuras, cuando existan entregas versionadas |

**Flujo paso a paso (ejemplo con una tarea):**

```powershell
cd "C:\Users\Sena\Downloads\Enterprise Healthcare SaaS UI\SITBIO-Desktop"
git checkout develop
git pull
git checkout -b feature/base-datos-postgresql
# ... trabajar ...
git add src/client/Services/RepositorioApi.cs   # agregar SOLO los archivos de la tarea
git commit -m "feat(datos): repositorio que consume la API REST"
git push -u origin feature/base-datos-postgresql
# Abrir el PR feature/... → develop en GitHub, revisar y mergear.
```

**Mensajes de commit (Conventional Commits, en español):**

| Tipo | Uso | Ejemplo |
|---|---|---|
| `feat` | nueva funcionalidad | `feat(kanban): columna de verificación con contador` |
| `fix` | corrección de error | `fix(login): validación de contraseña multi-tenant` |
| `docs` | documentación | `docs(readme): flujo GitFlow y arquitectura oficial` |
| `refactor` | reestructurar sin cambiar el comportamiento | `refactor(shell): plantilla propia del menú lateral` |
| `test`, `chore`, `style` | pruebas, soporte y formato | `chore: gitattributes para finales de línea` |

Reglas: asunto en **imperativo** y ≤ 72 caracteres sin punto final; **un commit = una idea**;
revisar con `git status` y `git diff` **antes** de `git add` (evitar `git add -A` automático).

**Proteger `main` en GitHub** (la CLI `gh` no está instalada, se hace en la web):
*Settings → Branches → Add branch protection rule* con `main` → activar
**Only allow pull requests to merge into this branch** (y opcionalmente
*Require status checks* cuando exista CI con GitHub Actions).

Otras buenas prácticas ya aplicadas en el repo: `.gitignore` (excluye `bin/`, `obj/`, `.vs/`) y
`.gitattributes` (`* text=auto`: LF en el repositorio, CRLF solo en Windows).

> Nota: `git push` escribe su progreso en *stderr*; en PowerShell puede verse en rojo aunque
> funciona correctamente — verificar con `git ls-remote origin <rama>`.

---

## 11. Siguientes pasos

1. ~~**Fase 2 — Backend**~~ ✅ `src/backend/SITBIO.Api/`: API REST ASP.NET Core 8 (JWT + BCrypt,
   RBAC, aislamiento por tenant) con EF Core y **PostgreSQL 16**, siembra equivalente a
   `DemoDataProvider` y migraciones automáticas al arrancar (secciones 2 y 4.1).
2. ~~**Cliente**~~ ✅ `RepositorioApi : IRepositorioDatos` con interruptor **Demo/Api** en
   `AppServicios.Inicializar()`; los ViewModels solo recibieron las llamadas a los nuevos
   métodos `Actualizar*`/`AgregarIntervencion` de la interfaz.
3. **Fase 3 — MongoDB**: intervenciones con `camposDinamicos`, configuración de formularios y
   bitácora de auditoría (en la fase 2 quedaron en PostgreSQL como decisión de alcance).
4. **Fase 4 — Analítica**: pipeline Python con K-Means en `/src/analytics` (sección 14.1 del
   documento de arquitectura).
5. **Pendiente del equipo**: alinear el TFM del backend (`net8.0`) con el cliente (`net10.0`).
6. **Módulos "próximamente"**: Calibraciones e inventario de repuestos.

---

*Proyecto académico SENA — Centro de la Tecnología y Manufactura Avanzada (CTMA) · ADSO 2026.*
