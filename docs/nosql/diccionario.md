# Diccionario de datos — modelo documental v1 (E02)

Base `sitbio_lab_nosql`. Tipos según BSON/Extended JSON. "Nulo" = admite `null`; "Ausente" = el
campo puede no existir en el documento (la diferencia se ejemplifica en C07/C08).

## 1. Colección `tecnicos` (10 documentos)

| Campo | Tipo | Nulo | Dominio / formato | Reglas |
|---|---|---|---|---|
| `codigo` | string | No | `TEC-###` | **Identificador de negocio y llave única** (índice único) |
| `nombre` | string | No | 3–80 caracteres | Nombre completo |
| `email` | string | No | correo válido | Único por colección (validación en carga) |
| `servicio` | string | No | Quirófano, Imagenología, Urgencias, UCI, Laboratorio, Esterilización, Infraestructura | "Sector" clínico del técnico |
| `especialidades` | array\<string\> | No | ≥ 1 elemento | Arreglo de especialidades (consultas de arreglo) |
| `usuario_id` | int | No | ≥ 1 | Referencia 1:1 al usuario de PostgreSQL; **único** |
| `creado` | date | No | BSON Date (UTC) | Fecha de creación del registro |

## 2. Colección `equipos` (15 documentos)

| Campo | Tipo | Nulo | Dominio / formato | Reglas |
|---|---|---|---|---|
| `codigo` | string | No | `EQP-###` | **Llave única** (índice único) |
| `nombre` | string | No | 3–120 caracteres | Nombre del equipo |
| `categoria` | string | No | p. ej. Autoclave, Ventilador… | Familia del equipo |
| `sector` | string | No | igual dominio que `tecnicos.servicio` | Sector **actual** del equipo |
| `estado` | string | No | `operativo` \| `mantenimiento` \| `fuera_de_servicio` | Estado **actual** (el histórico vive en `intervenciones.equipo_snapshot`) |
| `ubicacion` | string | No | p. ej. `Piso 3 - Quirófano 2` | Ubicación **actual** |
| `responsable_id` | string | No | `TEC-###` | Referencia al técnico responsable; **debe existir** (validación previa a la escritura) |
| `campos_dinamicos.clasificacion_riesgo` | string | No | `bajo` \| `medio` \| `alto` (**máx. 10 caracteres**) | **Atributo propio de la adaptación** (ver decisiones.md) |
| `campos_dinamicos.resolucion_sanitaria` | string | Sí | p. ej. `INVIMA 2019-001234` | Resolución/registro sanitario si aplica |
| `fecha_instalacion` | date | No | BSON Date (UTC) | Fecha de instalación |
| `creado` | date | No | BSON Date (UTC) | Creación del registro |

## 3. Colección `intervenciones` (30 documentos)

| Campo | Tipo | Nulo | Dominio / formato | Reglas |
|---|---|---|---|---|
| `codigo` | string | No | `INT-###` | **Llave única** (índice único) |
| `equipo_id` | string | No | `EQP-###` | Referencia; **debe existir** |
| `tecnico_id` | string | No | `TEC-###` | Referencia; **debe existir** |
| `tipo` | string | No | `preventivo` \| `correctivo` \| `calibracion` | Tipo de intervención |
| `estado` | string | No | `programada` (10) \| `realizada` (15) \| `cancelada` (5) | Distribución fijada por la semilla |
| `fecha_inicio` | date | No | BSON Date (UTC) | Programadas: fecha futura; realizadas/canceladas: pasada |
| `fecha_fin` | date | Sí | BSON Date (UTC) | `null` mientras no se realice |
| `repuestos` | array\<object\> | No | `[{ codigo: "REP-###", nombre: string, cantidad: int ≥ 1 }]` | Puede ser `[]` (sin repuestos) |
| `horas_invertidas` | double | No | ≥ 0 | Solo tiene sentido en realizadas |
| `notas` | string | Sí | texto libre | Admite `null` (sin notas) u **ausente** (nunca registrado) |
| `equipo_snapshot.ubicacion` | string | No | como `equipos.ubicacion` | Ubicación del equipo **al momento de la intervención** |
| `equipo_snapshot.estado` | string | No | dominio de `equipos.estado` | Estado del equipo **al momento de la intervención** |
| `equipo_snapshot.capturado_en` | date | No | BSON Date (UTC) | Momento de la captura; **≤ `fecha_inicio`** |

## 4. Reglas entre campos

1. `intervenciones.equipo_id` y `.tecnico_id` deben corresponder a un `codigo` existente en
   `equipos` / `tecnicos` (referencial por validación, no por FK).
2. Si `estado = "realizada"`, entonces `fecha_fin` no es `null` y `horas_invertidas > 0`.
3. Si `estado = "programada"`, entonces `fecha_fin` es `null`.
4. `equipo_snapshot.capturado_en ≤ fecha_inicio` (el snapshot no puede ser posterior al inicio).
5. `equipos.sector` pertenece al mismo dominio que `tecnicos.servicio` (misma taxonomía de servicios).

## 5. Anexo v1.1 (ampliación de E03)

Campos agregados en la copia `s03_tecnicos` a TEC-001, TEC-002 y TEC-003 — ver
`anexo_v1_1.md`. Resumen: `intentos_contacto` (int ≥ 0), `contacto_alterno` (string **o null o
ausente**), `diagnosticos` (array de `{ area, puntaje }`, **máximo 2**), `temporal` (bool).
