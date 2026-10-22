// 14_variante_tmp101.js — variante individual sobre TMP-101 (E03)
// Comprobar responsable → insertar → $set → $inc → borrar, verificando que los
// registros ajenos no cambian.
// Uso desde la raíz del repositorio:
//   mongosh --quiet --file scripts/14_variante_tmp101.js
const dbLab = db.getSiblingDB("sitbio_lab_nosql");
const tmp = dbLab.getCollection("tmp_laboratorio");
const eqp = dbLab.getCollection("s03_equipos");
const itr = dbLab.getCollection("s03_intervenciones");

print("== Variante individual: TMP-101 ==");

// 1) comprobar el responsable del equipo antes de operar
const eq = eqp.findOne({ codigo: "EQP-002" });
print(`1) responsable de EQP-002 = ${eq.responsable_id} (comprobado antes de operar)`);

// 2) insertar TMP-101 SOLO si las referencias existen
const tec = dbLab.getCollection("s03_tecnicos");
const codigos = new Set(tec.find({}, { codigo: 1 }).toArray().map((t) => t.codigo));
const doc = { codigo: "TMP-101", equipo_id: eq.codigo, tecnico_id: eq.responsable_id, reintentos: 0, notas: null };
if (!codigos.has(doc.tecnico_id)) throw new Error("RECHAZADO: tecnico_id inexistente");
tmp.insertOne(doc);
print(`2) insertOne TMP-101 → ${EJSON.stringify(tmp.findOne({ codigo: "TMP-101" }, { _id: 0 }))}`);

// 3) $set de notas y $inc de reintentos
tmp.updateOne({ codigo: "TMP-101" }, { $set: { notas: "Prueba de variante individual." }, $inc: { reintentos: 1 } });
const actualizado = tmp.findOne({ codigo: "TMP-101" }, { _id: 0 });
print(`3) $set + $inc → ${EJSON.stringify(actualizado)}`);

// 4) borrar con filtro explícito
const r = tmp.deleteOne({ codigo: "TMP-101" });
print(`4) deleteOne({ codigo: "TMP-101" }) → deleted=${r.deletedCount} · tmp_laboratorio=${tmp.countDocuments()}`);

// 5) los registros ajenos no cambiaron
const eqIntacto = eqp.findOne({ codigo: "EQP-002" }, { responsable_id: 1 });
const nInterv = itr.countDocuments();
print(`5) verificación: EQP-002.responsable_id = ${eqIntacto.responsable_id} (igual que al inicio) ${eqIntacto.responsable_id === eq.responsable_id ? "✓" : "✗"} · s03_intervenciones = ${nInterv} (esperado 30) ${nInterv === 30 ? "✓" : "✗"}`);
print("Listo.");
