// 10_crear_copias_s03.js — crea la copia S03 de las colecciones (E03)
// Uso desde la raíz del repositorio:
//   mongosh --quiet --file scripts/10_crear_copias_s03.js
const dbLab = db.getSiblingDB("sitbio_lab_nosql");
const DATASET = "S03_SITBIO_V1";

print("== Creación de copias S03 (dataset " + DATASET + ") ==");
for (const nombre of ["tecnicos", "equipos", "intervenciones"]) {
  const origen = dbLab.getCollection(nombre);
  const destino = dbLab.getCollection("s03_" + nombre);
  destino.drop();
  const docs = origen.find().toArray();
  if (docs.length > 0) {
    destino.insertMany(docs.map((d) => Object.assign({}, d, { dataset: DATASET })));
  }
  print(`s03_${nombre}: ${destino.countDocuments()} documento(s) · dataset = ${destino.findOne()?.dataset}`);
}
print("Listo.");
