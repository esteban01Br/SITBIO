// 04_error_controlado.js — error controlado: referencia inexistente rechazada antes de escribir (E02)
// Equivalente adaptado del paso "cambiar responsable_id a TEC-999, registrar el rechazo y restaurar".
// Uso desde la raíz del repositorio:
//   mongosh --quiet --file scripts/04_error_controlado.js
const dbLab = db.getSiblingDB("sitbio_lab_nosql");

print("== Error controlado: escritura con referencia inexistente ==");

const antes = dbLab.equipos.findOne({ codigo: "EQP-002" });
print(`ANTES       → EQP-002.responsable_id = "${antes.responsable_id}"`);

const codigos = new Set(dbLab.tecnicos.find({}, { codigo: 1 }).toArray().map((t) => t.codigo));
const intento = Object.assign({}, antes, { responsable_id: "TEC-999" });

// Misma validación previa que usa 02_cargar_semilla.js y la API de SITBIO
if (!codigos.has(intento.responsable_id)) {
  print(`RECHAZADO   → responsable_id="TEC-999" no existe en tecnicos. La escritura NO se ejecuta.`);
} else {
  dbLab.equipos.replaceOne({ codigo: "EQP-002" }, intento);
  print("Escrito (no debía pasar).");
}

const despues = dbLab.equipos.findOne({ codigo: "EQP-002" });
const sinCambios = despues.responsable_id === antes.responsable_id;
print(`DESPUÉS     → EQP-002.responsable_id = "${despues.responsable_id}"  ${sinCambios ? "(sin cambios ✓)" : "(¡SE MODIFICÓ! ✗)"}`);
print("Aprendizaje: en MongoDB la integridad referencial se garantiza validando ANTES de escribir; " +
  "no existe el rechazo automático de una llave foránea como en PostgreSQL.");
