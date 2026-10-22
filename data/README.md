# Datos de la semilla (E02)

Archivos **Extended JSON** (extensión `.ejson`) listos para `EJSON.parse` en mongosh.

| Archivo | Colección | Documentos | Contenido |
|---|---|---|---|
| `tecnicos_v1.ejson` | `tecnicos` | **10** | Técnicos biomédicos con servicio y especialidades |
| `equipos_v1.ejson` | `equipos` | **15** | Equipos con sector, estado actual y `campos_dinamicos` |
| `intervenciones_v1.ejson` | `intervenciones` | **30** | **15 realizadas, 10 programadas, 5 canceladas** |

## Por qué Extended JSON

Las fechas deben viajar como **BSON Date** (no como texto): se escriben como
`{ "$date": "2026-09-15T14:30:00Z" }`. Si se usara `JSON.parse` normal, Mongo guardaría strings y
las consultas por rango de fechas (Q04, C04) dejarían de funcionar. Por eso los scripts usan
`EJSON.parse`.

## Reglas de la semilla

- Códigos legibles e irrepetibles: `TEC-###`, `EQP-###`, `INT-###` (llave de upsert en la carga).
- Todas las referencias (`responsable_id`, `equipo_id`, `tecnico_id`) apuntan a códigos existentes
  de esta misma semilla.
- Las intervenciones programadas tienen `fecha_fin: null`; las realizadas, `horas_invertidas > 0`.
- Cada intervención lleva `equipo_snapshot` con la ubicación/estado del equipo en ese momento
  (algunos difieren del estado actual a propósito, para el contraste histórico-vs-actual).

> Regenerar estos archivos: son texto plano UTF-8; se pueden editar a mano y volver a ejecutar
> `scripts/02_cargar_semilla.js` (es idempotente: la segunda carga no aumenta los conteos).
