"""
workflow_parser.py
Parses C# Workflow class files (BN.WebLicenze pattern) into a structured dict.
"""

import re
import json
from dataclasses import dataclass, field, asdict
from typing import Optional


# ── DATA CLASSES ──────────────────────────────────────────────────────────────

@dataclass
class InputItem:
    label: str
    description: str = ""
    edit_label: str = ""       # range / type hint for Edit inputs


@dataclass
class Node:
    id: str
    key: str
    title: str
    type: str                  # single | edit | multi | summary | outcome
    input_type: str            # single | edit | multi | blob | text | null
    allow_no_choice: bool = False
    items: list[InputItem] = field(default_factory=list)
    is_commented: bool = False
    is_orphan: bool = False    # set post-parse


@dataclass
class Edge:
    from_id: str
    to_id: str
    label: str = ""
    color: str = "#4a5a7a"
    is_runtime_branch: bool = False
    runtime_condition: str = ""


@dataclass
class Warning:
    type: str                  # orphan | commented | runtime | merge | info
    message: str


@dataclass
class WorkflowData:
    workflow_name: str
    runtime_params: list[dict] = field(default_factory=list)
    nodes: list[Node] = field(default_factory=list)
    edges: list[Edge] = field(default_factory=list)
    warnings: list[Warning] = field(default_factory=list)
    metadata: dict = field(default_factory=dict)


# ── HELPERS ───────────────────────────────────────────────────────────────────

def strip_html(text: str) -> str:
    return re.sub(r"<[^>]+>", "", text).strip()

def clean_string(s: str) -> str:
    """Remove surrounding quotes and trim."""
    s = s.strip().strip('"').strip("'")
    return strip_html(s)

def is_line_commented(line: str) -> bool:
    return line.strip().startswith("//")

def parse_input_item_json(raw: str) -> InputItem:
    """Parse an InputItem whose content is a JSON-like string: {'Key':..., 'Text':..., 'DataType':...}"""
    text_m  = re.search(r"'Text'\s*:\s*'([^']*)'", raw)
    dtype_m = re.search(r"'DataType'\s*:\s*'([^']*)'", raw)
    min_m   = re.search(r"'MinValue'\s*:\s*(\d+)", raw)
    max_m   = re.search(r"'MaxValue'\s*:\s*(\d+)", raw)
    tag_m   = re.search(r"'Tag'\s*:\s*'?([^',}]+)'?", raw)

    label = tag_m.group(1).strip() if tag_m else ""
    description = text_m.group(1).strip() if text_m else raw[:60]
    dtype = dtype_m.group(1) if dtype_m else ""

    edit_label = ""
    if dtype == "integer" and min_m and max_m:
        edit_label = f"qty {min_m.group(1)}–{max_m.group(1)}"
    elif dtype == "blob":
        edit_label = "upload"
    elif dtype == "text":
        edit_label = "textarea"

    # Map input_type from DataType
    return InputItem(label=label, description=description, edit_label=edit_label)

def map_input_type(cs_input_type: str) -> str:
    mapping = {
        "InputType.Single":   "single",
        "InputType.Multiple": "multi",
        "InputType.Edit":     "edit",
    }
    return mapping.get(cs_input_type, "single")

def map_node_type(input_type: str) -> str:
    return {"edit": "edit", "multi": "multi"}.get(input_type, "single")

# ── COLOR ASSIGNMENT ──────────────────────────────────────────────────────────

PALETTE = [
    "#3b82f6",  # blue
    "#f59e42",  # orange
    "#a78bfa",  # purple
    "#22d3a0",  # teal
    "#f87171",  # red
    "#facc15",  # yellow
    "#34d399",  # green
    "#60a5fa",  # light blue
    "#fb923c",  # amber
]

def assign_colors(nodes: list[Node], edges: list[Edge]) -> None:
    """
    BFS from root nodes — each branch fan-out gets a new color.
    Final nodes (summary/outcome/uploadFile) always grey.
    """
    FINAL_IDS = {"uploadFile", "summary", "outcome"}
    FINAL_COLOR = "#4a5a7a"
    RUNTIME_COLOR = "#c8943a"

    node_map = {n.id: n for n in nodes}
    out_map: dict[str, list[Edge]] = {n.id: [] for n in nodes}
    for e in edges:
        if e.from_id in out_map:
            out_map[e.from_id].append(e)

    # Color runtime edges first
    for e in edges:
        if e.is_runtime_branch:
            e.color = RUNTIME_COLOR

    # BFS color propagation
    color_of: dict[str, str] = {}
    palette_idx = [0]

    def next_color():
        c = PALETTE[palette_idx[0] % len(PALETTE)]
        palette_idx[0] += 1
        return c

    # Find roots
    in_ids = {e.to_id for e in edges}
    roots = [n.id for n in nodes if n.id not in in_ids]

    queue = list(roots)
    for r in roots:
        color_of[r] = next_color()

    visited = set(roots)
    while queue:
        nid = queue.pop(0)
        out_edges = out_map.get(nid, [])
        fan = [e for e in out_edges if not e.is_runtime_branch]
        multi_branch = len(fan) > 1

        for i, e in enumerate(fan):
            if e.to_id in FINAL_IDS:
                e.color = FINAL_COLOR
                continue
            if multi_branch:
                c = next_color()
            else:
                c = color_of.get(nid, next_color())
            e.color = c
            if e.to_id not in visited:
                color_of[e.to_id] = c
                visited.add(e.to_id)
                queue.append(e.to_id)
            else:
                # Convergence — keep existing color, just color the edge
                e.color = color_of.get(e.to_id, c)


# ── MAIN PARSER ───────────────────────────────────────────────────────────────

class WorkflowParser:

    def __init__(self, source: str):
        self.source = source
        self.lines = source.splitlines()

    # ── TOP-LEVEL ──

    def parse(self) -> WorkflowData:
        name = self._parse_class_name()
        params = self._parse_constructor_params()
        runtime_params = self._parse_runtime_params()

        active_blocks = self._extract_method_blocks(commented=False)
        commented_blocks = self._extract_method_blocks(commented=True)

        active_nodes, active_edges = self._parse_blocks(active_blocks, commented=False)
        commented_nodes, _ = self._parse_blocks(commented_blocks, commented=True)

        all_nodes = active_nodes + commented_nodes
        all_edges = active_edges

        # Implicit summary → outcome edge (framework convention, never written explicitly)
        active_ids = {n.id for n in active_nodes}
        if "summary" in active_ids and "outcome" in active_ids:
            if not any(e.from_id == "summary" and e.to_id == "outcome" for e in all_edges):
                all_edges.append(Edge(from_id="summary", to_id="outcome"))

        # Mark orphans
        reachable = {e.to_id for e in all_edges} | {e.from_id for e in all_edges}
        for n in active_nodes:
            if n.id not in reachable and n.type not in ("summary", "outcome"):
                n.is_orphan = True

        assign_colors(active_nodes, all_edges)

        # Convergence points
        from collections import Counter
        in_counts = Counter(e.to_id for e in all_edges)
        convergence = [nid for nid, cnt in in_counts.items() if cnt > 1]

        warnings = self._build_warnings(active_nodes, commented_nodes, runtime_params, convergence, all_edges)

        metadata = {
            "total_active_nodes": len(active_nodes),
            "total_commented_nodes": len(commented_nodes),
            "total_edges": len(all_edges),
            "total_input_items": sum(len(n.items) for n in active_nodes),
            "convergence_points": convergence,
        }

        return WorkflowData(
            workflow_name=name,
            runtime_params=runtime_params,
            nodes=all_nodes,
            edges=all_edges,
            warnings=warnings,
            metadata=metadata,
        )

    # ── CLASS NAME ──

    def _parse_class_name(self) -> str:
        m = re.search(r"public\s+class\s+(\w+)\s*:", self.source)
        return m.group(1) if m else "UnknownWorkflow"

    # ── CONSTRUCTOR PARAMS ──

    def _parse_constructor_params(self) -> list[str]:
        m = re.search(r"public\s+\w+\s*\(([^)]+)\)", self.source)
        if not m:
            return []
        params = [p.strip() for p in m.group(1).split(",")]
        return [p for p in params if p and not p.startswith("//")]

    def _parse_runtime_params(self) -> list[dict]:
        """Extract private fields used for branching logic."""
        results = []
        for m in re.finditer(r"private\s+(\w+)\s+(\w+)\s*\{[^}]*\}\s*//\s*(.+)", self.source):
            results.append({"name": m.group(2), "type": m.group(1), "description": m.group(3).strip()})
        # Also check constructor body for runtime branching
        ctor_m = re.search(r"public\s+\w+\([^)]+\)[^{]*\{(.*?)(?=private\s+void)", self.source, re.DOTALL)
        if ctor_m:
            body = ctor_m.group(1)
            for m in re.finditer(r"this\.(\w+)\s*=\s*(\w+);", body):
                field_name = m.group(1)
                if not any(r["name"] == field_name for r in results):
                    results.append({"name": field_name, "type": "unknown", "description": "set in constructor"})
        return results

    # ── METHOD BLOCK EXTRACTION ──

    def _extract_method_blocks(self, commented: bool) -> list[str]:
        """
        Extract _AddActivity_ method bodies.
        commented=True  → methods entirely inside a block comment
        commented=False → active methods
        """
        blocks = []

        if not commented:
            pattern = re.compile(
                r"private\s+void\s+_AddActivity_\w+\s*\(Workflow\s+\w+\)\s*\{"
            )
            for m in pattern.finditer(self.source):
                start = m.start()
                # Check if this line is commented
                line_start = self.source.rfind('\n', 0, start) + 1
                prefix = self.source[line_start:start]
                if '//' in prefix:
                    continue
                body = self._extract_brace_block(start)
                if body:
                    blocks.append(body)
        else:
            # Extract commented-out method blocks
            # Find // private void _AddActivity_ patterns
            pattern = re.compile(r"//\s*private\s+void\s+_AddActivity_\w+")
            for m in pattern.finditer(self.source):
                block_lines = []
                pos = self.source.rfind('\n', 0, m.start()) + 1
                # Walk lines forward while they remain commented or blank
                idx = self.source.count('\n', 0, pos)
                depth = 0
                started = False
                for i in range(idx, min(idx + 200, len(self.lines))):
                    ln = self.lines[i]
                    stripped = ln.strip()
                    if not stripped:
                        if started:
                            break
                        continue
                    if not stripped.startswith('//'):
                        break
                    content = stripped[2:].strip()
                    block_lines.append(content)
                    if '{' in content:
                        depth += content.count('{')
                        started = True
                    if '}' in content:
                        depth -= content.count('}')
                        if started and depth <= 0:
                            break
                if block_lines:
                    blocks.append('\n'.join(block_lines))
        return blocks

    def _extract_brace_block(self, start: int) -> str:
        """Extract a {} block starting at `start`, returns full text."""
        depth = 0
        i = start
        src = self.source
        while i < len(src):
            if src[i] == '{':
                depth += 1
            elif src[i] == '}':
                depth -= 1
                if depth == 0:
                    return src[start:i+1]
            i += 1
        return src[start:]

    # ── BLOCK PARSING ──

    def _parse_blocks(self, blocks: list[str], commented: bool) -> tuple[list[Node], list[Edge]]:
        nodes = []
        edges = []
        for block in blocks:
            node, block_edges = self._parse_activity_block(block, commented)
            if node:
                nodes.append(node)
                edges.extend(block_edges)
        return nodes, edges

    def _parse_activity_block(self, block: str, commented: bool) -> tuple[Optional[Node], list[Edge]]:
        # Extract activity key
        key_m = re.search(r'CreateActivity\s*\(\s*"([^"]+)"', block)
        summary_m = re.search(r'CreateSummaryActivity\s*\(', block)
        outcome_m = re.search(r'CreateOutcomeActivity\s*\(', block)

        if summary_m and not key_m:
            node = Node(id="summary", key="summary",
                        title=self._extract_title(block) or "Riepilogo",
                        type="summary", input_type="null", is_commented=commented)
            return node, []

        if outcome_m and not key_m:
            node = Node(id="outcome", key="outcome",
                        title=self._extract_title(block) or "Risultato finale",
                        type="outcome", input_type="null", is_commented=commented)
            return node, []

        if not key_m:
            return None, []

        key = key_m.group(1)
        title = self._extract_title(block) or key
        allow_no_choice = bool(re.search(r'AllowNoChoice\s*=\s*true', block))

        # Input type
        input_type_raw = self._extract_input_type(block)
        input_type = map_input_type(input_type_raw)
        node_type = map_node_type(input_type)
        if node_type == "single" and input_type == "single":
            node_type = "single"

        # Items
        items = self._extract_items(block, input_type)

        # Detect blob/text from items
        for it in items:
            if it.edit_label == "upload":
                input_type = "blob"
                break
            elif it.edit_label == "textarea":
                input_type = "text"
                break

        node = Node(
            id=key, key=key, title=title,
            type=node_type, input_type=input_type,
            allow_no_choice=allow_no_choice,
            items=items, is_commented=commented,
        )

        edges = [] if commented else self._extract_edges(block, key)
        return node, edges

    def _extract_title(self, block: str) -> str:
        m = re.search(r'a\.Title\s*=\s*"([^"]+)"', block)
        return strip_html(m.group(1)) if m else ""

    def _extract_input_type(self, block: str) -> str:
        m = re.search(r'new\s+Input\s*\(\s*(InputType\.\w+)', block)
        return m.group(1) if m else "InputType.Single"

    def _extract_items(self, block: str, input_type: str) -> list[InputItem]:
        items = []

        if input_type == "edit":
            # JSON-style items: new InputItem("{ ... }")
            for m in re.finditer(r'new\s+InputItem\s*\(\s*"(\{[^"]+\})"\s*\)', block):
                items.append(parse_input_item_json(m.group(1)))
        else:
            # Normal items: new InputItem("key", "label") or new InputItem("key", "label", "tag")
            for m in re.finditer(
                r'new\s+InputItem\s*\(\s*"([^"]+)"\s*,\s*"([^"]+)"(?:\s*,\s*"([^"]*)")?\s*\)',
                block
            ):
                label = m.group(1)
                description = m.group(2)
                items.append(InputItem(label=label, description=description))

        return items

    def _extract_edges(self, block: str, from_id: str) -> list[Edge]:
        edges = []

        # CreateBranchToSummary
        if re.search(r'CreateBranchToSummary\s*\(', block):
            edges.append(Edge(from_id=from_id, to_id="summary"))

        # CreateBranchToOutcome
        if re.search(r'CreateBranchToOutcome\s*\(', block):
            edges.append(Edge(from_id=from_id, to_id="outcome"))

        # Runtime if/else branches
        runtime_edges = self._extract_runtime_branches(block, from_id)
        if runtime_edges:
            edges.extend(runtime_edges)
            return edges

        # Standard CreateBranchTo
        branch_pattern = re.compile(
            r'(?:Branch\s+\w+\s*=\s*)?'
            r'(?:\w+\s*=\s*)?'
            r'a\.CreateBranchTo\s*\(\s*"([^"]+)"\s*\)',
        )
        condition_pattern = re.compile(
            r'IfOutputContainsItem\s*\(\s*"([^"]+)"\s*\)'
        )

        # Collect all branches in order, with their conditions
        branch_matches = list(branch_pattern.finditer(block))
        cond_matches = list(condition_pattern.finditer(block))

        cond_map: dict[str, str] = {}
        for b in branch_matches:
            # Find conditions after this branch declaration
            b_end = b.end()
            for c in cond_matches:
                if c.start() > b_end:
                    cond_map[b.group(1)] = c.group(1)
                    break

        for b in branch_matches:
            to_id = b.group(1)
            label = cond_map.get(to_id, "")
            edges.append(Edge(from_id=from_id, to_id=to_id, label=label))

        return edges

    def _extract_runtime_branches(self, block: str, from_id: str) -> list[Edge]:
        """
        Detect if/else branching on a private field (e.g. tipoLicenza).
        Pattern: if (field == value) { b1 = a.CreateBranchTo("x"); } else { ... }
        """
        edges = []
        # Match: if (field == N) \n { \n b1 = a.CreateBranchTo("x"); }
        # or: if (field == 0 || field > 2) { b1 = ... }
        if_pattern = re.compile(
            r'if\s*\(([^)]+)\)\s*\{[^}]*?a\.CreateBranchTo\s*\(\s*"([^"]+)"\s*\)',
            re.DOTALL
        )
        else_pattern = re.compile(
            r'\}\s*else\s*(?:if\s*\(([^)]*)\)\s*)?\{[^}]*?a\.CreateBranchTo\s*\(\s*"([^"]+)"\s*\)',
            re.DOTALL
        )

        for m in if_pattern.finditer(block):
            condition = m.group(1).strip()
            to_id = m.group(2)
            edges.append(Edge(from_id=from_id, to_id=to_id,
                               label=self._simplify_condition(condition),
                               is_runtime_branch=True,
                               runtime_condition=condition))

        for m in else_pattern.finditer(block):
            condition = m.group(1).strip() if m.group(1) else "else"
            to_id = m.group(2)
            edges.append(Edge(from_id=from_id, to_id=to_id,
                               label=self._simplify_condition(condition) or "else",
                               is_runtime_branch=True,
                               runtime_condition=condition))

        return edges

    def _simplify_condition(self, cond: str) -> str:
        cond = cond.strip()
        # tipoLicenza == 1  →  tL=1
        cond = re.sub(r'(\w+)\s*==\s*(\d+)', r'tL=\2', cond)
        cond = re.sub(r'(\w+)\s*>\s*(\d+)', r'tL>\2', cond)
        cond = re.sub(r'\|\|', '|', cond)
        cond = re.sub(r'&&', '&', cond)
        if len(cond) > 20:
            cond = cond[:18] + '…'
        return cond

    # ── WARNINGS ──

    def _build_warnings(self, active: list[Node], commented: list[Node],
                        runtime_params: list[dict], convergence: list[str],
                        edges: list[Edge]) -> list[Warning]:
        warnings = []

        # Runtime params
        for p in runtime_params:
            warnings.append(Warning(
                type="runtime",
                message=f"Branch runtime su '{p['name']}' ({p['type']}): {p['description']}"
            ))

        # Orphan nodes
        for n in active:
            if n.is_orphan:
                warnings.append(Warning(
                    type="orphan",
                    message=f"Nodo '{n.id}' definito ma non raggiungibile da nessun edge attivo"
                ))

        # Commented nodes
        if commented:
            names = ", ".join(n.key for n in commented[:8])
            if len(commented) > 8:
                names += f" (e altri {len(commented)-8})"
            warnings.append(Warning(
                type="commented",
                message=f"Nodi commentati esclusi: {names}"
            ))

        # Convergence
        for nid in convergence:
            in_edges = [e for e in edges if e.to_id == nid]
            sources = ", ".join(e.from_id for e in in_edges)
            warnings.append(Warning(
                type="merge",
                message=f"Convergenza su '{nid}' da: {sources}"
            ))

        return warnings


# ── SERIALIZATION ─────────────────────────────────────────────────────────────

def to_dict(data: WorkflowData) -> dict:
    def node_dict(n: Node) -> dict:
        return {
            "id": n.id,
            "key": n.key,
            "title": n.title,
            "type": n.type,
            "inputType": n.input_type,
            "allowNoChoice": n.allow_no_choice,
            "items": [{"label": i.label, "description": i.description, "editLabel": i.edit_label} for i in n.items],
            "isCommented": n.is_commented,
            "isOrphan": n.is_orphan,
        }

    def edge_dict(e: Edge) -> dict:
        return {
            "from": e.from_id,
            "to": e.to_id,
            "label": e.label,
            "color": e.color,
            "isRuntimeBranch": e.is_runtime_branch,
            "runtimeCondition": e.runtime_condition,
        }

    return {
        "workflowName": data.workflow_name,
        "runtimeParams": data.runtime_params,
        "nodes": [node_dict(n) for n in data.nodes],
        "edges": [edge_dict(e) for e in data.edges],
        "warnings": [{"type": w.type, "message": w.message} for w in data.warnings],
        "metadata": data.metadata,
    }


def parse_file(path: str) -> dict:
    with open(path, encoding="utf-8-sig") as f:
        source = f.read()
    parser = WorkflowParser(source)
    data = parser.parse()
    return to_dict(data)
