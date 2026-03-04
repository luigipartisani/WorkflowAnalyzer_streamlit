# Workflow Analyzer — Streamlit App

Parser statico per classi C# `Workflow` (pattern BN.WebLicenze).  
Interfaccia Streamlit con diagramma D3, esplora nodi/connessioni e export JSON/HTML.

## Setup

```bash
pip install -r requirements.txt
streamlit run app.py
```

Si apre automaticamente su `http://localhost:8501`

## Come si usa

1. Trascina (o clicca) un file `.cs` nella sidebar
2. Il workflow viene parsato istantaneamente
3. Naviga tra le tab:
   - **⬡ Diagramma** — schema D3 interattivo con frecce Bézier
   - **◈ Nodi** — lista espandibile con filtri per tipo e ricerca
   - **→ Connessioni** — tutte le edge con colori e flag runtime
   - **{ } JSON** — JSON completo con navigazione per sezione
4. Dalla sidebar: scarica il JSON o l'HTML standalone

## File

| File | Descrizione |
|------|-------------|
| `app.py` | Streamlit UI |
| `parser.py` | Parser statico C# |
| `renderer.py` | Renderer HTML/D3 |
| `requirements.txt` | Dipendenze |

## Output JSON

```json
{
  "workflowName": "WorkflowPortale",
  "runtimeParams": [{ "name": "tipoLicenza", "type": "int", "description": "0->niente, 1->comm, 2->azi" }],
  "nodes": [{
    "id": "lic", "key": "lic",
    "title": "Che tipo di licenza desideri attivare?",
    "type": "single", "inputType": "single",
    "allowNoChoice": false,
    "items": [{ "label": "demo", "description": "Demo (15gg)", "editLabel": "" }],
    "isCommented": false, "isOrphan": false
  }],
  "edges": [{
    "from": "lic", "to": "sogg",
    "label": "tL=0|tL>2", "color": "#c8943a",
    "isRuntimeBranch": true, "runtimeCondition": "tipoLicenza == 0 || tipoLicenza > 2"
  }],
  "warnings": [{ "type": "runtime", "message": "..." }],
  "metadata": {
    "total_active_nodes": 23,
    "total_commented_nodes": 5,
    "total_edges": 45,
    "total_input_items": 69,
    "convergence_points": ["uploadFile", "summary"]
  }
}
```
