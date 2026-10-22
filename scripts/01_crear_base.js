// 01_crear_base.js — crea la base del laboratorio y los índices únicos (E02)
// Uso desde la raíz del repositorio:
//   mongosh --quiet --file scripts/01_crear_base.js
const dbLab = db.getSiblingDB("sitbio_lab_nosql");

print("Base: " + dbLab.getName() + " · servidor MongoDB " + db.version() + " · mongosh " + version());

// Índices únicos por código de negocio (llaves de la semilla)
dbLab.tecnicos.createIndex({ codigo: 1 }, { unique: true, name: "ux_tecnicos_codigo" });
dbLab.tecnicos.createIndex({ email: 1 }, { unique: true, name: "ux_tecnicos_email" });
dbLab.tecnicos.createIndex({ usuario_id: 1 }, { unique: true, name: "ux_tecnicos_usuario" });
dbLab.equipos.createIndex({ codigo: 1 }, { unique: true, name: "ux_equipos_codigo" });
dbLab.intervenciones.createIndex({ codigo: 1 }, { unique: true, name: "ux_intervenciones_codigo" });
// Índices de apoyo a las consultas Q03/Q04
dbLab.intervenciones.createIndex({ equipo_id: 1, fecha_inicio: -1 }, { name: "ix_intervenciones_equipo_fecha" });
dbLab.intervenciones.createIndex({ estado: 1, fecha_inicio: 1 }, { name: "ix_intervenciones_estado_fecha" });

for (const c of ["tecnicos", "equipos", "intervenciones"]) {
  print(`${c}: ${dbLab.getCollection(c).countDocuments()} documento(s) · índices: ` +
    dbLab.getCollection(c).getIndexes().map(i => i.name).join(", "));
}
print("Listo.");
