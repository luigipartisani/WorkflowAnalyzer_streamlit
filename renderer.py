"""
renderer.py
Generates a self-contained HTML file with a D3 workflow diagram from parsed JSON.
"""

import json
import math


# ── LAYOUT ────────────────────────────────────────────────────────────────────

NW = 240
TH = 54    # title area height
IH = 17    # item row height
IP = 7     # item padding
GAP_Y = 80
GAP_X = 50


def node_height(node: dict) -> int:
    items = node.get("items", [])
    if not items:
        return TH + 20
    rows = sum(1 + (1 if it.get("description") else 0) for it in items)
    return TH + IP + rows * IH + IP


def auto_layout(nodes: list[dict], edges: list[dict]) -> list[dict]:
    """Topological sort → assign layer → assign x/y."""
    if not nodes:
        return []

    active = [n for n in nodes if not n.get("isCommented")]
    id_set = {n["id"] for n in active}

    # Build in-degree and out-map
    out_map = {n["id"]: [] for n in active}
    in_deg  = {n["id"]: 0  for n in active}
    for e in edges:
        f, t = e["from"], e["to"]
        if f in out_map and t in id_set:
            out_map[f].append(t)
            in_deg[t] = in_deg.get(t, 0) + 1

    # Kahn's algorithm for layers
    layer = {}
    queue = [n["id"] for n in active if in_deg.get(n["id"], 0) == 0]
    for nid in queue:
        layer[nid] = 0

    visited = set(queue)
    q = list(queue)
    while q:
        nid = q.pop(0)
        for nxt in out_map.get(nid, []):
            layer[nxt] = max(layer.get(nxt, 0), layer[nid] + 1)
            if nxt not in visited:
                visited.add(nxt)
                q.append(nxt)

    for n in active:
        if n["id"] not in layer:
            layer[n["id"]] = 0

    # Group by layer
    by_layer: dict[int, list] = {}
    for n in active:
        l = layer[n["id"]]
        by_layer.setdefault(l, []).append(n)

    layer_keys = sorted(by_layer.keys())

    # Assign heights
    nh_map = {n["id"]: node_height(n) for n in active}

    # Y positions
    layer_y: dict[int, float] = {}
    cur_y = 0.0
    for l in layer_keys:
        layer_y[l] = cur_y
        max_h = max(nh_map[n["id"]] for n in by_layer[l])
        cur_y += max_h + GAP_Y

    # X positions
    pos = {}
    for l in layer_keys:
        ln = by_layer[l]
        total_w = len(ln) * NW + (len(ln) - 1) * GAP_X
        start_x = -total_w / 2 + NW / 2
        for n in ln:
            h = nh_map[n["id"]]
            pos[n["id"]] = {
                "cx": start_x,
                "cy": layer_y[l] + h / 2,
                "nh": h,
                "rx": start_x - NW / 2,
                "ry": layer_y[l],
            }
            start_x += NW + GAP_X

    # Merge positions into node copies
    result = []
    for n in active:
        nd = dict(n)
        nd.update(pos.get(n["id"], {"cx": 0, "cy": 0, "nh": TH + 20, "rx": -NW/2, "ry": 0}))
        result.append(nd)

    return result


# ── HTML TEMPLATE ─────────────────────────────────────────────────────────────

def render_html(data: dict) -> str:
    positioned = auto_layout(data.get("nodes", []), data.get("edges", []))

    if not positioned:
        min_x, min_y, W, H = -400, -100, 800, 200
    else:
        PAD = 80
        min_x = min(n["rx"] for n in positioned) - PAD
        min_y = min(n["ry"] for n in positioned) - PAD
        max_x = max(n["rx"] + NW for n in positioned) + PAD
        max_y = max(n["ry"] + n["nh"] for n in positioned) + PAD
        W = max_x - min_x
        H = max_y - min_y

    json_data = json.dumps(data, ensure_ascii=False)
    json_pretty = json.dumps(data, indent=2, ensure_ascii=False)

    warnings_html = _render_warnings(data.get("warnings", []))
    stats = data.get("metadata", {})
    wf_name = data.get("workflowName", "Workflow")

    return f"""<!DOCTYPE html>
<html lang="it">
<head>
<meta charset="UTF-8">
<meta name="viewport" content="width=device-width, initial-scale=1.0">
<title>{wf_name} – Workflow Analyzer</title>
<script src="https://cdnjs.cloudflare.com/ajax/libs/d3/7.8.5/d3.min.js"></script>
<style>
@import url('https://fonts.googleapis.com/css2?family=JetBrains+Mono:wght@400;500;600;700&family=Sora:wght@300;400;500;600;700&display=swap');
:root{{
  --bg:#080b10;--surface:#0d1117;--surface2:#141922;--surface3:#1a2130;
  --border:#1e2a3a;--border2:#253040;--accent:#3b82f6;--accent2:#6366f1;
  --green:#22d3a0;--orange:#f59e42;--red:#f87171;--text:#e2e8f0;--text2:#94a3b8;--text3:#4a5a72;
}}
*{{box-sizing:border-box;margin:0;padding:0;}}
body{{background:var(--bg);color:var(--text);font-family:'Sora',sans-serif;height:100vh;display:flex;flex-direction:column;overflow:hidden;}}
.topbar{{display:flex;align-items:center;gap:16px;padding:0 24px;height:52px;background:var(--surface);border-bottom:1px solid var(--border);flex-shrink:0;}}
.logo{{display:flex;align-items:center;gap:10px;font-family:'JetBrains Mono',monospace;font-size:13px;font-weight:600;}}
.logo-icon{{width:28px;height:28px;background:linear-gradient(135deg,var(--accent),var(--accent2));border-radius:7px;display:flex;align-items:center;justify-content:center;font-size:14px;}}
.wf-name{{font-family:'JetBrains Mono',monospace;font-size:12px;color:var(--text2);background:var(--surface3);padding:3px 10px;border-radius:6px;border:1px solid var(--border2);}}
.topbar-sep{{width:1px;height:20px;background:var(--border2);}}
.tab-group{{display:flex;gap:2px;}}
.tab{{padding:6px 14px;border-radius:6px;font-size:12px;font-weight:500;color:var(--text2);cursor:pointer;transition:all .15s;font-family:'JetBrains Mono',monospace;border:1px solid transparent;}}
.tab:hover{{background:var(--surface3);color:var(--text);}}
.tab.active{{background:var(--surface3);color:var(--text);border-color:var(--border2);}}
.topbar-right{{margin-left:auto;display:flex;align-items:center;gap:8px;}}
.btn{{display:flex;align-items:center;gap:6px;padding:6px 12px;border-radius:7px;font-size:12px;font-weight:600;cursor:pointer;border:1px solid var(--border2);background:var(--surface3);color:var(--text2);transition:all .15s;font-family:'Sora',sans-serif;}}
.btn:hover{{color:var(--text);border-color:var(--accent);}}
.main{{flex:1;display:flex;overflow:hidden;}}
.sidebar{{width:300px;flex-shrink:0;border-right:1px solid var(--border);display:flex;flex-direction:column;background:var(--surface);overflow:hidden;}}
.sidebar-section{{padding:14px 16px;border-bottom:1px solid var(--border);}}
.sidebar-section h3{{font-size:10px;font-weight:600;font-family:'JetBrains Mono',monospace;color:var(--text3);letter-spacing:.1em;text-transform:uppercase;margin-bottom:10px;}}
.stats-grid{{display:grid;grid-template-columns:1fr 1fr;gap:7px;}}
.stat-card{{background:var(--surface2);border:1px solid var(--border);border-radius:8px;padding:9px 11px;}}
.stat-val{{font-family:'JetBrains Mono',monospace;font-size:20px;font-weight:700;}}
.stat-label{{font-size:10px;color:var(--text3);margin-top:1px;}}
.warnings-scroll{{flex:1;overflow-y:auto;padding:10px 14px;display:flex;flex-direction:column;gap:6px;}}
.warn-item{{font-size:10.5px;font-family:'JetBrains Mono',monospace;padding:7px 10px;border-radius:7px;line-height:1.45;}}
.warn-o{{background:#1a1005;border:1px solid #3a2008;color:#c8943a;}}
.warn-b{{background:#040e1a;border:1px solid #0a2a4a;color:#5a9ad0;}}
.content{{flex:1;display:flex;flex-direction:column;overflow:hidden;}}
.view{{flex:1;overflow:hidden;display:none;}}
.view.active{{display:flex;flex-direction:column;}}
#diagramView{{background:var(--bg);}}
.diagram-canvas{{flex:1;overflow:auto;display:flex;align-items:flex-start;padding:40px;}}
#jsonView{{background:var(--surface);}}
.json-toolbar{{display:flex;align-items:center;gap:10px;padding:10px 16px;border-bottom:1px solid var(--border);flex-shrink:0;}}
.json-toolbar span{{font-size:11px;font-family:'JetBrains Mono',monospace;color:var(--text3);}}
.json-output{{flex:1;overflow:auto;padding:20px 24px;font-family:'JetBrains Mono',monospace;font-size:12px;line-height:1.7;color:#7dd3fc;white-space:pre;}}
.legend{{padding:10px 16px;border-top:1px solid var(--border);display:flex;gap:16px;flex-wrap:wrap;font-size:10.5px;color:var(--text3);background:var(--surface);align-items:center;flex-shrink:0;}}
.li{{display:flex;align-items:center;gap:6px;}}
.ld{{width:10px;height:10px;border-radius:3px;}}
.ls{{width:1px;height:14px;background:var(--border2);margin:0 2px;}}
.node-group{{cursor:default;}}
.node-group:hover .node-bg{{filter:brightness(1.15);}}
.edge{{fill:none;stroke-width:2;}}
</style>
</head>
<body>
<div class="topbar">
  <div class="logo"><div class="logo-icon">⬡</div>Workflow Analyzer</div>
  <div class="topbar-sep"></div>
  <span class="wf-name">{wf_name}</span>
  <div class="topbar-sep"></div>
  <div class="tab-group">
    <div class="tab active" onclick="switchTab('diagramView',this)">⬡ Diagramma</div>
    <div class="tab" onclick="switchTab('jsonView',this)">{{ }} JSON</div>
  </div>
  <div class="topbar-right">
    <button class="btn" onclick="downloadJson()">↓ JSON</button>
    <button class="btn" onclick="downloadSvg()">↓ SVG</button>
  </div>
</div>
<div class="main">
  <div class="sidebar">
    <div class="sidebar-section">
      <h3>Statistiche</h3>
      <div class="stats-grid">
        <div class="stat-card"><div class="stat-val" style="color:var(--accent)">{stats.get('total_active_nodes',0)}</div><div class="stat-label">Nodi attivi</div></div>
        <div class="stat-card"><div class="stat-val" style="color:var(--green)">{len(data.get('edges',[]))}</div><div class="stat-label">Connessioni</div></div>
        <div class="stat-card"><div class="stat-val" style="color:var(--orange)">{stats.get('total_input_items',0)}</div><div class="stat-label">Input items</div></div>
        <div class="stat-card"><div class="stat-val" style="color:var(--text3)">{stats.get('total_commented_nodes',0)}</div><div class="stat-label">Commentati</div></div>
      </div>
    </div>
    <div class="sidebar-section" style="flex:1;overflow:hidden;display:flex;flex-direction:column;">
      <h3>Note &amp; avvisi</h3>
      <div class="warnings-scroll">{warnings_html}</div>
    </div>
  </div>
  <div class="content">
    <div class="view active" id="diagramView">
      <div class="diagram-canvas" id="diagramCanvas"></div>
      <div class="legend">
        <div class="li"><div class="ld" style="background:#0a2040;border:1.5px solid #3b82f6;"></div>Single</div>
        <div class="li"><div class="ld" style="background:#180d32;border:1.5px solid #8b5cf6;"></div>Edit</div>
        <div class="li"><div class="ld" style="background:#0f2010;border:1.5px solid #22c55e;"></div>Multi</div>
        <div class="li"><div class="ld" style="background:#082818;border:1.5px solid #22d3a0;"></div>Summary</div>
        <div class="li"><div class="ld" style="background:#280a0a;border:1.5px solid #f87171;"></div>Outcome</div>
        <div class="ls"></div>
        <div class="li"><svg width="24" height="10"><line x1="0" y1="5" x2="24" y2="5" stroke="#c8943a" stroke-width="2" stroke-dasharray="5,3"/></svg>Runtime branch</div>
        <div class="li"><div class="ld" style="background:#1a1005;border:1.5px dashed #c8943a;"></div>Orphan</div>
      </div>
    </div>
    <div class="view" id="jsonView">
      <div class="json-toolbar">
        <span id="jsonStats">{len(json_pretty)} chars · {len(json_pretty.splitlines())} righe</span>
        <button class="btn" onclick="copyJson(this)" style="margin-left:auto;">⎘ Copia</button>
      </div>
      <div class="json-output" id="jsonOutput"></div>
    </div>
  </div>
</div>

<script>
const DATA = {json_data};
const JSON_PRETTY = {json.dumps(json_pretty, ensure_ascii=False)};

// ── TABS ──
function switchTab(id, el) {{
  document.querySelectorAll('.tab').forEach(t => t.classList.remove('active'));
  document.querySelectorAll('.view').forEach(v => v.classList.remove('active'));
  el.classList.add('active');
  document.getElementById(id).classList.add('active');
}}

// ── JSON VIEW ──
document.getElementById('jsonOutput').textContent = JSON_PRETTY;

function copyJson(btn) {{
  navigator.clipboard.writeText(JSON_PRETTY);
  btn.textContent = '✓ Copiato';
  setTimeout(() => btn.textContent = '⎘ Copia', 2000);
}}

function downloadJson() {{
  const blob = new Blob([JSON_PRETTY], {{type:'application/json'}});
  const a = Object.assign(document.createElement('a'), {{href:URL.createObjectURL(blob), download: DATA.workflowName+'.json'}});
  a.click();
}}

function downloadSvg() {{
  const svgEl = document.querySelector('#diagramCanvas svg');
  if (!svgEl) return;
  const blob = new Blob([svgEl.outerHTML], {{type:'image/svg+xml'}});
  const a = Object.assign(document.createElement('a'), {{href:URL.createObjectURL(blob), download: DATA.workflowName+'.svg'}});
  a.click();
}}

// ── DIAGRAM ──
const NW=240, TH=54, IH=17, IP=7;
const positioned = {json.dumps(positioned, ensure_ascii=False)};
const edges = DATA.edges;

function wrap(t, max) {{
  if (!t || t.length <= max) return [t||''];
  const words=t.split(' '); const lines=[]; let cur='';
  for (const w of words) {{
    if ((cur+' '+w).trim().length > max) {{ lines.push(cur.trim()); cur=w; }}
    else cur=(cur+' '+w).trim();
  }}
  if (cur) lines.push(cur.trim());
  return lines;
}}

if (positioned.length) {{
  const PAD=80;
  const minX=Math.min(...positioned.map(n=>n.rx))-PAD;
  const minY=Math.min(...positioned.map(n=>n.ry))-PAD;
  const maxX=Math.max(...positioned.map(n=>n.rx+NW))+PAD;
  const maxY=Math.max(...positioned.map(n=>n.ry+n.nh))+PAD;
  const W=maxX-minX, H=maxY-minY;

  const NM={{}};
  positioned.forEach(n=>NM[n.id]=n);

  const svg=d3.select('#diagramCanvas').append('svg')
    .attr('width',W).attr('height',H)
    .attr('viewBox',`${{minX}} ${{minY}} ${{W}} ${{H}}`)
    .style('overflow','visible')
    .style('margin','0 auto')
    .style('display','block');

  const defs=svg.append('defs');
  const cols=[...new Set(edges.map(e=>e.color).filter(Boolean))];
  cols.forEach(c=>{{
    defs.append('marker').attr('id','a'+c.slice(1))
      .attr('viewBox','0 -5 10 10').attr('refX',8).attr('refY',0)
      .attr('markerWidth',6).attr('markerHeight',6).attr('orient','auto')
      .append('path').attr('d','M0,-5L10,0L0,5').attr('fill',c);
  }});

  edges.forEach(e=>{{
    const s=NM[e.from], t=NM[e.to];
    if(!s||!t) return;
    const col=e.color||'#4a5a7a';
    const sx=s.cx, sy=s.ry+s.nh, tx=t.cx, ty=t.ry;
    const cv=Math.min(Math.abs(ty-sy)*.5,160);
    const pathD=Math.abs(sx-tx)<4?`M${{sx}},${{sy}} L${{tx}},${{ty}}`:`M${{sx}},${{sy}} C${{sx}},${{sy+cv}} ${{tx}},${{ty-cv}} ${{tx}},${{ty}}`;
    svg.append('path').attr('d',pathD).attr('class','edge')
      .attr('stroke',col).attr('opacity',e.isRuntimeBranch?.55:.85)
      .attr('stroke-dasharray',e.isRuntimeBranch?'6,4':null)
      .attr('marker-end',`url(#a${{col.slice(1)}})`);
    if(e.label){{
      const lx=(sx+tx)/2, ly=(sy+ty)/2;
      const bw=e.label.length*7+16;
      svg.append('rect').attr('x',lx-bw/2).attr('y',ly-11).attr('width',bw).attr('height',20).attr('rx',10)
        .attr('fill','#080b10').attr('stroke',col).attr('stroke-width',1)
        .attr('stroke-dasharray',e.isRuntimeBranch?'4,3':null);
      svg.append('text').attr('x',lx).attr('y',ly+1).attr('text-anchor','middle').attr('dominant-baseline','middle')
        .attr('fill',col).style('font-family',"'JetBrains Mono',monospace").style('font-size','9.5px').text(e.label);
    }}
  }});

  const TS={{
    single:  {{fill:'#0a2040',stroke:'#3b82f6',key:'#60a5fa',hdr:'#0d2a52'}},
    edit:    {{fill:'#180d32',stroke:'#8b5cf6',key:'#a78bfa',hdr:'#1e0f40'}},
    multi:   {{fill:'#0f2010',stroke:'#22c55e',key:'#4ade80',hdr:'#142a18'}},
    summary: {{fill:'#082818',stroke:'#22d3a0',key:'#22d3a0',hdr:'#0d3a22'}},
    outcome: {{fill:'#280a0a',stroke:'#f87171',key:'#f87171',hdr:'#3a1010'}},
  }};

  positioned.forEach(n=>{{
    const type=n.type||'single';
    const s=TS[type]||TS.single;
    const g=svg.append('g').attr('class','node-group');
    const {{rx,ry,nh:h}}=n;

    g.append('rect').attr('x',rx+2).attr('y',ry+4).attr('width',NW).attr('height',h).attr('rx',10).attr('fill','#000').attr('opacity',.3);
    g.append('rect').attr('class','node-bg').attr('x',rx).attr('y',ry).attr('width',NW).attr('height',h).attr('rx',10)
      .attr('fill',s.fill).attr('stroke',s.stroke).attr('stroke-width',1.8)
      .attr('stroke-dasharray',n.isOrphan?'5,3':null);
    g.append('rect').attr('x',rx).attr('y',ry).attr('width',NW).attr('height',TH).attr('rx',10).attr('fill',s.hdr);
    g.append('rect').attr('x',rx).attr('y',ry+TH-10).attr('width',NW).attr('height',10).attr('fill',s.hdr);
    g.append('rect').attr('x',rx).attr('y',ry+10).attr('width',4).attr('height',h-20).attr('rx',2).attr('fill',s.stroke).attr('opacity',.9);

    g.append('text').attr('x',rx+12).attr('y',ry+14).attr('dominant-baseline','middle')
      .attr('fill',s.key).style('font-family',"'JetBrains Mono',monospace").style('font-size','9px').style('font-weight','600')
      .text(n.key||n.id);

    if(n.allowNoChoice){{
      g.append('rect').attr('x',rx+NW-82).attr('y',ry+5).attr('width',76).attr('height',15).attr('rx',7)
        .attr('fill','#1a1005').attr('stroke','#5a3a10').attr('stroke-width',1);
      g.append('text').attr('x',rx+NW-44).attr('y',ry+12.5).attr('text-anchor','middle').attr('dominant-baseline','middle')
        .attr('fill','#c8943a').style('font-family',"'JetBrains Mono',monospace").style('font-size','8px').text('AllowNoChoice');
    }}
    if(n.isOrphan){{
      g.append('rect').attr('x',rx+NW-58).attr('y',ry+5).attr('width',52).attr('height',15).attr('rx',7)
        .attr('fill','#1a1005').attr('stroke','#5a3a10').attr('stroke-width',1);
      g.append('text').attr('x',rx+NW-32).attr('y',ry+12.5).attr('text-anchor','middle').attr('dominant-baseline','middle')
        .attr('fill','#c8943a').style('font-family',"'JetBrains Mono',monospace").style('font-size','8px').text('orphan');
    }}

    const ty0=(n.allowNoChoice||n.isOrphan)?ry+27:ry+23;
    wrap(n.title||'',27).forEach((line,i)=>
      g.append('text').attr('x',rx+12).attr('y',ty0+i*15).attr('dominant-baseline','middle')
        .attr('fill','#cdd2e8').style('font-family',"'Sora',sans-serif").style('font-size','11px').style('font-weight','600')
        .text(line));

    if(n.items&&n.items.length){{
      g.append('line').attr('x1',rx+4).attr('y1',ry+TH).attr('x2',rx+NW-4).attr('y2',ry+TH)
        .attr('stroke',s.stroke).attr('stroke-width',.7).attr('opacity',.35);
      let cy2=ry+TH+IP+IH/2;
      n.items.forEach(it=>{{
        drawIcon(g,rx+10,cy2,n.inputType);
        g.append('text').attr('x',rx+28).attr('y',cy2).attr('dominant-baseline','middle')
          .attr('fill','#aabbd0').style('font-family',"'JetBrains Mono',monospace").style('font-size','9px').style('font-weight','600')
          .text(it.label||'');
        cy2+=IH;
        if(it.description){{
          const desc=it.description.length>34?it.description.slice(0,32)+'…':it.description;
          g.append('text').attr('x',rx+28).attr('y',cy2).attr('dominant-baseline','middle')
            .attr('fill','#3a4a62').style('font-family',"'Sora',sans-serif").style('font-size','8.5px').text(desc);
          cy2+=IH;
        }}
      }});
    }}

    if(type==='summary') g.append('text').attr('x',rx+NW/2).attr('y',ry+TH+13).attr('text-anchor','middle').attr('dominant-baseline','middle')
      .attr('fill','#22d3a0').style('font-family',"'JetBrains Mono',monospace").style('font-size','9px').text('← riepilogo di tutte le scelte →');
    if(type==='outcome') g.append('text').attr('x',rx+NW/2).attr('y',ry+TH+13).attr('text-anchor','middle').attr('dominant-baseline','middle')
      .attr('fill','#f87171').style('font-family',"'JetBrains Mono',monospace").style('font-size','9px').text('✓ attivazione completata');
  }});
}}

function drawIcon(g,x,y,itype){{
  if(itype==='single'){{
    g.append('circle').attr('cx',x+6).attr('cy',y).attr('r',5).attr('fill','none').attr('stroke','#7ec8f7').attr('stroke-width',1.5);
    g.append('circle').attr('cx',x+6).attr('cy',y).attr('r',2.2).attr('fill','#7ec8f7');
  }} else if(itype==='edit'){{
    g.append('rect').attr('x',x+1).attr('y',y-5).attr('width',10).attr('height',10).attr('rx',2.5).attr('fill','none').attr('stroke','#a78bfa').attr('stroke-width',1.5);
    g.append('path').attr('d',`M${{x+3}},${{y}} L${{x+6}},${{y+3}} L${{x+10}},${{y-4}}`).attr('fill','none').attr('stroke','#a78bfa').attr('stroke-width',1.3).attr('stroke-linecap','round');
  }} else if(itype==='multi'){{
    [-3,3].forEach(dy=>g.append('rect').attr('x',x+1).attr('y',y+dy-3.5).attr('width',8).attr('height',8).attr('rx',2).attr('fill','none').attr('stroke','#4ade80').attr('stroke-width',1.3));
  }} else {{
    const c='#7ec8f7';
    g.append('rect').attr('x',x+1).attr('y',y-5).attr('width',10).attr('height',10).attr('rx',2.5).attr('fill','none').attr('stroke',c).attr('stroke-width',1.5);
    g.append('path').attr('d',`M${{x+6}},${{y+2}} L${{x+6}},${{y-3}} M${{x+4}},${{y-1}} L${{x+6}},${{y-4}} L${{x+8}},${{y-1}}`).attr('fill','none').attr('stroke',c).attr('stroke-width',1.2).attr('stroke-linecap','round');
  }}
}}
</script>
</body>
</html>"""


def _render_warnings(warnings: list[dict]) -> str:
    if not warnings:
        return '<div style="font-size:11px;color:var(--text3);font-family:\'JetBrains Mono\',monospace;">Nessun avviso.</div>'
    parts = []
    for w in warnings:
        cls = "warn-o" if w["type"] in ("orphan", "commented") else "warn-b"
        icon = {"orphan": "⚠", "commented": "⚠", "runtime": "ℹ", "merge": "⇶", "info": "·"}.get(w["type"], "·")
        msg = w["message"].replace("&", "&amp;").replace("<", "&lt;").replace(">", "&gt;")
        parts.append(f'<div class="warn-item {cls}">{icon}&nbsp; {msg}</div>')
    return "\n".join(parts)
