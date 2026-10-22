# Integridad y política de eliminación (E02)

## 1. Modelo de referencias

MongoDB no impone llaves foráneas; SITBIO lo compensa con **tres capas**:

1. **Referencias por código legible** (`TEC-001`, `EQP-007`): estables entre cargas y auditables.
2. **Validación previa a toda escritura cruzada**: `02_cargar_semilla.js`, la API (controladores)
   y `12_demo_crud.js` verifican que las referencias existan **antes** de escribir. Si no existen,
   la escritura se **rechaza sin tocar la base** (evidencia: `evidencias/e02_error_controlado.txt`,
   intento fallido con `responsable_id = TEC-999`).
3. **Verificación de referencias rotas** al final de cada carga (`03_verificar.js` imprime
   `referencias_rotas = 0`).

**Dueños de las referencias**

| Referencia | Dueño | Política |
|---|---|---|
| `equipos.responsable_id` → técnico | El **equipo** apunta al técnico | Si un técnico deja de estar disponible, el equipo conserva la referencia y el coordinador lo reasigna (W02); no se borra el técnico |
| `intervenciones.equipo_id` → equipo | La **intervención** apunta al equipo | El equipo no se borra nunca: se marca `estado: "fuera_de_servicio"` (borrado lógico) |
| `intervenciones.tecnico_id` → técnico | La **intervención** apunta al técnico | Histórico inmutable: aunque el técnico se retire, la intervención conserva quién la hizo |
| `tecnicos.usuario_id` → usuario (PostgreSQL) | El **técnico** apunta al usuario | 1:1; el usuario de baja en PostgreSQL no se borra (campo `Activo = false`) |

## 2. Política de eliminación y archivo

- **`intervenciones`: append-only.** Nunca se ejecutan `deleteOne`/`updateOne` sobre
  intervenciones cerradas; los cambios se registran con `$set` de campos de cierre (W04).
  Es la misma filosofía de la bitácora `eventos_auditoria` de PostgreSQL (solo INSERT).
- **`equipos`: borrado lógico.** El estado `fuera_de_servicio` cumple el papel del borrado lógico.
  En el sistema relacional de SITBIO esto ya existe: `instituciones` se filtra con `"Estado" <> 2`
  (la API nunca devuelve instituciones eliminadas) — mismo patrón, distinto mecanismo.
- **`tecnicos`: no se borran.** Si es necesario excluirlo de los listados se filtra en la consulta
  (por ejemplo `temporal ≠ true` en Q01).
- **Único escenario con `deleteOne`**: documentos de prueba temporales creados por los scripts del
  laboratorio (`TMP-101`, `TMP-CRUD-*`), siempre con filtro explícito por código y con
  comprobación previa/posterior (ver `12_demo_crud.js` y `14_variante_tmp101.js`).

## 3. Concurrencia: qué falta (análisis honesto)

El entorno actual (`mongod` standalone, sin replica set) **no ofrece transacciones
multi-documento**, por lo que:

- No hay bloqueo entre el `updateOne` de una intervención y el del equipo relacionado.
- Falta un campo de **versión** para *optimistic locking* (p. ej. `version: int` y condición
  `{ codigo: "INT-007", version: 3 }` en cada `$set` con `$inc: { version: 1 }`).
- Falta índice de **colación simple** explícita para las consultas paginadas (C10 usa
  `sort({ sector: 1, codigo: 1 })` con colación simple por defecto).

Si SITBIO lleva el modelo a producción, la recomendación es levantar el conjunto de réplicas
(habilita transacciones) y añadir el control de versión antes de permitir escrituras concurrentes.
