using Microsoft.EntityFrameworkCore;
using SITBIO.Api.Domain;

namespace SITBIO.Api.Data;

/// <summary>
/// Siembra inicial equivalente al DemoDataProvider del cliente (misma data, mismos ids).
/// Se ejecuta una sola vez, cuando la base está vacía. Las contraseñas se guardan con
/// BCrypt (factor de trabajo 11) — nunca en texto plano.
/// </summary>
public static class SemillaDatos
{
    public static async Task SembrarAsync(SITBIOContext db)
    {
        if (await db.Instituciones.AnyAsync()) return;

        var rnd = Random.Shared;

        // ============================================================
        // 1. INSTITUCIONES (tenants)
        // ============================================================
        db.Instituciones.AddRange(
            new Institucion
            {
                Id = 1, NIT = "890480123-4", Nombre = "Clínica San José de la Montaña",
                Ciudad = "Medellín", Direccion = "Cra. 53 # 41-50, El Poblado", Telefono = "604 444 80 80",
                ResolucionSanitaria = "Res. 3100/2019 – INVIMA", Plan = PlanSaaS.Profesional,
                Estado = EstadoTenant.Activo, FechaRegistro = new DateTime(2024, 3, 12), TotalCamas = 142
            },
            new Institucion
            {
                Id = 2, NIT = "890400987-1", Nombre = "Hospital Universitario del Valle de Aburrá",
                Ciudad = "Bello", Direccion = "Cra. 49 # 49-50", Telefono = "604 388 22 10",
                ResolucionSanitaria = "Res. 3100/2019 – Secretaría de Salud Antioquia", Plan = PlanSaaS.Institucional,
                Estado = EstadoTenant.Activo, FechaRegistro = new DateTime(2023, 8, 5), TotalCamas = 310
            },
            new Institucion
            {
                Id = 3, NIT = "900456123-7", Nombre = "IPS Salud Integral Envigado",
                Ciudad = "Envigado", Direccion = "Cra. 41 # 35-110", Telefono = "604 266 33 40",
                ResolucionSanitaria = "Res. 3100/2019 – INVIMA", Plan = PlanSaaS.Basico,
                Estado = EstadoTenant.EnPrueba, FechaRegistro = new DateTime(2025, 11, 20), TotalCamas = 48
            });

        // ============================================================
        // 2. USUARIOS (contraseña BCrypt por rol)
        // ============================================================
        var hoy = DateTime.Now;
        var usuarios = new List<Usuario>
        {
            U(1, 1, "Laura", "Gómez Ríos", "admin@sitbio.com", "Administradora de Sistema", RolSistema.Administrador, new DateTime(2024, 3, 12)),
            U(2, 1, "Andrés", "Montoya Vélez", "coordinador@sitbio.com", "Ingeniero Biomédico Coordinador", RolSistema.CoordinadorBiomedico, new DateTime(2024, 3, 15)),
            U(3, 1, "Camilo", "Restrepo Ruiz", "tecnico@sitbio.com", "Técnico Electrónica Biomédica", RolSistema.Tecnico, new DateTime(2024, 5, 2)),
            U(4, 1, "Valentina", "Ospina Lara", "enfermera@sitbio.com", "Jefe de Enfermería", RolSistema.PersonalClinico, new DateTime(2024, 6, 18)),
            U(5, 1, "Roberto", "Zapata Molina", "director@sitbio.com", "Director Ejecutivo", RolSistema.Director, new DateTime(2024, 3, 12)),
            U(6, 1, "Diana", "Caro Mejía", "auditor@sitbio.com", "Auditor Externo – INVIMA", RolSistema.Regulador, new DateTime(2025, 1, 9)),
            U(7, 1, "Jorge", "Henao Palacio", "tecnico2@sitbio.com", "Técnico Mecánica Biomédica", RolSistema.Tecnico, new DateTime(2024, 7, 1)),
            U(8, 1, "Sara", "Betancur Álvarez", "salud2@sitbio.com", "Auxiliar de Enfermería", RolSistema.PersonalClinico, new DateTime(2025, 2, 14)),
            U(9, 2, "Paola", "Ríos Estrada", "admin2@sitbio.com", "Administradora de Sistema", RolSistema.Administrador, new DateTime(2023, 8, 5)),
            U(10, 2, "Mauricio", "García Cano", "coord2@sitbio.com", "Coordinador de Ingeniería Biomédica", RolSistema.CoordinadorBiomedico, new DateTime(2023, 8, 20)),
            U(11, 2, "Felipe", "Toro Salazar", "tecnico3@sitbio.com", "Técnico Imagenología", RolSistema.Tecnico, new DateTime(2024, 1, 10)),
            U(12, 3, "Claudia", "Mejía Rendón", "admin3@sitbio.com", "Administradora de Sistema", RolSistema.Administrador, new DateTime(2025, 11, 20)),
            U(13, 3, "Óscar", "Villa Gómez", "coord3@sitbio.com", "Ingeniero Biomédico", RolSistema.CoordinadorBiomedico, new DateTime(2025, 12, 2))
        };

        foreach (var u in usuarios)
        {
            u.UltimoAcceso = hoy.AddDays(-rnd.Next(0, 9)).AddMinutes(-rnd.Next(0, 600));
            u.PasswordHash = BCrypt.Net.BCrypt.HashPassword(ClaveDemo(u.Rol));
            u.Telefono = $"300 {rnd.Next(200, 999)} {rnd.Next(1000, 9999)}";
        }
        db.Usuarios.AddRange(usuarios);

        // ============================================================
        // 3. TÉCNICOS (ids 1..6)
        // ============================================================
        var datosTecnicos = new (Usuario u, EspecialidadTecnica[] esp, double horas, double ant, int activas, int completadas, string turno)[]
        {
            (usuarios[2], new[] { EspecialidadTecnica.ElectronicaBiomedica, EspecialidadTecnica.Infusion }, 34, 30, 4, 68, "Diurno"),
            (usuarios[6], new[] { EspecialidadTecnica.Mecanica, EspecialidadTecnica.ElectronicaBiomedica }, 22, 36, 2, 54, "Diurno"),
            (usuarios[10], new[] { EspecialidadTecnica.Imagenologia, EspecialidadTecnica.ElectronicaBiomedica }, 38, 33, 5, 41, "Mixto"),
            (usuarios[1], new[] { EspecialidadTecnica.ElectronicaBiomedica, EspecialidadTecnica.Laboratorio }, 18, 24, 1, 22, "Administrativo")
        };

        var tecnicos = new List<Tecnico>();
        var idTecnico = 1;
        foreach (var d in datosTecnicos)
        {
            tecnicos.Add(new Tecnico
            {
                Id = idTecnico++, UsuarioId = d.u.Id, TenantId = d.u.TenantId,
                Nombre = NombreCompleto(d.u), Iniciales = Iniciales(d.u),
                Especialidades = d.esp.ToList(),
                Disponibilidad = d.horas > 36 ? DisponibilidadTecnico.EnCampo : DisponibilidadTecnico.Disponible,
                CapacidadHorasSemana = 40, HorasAsignadas = d.horas, HorasSemanaAnterior = d.ant,
                OrdenesActivas = d.activas, OrdenesCompletadas = d.completadas,
                Calificacion = Math.Round(3.9 + rnd.NextDouble() * 1.0, 1),
                Telefono = d.u.Telefono, Correo = d.u.Email, Turno = d.turno
            });
        }
        tecnicos.Add(new Tecnico
        {
            Id = idTecnico++, UsuarioId = 0, TenantId = 2, Nombre = "Esteban Cardona Peña", Iniciales = "EC",
            Especialidades = new List<EspecialidadTecnica> { EspecialidadTecnica.AireMedico, EspecialidadTecnica.Mecanica },
            Disponibilidad = DisponibilidadTecnico.Disponible, CapacidadHorasSemana = 40, HorasAsignadas = 26,
            HorasSemanaAnterior = 28, OrdenesActivas = 3, OrdenesCompletadas = 47, Calificacion = 4.6,
            Telefono = "301 555 2210", Correo = "ecardona@hospitalvalle.com", Turno = "Nocturno"
        });
        tecnicos.Add(new Tecnico
        {
            Id = idTecnico, UsuarioId = 0, TenantId = 3, Nombre = "Luisa Fernanda Arango", Iniciales = "LA",
            Especialidades = new List<EspecialidadTecnica> { EspecialidadTecnica.ElectronicaBiomedica },
            Disponibilidad = DisponibilidadTecnico.EnDescanso, CapacidadHorasSemana = 40, HorasAsignadas = 12,
            HorasSemanaAnterior = 31, OrdenesActivas = 1, OrdenesCompletadas = 19, Calificacion = 4.4,
            Telefono = "320 444 7788", Correo = "larango@saludintegral.co", Turno = "Diurno"
        });
        db.Tecnicos.AddRange(tecnicos);

        // ============================================================
        // 4. EQUIPOS (ids 1..58)
        // ============================================================
        var cat = new[]
        {
            "Monitor multiparamétrico", "Bomba de infusión", "Desfibrilador", "Respirador mecánico",
            "Electrocardiógrafo", "Autoclave", "Camilla de atención", "Oxímetro de pulso",
            "Ultrasonido diagnóstico", "Bomba de aspiración", "Incubadora neonatal", "Mesa de quirófano",
            "Central de monitoreo", "Generador de Rayos X", "Balanza de precisión", "Analizador clínico"
        };
        var marcas = new[] { "Philips", "Mindray", "GE Healthcare", "Dräger", "Welch Allyn", "Baxter", "Getinge", "Siemens Healthineers" };
        var ubics = new[] { "UCI – Sala 1", "Quirófano 2", "Emergencias", "Hospitalización 3°", "Sala de partos", "Neonatología", "Laboratorio central", "Imagenología", "Esterilización", "Consultorio 105" };
        var estadosEquipo = new[]
        {
            EstadoEquipo.Operativo, EstadoEquipo.Operativo, EstadoEquipo.Operativo, EstadoEquipo.Operativo,
            EstadoEquipo.Operativo, EstadoEquipo.Operativo, EstadoEquipo.EnMantenimiento,
            EstadoEquipo.EnCalibracion, EstadoEquipo.FueraDeServicio, EstadoEquipo.Operativo,
            EstadoEquipo.Operativo, EstadoEquipo.Operativo, EstadoEquipo.Baja, EstadoEquipo.Operativo
        };

        var equipos = new List<Equipo>();
        var idEquipo = 1;

        // Equipos de la clínica principal (tenant 1)
        for (var i = 0; i < 34; i++)
        {
            var categoria = cat[i % cat.Length];
            var instalacion = new DateTime(2019 + rnd.Next(0, 6), rnd.Next(1, 13), rnd.Next(1, 28));
            var estado = estadosEquipo[i % estadosEquipo.Length];
            var ultimo = DateTime.Today.AddDays(-rnd.Next(5, 160));
            var proximo = ultimo.AddDays(rnd.Next(-30, 120));

            equipos.Add(new Equipo
            {
                Id = idEquipo, TenantId = 1,
                CodigoInventario = $"BM-00{i + 1:D3}",
                Nombre = categoria,
                Categoria = categoria,
                Marca = marcas[rnd.Next(marcas.Length)],
                Modelo = $"MX-{rnd.Next(100, 999)}{(char)('A' + rnd.Next(0, 6))}",
                NumeroSerie = $"SN{rnd.Next(100000, 999999)}{rnd.Next(10, 99)}",
                Ubicacion = ubics[rnd.Next(ubics.Length)],
                Responsable = rnd.Next(3) == 0 ? "Servicio externo" : "Ingeniería biomédica",
                Estado = estado,
                FechaInstalacion = instalacion,
                UltimoMantenimiento = ultimo,
                ProximoMantenimiento = proximo,
                UltimaCalibracion = ultimo.AddDays(-rnd.Next(60, 200)),
                ProximaCalibracion = DateTime.Today.AddDays(rnd.Next(-15, 90)),
                VidaUtilMeses = 60 + rnd.Next(0, 61),
                ValorAdquisicion = Math.Round((decimal)(2_500_000 + rnd.NextDouble() * 180_000_000), 0),
                Fabricante = marcas[rnd.Next(marcas.Length)],
                ClaseRiesgo = rnd.Next(4) == 0 ? "III" : rnd.Next(3) == 0 ? "I" : "II",
                OrdenesAbiertas = rnd.Next(0, 4),
                TotalIntervenciones = rnd.Next(1, 18),
                CamposDinamicos = new Dictionary<string, string>
                {
                    ["N° de patrimonio"] = $"PAT-{rnd.Next(1000, 9999)}",
                    ["Tensión de servicio"] = rnd.Next(2) == 0 ? "110 V" : "220 V",
                    ["Clase eléctrica"] = rnd.Next(3) == 0 ? "Clase II" : "Clase I"
                }
            });
            idEquipo++;
        }

        // Equipo crítico destacado para el caso de uso del panel
        equipos.Add(new Equipo
        {
            Id = idEquipo, TenantId = 1, CodigoInventario = "BM-0101",
            Nombre = "Monitor multiparamétrico IntelliVue MX450", Categoria = "Monitor multiparamétrico",
            Marca = "Philips", Modelo = "MX450", NumeroSerie = "PH8842117",
            Ubicacion = "UCI – Sala 1", Responsable = "Ingeniería biomédica",
            Estado = EstadoEquipo.EnMantenimiento, FechaInstalacion = new DateTime(2021, 6, 14),
            UltimoMantenimiento = DateTime.Today.AddDays(-72), ProximoMantenimiento = DateTime.Today.AddDays(-4),
            UltimaCalibracion = DateTime.Today.AddDays(-170), ProximaCalibracion = DateTime.Today.AddDays(21),
            VidaUtilMeses = 96, ValorAdquisicion = 68_400_000, Fabricante = "Philips",
            ClaseRiesgo = "III", OrdenesAbiertas = 2, TotalIntervenciones = 9,
            CamposDinamicos = new Dictionary<string, string>
            {
                ["N° de patrimonio"] = "PAT-4417", ["Tensión de servicio"] = "110 V", ["Clase eléctrica"] = "Clase I"
            }
        });
        idEquipo++;

        // Otros tenants
        for (var t = 2; t <= 3; t++)
        {
            var cantidad = t == 2 ? 16 : 7;
            for (var i = 0; i < cantidad; i++)
            {
                var categoria = cat[(i + t) % cat.Length];
                equipos.Add(new Equipo
                {
                    Id = idEquipo, TenantId = t,
                    CodigoInventario = $"BM-{t}0{i + 1:D3}",
                    Nombre = categoria, Categoria = categoria,
                    Marca = marcas[(i + t) % marcas.Length],
                    Modelo = $"MD-{rnd.Next(200, 999)}",
                    NumeroSerie = $"SN{rnd.Next(100000, 999999)}",
                    Ubicacion = ubics[(i + t) % ubics.Length],
                    Responsable = "Ingeniería biomédica",
                    Estado = estadosEquipo[(i + t) % estadosEquipo.Length],
                    FechaInstalacion = new DateTime(2020 + rnd.Next(0, 5), rnd.Next(1, 13), rnd.Next(1, 28)),
                    UltimoMantenimiento = DateTime.Today.AddDays(-rnd.Next(10, 200)),
                    ProximoMantenimiento = DateTime.Today.AddDays(rnd.Next(-20, 150)),
                    VidaUtilMeses = 72, ValorAdquisicion = Math.Round((decimal)(3_000_000 + rnd.NextDouble() * 90_000_000), 0),
                    Fabricante = marcas[(i + t) % marcas.Length], ClaseRiesgo = "II",
                    TotalIntervenciones = rnd.Next(0, 12)
                });
                idEquipo++;
            }
        }
        db.Equipos.AddRange(equipos);

        // ============================================================
        // 5. FORMULARIOS DINÁMICOS (ids 1..5)
        // ============================================================
        var formularios = new List<FormularioDinamico>
        {
            new()
            {
                Id = 1, TenantId = 1, Nombre = "Inspección de seguridad eléctrica",
                Descripcion = "Verificación de puesta a tierra, fugas de corriente y estado del cableado.",
                Categoria = "Seguridad", Autor = "Laura Gómez Ríos", FechaCreacion = new DateTime(2025, 2, 11),
                Version = 3, Aplicaciones = 128,
                Campos = new List<CampoDinamico>
                {
                    new() { Id = 1, Etiqueta = "¿Presenta fugas de corriente?", Tipo = TipoCampoDinamico.Booleano, Requerido = true, Orden = 1 },
                    new() { Id = 2, Etiqueta = "Valor de la corriente de fuga (mA)", Tipo = TipoCampoDinamico.Numero, Requerido = true, Orden = 2, Ayuda = "Máximo permitido 0.5 mA" },
                    new() { Id = 3, Etiqueta = "Estado del cable de alimentación", Tipo = TipoCampoDinamico.Seleccion, Requerido = true, Orden = 3, Opciones = new List<string> { "Bueno", "Regular", "Defectuoso" } },
                    new() { Id = 4, Etiqueta = "Observaciones", Tipo = TipoCampoDinamico.TextoLargo, Orden = 4 }
                }
            },
            new()
            {
                Id = 2, TenantId = 1, Nombre = "Checklist de mantenimiento preventivo",
                Descripcion = "Rutina trimestral aplicable a equipos de soporte vital.",
                Categoria = "Mantenimiento", Autor = "Andrés Montoya Vélez", FechaCreacion = new DateTime(2024, 9, 3),
                Version = 5, Aplicaciones = 412,
                Campos = new List<CampoDinamico>
                {
                    new() { Id = 1, Etiqueta = "Limpieza de filtros", Tipo = TipoCampoDinamico.Booleano, Requerido = true, Orden = 1 },
                    new() { Id = 2, Etiqueta = "Prueba de batería (min)", Tipo = TipoCampoDinamico.Numero, Requerido = true, Orden = 2 },
                    new() { Id = 3, Etiqueta = "Firmware versión", Tipo = TipoCampoDinamico.Texto, Orden = 3 },
                    new() { Id = 4, Etiqueta = "Fecha de la prueba funcional", Tipo = TipoCampoDinamico.Fecha, Requerido = true, Orden = 4 },
                    new() { Id = 5, Etiqueta = "Resultado global", Tipo = TipoCampoDinamico.Seleccion, Requerido = true, Orden = 5, Opciones = new List<string> { "Aprobado", "Con observaciones", "Rechazado" } }
                }
            },
            new()
            {
                Id = 3, TenantId = 1, Nombre = "Recepción de equipos biomédicos",
                Descripcion = "Alta inicial de hoja de vida al ingreso del activo.",
                Categoria = "Activos", Autor = "Laura Gómez Ríos", FechaCreacion = new DateTime(2024, 5, 20),
                Version = 2, Aplicaciones = 64,
                Campos = new List<CampoDinamico>
                {
                    new() { Id = 1, Etiqueta = "N° de patrimonio", Tipo = TipoCampoDinamico.Texto, Requerido = true, Orden = 1 },
                    new() { Id = 2, Etiqueta = "Fecha de instalación", Tipo = TipoCampoDinamico.Fecha, Requerido = true, Orden = 2 },
                    new() { Id = 3, Etiqueta = "Responsable del área", Tipo = TipoCampoDinamico.Texto, Orden = 3 },
                    new() { Id = 4, Etiqueta = "Cuenta con manifiesto de conformidad", Tipo = TipoCampoDinamico.Booleano, Requerido = true, Orden = 4 }
                }
            },
            new()
            {
                Id = 4, TenantId = 1, Nombre = "Verificación de calibración",
                Descripcion = "Registro metrológico exigido por INVIMA y Secretarías de Salud.",
                Categoria = "Calibración", Autor = "Andrés Montoya Vélez", FechaCreacion = new DateTime(2025, 6, 17),
                Version = 1, Aplicaciones = 37,
                Campos = new List<CampoDinamico>
                {
                    new() { Id = 1, Etiqueta = "Certificado de calibración", Tipo = TipoCampoDinamico.Texto, Requerido = true, Orden = 1 },
                    new() { Id = 2, Etiqueta = "Desviación estándar (%)", Tipo = TipoCampoDinamico.Numero, Requerido = true, Orden = 2 },
                    new() { Id = 3, Etiqueta = "Próxima calibración", Tipo = TipoCampoDinamico.Fecha, Requerido = true, Orden = 3 }
                }
            },
            new()
            {
                Id = 5, TenantId = 2, Nombre = "Inspección de red de gases medicinales",
                Descripcion = "Verificación de presión, puridad y señalización de la red.",
                Categoria = "Seguridad", Autor = "Paola Ríos Estrada", FechaCreacion = new DateTime(2024, 11, 8),
                Version = 2, Aplicaciones = 55,
                Campos = new List<CampoDinamico>
                {
                    new() { Id = 1, Etiqueta = "Presión de línea (kg/cm²)", Tipo = TipoCampoDinamico.Numero, Requerido = true, Orden = 1 },
                    new() { Id = 2, Etiqueta = "¿Detecta fugas?", Tipo = TipoCampoDinamico.Booleano, Requerido = true, Orden = 2 },
                    new() { Id = 3, Etiqueta = "Observaciones", Tipo = TipoCampoDinamico.TextoLargo, Orden = 3 }
                }
            }
        };
        db.Formularios.AddRange(formularios);

        // ============================================================
        // 6. ÓRDENES DE TRABAJO (ids 1..46, códigos SIT-2026-000001…)
        // ============================================================
        var titulos = new[]
        {
            "Monitor no enciende", "Alarma intermitente de batería baja", "Error de calibración de presión",
            "Fuga en el circuito de infusión", "Pantalla con parpadeo", "No imprime el registro",
            "Temperatura fuera de rango", "Vibración anómala del motor", "Sensor de oxígeno sin lectura",
            "Mantenimiento preventivo trimestral", "Falla en el arranque automático", "Perdida de señal ECG",
            "Ruido en la bomba de aspiración", "Desajuste en la manguera de oxígeno", "Batería no carga",
            "Error de comunicación con la central", "Tapa del autoclave mal sellada", "Puerta no cierra correctamente"
        };
        var descripciones = new[]
        {
            "El personal reporta que el equipo dejó de responder durante la ronda de enfermería.",
            "Se detecta alarma persistente al conectar el equipo a la línea eléctrica del servicio.",
            "El equipo entrega valores fuera de rango comparados con el patrón de referencia.",
            "Se observa pérdida de presión durante la operación normal del dispositivo.",
            "La falla interrumpe la atención del paciente y requiere soporte inmediato."
        };

        var idsEquipos = equipos.Where(e => e.TenantId == 1).Select(e => e.Id).ToList();
        var coordinador = usuarios.First(u => u.Rol == RolSistema.CoordinadorBiomedico && u.TenantId == 1);
        var clinico = usuarios.First(u => u.Rol == RolSistema.PersonalClinico && u.TenantId == 1);
        var clinico2 = usuarios.Last(u => u.Rol == RolSistema.PersonalClinico && u.TenantId == 1);
        var tecnicosTenant1 = tecnicos.Where(t => t.TenantId == 1).ToList();

        var estadosOrden = new[]
        {
            EstadoOrden.Cerrada, EstadoOrden.Cerrada, EstadoOrden.Cerrada, EstadoOrden.Cerrada,
            EstadoOrden.Cerrada, EstadoOrden.EnProceso, EstadoOrden.EnProceso, EstadoOrden.Asignada,
            EstadoOrden.Nueva, EstadoOrden.Nueva, EstadoOrden.Verificacion, EstadoOrden.EnEsperaRepuesto,
            EstadoOrden.Cancelada, EstadoOrden.Cerrada, EstadoOrden.EnProceso, EstadoOrden.Cerrada,
            EstadoOrden.Nueva, EstadoOrden.Asignada, EstadoOrden.Cerrada, EstadoOrden.Cerrada
        };
        var tipos = new[]
        {
            TipoMantenimiento.Correctivo, TipoMantenimiento.Correctivo, TipoMantenimiento.Preventivo,
            TipoMantenimiento.Calibracion, TipoMantenimiento.Correctivo, TipoMantenimiento.Preventivo,
            TipoMantenimiento.Predictivo, TipoMantenimiento.Correctivo, TipoMantenimiento.Correctivo,
            TipoMantenimiento.Preventivo
        };
        var prioridades = new[]
        {
            Prioridad.Critica, Prioridad.Alta, Prioridad.Media, Prioridad.Alta, Prioridad.Media,
            Prioridad.Baja, Prioridad.Critica, Prioridad.Media, Prioridad.Alta, Prioridad.Baja
        };

        var ordenes = new List<OrdenTrabajo>();
        for (var i = 0; i < 46; i++)
        {
            var idEq = idsEquipos[rnd.Next(idsEquipos.Count)];
            var equipo = equipos.First(e => e.Id == idEq);
            var estado = estadosOrden[i % estadosOrden.Length];
            var tipo = tipos[rnd.Next(tipos.Length)];
            var prioridad = prioridades[rnd.Next(prioridades.Length)];
            var solicitante = rnd.Next(3) == 0 ? clinico2 : clinico;
            var tecnico = rnd.Next(6) == 0 ? null : tecnicosTenant1[rnd.Next(tecnicosTenant1.Count)];
            var diasAtras = estado == EstadoOrden.Cerrada ? rnd.Next(10, 190) : rnd.Next(1, 26);
            var solicitud = DateTime.Now.AddDays(-diasAtras).AddMinutes(-rnd.Next(0, 900));

            var orden = new OrdenTrabajo
            {
                Id = i + 1, TenantId = 1,
                Codigo = $"SIT-{DateTime.Today.Year}-{i + 1:D6}",
                Titulo = titulos[rnd.Next(titulos.Length)],
                Descripcion = descripciones[rnd.Next(descripciones.Length)],
                EquipoId = equipo.Id, EquipoNombre = equipo.Nombre, EquipoUbicacion = equipo.Ubicacion,
                Tipo = tipo, Prioridad = prioridad, Estado = estado,
                SolicitanteId = solicitante.Id, SolicitanteNombre = NombreCompleto(solicitante),
                FechaSolicitud = solicitud,
                SLAHoras = prioridad == Prioridad.Critica ? 4 : prioridad == Prioridad.Alta ? 12 : prioridad == Prioridad.Baja ? 72 : 24,
                CostoEstimado = Math.Round((decimal)(120_000 + rnd.NextDouble() * 4_500_000), 0),
                ReporteSala = equipo.Ubicacion
            };

            if (estado != EstadoOrden.Nueva)
            {
                orden.TecnicoId = tecnico?.Id;
                orden.TecnicoNombre = tecnico?.Nombre ?? "Sin asignar";
                orden.FechaAsignacion = solicitud.AddHours(rnd.Next(1, 12));
            }

            if (estado is EstadoOrden.EnProceso or EstadoOrden.Verificacion or EstadoOrden.Cerrada)
                orden.FechaInicio = orden.FechaAsignacion?.AddHours(rnd.Next(1, 20));

            if (estado is EstadoOrden.Cerrada)
            {
                orden.FechaCierre = (orden.FechaInicio ?? solicitud).AddHours(rnd.Next(3, 60));
                orden.CostoReal = orden.CostoEstimado;
            }

            // Historial de trazabilidad (línea de tiempo)
            orden.Historial.Add(new EventoOrden
            {
                Fecha = solicitud, Usuario = NombreCompleto(solicitante), Accion = "Solicitud creada",
                Detalle = $"Reporte de falla para {equipo.CodigoInventario}", Icono = ""
            });

            if (orden.FechaAsignacion is not null && tecnico is not null)
                orden.Historial.Add(new EventoOrden
                {
                    Fecha = orden.FechaAsignacion.Value, Usuario = NombreCompleto(coordinador), Accion = "Orden asignada",
                    Detalle = $"Asignada a {tecnico.Nombre}", Icono = ""
                });

            if (orden.FechaInicio is not null)
                orden.Historial.Add(new EventoOrden
                {
                    Fecha = orden.FechaInicio.Value, Usuario = tecnico?.Nombre ?? "Técnico", Accion = "Intervención iniciada",
                    Detalle = "El técnico abrió la intervención en campo", Icono = ""
                });

            if (estado == EstadoOrden.EnEsperaRepuesto)
                orden.Historial.Add(new EventoOrden
                {
                    Fecha = DateTime.Now.AddDays(-1), Usuario = tecnico?.Nombre ?? "Técnico", Accion = "Espera de repuesto",
                    Detalle = "Repuesto solicitado a proveedor, entrega estimada en 48 horas", Icono = ""
                });

            if (estado == EstadoOrden.Verificacion)
                orden.Historial.Add(new EventoOrden
                {
                    Fecha = DateTime.Now.AddHours(-6), Usuario = NombreCompleto(coordinador), Accion = "En verificación",
                    Detalle = "Pendiente validación del coordinador biomédico", Icono = ""
                });

            if (estado is EstadoOrden.Cerrada)
                orden.Historial.Add(new EventoOrden
                {
                    Fecha = orden.FechaCierre!.Value, Usuario = NombreCompleto(coordinador), Accion = "Orden cerrada",
                    Detalle = "Trabajo verificado y certificado", Icono = ""
                });

            if (estado == EstadoOrden.Cancelada)
                orden.Historial.Add(new EventoOrden
                {
                    Fecha = solicitud.AddHours(5), Usuario = NombreCompleto(coordinador), Accion = "Orden cancelada",
                    Detalle = "Se determinó que la falla era operativa del usuario", Icono = ""
                });

            if (rnd.Next(2) == 0)
                orden.Comentarios.Add(new ComentarioOrden
                {
                    Fecha = solicitud.AddHours(rnd.Next(2, 40)), Autor = NombreCompleto(solicitante),
                    RolAutor = SiglaRol(solicitante.Rol), Texto = "Se solicita prioridad por tratarse de equipo de soporte vital."
                });

            if (rnd.Next(3) == 0 && tecnico is not null)
                orden.Comentarios.Add(new ComentarioOrden
                {
                    Fecha = solicitud.AddHours(rnd.Next(6, 60)), Autor = tecnico.Nombre,
                    RolAutor = "TEC", Texto = "Se realiza diagnóstico inicial y se confirma la necesidad de repuesto."
                });

            ordenes.Add(orden);
        }
        db.Ordenes.AddRange(ordenes);

        // ============================================================
        // 7. INTERVENCIONES (ids 1..9)
        // ============================================================
        var cerradas = ordenes.Where(o => o.Estado == EstadoOrden.Cerrada).Take(6).ToList();
        var enProceso = ordenes.Where(o => o.Estado == EstadoOrden.EnProceso).Take(3).ToList();
        var intervenciones = new List<Intervencion>();
        var idIntervencion = 1;

        foreach (var orden in cerradas.Concat(enProceso))
        {
            var activa = orden.Estado == EstadoOrden.EnProceso;
            var tieneRepuestos = orden.Tipo == TipoMantenimiento.Correctivo;

            intervenciones.Add(new Intervencion
            {
                Id = idIntervencion++, TenantId = 1,
                OrdenId = orden.Id, OrdenCodigo = orden.Codigo,
                EquipoId = orden.EquipoId, EquipoNombre = orden.EquipoNombre,
                TecnicoId = orden.TecnicoId ?? 1, TecnicoNombre = orden.TecnicoNombre,
                Estado = activa ? EstadoIntervencion.EnEjecucion : EstadoIntervencion.Validada,
                FechaInicio = orden.FechaInicio ?? orden.FechaSolicitud.AddHours(3),
                FechaFin = activa ? null : orden.FechaCierre,
                Diagnostico = "Se identificó falla en la tarjeta de potencia asociada a la alimentación del equipo.",
                CausaRaiz = "Degradación progresiva de los capacitores por edad de servicio y sobretensiones de la línea.",
                AccionesRealizadas = "Desarme controlado, reemplazo de la tarjeta de potencia, prueba de funcionamiento con carga simulada y verificación de puesta a tierra.",
                PruebasRealizadas = "Prueba funcional completa, verificación de parámetros eléctricos y prueba de seguridad eléctrica.",
                ResultadoPrueba = activa ? "" : "Aprobado – parámetros dentro de rango",
                NotasCampo = "Se recomienda estabilizador de voltaje para el puesto donde opera el equipo.",
                Recomendaciones = activa ? "" : "Programar verificación de batería en 6 meses.",
                HorasInvertidas = Math.Round(1.5 + rnd.NextDouble() * 7, 1),
                ManoObra = Math.Round((decimal)(180_000 + rnd.NextDouble() * 900_000), 0),
                Repuestos = tieneRepuestos
                    ? new List<RepuestoConsumido>
                    {
                        new() { Codigo = "RP-0142", Nombre = "Tarjeta de potencia", Cantidad = 1, PrecioUnitario = 1_250_000 },
                        new() { Codigo = "RP-0311", Nombre = "Capacitor 470µF 400V", Cantidad = 4, PrecioUnitario = 18_500 }
                    }
                    : new List<RepuestoConsumido>
                    {
                        new() { Codigo = "RP-0007", Nombre = "Filtro de aire", Cantidad = 2, PrecioUnitario = 42_000 }
                    },
                Evidencias = new List<string> { "foto_equipo_antes.jpg", "foto_tarjeta_reemplazada.jpg", "certificado_prueba.pdf" }
            });
        }
        db.Intervenciones.AddRange(intervenciones);

        // ============================================================
        // 8. BITÁCORA DE AUDITORÍA (140 eventos, ids 1..140)
        // ============================================================
        var acciones = new[]
        {
            AccionAuditoria.InicioSesion, AccionAuditoria.Creacion, AccionAuditoria.Actualizacion,
            AccionAuditoria.Asignacion, AccionAuditoria.CambioEstado, AccionAuditoria.Exportacion,
            AccionAuditoria.Consulta, AccionAuditoria.Validacion, AccionAuditoria.CierreSesion
        };
        var entidades = new[] { "OrdenTrabajo", "Equipo", "Usuario", "FormularioDinamico", "Intervencion", "Reporte" };
        var detalles = new[]
        {
            "Registro modificado desde el módulo de escritorio",
            "Consulta de hoja de vida del activo biomédico",
            "Cambio de estado aprobado por coordinador biomédico",
            "Exportación de reporte de cumplimiento en PDF",
            "Asignación de orden de trabajo a técnico de campo",
            "Validación de intervención con evidencias adjuntas"
        };

        var auditoria = new List<EventoAuditoria>();
        for (var i = 1; i <= 140; i++)
        {
            var usuario = usuarios[rnd.Next(usuarios.Count)];
            var accion = acciones[rnd.Next(acciones.Length)];
            var entidad = entidades[rnd.Next(entidades.Length)];

            auditoria.Add(new EventoAuditoria
            {
                Id = i, TenantId = usuario.TenantId,
                Fecha = DateTime.Now.AddMinutes(-rnd.Next(0, 60 * 24 * 45)),
                Usuario = NombreCompleto(usuario), Rol = SiglaRol(usuario.Rol),
                Accion = accion, Entidad = entidad,
                Referencia = entidad switch
                {
                    "OrdenTrabajo" => $"SIT-{DateTime.Today.Year}-{rnd.Next(1, 47):D6}",
                    "Equipo" => $"BM-00{rnd.Next(1, 35):D3}",
                    "Usuario" => usuario.Email,
                    "FormularioDinamico" => $"FRM-{rnd.Next(1, 5):D3}",
                    "Intervencion" => $"INT-{rnd.Next(1, 10):D4}",
                    _ => $"RPT-{rnd.Next(100, 999)}"
                },
                Detalle = detalles[rnd.Next(detalles.Length)],
                Dispositivo = rnd.Next(5) == 0 ? "Web responsiva" : $"Escritorio WPF · {Environment.MachineName}"
            });
        }
        db.Auditoria.AddRange(auditoria);

        // ============================================================
        // 9. NOTIFICACIONES (ids 1..6)
        // ============================================================
        db.Notificaciones.AddRange(
            new Notificacion { Id = 1, TenantId = 1, Titulo = "SLA vencido", Mensaje = "La orden SIT-2026-000008 superó el tiempo límite de atención.", Fecha = DateTime.Now.AddMinutes(-18), Tipo = "danger", Icono = "" },
            new Notificacion { Id = 2, TenantId = 1, Titulo = "Preventivo vencido", Mensaje = "3 equipos biomédicos presentan mantenimiento preventivo atrasado.", Fecha = DateTime.Now.AddHours(-3), Tipo = "warning", Icono = "" },
            new Notificacion { Id = 3, TenantId = 1, Titulo = "Repuesto recibido", Mensaje = "Ingresó a almacén el filtro de aire solicitado.", Fecha = DateTime.Now.AddHours(-7), Tipo = "info", Icono = "" },
            new Notificacion { Id = 4, TenantId = 1, Titulo = "Orden cerrada", Mensaje = "La orden SIT-2026-000004 fue verificada y certificada.", Fecha = DateTime.Now.AddDays(-1), Tipo = "success", Icono = "" },
            new Notificacion { Id = 5, TenantId = 1, Titulo = "Calibración próxima", Mensaje = "El monitor BM-0101 vence calibración en 21 días.", Fecha = DateTime.Now.AddDays(-1), Tipo = "warning", Icono = "" },
            new Notificacion { Id = 6, TenantId = 2, Titulo = "Recordatorio", Mensaje = "Revisión semanal de indicadores programada.", Fecha = DateTime.Now.AddHours(-5), Tipo = "info", Icono = "" });

        await db.SaveChangesAsync();

        // Las columnas de identidad arrancan en 1 y ya sembramos ids explícitos:
        // hay que mover las secuencias al máximo id insertado para evitar colisiones.
        var tablas = new[]
        {
            "instituciones", "usuarios", "equipos", "ordenes_trabajo", "tecnicos",
            "intervenciones", "formularios", "eventos_auditoria", "notificaciones"
        };
        foreach (var tabla in tablas)
        {
            // pg_get_serial_sequence toma el nombre de columna literal (sin interpretar
            // comillas): debe ir 'Id' tal cual. MAX sí requiere "Id" entre comillas.
            var sql = $"SELECT setval(pg_get_serial_sequence('{tabla}', 'Id'), " +
                      $"COALESCE((SELECT MAX(\"Id\") FROM {tabla}), 1), true)";
            await db.Database.ExecuteSqlRawAsync(sql);
        }
    }

    // ------------------------------------------------------------
    // Helpers (equivalentes a los del DemoDataProvider)
    // ------------------------------------------------------------
    private static Usuario U(int id, int tenant, string nombres, string apellidos, string email,
        string cargo, RolSistema rol, DateTime registro) => new()
    {
        Id = id, TenantId = tenant, Nombres = nombres, Apellidos = apellidos,
        Email = email, Cargo = cargo, Rol = rol, FechaRegistro = registro, Activo = true
    };

    /// <summary>Clave demo por rol; se muestran accesos directos en la pantalla de acceso.</summary>
    private static string ClaveDemo(RolSistema rol) => rol switch
    {
        RolSistema.Administrador => "Admin123!",
        RolSistema.CoordinadorBiomedico => "Coord123!",
        RolSistema.Tecnico => "Tec123!",
        RolSistema.PersonalClinico => "Clini123!",
        RolSistema.Director => "Dir123!",
        RolSistema.Regulador => "Audit123!",
        _ => "SITBIO2026!"
    };

    private static string NombreCompleto(Usuario u) => $"{u.Nombres} {u.Apellidos}".Trim();

    private static string Iniciales(Usuario u)
    {
        var n = string.IsNullOrWhiteSpace(u.Nombres) ? "?" : u.Nombres.Trim()[..1];
        var a = string.IsNullOrWhiteSpace(u.Apellidos) ? "" : u.Apellidos.Trim()[..1];
        return (n + a).ToUpperInvariant();
    }

    private static string SiglaRol(RolSistema rol) => rol switch
    {
        RolSistema.Administrador => "ADM",
        RolSistema.CoordinadorBiomedico => "CBM",
        RolSistema.Tecnico => "TEC",
        RolSistema.PersonalClinico => "PCL",
        RolSistema.Director => "DIR",
        RolSistema.Regulador => "REG",
        _ => "---"
    };
}
