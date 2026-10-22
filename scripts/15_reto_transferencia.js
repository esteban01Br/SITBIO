// 15_reto_transferencia.js — reto opcional: septiembre 2026 y contraste histórico vs actual (E03)
// Equivalente adaptado de "asesorías realizadas en septiembre y contraste de sector histórico vs actual".
// Uso desde la raíz del repositorio:
//   mongosh --quiet --file scripts/15_reto_transferencia.js
const dbLab = db.getSiblingDB("sitbio_lab_nosql");
const itr = dbLab.getCollection("s03_intervenciones");
const eqp = dbLab.getCollection("s03_equipos");

print("== Reto de transferencia ==");

// 1) Intervenciones realizadas en septiembre de 2026 (rango UTC)
const septiembre = itr.find(
  { estado: "realizada", fecha_inicio: { $gte: ISODate("2026-09-01T00:00:00Z"), $lt: ISODate("2026-10-01T00:00:00Z") } },
  { codigo: 1, equipo_id: 1, tecnico_id: 1, tipo: 1, horas_invertidas: 1, fecha_inicio: 1, equipo_snapshot: 1, _id: 0 }
).sort({ fecha_inicio: 1 }).toArray();
print(`1) realizadas en septiembre 2026: ${septiembre.length}`);
for (const d of septiembre) print(`   ${d.codigo} · ${d.equipo_id} · ${d.tecnico_id} · ${d.tipo} · ${EJSON.stringify(d.fecha_inicio)}`);

// 2) Contraste del snapshot histórico vs el estado ACTUAL del equipo (equivale a sector histórico vs actual)
// Se elige una intervención cuyo equipo haya CAMBIADO después del snapshot (INT-014: EQP-014 pasó de operativo a fuera_de_servicio)
const muestra = septiembre.find((d) => {
  const eq = eqp.findOne({ codigo: d.equipo_id }, { estado: 1, ubicacion: 1 });
  return eq && (eq.estado !== d.equipo_snapshot.estado || eq.ubicacion !== d.equipo_snapshot.ubicacion);
}) || septiembre[1];
const equipo = eqp.findOne({ codigo: muestra.equipo_id });
print(`2) contraste para ${muestra.codigo} sobre ${equipo.codigo}:`);
print(`   HISTÓRICO (snapshot de la intervención): ubicacion="${muestra.equipo_snapshot.ubicacion}" estado="${muestra.equipo_snapshot.estado}" capturado=${EJSON.stringify(muestra.equipo_snapshot.capturado_en)}`);
print(`   ACTUAL (documento del equipo):           ubicacion="${equipo.ubicacion}" estado="${equipo.estado}"`);
print(muestra.equipo_snapshot.estado === equipo.estado && muestra.equipo_snapshot.ubicacion === equipo.ubicacion
  ? "   lectura: sin cambios entre el momento de la intervención y hoy."
  : "   lectura: el equipo cambió DESPUÉS de la intervención; el snapshot conserva el momento histórico (decisión D-03).");
print("== Reto terminado ==");
