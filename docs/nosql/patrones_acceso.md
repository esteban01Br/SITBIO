# Patrones de acceso — modelo documental SITBIO (E02)

Base de datos: `sitbio_lab_nosql` · Colecciones: `tecnicos` (10), `equipos` (15), `intervenciones` (30).

> **Adaptación del caso de la guía** (pendiente de validación por el instructor):
> emprendedores → **técnicos**, iniciativas → **equipos biomédicos**, asesorías → **intervenciones técnicas**.

## Consultas frecuentes (Q01–Q06)

| Código | Consulta | Actor | Filtros | Salida (proyección) | Orden | Frecuencia |
|---|---|---|---|---|---|---|
| Q01 | Técnicos disponibles con su carga actual | Coordinador | `temporal: { $ne: true }` | `codigo`, `nombre`, `servicio`, `especialidades` | `nombre` asc | Varias veces por turno |
| Q02 | Hoja de vida de un equipo (documento completo) | Coordinador biomédico | `codigo: "EQP-00x"` | Documento completo, incluye `campos_dinamicos` | — | Diaria |
| Q03 | Historial de intervenciones de un equipo | Técnico / coordinador | `equipo_id: "EQP-00x"` | `codigo`, `fecha_inicio`, `tipo`, `estado`, `equipo_snapshot` | `fecha_inicio` desc | Diaria |
| Q04 | Intervenciones realizadas en un rango de fechas (reportes) | Directivo | `estado: "realizada"` + `fecha_inicio` en rango UTC | `fecha_inicio`, `equipo_id`, `tipo`, `horas_invertidas` | `fecha_inicio` asc | Semanal |
| Q05 | Equipos de un sector o con riesgo alto | Coordinador | `sector` y/o `campos_dinamicos.clasificacion_riesgo` | `codigo`, `nombre`, `sector`, `ubicacion` | `codigo` asc | Diaria |
| Q06 | Repuestos más usados (plan de compras) | Logística | `repuestos.0` existe → `unwind` + `group` | `repuestos.codigo`, total de unidades | total desc | Mensual |

## Escrituras (W01–W04)

| Código | Escritura | Actor | Operación | Reglas |
|---|---|---|---|---|
| W01 | Registrar una intervención | Técnico | `insertOne` en `intervenciones` | **Validación previa**: `equipo_id` y `tecnico_id` deben existir; si no, la escritura se rechaza antes de ejecutarse (evidencia: `scripts/04_error_controlado.js`) |
| W02 | Actualizar estado o ubicación actual de un equipo | Coordinador | `updateOne` + `$set` | Solo afecta el documento actual del equipo; el histórico vive en los `equipo_snapshot` de las intervenciones |
| W03 | Actualizar contacto del técnico | Técnico | `updateOne` + `$set` (`contacto_alterno`) y `$inc` (`intentos_contacto`) | Admite `null` explícito (no tiene alterno) o campo ausente (nunca se registró); la diferencia se documenta en C07/C08 |
| W04 | Cerrar una intervención | Coordinador | `updateOne` + `$set` (`estado: "realizada"`, `fecha_fin`, `horas_invertidas`) | El histórico **nunca** se borra ni se reescribe (append-only) |

## Decisiones transversales de acceso

- Las referencias entre colecciones usan **códigos legibles** (`TEC-001`, `EQP-007`), no `ObjectId`:
  las evidencias se leen humanamente y los códigos son estables entre cargas (ver `integridad.md`).
- MongoDB no tiene llaves foráneas: **toda escritura cruzada valida referencias antes de escribir**
  (patrón repetido en `02_cargar_semilla.js`, la API de SITBIO y `12_demo_crud.js`).
- Las fechas se almacenan como BSON Date (Extended JSON `$date`) en **UTC**, para que los rangos
  (Q04, C04) no dependan de la zona horaria de la máquina.
