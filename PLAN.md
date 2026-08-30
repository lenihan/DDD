# DDD as an LLM-Native 3D Tool: MCP Server Proposal

## Context

DDD (https://github.com/lenihan/DDD) is a PowerShell module for cross-platform
3D visualization. Today it does one thing well: pipe `Point`/`Vector`/`Matrix`
objects to `Out-3d` (`o3d`) and get an animated turntable rendered as sixel
graphics directly in the terminal — no GPU, no window, same code path on
Windows/Linux/Mac. `Out-3d` also has a full interactive control layer:
rotate/roll/zoom, ortho/perspective toggle, an FPS overlay, and a resumable
auto-rotate turntable.

The idea: turn DDD into a real modeling/visualization toolkit — first for a
human driving it from PowerShell (build meshes, load/save `.ply` files,
animate, graph data), and eventually for an LLM to drive directly via MCP
("make me a gear with 12 teeth" → a rendered/exported 3D object) rather than
something that has to be scripted by hand.

**Current gap:** DDD is a *point/vector visualizer*, not yet a *mesh tool*.
There's no `Mesh` type (vertices/faces/normals/colors), no primitive-shape
cmdlets, no face rendering, no file I/O, and no animation or graphing
support. Part 1 below is the near-term build-out that closes that gap, in
the order it needs to happen. Parts 2 and 3 (MCP server, discoverability)
are the longer-term payoff once Part 1 exists.

---

## Part 1: Geometry, rendering, and content pipeline

This is the near-term work, in build order — each step is a prerequisite
for the ones after it.

### 1a. Mesh representation + color
A `Mesh` type: a vertex list and a triangle-index face list. Triangulate at
creation time rather than supporting arbitrary N-gons — a rasterizer and
`.ply` both want triangles anyway. Per vertex: `Position`, optional
`Normal`, optional `Color`. Color and normal live **per-vertex**, not
per-face — that's a superset (flat shading is just "every vertex of a face
shares a value"), and it's what lets smooth-shaded surfaces work later
without a second data model. If a vertex has no normal, derive a face
normal from winding order on demand (culling/shading still work).

### 1b. `Out-3d` rendering support
Extend the renderer to actually draw meshes, not just points:
- Point / line / face render modes (mixable — e.g. wireframe over solid)
- A filled-triangle rasterizer in `Framebuffer` (today it only has
  `DrawLine` — no polygon fill exists at all)
- Backface culling (`dot(normal, viewDir) < 0` → skip), on by default for
  face mode, with a flag to disable it for debugging
- Normal visualization mode — draw a short line from each face/vertex along
  its normal, for debugging geometry
- A fixed camera-relative "headlamp" placeholder light
  (`intensity = dot(normal, viewDir)`) so faces are legible (a single flat
  color per face is an unreadable silhouette) until 1e adds real lights

### 1c. `.ply` import/export
Read and write `.ply` (Stanford Polygon File Format) — chosen over
inventing a DDD-native format because it already covers exactly the 1a data
model (vertices, triangle faces, optional per-vertex normal, optional
per-vertex color) and is supported by essentially every other 3D tool.
Start with ASCII `.ply` for both directions (trivial to hand-inspect and
debug); add binary later only if load speed on large meshes is actually a
problem. `New-Mesh`/`Import-Ply` and `Export-Ply` (or `Import-Mesh
-Format Ply` / `Export-Mesh -Format Ply` if a format-agnostic wrapper feels
better once more formats exist — decide at implementation time).

### 1d. Primitives
Built on the 1a mesh model: `New-Box`/`New-Cube` (alias), `New-Sphere`,
`New-Cylinder`, `New-Cone`, `New-Torus`, `New-Plane`. `New-Teapot` last —
the Utah teapot is fixed reference data (a hardcoded set of Bezier control
points, tessellated at N segments), a fun "real 3D tool" flex but not a
general capability like the others.

### 1e. `New-Light` / `New-Material`
Replaces the 1b headlamp placeholder with real shading — ambient + diffuse
(+ specular if it's cheap) lighting driven by one or more lights, materials
carrying at least a base color and shading coefficients.

### 1f. Animation (flip-book)
No interpolation engine inside DDD — the **caller** supplies a distinct
scene/mesh per frame (e.g. a PowerShell loop that moves/reshapes a mesh and
emits it each tick); DDD's job is just consuming that per-frame sequence
three ways:
- **Live playback** — extend `Out-3d`'s existing render loop to step
  through caller-supplied frames instead of (or in addition to) the
  turntable
- **Geometry export** — write each frame as a numbered `.ply` file
  (`frame0001.ply`, `frame0002.ply`, ...)
- **Image export** — render each frame and write it as a numbered `.png`
  file, for stitching into a GIF/video externally or embedding in docs

A shared "render one frame" core should back all three paths so playback
and both export modes can't drift apart.

### 1g. Graphing
A new use case, built on top of everything above (points/lines/faces
rendering, primitives-as-building-blocks, the mesh model) rather than
before it: `New-LineGraph` / `New-BarChart` / `New-ScatterPlot` (2D,
PowerShell data → points/lines, camera locked to an orthographic front
view) and `New-Surface` (3D — e.g. a height-field grid turned into a mesh).
Axis/tick/label text reuses the existing `BitmapFont`. Goal: turn PowerShell
data into a readable graph with one cmdlet, no manual geometry.

---

## Part 2: Wrap it as an MCP server

Once Part 1 exists, this is the single highest-leverage move for LLM
discoverability. Rather than an LLM writing PowerShell text that *might*
work, an MCP server exposes DDD's cmdlets as structured tools any MCP-aware
agent (Claude Code, Claude.ai, others) can call directly, with typed
parameters and descriptions telling the model exactly what each tool does.

### Why MCP over "LLM writes PowerShell"
- Structured params instead of free-text code generation — fewer syntax
  errors, no need for the model to remember exact cmdlet names/flags
  perfectly
- Discoverable: any MCP client can introspect available tools and their
  schemas without reading source
- Composable: an agent can chain `create_box` → `create_cylinder` →
  `boolean_subtract` → `export_ply` as separate tool calls, inspecting
  intermediate state between steps (useful for iterating on a design)
- Works from any MCP host, not just PowerShell-literate agents

### Suggested tool surface (v1)

Keep the tool set small and orthogonal at first — a handful of well-described
tools beats twenty half-finished ones.

```
create_primitive(shape: "box"|"sphere"|"cylinder"|"cone"|"torus"|"teapot",
                  dimensions: {...}, position: [x,y,z]) -> mesh_id

transform_mesh(mesh_id, operation: "translate"|"rotate"|"scale",
               params: {...}) -> mesh_id

boolean_op(mesh_id_a, mesh_id_b, operation: "union"|"subtract"|"intersect")
    -> mesh_id

array_mesh(mesh_id, pattern: "linear"|"circular", count, spacing/radius)
    -> mesh_id

export_mesh(mesh_id, format: "ply"|"glb", path) -> file_path

render_preview(mesh_id) -> sixel/image (wraps Out-3d for a quick look)

list_scene() -> current mesh_ids and their descriptions (lets the agent
    track what it's built across a multi-step session)
```

Each tool's description (the string an MCP client shows the model) should
read like documentation, not a code comment — state units assumed (mm?
unitless?), coordinate convention (Y-up or Z-up), and any size limits.
That description *is* the API contract as far as the model is concerned.

### Boolean ops are a separate hard problem
Real CSG (union/subtract/intersect on arbitrary meshes) is a substantial
algorithm in its own right, not something to hand-roll casually alongside
everything else in Part 1. Treat "bring in a well-tested library" vs.
"build it from scratch" as its own decision when this is actually scoped —
don't block primitives/animation/graphing on it.

### Scene format for multi-object work
`.ply` (from 1c) covers a single mesh well but has no concept of multiple
positioned objects, materials, or lights. Once that's needed (multi-object
scenes, post-Part-1), lean on **glTF/GLB** — an existing, well-supported
standard for exactly that — rather than inventing a DDD-native scene
format.

### Session/state model
Decide early: does the server hold mesh state across calls (a "scene graph"
the agent builds up over a conversation), or is every call stateless with
meshes passed by reference/serialized data? A stateful scene graph
(`list_scene`, `mesh_id` handles) matches how a human would iterate on a
model and is more natural for an agent doing multi-step design work.

---

## Part 3: Everything else that helps LLMs find and use it

1. **A single machine-readable reference doc** — one file listing every
   cmdlet/tool, its parameters, types, and 2-3 example calls. This is what
   an LLM actually reads before using the tool; scattered comment-based
   help across source files is much weaker context than one dense
   reference doc.

2. **Example-driven docs, not just API reference** — pairs like "a bolt is:
   `New-Cylinder ... | Join-Mesh (New-Cylinder <head params> | Move-Mesh ...)`"
   teach the *vocabulary* of composing primitives, which is what a model
   generalizes from far better than a flat parameter list.

3. **A Claude Skill** — package the cmdlet/tool vocabulary as a SKILL.md
   (examples + when-to-use guidance) so Claude specifically gets good at
   driving DDD. This is a much smaller lift than the MCP server and can
   be done in parallel — worth doing even before boolean ops etc. exist,
   just covering what's there today.

4. **Keep README/PSGallery/GitHub topics current** — this is what surfaces
   in a web search or training data for "3D modeling PowerShell" or
   "parametric CLI CAD," which matters for both human discovery and future
   model training data.

5. **A couple of concrete demo pieces** — "prompt → DDD scene → rendered
   turntable → exported `.ply`" is a much stronger portfolio artifact than
   the visualizer alone. Doesn't need to be fancy; a gear, a bracket, a
   simple enclosure are enough to prove the pipeline end to end.

---

## Rough sequencing

1. Mesh representation + color (1a)
2. `Out-3d` face/line/point rendering: fill rasterizer, backface culling,
   normal viz, headlamp shading placeholder (1b)
3. `.ply` import/export (1c)
4. Primitives: box, sphere, cylinder, cone, torus, plane, then teapot (1d)
5. `New-Light` / `New-Material`, replacing the headlamp placeholder (1e)
6. Animation: caller-driven frame sequences → live `Out-3d` playback,
   `.ply`-per-frame export, `.png`-per-frame export (1f)
7. Graphing: 2D line/bar/scatter, 3D surface (1g)
8. MCP server wrapping primitives/transforms/export (Part 2) — small
   surface, but already lets an agent build and export simple shapes
   end to end
9. Boolean ops + arrays (Part 2) — unblocks real mechanical/parametric
   parts; scope CSG (library vs. hand-rolled) separately when this starts
10. Claude Skill + reference doc + demo pieces (Part 3) — discoverability
    layer, can start in parallel with step 1 since it doesn't block on new
    code

The niche here (parametric, code-defined, LLM-authorable CLI geometry) is
genuinely underserved compared to neural text-to-3D (Meshy, Tripo, Luma) —
those aim at organic/game-art assets. This lane is closer to OpenSCAD's or
CadQuery's audience: mechanical, procedural, engineering-precise shapes.
That's a smaller audience but a much less crowded one, and it's a better
match for what LLMs are actually good at (writing composable, precise code)
than for neural mesh diffusion.
