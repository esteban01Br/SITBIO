# SITBIO · Analytics — Notebook de la Semana 2 (Ciencia de Datos con Python)

Ejercicios de **estructuras básicas, NumPy, Pandas e inspección inicial** de la guía de Ciencia de
Datos (Semana 2), aplicados a **datos reales de SITBIO** exportados de PostgreSQL
(instituciones de salud del modo Api).

## Estructura (igual que la guía)

```
src/analytics/
├── data/raw/        equipos.csv · intervenciones.xlsx   (datos de partida, rutas relativas)
├── data/processed/  datasets procesados generados por el notebook (CSV + Excel)
├── notebooks/       01_analitica_sitbio_semana2.ipynb   (el entregable)
├── outputs/         copia ejecutada del notebook y su versión HTML
├── requirements.txt librerías: pandas, numpy, openpyxl
└── README.md        este archivo
```

## Entorno virtual (Windows)

```powershell
cd src/analytics
python -m venv .venv
.venv\Scripts\Activate.ps1     # activación (PowerShell)
python -m pip install --upgrade pip
pip install -r requirements.txt
python -c "import pandas, numpy, openpyxl; print(pandas.__version__, numpy.__version__, openpyxl.__version__)"
```

> La carpeta `.venv` **no se sube al repositorio** (ya está en `.gitignore`); cada quien la crea
> localmente. Todas las rutas dentro del notebook son **relativas** (`Path("..") / "data" / "raw"`),
> nunca absolutas.

## Cómo ejecutar el notebook

```powershell
# Opción A — en VS Code / Jupyter: abrir notebooks/01_analitica_sitbio_semana2.ipynb y "Run All"
# Opción B — desde la terminal (con el .venv activado):
jupyter nbconvert --to notebook --execute notebooks/01_analitica_sitbio_semana2.ipynb --inplace
```

El notebook genera en `data/processed/` los archivos procesados con columnas calculadas
(`horas_promedio_tecnico`, `sla_cumplido`, etc.). La carpeta `outputs/` guarda la copia ejecutada
(`.ipynb` y `.html`) como evidencia.

## Lista de chequeo de la guía (9 criterios)

| # | Criterio | Dónde está |
|---|---|---|
| 1 | Entorno virtual activado y librerías verificadas | Sección 2 del notebook + este README |
| 2 | Importaciones correctas (`pandas`, `numpy`, `pathlib`) | Sección 2 |
| 3 | Estructuras: lista, diccionario y lista de diccionarios (+ reto con 2 registros) | Sección 3 |
| 4 | NumPy: arreglos, promedio, máximo, mínimo, filtros y operación vectorizada | Sección 4 |
| 5 | Series y DataFrames: creación e identificación de filas, columnas e índices | Sección 5 |
| 6 | Lectura de CSV y Excel con rutas relativas | Sección 6 |
| 7 | Inspección: `head`, `tail`, `shape`, `columns`, `dtypes`, `info`, `describe` (incl. `include="all"`) | Sección 7 |
| 8 | Exportación del dataset procesado a CSV y Excel en `data/processed/` | Sección 10 |
| 9 | Conclusiones (estructuras, diferencias y buenas prácticas) | Sección 11 |

Más la celda de **Hallazgos iniciales** en Markdown (Sección 8), exigida aparte por la guía.

## Origen de los datos

`data/raw/equipos.csv` y `data/raw/intervenciones.xlsx` se exportaron desde **la propia base de
PostgreSQL de SITBIO** (institución tenant 1) con `\copy … TO … WITH CSV HEADER`, por lo que son
datos del dominio real del proyecto (equipos biomédicos e intervenciones), no un dataset ajeno.
