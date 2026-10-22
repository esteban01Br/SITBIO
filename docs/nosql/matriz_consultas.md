# Matriz de consultas C01–C10 (E03) — esperado vs obtenido

Salidas reales de `scripts/13_consultas_aprendiz.js` sobre `s03_*` (evidencia: `evidencias/e03_consultas.txt`).

| Código | Requisito | Filtro | Proyección | Orden | Esperado | Obtenido | Evidencia |
|---|---|---|---|---|---|---|---|
| C01 | Intervenciones programadas | `{ estado: "programada" }` | código, fecha_inicio | fecha_inicio asc | 10 | **10** ✓ | e03_consultas.txt |
| C02 | Equipos del sector Imagenología | `{ sector: "Imagenología" }` | código, nombre | código asc | 3 (EQP-003/011/015) | **3** ✓ | e03_consultas.txt |
| C03 | Riesgo alto (campo anidado) | `{ "campos_dinamicos.clasificacion_riesgo": "alto" }` | código, riesgo | — | 5 | **5** ✓ | e03_consultas.txt |
| C04 | Rango de fechas UTC (septiembre 2026) | `{ estado: "realizada", fecha_inicio: { $gte, $lt } }` | código, fecha_inicio | fecha_inicio asc | 5 (INT-011..015) | **5** ✓ | e03_consultas.txt |
| C05 | Snapshot no operativo (histórico vs actual) | `{ "equipo_snapshot.estado": { $ne: "operativo" } }` | código, equipo_id, snapshot.estado | — | contraste por fila | **7 filas** con contraste histórico/actual | e03_consultas.txt |
| C06 | Arreglo de repuestos | `{ "repuestos.0": { $exists: true } }` vs `{ repuestos: { $size: 0 } }` | código, repuestos | — | 10 con / 20 sin (= 30) | **10 / 20 = 30** ✓ | e03_consultas.txt |
| C07 | `null` de contacto | `{ contacto_alterno: null }` | código, contacto_alterno | código asc | null **y** ausentes | **9**: TEC-002 (null) + 8 ausentes | e03_consultas.txt |
| C08 | Ausente vs presente | `{ contacto_alterno: { $exists: false/true } }` | código | código asc | 8 ausentes / 2 presentes | **8 / 2** ✓ (TEC-003 distinguido de TEC-002) | e03_consultas.txt |
| C09 | `$elemMatch` mismo elemento | `{ diagnosticos: { $elemMatch: { area: "Aspiración", puntaje: { $gte: 80 } } } }` vs filtro cruzado | código, diagnosticos | — | elemMatch 0 · cruzado falsopositivo | **0 / 1 (TEC-003)** ✓ | e03_consultas.txt |
| C10 | Paginación de 3 por `{ sector: 1, codigo: 1 }` | skip/limit vs `$or` con clave anterior | código, sector | sector asc, código asc | 15 únicos por método, mismo orden | **15/15 únicos ✓ y orden idéntico** | e03_consultas.txt |

## Lecturas clave sustentables

- **C07 vs C08**: en MongoDB `{ campo: null }` casa con el `null` explícito **y** con el campo
  ausente; para distinguirlos se usa `$exists`.
- **C09**: el filtro cruzado `{ "diagnosticos.area": …, "diagnosticos.puntaje": … }` compara
  elementos **distintos** del arreglo; `$elemMatch` exige que ambas condiciones se cumplan en el
  **mismo** elemento (falso positivo demostrado con TEC-003).
- **C10**: `skip` puede duplicar/saltar registros si hay escrituras entre páginas; la continuación
  por clave (`$or` con el último `{sector, codigo}`) es estable.
- **C05/reto**: el `equipo_snapshot` conserva el momento histórico aunque el equipo cambie
  (decisión D-03 de `decisiones.md`).
