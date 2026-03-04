"""
app.py  –  Workflow Analyzer  ·  Streamlit UI
Run:  streamlit run app.py
"""

import json
import streamlit as st
from pathlib import Path

from parser import parse_file, WorkflowParser
from renderer import render_html, auto_layout, NW

# ── PAGE CONFIG ──────────────────────────────────────────────────────────────

st.set_page_config(
    page_title="Workflow Analyzer",
    page_icon="⬡",
    layout="wide",
    initial_sidebar_state="expanded",
)

# ── CUSTOM CSS ───────────────────────────────────────────────────────────────

st.markdown("""
<style>
@import url('https://fonts.googleapis.com/css2?family=JetBrains+Mono:wght@400;500;600;700&family=Sora:wght@300;400;500;600;700&display=swap');

/* ── global ── */
html, body, [class*="css"] {
    font-family: 'Sora', sans-serif;
}
.stApp {
    background: #080b10;
    color: #e2e8f0;
}

/* ── sidebar ── */
[data-testid="stSidebar"] {
    background: #0d1117 !important;
    border-right: 1px solid #1e2a3a !important;
}
[data-testid="stSidebar"] * {
    color: #e2e8f0 !important;
}

/* ── hide default streamlit chrome ── */
#MainMenu, footer, header { visibility: hidden; }
[data-testid="stToolbar"] { display: none; }
.block-container { padding-top: 1.5rem !important; padding-bottom: 1rem !important; }

/* ── metric cards ── */
[data-testid="metric-container"] {
    background: #0d1117;
    border: 1px solid #1e2a3a;
    border-radius: 10px;
    padding: 12px 16px !important;
}
[data-testid="metric-container"] label {
    font-family: 'JetBrains Mono', monospace !important;
    font-size: 10px !important;
    color: #4a5a72 !important;
    text-transform: uppercase;
    letter-spacing: .08em;
}
[data-testid="metric-container"] [data-testid="stMetricValue"] {
    font-family: 'JetBrains Mono', monospace !important;
    font-size: 26px !important;
    font-weight: 700 !important;
}

/* ── tabs ── */
[data-testid="stTabs"] [role="tab"] {
    font-family: 'JetBrains Mono', monospace !important;
    font-size: 12px !important;
    color: #4a5a72 !important;
    background: transparent !important;
    border: 1px solid transparent !important;
    border-radius: 7px !important;
    padding: 6px 16px !important;
}
[data-testid="stTabs"] [role="tab"][aria-selected="true"] {
    background: #141922 !important;
    color: #e2e8f0 !important;
    border-color: #253040 !important;
}
[data-testid="stTabContent"] {
    background: #080b10 !important;
    border: 1px solid #1e2a3a !important;
    border-radius: 10px !important;
    padding: 20px !important;
    margin-top: 8px !important;
}

/* ── file uploader ── */
[data-testid="stFileUploader"] {
    background: #0d1117 !important;
    border: 1.5px dashed #253040 !important;
    border-radius: 10px !important;
}
[data-testid="stFileUploader"]:hover {
    border-color: #3b82f6 !important;
}

/* ── buttons ── */
.stButton > button {
    font-family: 'Sora', sans-serif !important;
    font-weight: 600 !important;
    font-size: 13px !important;
    border-radius: 8px !important;
    border: 1px solid #253040 !important;
    background: #141922 !important;
    color: #94a3b8 !important;
    transition: all .15s !important;
    padding: 8px 18px !important;
}
.stButton > button:hover {
    color: #e2e8f0 !important;
    border-color: #3b82f6 !important;
    background: #1a2130 !important;
}
.stButton > button[kind="primary"] {
    background: #3b82f6 !important;
    color: #fff !important;
    border-color: #3b82f6 !important;
}
.stButton > button[kind="primary"]:hover {
    background: #2563eb !important;
}

/* ── code / json ── */
[data-testid="stCodeBlock"] {
    background: #0d1117 !important;
    border: 1px solid #1e2a3a !important;
    border-radius: 8px !important;
}

/* ── warning / info boxes ── */
.warn-card {
    background: #1a1005;
    border: 1px solid #3a2008;
    border-radius: 8px;
    padding: 9px 13px;
    font-family: 'JetBrains Mono', monospace;
    font-size: 11px;
    color: #c8943a;
    margin-bottom: 6px;
    line-height: 1.5;
}
.info-card {
    background: #040e1a;
    border: 1px solid #0a2a4a;
    border-radius: 8px;
    padding: 9px 13px;
    font-family: 'JetBrains Mono', monospace;
    font-size: 11px;
    color: #5a9ad0;
    margin-bottom: 6px;
    line-height: 1.5;
}
.merge-card {
    background: #0a1a0a;
    border: 1px solid #1a3a1a;
    border-radius: 8px;
    padding: 9px 13px;
    font-family: 'JetBrains Mono', monospace;
    font-size: 11px;
    color: #4ade80;
    margin-bottom: 6px;
    line-height: 1.5;
}

/* ── section headers ── */
.section-label {
    font-family: 'JetBrains Mono', monospace;
    font-size: 10px;
    font-weight: 600;
    color: #4a5a72;
    text-transform: uppercase;
    letter-spacing: .1em;
    margin-bottom: 10px;
}

/* ── node table ── */
.node-row {
    display: flex;
    align-items: center;
    gap: 10px;
    padding: 8px 12px;
    background: #0d1117;
    border: 1px solid #1e2a3a;
    border-radius: 8px;
    margin-bottom: 5px;
    font-size: 12px;
}
.node-key {
    font-family: 'JetBrains Mono', monospace;
    font-size: 10px;
    font-weight: 600;
    padding: 2px 8px;
    border-radius: 5px;
    flex-shrink: 0;
}
.badge-single  { background:#0a2040; color:#60a5fa; border:1px solid #1a3a60; }
.badge-edit    { background:#180d32; color:#a78bfa; border:1px solid #2a1a4a; }
.badge-multi   { background:#0f2010; color:#4ade80; border:1px solid #1a3a1a; }
.badge-summary { background:#082818; color:#22d3a0; border:1px solid #0d3a22; }
.badge-outcome { background:#280a0a; color:#f87171; border:1px solid #3a1010; }

/* ── topbar logo ── */
.topbar {
    display: flex;
    align-items: center;
    gap: 12px;
    margin-bottom: 20px;
    padding-bottom: 16px;
    border-bottom: 1px solid #1e2a3a;
}
.logo-icon {
    width: 36px; height: 36px;
    background: linear-gradient(135deg, #3b82f6, #6366f1);
    border-radius: 9px;
    display: flex; align-items: center; justify-content: center;
    font-size: 18px;
    flex-shrink: 0;
}
.logo-text { font-family: 'JetBrains Mono', monospace; font-size: 15px; font-weight: 700; }
.logo-sub  { font-family: 'JetBrains Mono', monospace; font-size: 10px; color: #4a5a72; margin-top: 1px; }

/* ── divider ── */
hr { border-color: #1e2a3a !important; }

/* ── iframe diagram ── */
.diagram-frame {
    border-radius: 10px;
    border: 1px solid #1e2a3a;
    overflow: hidden;
}
</style>
""", unsafe_allow_html=True)


# ── HELPERS ──────────────────────────────────────────────────────────────────

TYPE_COLORS = {
    "single":  ("badge-single",  "Single"),
    "edit":    ("badge-edit",    "Edit"),
    "multi":   ("badge-multi",   "Multi"),
    "summary": ("badge-summary", "Summary"),
    "outcome": ("badge-outcome", "Outcome"),
}

WARN_ICONS = {
    "orphan":    ("⚠", "warn-card"),
    "commented": ("⚠", "warn-card"),
    "runtime":   ("ℹ", "info-card"),
    "merge":     ("⇶", "merge-card"),
    "info":      ("·", "info-card"),
}

def warn_html(w: dict) -> str:
    icon, cls = WARN_ICONS.get(w["type"], ("·", "info-card"))
    msg = w["message"].replace("<", "&lt;").replace(">", "&gt;")
    return f'<div class="{cls}">{icon}&nbsp; {msg}</div>'


def node_row_html(n: dict) -> str:
    t = n.get("type", "single")
    badge_cls, badge_label = TYPE_COLORS.get(t, ("badge-single", t))
    orphan = ' <span style="font-size:9px;color:#c8943a;border:1px solid #5a3a10;border-radius:4px;padding:1px 5px;">orphan</span>' if n.get("isOrphan") else ""
    anc = ' <span style="font-size:9px;color:#c8943a;border:1px solid #5a3a10;border-radius:4px;padding:1px 5px;">AllowNoChoice</span>' if n.get("allowNoChoice") else ""
    items_count = len(n.get("items", []))
    ic = f'<span style="margin-left:auto;font-family:\'JetBrains Mono\',monospace;font-size:10px;color:#4a5a72;">{items_count} item{"s" if items_count!=1 else ""}</span>'
    title = n.get("title","")[:55] + ("…" if len(n.get("title","")) > 55 else "")
    return f"""<div class="node-row">
  <span class="node-key {badge_cls}">{badge_label}</span>
  <span style="font-family:'JetBrains Mono',monospace;font-size:11px;color:#60a5fa;">{n.get('key','')}</span>
  <span style="font-size:11.5px;color:#94a3b8;">{title}</span>
  {orphan}{anc}{ic}
</div>"""


def parse_source(source: str) -> dict:
    parser = WorkflowParser(source)
    from parser import to_dict
    data = parser.parse()
    return to_dict(data)


# ── SIDEBAR ──────────────────────────────────────────────────────────────────

with st.sidebar:
    st.markdown("""
    <div class="topbar">
      <div class="logo-icon">⬡</div>
      <div>
        <div class="logo-text">Workflow Analyzer</div>
        <div class="logo-sub">BN.WebLicenze · C# Parser</div>
      </div>
    </div>
    """, unsafe_allow_html=True)

    st.markdown('<div class="section-label">File sorgente</div>', unsafe_allow_html=True)
    uploaded = st.file_uploader(
        "Carica il file .cs",
        type=["cs"],
        label_visibility="collapsed",
    )

    # ── Quick-load from workflows/ folder ────────────────────────────────────
    WORKFLOWS_DIR = Path(__file__).parent / "workflows"
    workflow_files = sorted(WORKFLOWS_DIR.glob("*.cs")) if WORKFLOWS_DIR.exists() else []

    if workflow_files:
        st.markdown('<div class="section-label" style="margin-top:14px;">Caricamento rapido</div>', unsafe_allow_html=True)
        options = ["— seleziona —"] + [f.name for f in workflow_files]
        selected = st.selectbox(
            "Workflow",
            options,
            label_visibility="collapsed",
            key="quick_select",
        )
        if selected != "— seleziona —":
            quick_path = WORKFLOWS_DIR / selected
            if ("filename" not in st.session_state or st.session_state.filename != selected):
                with st.spinner("Analisi in corso…"):
                    source = quick_path.read_text(encoding="utf-8-sig")
                    st.session_state.data = parse_source(source)
                    st.session_state.filename = selected

    st.markdown("<hr>", unsafe_allow_html=True)

    if "data" in st.session_state:
        data = st.session_state.data
        meta = data.get("metadata", {})
        active = [n for n in data["nodes"] if not n.get("isCommented")]
        commented = [n for n in data["nodes"] if n.get("isCommented")]

        st.markdown('<div class="section-label">Statistiche</div>', unsafe_allow_html=True)
        c1, c2 = st.columns(2)
        c1.metric("Nodi attivi", meta.get("total_active_nodes", 0))
        c2.metric("Connessioni", len(data.get("edges", [])))
        c1.metric("Input items", meta.get("total_input_items", 0))
        c2.metric("Commentati", meta.get("total_commented_nodes", 0))

        st.markdown("<hr>", unsafe_allow_html=True)
        st.markdown('<div class="section-label">Avvisi</div>', unsafe_allow_html=True)
        for w in data.get("warnings", []):
            st.markdown(warn_html(w), unsafe_allow_html=True)

        st.markdown("<hr>", unsafe_allow_html=True)

        # Download buttons
        st.markdown('<div class="section-label">Esporta</div>', unsafe_allow_html=True)
        json_str = json.dumps(data, indent=2, ensure_ascii=False)
        st.download_button(
            "↓ Scarica JSON",
            data=json_str,
            file_name=f"{data['workflowName']}.json",
            mime="application/json",
            use_container_width=True,
        )
        html_str = render_html(data)
        st.download_button(
            "↓ Scarica HTML",
            data=html_str,
            file_name=f"{data['workflowName']}.html",
            mime="text/html",
            use_container_width=True,
        )
    else:
        st.markdown("""
        <div style="text-align:center;padding:30px 10px;color:#4a5a72;font-size:12px;font-family:'JetBrains Mono',monospace;">
            Carica un file .cs<br>per iniziare
        </div>
        """, unsafe_allow_html=True)


# ── MAIN CONTENT ─────────────────────────────────────────────────────────────

# Parse on upload
if uploaded and ("filename" not in st.session_state or st.session_state.filename != uploaded.name):
    with st.spinner("Analisi in corso…"):
        source = uploaded.read().decode("utf-8-sig")
        st.session_state.data = parse_source(source)
        st.session_state.filename = uploaded.name
        st.session_state.quick_select = "— seleziona —"
    st.rerun()

# ── EMPTY STATE ──
if "data" not in st.session_state:
    st.markdown("""
    <div style="display:flex;flex-direction:column;align-items:center;justify-content:center;
                height:70vh;gap:16px;color:#4a5a72;text-align:center;">
      <div style="font-size:64px;opacity:.15;">⬡</div>
      <div style="font-family:'JetBrains Mono',monospace;font-size:18px;font-weight:700;color:#1e2a3a;">
        Workflow Analyzer
      </div>
      <div style="font-size:13px;max-width:340px;line-height:1.7;color:#253040;">
        Carica un file <code style="color:#3b82f6;background:#0a1a2a;padding:2px 6px;border-radius:4px;">.cs</code>
        con una classe Workflow dalla sidebar per generare il diagramma e il JSON strutturato.
      </div>
    </div>
    """, unsafe_allow_html=True)
    st.stop()

# ── LOADED STATE ──
data = st.session_state.data
wf_name = data["workflowName"]
active_nodes = [n for n in data["nodes"] if not n.get("isCommented")]

# Page header
col_title, col_badge = st.columns([6, 1])
with col_title:
    st.markdown(f"""
    <div style="display:flex;align-items:center;gap:12px;margin-bottom:4px;">
      <div style="font-family:'JetBrains Mono',monospace;font-size:22px;font-weight:700;color:#e2e8f0;">{wf_name}</div>
      <div style="font-family:'JetBrains Mono',monospace;font-size:11px;color:#3b82f6;
                  background:#0a1a2a;padding:3px 10px;border-radius:6px;border:1px solid #1a3a5a;">
        BN.WebLicenze.Controllers
      </div>
    </div>
    <div style="font-family:'JetBrains Mono',monospace;font-size:11px;color:#4a5a72;">
        {data['metadata'].get('total_active_nodes',0)} nodi · {len(data['edges'])} connessioni · {data['metadata'].get('total_input_items',0)} input items
    </div>
    """, unsafe_allow_html=True)

st.markdown("<hr>", unsafe_allow_html=True)

# ── TABS ──
tab_diagram, tab_nodes, tab_edges, tab_json = st.tabs([
    "⬡  Diagramma",
    "◈  Nodi",
    "→  Connessioni",
    "{ }  JSON",
])

# ── TAB 1: DIAGRAM ──────────────────────────────────────────────────────────
with tab_diagram:
    html_content = render_html(data)
    # Extract just the SVG/D3 part, embedded in a scrollable div
    # We embed the whole HTML in an iframe for clean rendering
    st.markdown('<div class="diagram-frame">', unsafe_allow_html=True)
    st.components.v1.html(html_content, height=820, scrolling=True)
    st.markdown('</div>', unsafe_allow_html=True)

# ── TAB 2: NODES ────────────────────────────────────────────────────────────
with tab_nodes:
    # Filter controls
    fc1, fc2, fc3 = st.columns([2, 2, 3])
    with fc1:
        show_commented = st.toggle("Mostra commentati", value=False)
    with fc2:
        type_filter = st.selectbox(
            "Tipo", ["Tutti", "single", "edit", "multi", "summary", "outcome"],
            label_visibility="collapsed"
        )
    with fc3:
        search = st.text_input("🔍 Cerca nodo…", placeholder="key o titolo", label_visibility="collapsed")

    nodes_to_show = data["nodes"] if show_commented else active_nodes
    if type_filter != "Tutti":
        nodes_to_show = [n for n in nodes_to_show if n.get("type") == type_filter]
    if search:
        s = search.lower()
        nodes_to_show = [n for n in nodes_to_show if s in n.get("key","").lower() or s in n.get("title","").lower()]

    st.markdown(f'<div class="section-label">{len(nodes_to_show)} nodi</div>', unsafe_allow_html=True)

    for n in nodes_to_show:
        with st.expander(f"{'💬 ' if n.get('isCommented') else ''}{'⚠ ' if n.get('isOrphan') else ''}{n['key']}  ·  {n.get('title','')[:60]}"):
            c1, c2, c3 = st.columns(3)
            c1.markdown(f"**Tipo:** `{n.get('type','')}`")
            c2.markdown(f"**InputType:** `{n.get('inputType','')}`")
            c3.markdown(f"**AllowNoChoice:** `{n.get('allowNoChoice', False)}`")

            items = n.get("items", [])
            if items:
                st.markdown("**Items:**")
                rows = []
                for it in items:
                    el = f" `[{it['editLabel']}]`" if it.get("editLabel") else ""
                    rows.append(f"- `{it['label']}` — {it.get('description','')}{el}")
                st.markdown("\n".join(rows))

            flags = []
            if n.get("isCommented"): flags.append("🔇 Commentato")
            if n.get("isOrphan"):    flags.append("⚠ Orfano (non raggiungibile)")
            if flags:
                st.markdown(" · ".join(flags))

# ── TAB 3: EDGES ────────────────────────────────────────────────────────────
with tab_edges:
    edges = data.get("edges", [])
    node_map = {n["id"]: n for n in data["nodes"]}

    ec1, ec2 = st.columns([3, 2])
    with ec1:
        edge_search = st.text_input("🔍 Filtra connessione…", placeholder="from / to / label", label_visibility="collapsed")
    with ec2:
        runtime_only = st.toggle("Solo runtime branch", value=False)

    edges_to_show = edges
    if runtime_only:
        edges_to_show = [e for e in edges if e.get("isRuntimeBranch")]
    if edge_search:
        s = edge_search.lower()
        edges_to_show = [e for e in edges_to_show if s in e["from"].lower() or s in e["to"].lower() or s in e.get("label","").lower()]

    st.markdown(f'<div class="section-label">{len(edges_to_show)} connessioni</div>', unsafe_allow_html=True)

    for e in edges_to_show:
        col = e.get("color", "#4a5a7a")
        label = f'`{e["label"]}`' if e.get("label") else "*(unconditional)*"
        runtime_badge = ' 🔀 **runtime**' if e.get("isRuntimeBranch") else ""
        cond = f' · condizione: `{e["runtimeCondition"]}`' if e.get("runtimeCondition") else ""
        from_title = node_map.get(e["from"], {}).get("title", "")[:35]
        to_title   = node_map.get(e["to"],   {}).get("title", "")[:35]
        st.markdown(
            f'<div style="display:flex;align-items:center;gap:8px;padding:8px 12px;'
            f'background:#0d1117;border:1px solid #1e2a3a;border-radius:8px;margin-bottom:5px;">'
            f'<span style="width:10px;height:10px;border-radius:50%;background:{col};flex-shrink:0;"></span>'
            f'<span style="font-family:\'JetBrains Mono\',monospace;font-size:11px;color:#60a5fa;">{e["from"]}</span>'
            f'<span style="color:#4a5a72;">→</span>'
            f'<span style="font-family:\'JetBrains Mono\',monospace;font-size:11px;color:#60a5fa;">{e["to"]}</span>'
            f'<span style="font-size:10px;color:#94a3b8;flex:1;">{e.get("label","")}</span>'
            f'{"<span style=\"font-size:9px;color:#c8943a;border:1px solid #5a3a10;border-radius:4px;padding:1px 6px;\">runtime</span>" if e.get("isRuntimeBranch") else ""}'
            f'</div>',
            unsafe_allow_html=True
        )

# ── TAB 4: JSON ─────────────────────────────────────────────────────────────
with tab_json:
    json_str = json.dumps(data, indent=2, ensure_ascii=False)

    jc1, jc2, jc3 = st.columns([3, 1, 1])
    with jc1:
        st.markdown(
            f'<div style="font-family:\'JetBrains Mono\',monospace;font-size:11px;color:#4a5a72;">'
            f'{len(json_str):,} chars · {len(json_str.splitlines())} righe</div>',
            unsafe_allow_html=True
        )
    with jc2:
        section = st.selectbox("Sezione", ["completo", "nodes", "edges", "warnings", "metadata"], label_visibility="collapsed")
    with jc3:
        st.download_button("↓ JSON", json_str, file_name=f"{wf_name}.json", mime="application/json", use_container_width=True)

    if section == "completo":
        display_data = data
    else:
        display_data = data.get(section, {})

    st.code(json.dumps(display_data, indent=2, ensure_ascii=False), language="json")
