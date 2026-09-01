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
debug). **Done** — `Import-Ply`/`Export-Ply` read/write ASCII `.ply` today.
Binary *read* support is still needed, though: 1d bundles the reference
meshes as binary `.ply` assets (see below), so `PlyFormat` needs to parse
binary alongside ASCII before that can work. *Write* can stay ASCII-only
for now — hand-authored meshes don't need it.

### 1d. Primitives
Built on the 1a mesh model: `New-Box`/`New-Cube` (alias), `New-Sphere`,
`New-Cylinder`, `New-Cone`, `New-Torus`, `New-Plane`. Then the classic
reference test meshes — fixed data, not a general capability like the
parametric shapes above, but instantly recognizable to anyone who's
touched 3D graphics and useful as demo/test content:
`New-Teapot`, `New-Suzanne`, `New-StanfordBunny`. None of these are
hand-encoded (not even the teapot's Bezier control points) — each is
bundled as an embedded **binary** `.ply` resource and loaded at runtime
through `PlyFormat`, since the canonical distributions of all three are
commonly binary `.ply` already: smaller and more authentic than
transcribing/re-exporting to ASCII, and a real-world exercise for the
binary reader rather than only hand-written test fixtures. This is the
forcing function for the binary-read support flagged in 1c.

### 1e. `New-Light` / `New-Material` (+ `New-CornellBox`)
Replaces the 1b headlamp placeholder with real shading — ambient + diffuse
(+ specular if it's cheap) lighting driven by one or more lights, materials
carrying at least a base color and shading coefficients. Also
`New-CornellBox` — the classic Cornell Box test scene (a room with
red/green side walls and two blocks), purpose-built for testing how
convincing shading/lighting looks. Unlike the 1d reference meshes it's
small and geometrically simple (a handful of rectangular faces with
specific per-vertex colors), so worth hand-authoring directly rather than
sourcing as an asset — and it pairs naturally with this step since
validating `New-Light`/`New-Material` against it is exactly what it's for.

### 1f. Animation (flip-book) + Cameras
No interpolation engine inside DDD — the **caller** supplies a distinct
scene per frame (e.g. a PowerShell loop that moves/reshapes meshes and
emits them each tick), optionally paired with an explicit `Camera` for that
frame (position, look-at target, up, FOV, near/far, perspective/ortho)
rather than `Out-3d`'s normal auto-fit interactive turntable — directed,
repeatable shots are the point of rendering video, not just watching an
object spin. DDD's job is just consuming that per-frame (scene, camera)
sequence four ways:
- **Live playback** — extend `Out-3d`'s existing render loop to step
  through caller-supplied frames instead of (or in addition to) the
  turntable
- **Geometry export** — write each frame as a numbered `.ply` file
  (`frame0001.ply`, `frame0002.ply`, ...) - mesh-only, works today without
  waiting on 1h
- **Scene export** — once 1h's glTF writer exists, write each frame as a
  standalone numbered `.glb` instead (`frame0001.glb`, ...), capturing the
  mesh(es), materials, lights, and camera together rather than just
  geometry. This is the same writer 1h needs anyway, just called once per
  frame - the payoff is that each frame becomes an independently editable
  file in any glTF-aware tool (Blender, etc.), not an opaque intermediate:
  open one frame, tweak it, re-render just that frame.
- **Image export** — render each frame (using its camera, if given) and
  write it as a numbered `.png` file, for stitching into a video externally
  (e.g. `ffmpeg`) or embedding in docs

A shared "render one frame" core should back all four paths so playback
and every export mode can't drift apart. The `Camera` type is real DDD
data independent of glTF, but 1h's glTF camera import targets the same
type. Combined with 1h's "bake glTF animation on import," this closes a
full loop: import a real animated `.gltf` (authored elsewhere, with real
keyframes) → DDD samples it into a frame sequence → explode that into
per-frame `.glb` files → hand-edit any single frame in an external tool →
render each frame to `.png` → stitch into a video externally.

### 1g. Rendering correctness + performance: Z-buffer (done), multi-core

~~Solid mode had no depth buffer or face sorting at all~~ - fixed:
`Framebuffer` now carries a real per-pixel depth buffer (same dimensions,
one float per pixel, reset to +Infinity on `Clear`) and `FillTriangle`
compares interpolated depth before writing instead of trusting draw
order. Was invisible for convex primitives (backface culling alone hid
every face that would've needed depth testing) but a real gap for
non-convex or multi-object scenes — `New-CornellBox` (a room plus two
blocks) was exactly the kind of scene where it could show up, and it had
only ever been unit-tested for winding/vertex counts, never actually
checked for correct occlusion. Verified via direct `Framebuffer`
depth-ordering tests plus a `Rasterizer` test rendering two
same-screen-footprint, different-depth meshes both insertion orders and
confirming the nearer one always wins.

Still open: multi-core rendering, which the depth buffer now enables
safely - partition `Mesh.Faces` across threads, each rasterizing into
the *same* framebuffer/depth-buffer with a depth-compare-and-write per
pixel — correct regardless of which thread gets there first, no per-face
ordering dependency. (A simpler alternative if that feels premature:
partition the *framebuffer* by row-band instead, one thread per band,
each rasterizing every face but only writing pixels in its own rows — no
synchronization needed, but doesn't do anything the depth buffer above
doesn't already cover on its own.) Multi-core performance matters most
once frame rendering is happening in bulk for video (1f) rather than
once per interactive frame.

### 1h. glTF import/export
Treated as a first-class DDD format alongside `.ply`, not just a
someday-maybe note — but scoped deliberately, since glTF 2.0's spec has a
long tail and DDD's renderer is a CPU/sixel software rasterizer, not a
GPU: some glTF features cost far more to support well than they'd ever be
visible as, at typical sixel/terminal resolution and palette size.

**In scope:**
- JSON parsing (`System.Text.Json`, already in the BCL, no new dependency)
  and the `.glb` binary container (a single self-contained file, same
  instinct as bundling the 1d reference meshes as binary `.ply`) —
  prioritized over loose `.gltf` + `.bin` + texture files
- Buffer/BufferView/Accessor — glTF's generic typed-binary-data layer
  (component type, vector width, stride, min/max), the plumbing everything
  else sits on
- Mesh import/export (`primitive.attributes` POSITION/NORMAL/COLOR_0 +
  `indices` ↔ `Mesh`) — maps closely onto what `PlyFormat` already does;
  non-TRIANGLES primitive modes (LINES/POINTS) map onto `RenderMode` or
  are rejected on import
- Materials, **scalar factors only** (`baseColorFactor`, `metallicFactor`,
  `roughnessFactor`, `emissiveFactor`) mapped onto an extended `Material`,
  still shaded per-face like today — `metallic`/`roughness` adjust the
  existing ambient/diffuse/specular formula rather than a real
  Cook-Torrance BRDF
- Scene graph, **flattened on import**: bake each node's world transform
  into its mesh's vertices once, rather than building a live hierarchical
  scene graph DDD doesn't otherwise have
- Lights (`KHR_lights_punctual`, now near-core) ↔ `Light` — needs a
  `LightKind.Spot` added; also reopens per-light color, deliberately
  dropped in 1e for palette-quantization reasons
- Cameras ↔ the new `Camera` type from 1f — wanted independent of glTF
  too, for directing precise video shots rather than only the auto-fit
  interactive turntable
- Animation, **baked on import**: evaluate glTF's keyframe samplers
  (LINEAR/STEP/CUBICSPLINE) at N sampled times and produce a caller-style
  frame sequence — feeds directly into 1f's existing playback/export
  mechanism rather than needing DDD's renderer to understand live
  interpolation. Paired with mesh/material/light/camera **export** above,
  this is also what makes 1f's per-frame `.glb` export meaningful: bake an
  animated `.gltf` down to a frame sequence, then re-export each frame as
  its own editable `.glb`.

**Explicitly out of scope (for now):**
- Textures — waiting to see how far scalar-only materials get first. DDD
  is mostly used to *create* content (procedural/generated meshes), where
  materials/shading standing in for textures is often good enough; if
  that holds up, the case for textures gets a lot weaker. Real textures
  would also need an image decoder (PNG at minimum — .NET's built-in image
  support is Windows-only, so this means a new cross-platform dependency
  or a hand-rolled decoder) and a rewrite of Solid-mode shading from
  per-face to per-pixel (barycentric-interpolated UV/normal sampled inside
  `FillTriangle`), which would also force `SixelEncoder` from
  exact-palette-match to nearest-color-match, since per-pixel sampled/lit
  color won't fit a small discrete level set. By far the single biggest
  item on this list — sized separately if/when it's actually needed.
- Skinning/skeletal animation — a large, separate subsystem (bind poses,
  joint hierarchies, per-vertex weight blending) with no fit for any
  current DDD use case.
- Morph targets — moderate cost, lower priority than the above; revisit
  later.
- Any extension beyond `KHR_lights_punctual` (Draco compression, vendor
  extensions, etc.).

### 1i. Graphing
A new use case, built on top of everything above (points/lines/faces
rendering, primitives-as-building-blocks, the mesh model) rather than
before it: `New-LineGraph` / `New-BarChart` / `New-ScatterPlot` (2D,
PowerShell data → points/lines) and `New-Surface` (3D — e.g. a height-field
grid turned into a mesh). Goal: turn PowerShell data into a readable graph
with one cmdlet, no manual geometry.

**Done**: `New-ScatterPlot` (`-Y`/optional `-X`, returns `Point`s) and
`New-BarChart` (same input, returns one thin `Mesh` box per value, via
`Primitives.Box`) - both land data on the Z=0 plane and lean on `Out-3d`
already drawing X/Y axis lines through every scene for free, rather than
needing a locked/dedicated camera view (no `Camera`-rendering integration
exists yet - see 1f). **Not done**: `New-LineGraph` (needs either a new
line-segment primitive or ribbon-mesh geometry, deferred to keep this
first pass small), `New-Surface`, and numeric tick/axis labels (need real
screen-space text tied to *projected* data coordinates - `BitmapFont`
today only draws `Out-3d`'s own fixed-position FPS/instructions overlay,
not arbitrary 3D-anchored text, so this needs new `Rasterizer` support).

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
positioned objects, materials, lights, or cameras. See 1h for the scoped
glTF/GLB import/export plan that covers this, now pulled into Part 1
rather than deferred — the trigger condition (multi-object scenes with
lights/materials) already exists as of 1e.

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
4. Primitives: box, sphere, cylinder, cone, torus, plane, then the
   reference meshes — teapot, Suzanne, Stanford bunny (1d)
5. `New-Light` / `New-Material`, replacing the headlamp placeholder, plus
   the Cornell Box test scene (1e)
6. Animation + Cameras: caller-driven (scene, camera) frame sequences →
   live `Out-3d` playback, `.ply`-per-frame export, `.png`-per-frame
   export for video (1f) — **in progress**
7. Rendering correctness + performance: Z-buffer (fixes a real
   depth-ordering gap for non-convex/multi-object scenes), multi-core
   rendering (1g)
8. glTF import/export: mesh/materials(scalar)/scene(flattened)/lights/
   cameras/animation(baked); textures and skinning explicitly deferred
   (1h)
9. Graphing: 2D line/bar/scatter, 3D surface (1i)
10. MCP server wrapping primitives/transforms/export (Part 2) — small
    surface, but already lets an agent build and export simple shapes
    end to end
11. Boolean ops + arrays (Part 2) — unblocks real mechanical/parametric
    parts; scope CSG (library vs. hand-rolled) separately when this starts
12. Claude Skill + reference doc + demo pieces (Part 3) — discoverability
    layer, can start in parallel with step 1 since it doesn't block on new
    code

The niche here (parametric, code-defined, LLM-authorable CLI geometry) is
genuinely underserved compared to neural text-to-3D (Meshy, Tripo, Luma) —
those aim at organic/game-art assets. This lane is closer to OpenSCAD's or
CadQuery's audience: mechanical, procedural, engineering-precise shapes.
That's a smaller audience but a much less crowded one, and it's a better
match for what LLMs are actually good at (writing composable, precise code)
than for neural mesh diffusion.
