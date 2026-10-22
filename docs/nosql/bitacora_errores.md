# Bitácora de errores — laboratorio NoSQL (E02/E03)

Errores **reales** encontrados al trabajar con MongoDB/mongosh, con su corrección y prueba posterior.

---

## Error 1 · `TypeError: object is not iterable` con `find().map()` en mongosh

- **Contexto**: E02 — `scripts/02_cargar_semilla.js`, construcción del conjunto de códigos de
  técnicos para validar referencias antes de cargar los equipos.
- **Síntoma exacto**:
  ```
  TypeError: object is not iterable (cannot read property Symbol(Symbol.iterator))
  ```
  La carga se detenía en equipos (quedaban 0 de 15) aunque los técnicos sí se escribían (10).
  Evidencia conservada: `evidencias/e02_intento_fallido_cursor.txt`.
- **Causa**: en mongosh, `db.coleccion.find().map(f)` **devuelve el cursor transformado**, no un
  array; `new Set(cursor)` lanza el `TypeError`. (En el driver de Node el comportamiento encadena
  igual, pero allí se consume con `forEach`/`toArray`.)
- **Corrección**: usar siempre `find(...).toArray().map(...)` antes de construir conjuntos:
  ```js
  const codTecnicos = new Set(dbLab.tecnicos.find({}, { codigo: 1 }).toArray().map((t) => t.codigo));
  ```
  Además `coleccion.collectionName` imprimía `[object Object]` en el mensaje; se pasa el nombre de
  la colección como parámetro explícito.
- **Prueba posterior**: recarga limpia completa — 1ª carga 10/15/30 sin rechazos,
  2ª carga idéntica, verificación 5/5 (`evidencias/e02_carga_primera.txt`,
  `e02_verificacion.txt`).
- **Aprendizaje**: en mongosh nada debe asumirse iterable: `toArray()` es el puente explícito
  entre cursor y arreglo; verificar siempre los conteos después de cada fase de carga.

---

## Error 2 · Proyección incompleta y llave sobrante en C09/reto (E03)

- **Contexto**: E03 — primera ejecución de `13_consultas_aprendiz.js` y `15_reto_transferencia.js`.
- **Síntoma exacto**:
  1. `SyntaxError: Unexpected token, expected "," (70:166)` en la llamada `titulo()` de C09.
  2. `TypeError: Cannot read properties of undefined (reading 'ubicacion')` en el reto.
- **Causa**: (1) un `}` de más al cerrar el objeto del filtro de C09; (2) la consulta del reto
  **no proyectaba `equipo_snapshot`**, así que `muestra.equipo_snapshot` llegaba `undefined`.
- **Corrección**: quitar la llave sobrante y agregar `equipo_snapshot: 1` a la proyección. De paso
  se corrigió un *esperado* equivocado de C02 (EQP-002 pertenece a Urgencias: el resultado correcto
  son 3 equipos, no 4).
- **Prueba posterior**: C01–C10 en verde y el reto muestra el contraste INT-014
  (`histórico="operativo"` vs `actual="fuera_de_servicio"`) — `evidencias/e03_consultas.txt` y
  `e03_reto.txt`.
- **Aprendizaje**: la proyección de MongoDB recorta el documento real: todo campo que se vaya a
  usar después debe incluirse explícitamente; y el "esperado" de la matriz debe salir de los datos
  sembrados, no de la intuición.

---
