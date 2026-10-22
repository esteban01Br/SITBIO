# Laboratorio NoSQL — modelo documental y CRUD (E02 + E03)

Entregables de **Bases de Datos NoSQL** dentro del repositorio SITBIO, con el caso **adaptado** a
dominio del proyecto (pendiente de validación por el instructor Wilson Castro Gil).

## 1. Adaptación declarada (guía §6)

| Caso de la guía | Equivalente en SITBIO |
|---|---|
| emprendedores (responsable) | **técnicos** (`tecnicos`, con `usuario_id` 1:1 al usuario de PostgreSQL) |
| iniciativas | **equipos biomédicos** (`equipos`, con sector y `campos_dinamicos`) |
| asesorías (historial) | **intervenciones técnicas** (`intervenciones`, historial creciente) |
| sector histórico | **`equipo_snapshot`** (ubicación/estado al momento de la intervención) |
| atributo sectorial pequeño | **`campos_dinamicos.clasificacion_riesgo`** (bajo/medio/alto, máx. 10 caracteres) |
| temas (arreglo) | **`repuestos`** (arreglo con código, nombre y cantidad) |
| diagnosticos (v1.1) | **`diagnosticos`** de `s03_tecnicos` (máx. 2, con `area` y `puntaje`) |

## 2. Requisitos

- **MongoDB** (probado con servidor **7.0.43**, servicio local, puerto 27017) y
  **mongosh** (probado con **2.13.0**). Versiones registradas en `evidencias/e02_versiones.txt`.
- **Sin secretos**: la conexión es a `localhost` sin usuario/contraseña; no hay `.env` ni
  credenciales en ningún archivo de esta carpeta.

## 3. Documentos del modelo (carpeta superior `docs/nosql/`)

`patrones_acceso.md` (Q01–Q06 y W01–W04) · `modelo_v1/` (diagrama y cardinalidades 1:1, 1:N y
prototipo N:M) · `decisiones.md` (D-01 historial, D-02 perfil/atributos, D-03 snapshot) ·
`diccionario.md` + `anexo_v1_1.md` · `integridad.md` · `bitacora_errores.md` ·
`matriz_consultas.md` (C01–C10 esperado/obtenido) · `evidencias/` (salidas reales).

## 4. Orden de ejecución (desde la raíz del repositorio)

```powershell
# E02 — modelo documental v1
mongosh --quiet --file scripts/01_crear_base.js       # base + índices únicos
mongosh --quiet --file scripts/02_cargar_semilla.js   # 1ª carga: 10 / 15 / 30
mongosh --quiet --file scripts/02_cargar_semilla.js   # 2ª carga: los conteos NO aumentan (upsert)
mongosh --quiet --file scripts/03_verificar.js        # 5 verificaciones ✓
mongosh --quiet --file scripts/04_error_controlado.js # TEC-999 rechazado, BD intacta

# E03 — CRUD y consultas
mongosh --quiet --file scripts/10_crear_copias_s03.js    # s03_tecnicos/equipos/intervenciones
mongosh --quiet --file scripts/11_ampliacion_v11.js      # campos v1.1 (TEC-001/002/003)
mongosh --quiet --file scripts/12_demo_crud.js           # insert/update/delete con comprobación
mongosh --quiet --file scripts/13_consultas_aprendiz.js  # C01–C10 (solo lectura)
mongosh --quiet --file scripts/14_variante_tmp101.js     # variante individual TMP-101
mongosh --quiet --file scripts/15_reto_transferencia.js  # reto: septiembre 2026 + snapshot
```

## 5. Conteos esperados

`tecnicos=10` · `equipos=15` · `intervenciones=30` (15 realizadas, 10 programadas, 5 canceladas) ·
copias `s03_*` idénticas · **0 referencias rotas**. Repetir `02_cargar_semilla.js` **no** aumenta
los conteos porque usa `replaceOne` + `upsert` por `codigo`.

## 6. Cómo recuperar un intento incompleto

Si una carga quedó a medias (p. ej. por referencias rotas):

```powershell
mongosh --quiet --eval "db.getSiblingDB('sitbio_lab_nosql').dropDatabase()"
mongosh --quiet --file scripts/01_crear_base.js
mongosh --quiet --file scripts/02_cargar_semilla.js
```

La carga es **idempotente**: no hace falta borrar para reintentar con los mismos datos; el borrado
solo es necesario para arrancar de cero.

## 7. Diferencias frente a E01 (qué aporta esta entrega)

Además de los documentos del modelo, esta entrega añade el **dataset real** de la ampliación
v1.1, el CRUD demostrado con comprobación previa/posterior, las diez consultas con matriz
esperado/obtenido y una bitácora con errores reales de esta máquina (ver `bitacora_errores.md`).

## 8. Declaración de apoyos

Esta entrega se desarrolló con asistencia de **IA (asistente de código OpenCode, modelo MiMo)** para
la redacción de documentos, la generación de la semilla y la escritura de los scripts; **verificó
por su cuenta** (antes de considerar válido cualquier resultado): la ejecución de los 10 scripts
sobre su propia instancia de MongoDB, las salidas de `evidencias/`, que la segunda carga no
duplica documentos, que `TEC-999` se rechaza sin tocar la base y la lectura de cada salida de la
matriz C01–C10. *— borrador para que Esteban Bedoya Rojo lo ajuste a su voz antes de entregar.*

## 9. Pendientes de coordinación

- Revisión cruzada con otro aprendiz (lista en `evidencias/README.md`).
- Validación de la adaptación por el instructor (preguntas de la guía §9).
