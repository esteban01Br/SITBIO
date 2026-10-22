# Anexo del diccionario — ampliación v1.1 (E03)

Campos agregados en la copia **`s03_tecnicos`** (dataset `S03_SITBIO_V1`) a TEC-001, TEC-002 y
TEC-003 mediante `scripts/11_ampliacion_v11.js`. En la base v1 (`tecnicos`) **no existen**.

| Campo | Tipo | Nulo | Dominio / reglas |
|---|---|---|---|
| `intentos_contacto` | int | No | ≥ 0 · se incrementa con `$inc` en W03 |
| `contacto_alterno` | string | **Sí** | Admite valor, `null` explícito o **campo ausente** (los tres casos están en la semilla) |
| `diagnosticos` | array\<object\> | No | `[{ area: string, puntaje: int 0–100 }]` · **máximo 2 elementos** (regla de validación de la aplicación) |
| `diagnosticos[].area` | string | No | Área evaluada del técnico |
| `diagnosticos[].puntaje` | int | No | 0–100 · permite consultas `$elemMatch` (C09) |
| `temporal` | bool | No | `true` = técnico de apoyo temporal (se filtra en Q01) |

## Valores cargados

| Técnico | `intentos_contacto` | `contacto_alterno` | `diagnosticos` | `temporal` |
|---|---|---|---|---|
| TEC-001 | 2 | `"3015550101"` (presente) | Electrocardiografía 85 · Monitoreo 90 | false |
| TEC-002 | 0 | `null` (explícito) | Imagenología 78 | false |
| TEC-003 | 1 | **ausente** (`$unset`) | Bombas de infusión 80 · Aspiración 70 | true |

Estos tres estados distintos de `contacto_alterno` son los que permiten las consultas
**C07** (filtro `null`) y **C08** (`$exists`), y la oposición **C09** (`$elemMatch` vs filtro cruzado).
