// 11_ampliacion_v11.js — ampliación v1.1 sobre s03_tecnicos (E03)
// Agrega: intentos_contacto, contacto_alterno (null / ausente / presente),
// diagnosticos (máx. 2, con area y puntaje) y temporal. Equivalente adaptado de INI-001/002/003.
// Uso desde la raíz del repositorio:
//   mongosh --quiet --file scripts/11_ampliacion_v11.js
const dbLab = db.getSiblingDB("sitbio_lab_nosql");
const t = dbLab.getCollection("s03_tecnicos");

print("== Ampliación v1.1 sobre s03_tecnicos ==");

// TEC-001: contacto_alterno PRESENTE · 2 diagnósticos (ambos ≥ 80)
t.updateOne({ codigo: "TEC-001" }, {
  $set: {
    intentos_contacto: 2, contacto_alterno: "3015550101", temporal: false,
    diagnosticos: [
      { area: "Electrocardiografía", puntaje: 85 },
      { area: "Monitoreo", puntaje: 90 },
    ],
  },
});
// TEC-002: contacto_alterno NULL explícito · 1 diagnóstico (78)
t.updateOne({ codigo: "TEC-002" }, {
  $set: {
    intentos_contacto: 0, contacto_alterno: null, temporal: false,
    diagnosticos: [{ area: "Imagenología", puntaje: 78 }],
  },
});
// TEC-003: contacto_alterno AUSENTE ($unset) · 2 diagnósticos (80 y 70) · temporal
t.updateOne({ codigo: "TEC-003" }, {
  $set: {
    intentos_contacto: 1, temporal: true,
    diagnosticos: [
      { area: "Bombas de infusión", puntaje: 80 },
      { area: "Aspiración", puntaje: 70 },
    ],
  },
});
t.updateOne({ codigo: "TEC-003" }, { $unset: { contacto_alterno: "" } });

print("Estado v1.1 de TEC-001..003:");
t.find({ codigo: { $in: ["TEC-001", "TEC-002", "TEC-003"] } }, { codigo: 1, intentos_contacto: 1, contacto_alterno: 1, diagnosticos: 1, temporal: 1 })
  .sort({ codigo: 1 })
  .forEach((d) => print(EJSON.stringify(d)));
print("Listo.");
