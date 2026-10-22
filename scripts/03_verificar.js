// 03_verificar.js — verifica conteos, integridad y reglas del diccionario (E02)
// Uso desde la raíz del repositorio:
//   mongosh --quiet --file scripts/03_verificar.js
const dbLab = db.getSiblingDB("sitbio_lab_nosql");

const nT = dbLab.tecnicos.countDocuments();
const nE = dbLab.equipos.countDocuments();
const nI = dbLab.intervenciones.countDocuments();

print("== Verificación de la semilla ==");
print(`1) conteos: tecnicos=${nT} (esperado 10) · equipos=${nE} (esperado 15) · intervenciones=${nI} (esperado 30)` +
  (nT === 10 && nE === 15 && nI === 30 ? "  ✓" : "  ✗"));

const dist = {};
dbLab.intervenciones.aggregate([{ $group: { _id: "$estado", n: { $sum: 1 } } }]).forEach((g) => (dist[g._id] = g.n));
print(`2) distribución intervenciones: ${JSON.stringify(dist)} (esperado realizada=15 programada=10 cancelada=5)` +
  (dist.realizada === 15 && dist.programada === 10 && dist.cancelada === 5 ? "  ✓" : "  ✗"));

const codT = new Set(dbLab.tecnicos.find({}, { codigo: 1 }).toArray().map((t) => t.codigo));
const codE = new Set(dbLab.equipos.find({}, { codigo: 1 }).toArray().map((e) => e.codigo));
let rotas = 0;
dbLab.equipos.find({}, { responsable_id: 1 }).forEach((e) => { if (!codT.has(e.responsable_id)) { rotas++; print(`   rota: equipo ${e.codigo} → responsable_id ${e.responsable_id}`); } });
dbLab.intervenciones.find({}, { equipo_id: 1, tecnico_id: 1 }).forEach((i) => {
  if (!codE.has(i.equipo_id)) { rotas++; print(`   rota: intervención ${i.codigo} → equipo_id ${i.equipo_id}`); }
  if (!codT.has(i.tecnico_id)) { rotas++; print(`   rota: intervención ${i.codigo} → tecnico_id ${i.tecnico_id}`); }
});
print(`3) referencias rotas: ${rotas} (esperado 0) ${rotas === 0 ? "✓" : "✗"}`);

const dominio = new Set(["bajo", "medio", "alto"]);
let riesgoMalos = 0;
dbLab.equipos.find({}, { codigo: 1, "campos_dinamicos.clasificacion_riesgo": 1 }).forEach((e) => {
  const r = e.campos_dinamicos?.clasificacion_riesgo;
  if (!dominio.has(r) || r.length > 10) { riesgoMalos++; print(`   inválido: ${e.codigo} → "${r}"`); }
});
print(`4) atributo propio campos_dinamicos.clasificacion_riesgo: ${riesgoMalos} valor(es) fuera de dominio/límite (esperado 0) ${riesgoMalos === 0 ? "✓" : "✗"}`);

let reglasMalas = 0;
dbLab.intervenciones.find().forEach((i) => {
  if (i.estado === "realizada" && (i.fecha_fin === null || i.horas_invertidas <= 0)) { reglasMalas++; print(`   regla rota: ${i.codigo} realizada sin fecha_fin/horas`); }
  if (i.estado === "programada" && i.fecha_fin !== null) { reglasMalas++; print(`   regla rota: ${i.codigo} programada con fecha_fin`); }
  if (i.equipo_snapshot && i.fecha_inicio && EJSON.stringify(i.equipo_snapshot.capturado_en) > EJSON.stringify(i.fecha_inicio)) { reglasMalas++; print(`   regla rota: ${i.codigo} snapshot posterior al inicio`); }
});
print(`5) reglas entre campos (diccionario §4): ${reglasMalas} rota(s) (esperado 0) ${reglasMalas === 0 ? "✓" : "✗"}`);

const muestras = [
  dbLab.intervenciones.findOne({ codigo: "INT-002" }),
  dbLab.equipos.findOne({ codigo: "EQP-002" }),
];
print("6) muestras: " + muestras.map((m) => `${m.codigo} (${m.equipo_snapshot ? "con snapshot, estado=" + m.equipo_snapshot.estado : "sector=" + m.sector})`).join(" · "));
print("== Verificación terminada ==");
