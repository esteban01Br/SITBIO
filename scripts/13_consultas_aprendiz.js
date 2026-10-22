// 13_consultas_aprendiz.js — consultas C01–C10, SOLO LECTURA (E03)
// Uso desde la raíz del repositorio:
//   mongosh --quiet --file scripts/13_consultas_aprendiz.js
const dbLab = db.getSiblingDB("sitbio_lab_nosql");
const tec = dbLab.getCollection("s03_tecnicos");
const eqp = dbLab.getCollection("s03_equipos");
const itr = dbLab.getCollection("s03_intervenciones");

function titulo(codigo, requisito, filtro) {
  print("");
  print(`--- ${codigo} · ${requisito}`);
  print(`filtro: ${EJSON.stringify(filtro)}`);
}

// C01 · Filtro por estado: intervenciones programadas
titulo("C01", "Intervenciones programadas", { estado: "programada" });
const c01 = itr.find({ estado: "programada" }, { codigo: 1, fecha_inicio: 1, _id: 0 }).sort({ fecha_inicio: 1 }).toArray();
print(`esperado: 10 · obtenido: ${c01.length} ${c01.length === 10 ? "✓" : "✗"} · ${c01.map((d) => d.codigo).join(", ")}`);

// C02 · Filtro por sector (equivalente a "sector" de la guía)
titulo("C02", "Equipos del sector Imagenología", { sector: "Imagenología" });
const c02 = eqp.find({ sector: "Imagenología" }, { codigo: 1, nombre: 1, _id: 0 }).sort({ codigo: 1 }).toArray();
print(`esperado: 3 (EQP-003, 011, 015 — EQP-002 pertenece a Urgencias pese a estar hoy en el piso de Imagenología) · obtenido: ${c02.length} ${c02.length === 3 ? "✓" : "✗"} · ${c02.map((d) => d.codigo).join(", ")}`);

// C03 · Campo anidado
titulo("C03", "Equipos con clasificación de riesgo alto (campos_dinamicos)", { "campos_dinamicos.clasificacion_riesgo": "alto" });
const c03 = eqp.find({ "campos_dinamicos.clasificacion_riesgo": "alto" }, { codigo: 1, "campos_dinamicos.clasificacion_riesgo": 1, _id: 0 }).toArray();
print(`esperado: 5 · obtenido: ${c03.length} ${c03.length === 5 ? "✓" : "✗"} · ${c03.map((d) => d.codigo).join(", ")}`);

// C04 · Rango de fechas UTC
titulo("C04", "Intervenciones realizadas en septiembre de 2026 (rango UTC)",
  { estado: "realizada", fecha_inicio: { $gte: ISODate("2026-09-01T00:00:00Z"), $lt: ISODate("2026-10-01T00:00:00Z") } });
const c04 = itr.find(
  { estado: "realizada", fecha_inicio: { $gte: ISODate("2026-09-01T00:00:00Z"), $lt: ISODate("2026-10-01T00:00:00Z") } },
  { codigo: 1, fecha_inicio: 1, _id: 0 }
).sort({ fecha_inicio: 1 }).toArray();
print(`esperado: 5 (INT-011..015) · obtenido: ${c04.length} ${c04.length === 5 ? "✓" : "✗"} · ${c04.map((d) => d.codigo).join(", ")}`);

// C05 · Snapshot histórico vs estado actual del equipo
titulo("C05", "Contraste de equipo_snapshot vs estado ACTUAL del equipo", "equipo_snapshot.estado ≠ equipos.estado");
const c05 = itr.find({ "equipo_snapshot.estado": { $ne: "operativo" } }, { codigo: 1, equipo_id: 1, "equipo_snapshot.estado": 1, _id: 0 }).toArray();
print(`esperado: snapshots no operativos (historial de fallas) · obtenido: ${c05.length}`);
for (const d of c05) {
  const actual = eqp.findOne({ codigo: d.equipo_id }, { estado: 1 });
  print(`  ${d.codigo} · ${d.equipo_id} · histórico="${d.equipo_snapshot.estado}" vs actual="${actual.estado}"`);
}

// C06 · Arreglo: intervenciones CON y SIN repuestos
titulo("C06", "Intervenciones con repuestos vs sin repuestos", '{ "repuestos.0": { $exists: true } }  vs  { repuestos: { $size: 0 } }');
const conRep = itr.find({ "repuestos.0": { $exists: true } }, { codigo: 1, repuestos: 1, _id: 0 }).toArray();
const sinRep = itr.find({ repuestos: { $size: 0 } }, { codigo: 1, _id: 0 }).toArray();
print(`con repuestos: ${conRep.length} (ej. ${conRep[0]?.codigo} → ${EJSON.stringify(conRep[0]?.repuestos)})`);
print(`sin repuestos ($size: 0): ${sinRep.length} · total ${conRep.length + sinRep.length} = 30 ✓`);

// C07 · null: el filtro null coincide con null Y con ausente
titulo("C07", "contacto_alterno null (enseña que también casa campo ausente)", { contacto_alterno: null });
const c07 = tec.find({ contacto_alterno: null }, { codigo: 1, contacto_alterno: 1, _id: 0 }).sort({ codigo: 1 }).toArray();
print(`obtenido: ${c07.length} → ${c07.map((d) => `${d.codigo}(${Object.prototype.hasOwnProperty.call(d, "contacto_alterno") ? "null" : "ausente"})`).join(", ")}`);
print("lectura: TEC-002 tiene null explícito y TEC-003 lo tiene ausente; el filtro { null } casó con ambos.");

// C08 · Ausencia vs presencia con $exists
titulo("C08", "contacto_alterno ausente vs presente", '{ contacto_alterno: { $exists: false } }  vs  { $exists: true }');
const ausentes = tec.find({ contacto_alterno: { $exists: false } }, { codigo: 1, _id: 0 }).sort({ codigo: 1 }).toArray();
const presentes = tec.find({ contacto_alterno: { $exists: true } }, { codigo: 1, _id: 0 }).sort({ codigo: 1 }).toArray();
print(`ausentes ($exists:false): ${ausentes.length} → ${ausentes.map((d) => d.codigo).join(", ")}`);
print(`presentes ($exists:true): ${presentes.length} → ${presentes.map((d) => d.codigo).join(", ")}`);
print("lectura: $exists:false distingue el ausente (TEC-003) del null (TEC-002), a diferencia de C07.");

// C09 · $elemMatch: diagnóstico con area y puntaje en el MISMO elemento
titulo("C09", "Diagnóstico con area='Aspiración' y puntaje>=80 en el MISMO elemento", { diagnosticos: { $elemMatch: { area: "Aspiración", puntaje: { $gte: 80 } } } });
const correcto = tec.find({ diagnosticos: { $elemMatch: { area: "Aspiración", puntaje: { $gte: 80 } } } }, { codigo: 1, _id: 0 }).toArray();
print(`$elemMatch (correcto): ${correcto.length} documento(s) ${correcto.length === 0 ? "✓ (ninguno: TEC-003 tiene Aspiración con 70)" : "✗"}`);
const cruzado = tec.find({ "diagnosticos.area": "Aspiración", "diagnosticos.puntaje": { $gte: 80 } }, { codigo: 1, diagnosticos: 1, _id: 0 }).toArray();
print(`filtro cruzado (INCORRECTO): ${cruzado.length} documento(s) → ${cruzado.map((d) => d.codigo).join(", ")}`);
print("lectura: el filtro cruzado falso-positivos a TEC-003 (área en un elemento, puntaje 80 en otro); $elemMatch exige el mismo elemento.");

// C10 · Paginación: skip/limit vs continuación por clave
titulo("C10", "Páginas de 3 equipos, orden { sector: 1, codigo: 1 }", 'skip/limit vs $or con clave anterior (keyset)');
const orden = { sector: 1, codigo: 1 };
let vistos = [];
for (let p = 0; p < 5; p++) {
  const pagina = eqp.find({}, { codigo: 1, sector: 1, _id: 0 }).sort(orden).skip(p * 3).limit(3).toArray();
  if (pagina.length === 0) break;
  print(`  skip/limit página ${p + 1}: ${pagina.map((d) => d.codigo).join(", ")}`);
  vistos = vistos.concat(pagina.map((d) => d.codigo));
}
const unicos = new Set(vistos);
print(`  skip/limit → ${vistos.length} códigos, ${unicos.size} únicos ${unicos.size === vistos.length ? "✓ (sin repetidos)" : "✗ (hubo repetidos)"}`);

// continuación por clave (keyset): sin saltos, usando el último visto
let clave = null, vistos2 = [], paginaN = 0;
while (true) {
  const filtro = clave ? { $or: [{ sector: { $gt: clave.sector } }, { sector: clave.sector, codigo: { $gt: clave.codigo } }] } : {};
  const pagina = eqp.find(filtro, { codigo: 1, sector: 1, _id: 0 }).sort(orden).limit(3).toArray();
  if (pagina.length === 0) break;
  paginaN++;
  print(`  keyset página ${paginaN}: ${pagina.map((d) => d.codigo).join(", ")}`);
  vistos2 = vistos2.concat(pagina.map((d) => d.codigo));
  clave = pagina[pagina.length - 1];
}
const unicos2 = new Set(vistos2);
print(`  keyset → ${vistos2.length} códigos, ${unicos2.size} únicos ${unicos2.size === vistos2.length ? "✓" : "✗"} · mismo orden y mismos códigos que skip/limit: ${JSON.stringify(vistos) === JSON.stringify(vistos2) ? "✓" : "✗"}`);
print("== Consultas terminadas ==");
