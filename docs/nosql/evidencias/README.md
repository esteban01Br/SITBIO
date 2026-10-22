# Evidencias — laboratorio NoSQL (E02/E03)

Salidas **reales** de mongosh en este equipo (no copiadas de otro lado).
Versiones registradas en `e02_versiones.txt`: **mongosh 2.13.0** · **servidor MongoDB 7.0.43** (servicio local, puerto 27017), base `sitbio_lab_nosql`.

## E02 — modelo documental v1

| Archivo | Qué muestra |
|---|---|
| `e02_versiones.txt` | `mongosh --version` y `db.version()` |
| `e02_crear_base.txt` | Base + índices únicos creados |
| `e02_carga_primera.txt` | 1ª carga: tecnicos=10, equipos=15, intervenciones=30 · 0 rechazos |
| `e02_carga_segunda.txt` | 2ª carga repetida: **los conteos no aumentan** (`replaceOne` + upsert) |
| `e02_verificacion.txt` | 5 verificaciones ✓ (conteos, distribución 15/10/5, referencias rotas=0, atributo propio, reglas) |
| `e02_error_controlado.txt` | `responsable_id = TEC-999` → **rechazo previo a la escritura**, BD sin cambios |
| `e02_intento_fallido_cursor.txt` | Error real de la bitácora #1 (cursor vs array) — conservado a propósito |

## E03 — CRUD, consultas y ampliación v1.1

| Archivo | Qué muestra |
|---|---|
| `e03_copias_s03.txt` | Colecciones `s03_*` con dataset `S03_SITBIO_V1` |
| `e03_ampliacion_v11.txt` | Campos v1.1 aplicados a TEC-001/002/003 (null, ausente y presente) |
| `e03_crud.txt` | Demo CRUD con comprobación previa/posterior |
| `e03_consultas.txt` | C01–C10 con esperado y obtenido |
| `e03_consultas.txt` + `../matriz_consultas.md` | Matriz esperado/obtenido con las mismas salidas |
| `e03_variante_tmp101.txt` | Variante individual sobre TMP-101 |
| `e03_reto.txt` | Reto de transferencia: septiembre 2026 + histórico vs actual |

## Lista de revisión E02/E03 (para la revisión cruzada con un compañero)

Instrucción: otro aprendiz reproduce las cargas y consultas **solo siguiendo** la
`README` de `docs/nosql/` y completa esta lista.

| # | Criterio | ¿Reproducido? | Observaciones |
|---|---|---|---|
| 1 | Ejecuté `01_crear_base.js` y `02_cargar_semilla.js` sin tocar nada más | ☐ | |
| 2 | Los conteos 10/15/30 coinciden con lo esperado | ☐ | |
| 3 | Repetir la carga no aumenta los conteos | ☐ | |
| 4 | `03_verificar.js` marca las 5 verificaciones ✓ | ☐ | |
| 5 | El error `TEC-999` se rechaza y la BD queda intacta | ☐ | |
| 6 | Las 10 consultas C01–C10 devuelven lo que dice la matriz | ☐ | |
| 7 | El README alcanza para trabajar sin preguntar pasos extra | ☐ | |

- **Fortalezas observadas**:
- **Dos hallazgos** (qué mejoraría):
- **Correcciones aplicadas tras la revisión**:
- Revisor: _________________ (ficha __________) · Fecha: __________
