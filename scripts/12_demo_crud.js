// 12_demo_crud.js — demostración CRUD sobre colecciones s03 (E03)
// insertOne/insertMany · updateOne/updateMany ($set, $inc) · deleteOne/deleteMany
// con filtro EXPLÍCITO y comprobación previa/posterior en cada paso.
// Uso desde la raíz del repositorio:
//   mongosh --quiet --file scripts/12_demo_crud.js
const dbLab = db.getSiblingDB("sitbio_lab_nosql");
const interv = dbLab.getCollection("s03_intervenciones");
const tec = dbLab.getCollection("s03_tecnicos");

const cuenta = (c) => c.countDocuments();
print("== Demo CRUD sobre s03_* ==");
print(`estado inicial: s03_intervenciones=${cuenta(interv)} · s03_tecnicos=${cuenta(tec)}`);

// --- insertOne (con validación de referencias previa, igual que la carga) ---
const codTec = new Set(tec.find({}, { codigo: 1 }).toArray().map((x) => x.codigo));
const codEqp = new Set(dbLab.getCollection("s03_equipos").find({}, { codigo: 1 }).toArray().map((x) => x.codigo));
const nueva = {
  codigo: "INT-901", equipo_id: "EQP-001", tecnico_id: "TEC-003", tipo: "correctivo",
  estado: "programada", fecha_inicio: EJSON.parse('{ "$date": "2026-12-01T09:00:00Z" }'),
  fecha_fin: null, repuestos: [], horas_invertidas: 0, notas: "Alta por demo CRUD.",
  equipo_snapshot: { ubicacion: "Piso 3 - Quirófano 2", estado: "operativo", capturado_en: EJSON.parse('{ "$date": "2026-12-01T09:00:00Z" }') },
};
if (!codTec.has(nueva.tecnico_id) || !codEqp.has(nueva.equipo_id)) throw new Error("referencias inválidas");
interv.insertOne(nueva);
print(`insertOne: INT-901 insertada → s03_intervenciones=${cuenta(interv)}`);

// --- insertMany ---
interv.insertMany([
  Object.assign({}, nueva, { codigo: "INT-902", notas: "Compañera de demo." }),
  Object.assign({}, nueva, { codigo: "INT-903", notas: "Compañera de demo." }),
]);
print(`insertMany: INT-902 e INT-903 → s03_intervenciones=${cuenta(interv)}`);

// --- updateOne con $set (filtro explícito por código) ---
const r1 = interv.updateOne({ codigo: "INT-901" }, { $set: { notas: "Actualizada con $set.", tipo: "preventivo" } });
print(`updateOne: filtro { codigo: "INT-901" } → matched=${r1.matchedCount} modified=${r1.modifiedCount} · notas="${interv.findOne({ codigo: "INT-901" }).notas}"`);

// --- updateMany con $inc ---
const r2 = tec.updateMany({ servicio: "Quirófano" }, { $inc: { intentos_contacto: 1 } });
const quirofano = tec.find({ servicio: "Quirófano" }, { codigo: 1, intentos_contacto: 1 }).toArray();
print(`updateMany: filtro { servicio: "Quirófano" } + $inc intentos_contacto → matched=${r2.matchedCount} modified=${r2.modifiedCount}`);
print(`  después: ${quirofano.map((x) => `${x.codigo}:${x.intentos_contacto}`).join(" · ")}`);

// --- deleteOne con filtro explícito ---
const r3 = interv.deleteOne({ codigo: "INT-901" });
print(`deleteOne: filtro { codigo: "INT-901" } → deleted=${r3.deletedCount} · s03_intervenciones=${cuenta(interv)}`);

// --- deleteMany con filtro explícito ---
const r4 = interv.deleteMany({ codigo: { $in: ["INT-902", "INT-903"] } });
print(`deleteMany: filtro { codigo: { $in: ["INT-902","INT-903"] } } → deleted=${r4.deletedCount} · s03_intervenciones=${cuenta(interv)}`);

// --- verificación: los registros ajenos no cambiaron ---
const intacta = interv.countDocuments({ codigo: { $regex: "^INT-0" } });
print(`verificación final: ${intacta} intervenciones originales intactas (esperado 30) ${intacta === 30 ? "✓" : "✗"} · s03_intervenciones final=${cuenta(interv)}`);
print("Listo.");
