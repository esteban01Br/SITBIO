// 02_cargar_semilla.js — carga la semilla EJSON de forma idempotente (E02)
// Uso desde la raíz del repositorio:
//   mongosh --quiet --file scripts/02_cargar_semilla.js
// Claves: EJSON.parse (conserva fechas BSON) · replaceOne + upsert por "codigo"
// (la segunda carga NO aumenta los conteos) · validación de referencias ANTES de escribir.
const fs = require("fs");
const dbLab = db.getSiblingDB("sitbio_lab_nosql");

function leerEJSON(ruta) {
  return EJSON.parse(fs.readFileSync(ruta, "utf8"));
}

let rechazos = 0;

function cargar(nombreArchivo, nombreColeccion, validar) {
  const coleccion = dbLab.getCollection(nombreColeccion);
  const docs = leerEJSON(`data/${nombreArchivo}`);
  let escritos = 0;
  for (const doc of docs) {
    try {
      if (validar) validar(doc); // ← rechazo previo a la escritura si algo falla
      coleccion.replaceOne({ codigo: doc.codigo }, doc, { upsert: true });
      escritos++;
    } catch (e) {
      rechazos++;
      print(`  [${doc.codigo}] ${e.message}`);
    }
  }
  print(`${nombreColeccion}: ${escritos} escrito(s) · ${docs.length - escritos} rechazado(s)`);
}

print("== Carga de semilla EJSON (base " + dbLab.getName() + ") ==");

// Nota: en mongosh find().map() devuelve el cursor transformado, no un array.
// Siempre se usa .toArray().map() para construir conjuntos de referencia.

// 1) Técnicos (sin referencias externas)
cargar("tecnicos_v1.ejson", "tecnicos", (d) => {
  if (!d.codigo || !d.nombre || !d.email) throw new Error("RECHAZADO: campos obligatorios faltantes");
});

// 2) Equipos → responsable debe existir en tecnicos
const codTecnicos = new Set(dbLab.tecnicos.find({}, { codigo: 1 }).toArray().map((t) => t.codigo));
cargar("equipos_v1.ejson", "equipos", (d) => {
  if (!codTecnicos.has(d.responsable_id))
    throw new Error(`RECHAZADO previo a la escritura: responsable_id="${d.responsable_id}" no existe en tecnicos`);
});

// 3) Intervenciones → equipo y técnico deben existir
const codEquipos = new Set(dbLab.equipos.find({}, { codigo: 1 }).toArray().map((e) => e.codigo));
cargar("intervenciones_v1.ejson", "intervenciones", (d) => {
  if (!codEquipos.has(d.equipo_id))
    throw new Error(`RECHAZADO previo a la escritura: equipo_id="${d.equipo_id}" no existe en equipos`);
  if (!codTecnicos.has(d.tecnico_id))
    throw new Error(`RECHAZADO previo a la escritura: tecnico_id="${d.tecnico_id}" no existe en tecnicos`);
});

print(`Conteos: tecnicos=${dbLab.tecnicos.countDocuments()} equipos=${dbLab.equipos.countDocuments()} intervenciones=${dbLab.intervenciones.countDocuments()} · rechazos=${rechazos}`);
print(rechazos === 0 ? "Carga completa sin referencias rotas." : "Hay documentos rechazados (revisar arriba).");
