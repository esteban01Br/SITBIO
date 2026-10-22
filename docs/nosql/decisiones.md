# Decisiones de diseño del modelo documental (E02)

Tres decisiones de **embeber vs referenciar** (patrón de la guía). Cada una con necesidad,
alternativas, elección, costo y condición de revisión.

---

## D-01 · Historial de intervenciones: ¿documento embebido o colección separada?

- **Necesidad**: responder Q03 (historial por equipo), Q04 (rango de fechas) y W04 (cerrar
  intervención) sobre un historial que **crece sin límite** (cada equipo acumula intervenciones
  cada vez que se interviene).
- **Alternativas**:
  1. Embeber `intervenciones: [...]` como arreglo dentro del documento del equipo.
  2. Colección propia `intervenciones` con `equipo_id` como referencia.
- **Elección**: **colección separada** (`intervenciones`).
- **Costo**: se pierde la lectura de "equipo + historial" en un solo `findOne`; hay que hacer dos
  lecturas o una agregación, y las referencias se validan en la aplicación (MongoDB no tiene FK).
- **Condición de revisión**: si más del 95 % de las lecturas siempre requirieran el historial
  completo y el crecimiento fuera acotado (p. ej. menos de 50 intervenciones por equipo), volver a
  evaluar el arreglo embebido (límite práctico: documentos < 16 MB y arrays < 100 niveles de profundidad).

---

## D-02 · Perfil del técnico: campos fijos o atributos variables embebidos

- **Necesidad**: cada técnico tiene datos fijos (nombre, servicio, correo) y **atributos variables**
  (especialidades, contacto alterno, intentos de contacto, diagnósticos de desempeño v1.1).
- **Alternativas**:
  1. Todo en colecciones separadas (`tecnicos` + `tecnicos_atributos`).
  2. Campos fijos + colección de atributos referenciada.
  3. Híbrido: campos fijos y atributos **embebidos** en el mismo documento.
- **Elección**: **híbrido (3)**: fijos a nivel raíz y variables embebidos (`especialidades` como
  arreglo y `campos_dinamicos` como objeto).
- **Costo**: el documento crece y toda actualización reescribe el documento completo (MongoDB es
  almacenamiento por documento); irrelevante a esta escala (10 técnicos).
- **Condición de revisión**: si los atributos pasan a ser crecientes por ejemplo certificaciones
  con PDF, historiales), moverlos a una subcolección `tecnicos_historial`.

---

## D-03 · Snapshot histórico del equipo: ¿referencia viva o copia embebida?

- **Necesidad**: responder "¿en qué **ubicación y estado** estaba el equipo cuando se intervino?",
  contrastándolo con el estado **actual** (reto de transferencia: histórico vs actual).
- **Alternativas**:
  1. Solo referencia viva (`equipo_id`) y leer la ubicación actual del equipo.
  2. Copia embebida del estado del equipo al momento de la intervención.
  3. Colección de eventos/auditoría de cambios de equipo.
- **Elección**: **copia embebida** `intervenciones.equipo_snapshot { ubicacion, estado, capturado_en }`.
- **Costo**: duplicidad deliberada: si el equipo se mueve después, el snapshot queda "desfasado"
  — que es justamente el objetivo (histórico inmutable). Requiere que W02 solo actualice al equipo
  actual y jamás a los snapshots pasados.
- **Condición de revisión**: si la institución exige trazabilidad formal de cada cambio (cumplimiento
  normativo), migrar a una colección de eventos `cambios_equipo` con inserción única y sin updates.

---

## Atributo propio de la adaptación (adaptación propia E02)

Se agregó el atributo sectorial **`campos_dinamicos.clasificacion_riesgo`**: tipo `string`,
dominio `{ bajo, medio, alto }`, límite 10 caracteres. Aparece en `equipos` (diccionario §3.2) y se
verifica en `03_verificar.js` (dominio y longitud). Justificación: los servicios clínicos exigen
priorizar equipos por riesgo sin tocar el esquema base.
