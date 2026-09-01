# DDD
Cross-platform (Windows, Linux, Mac) 3D tools for PowerShell.

![alt text](DDD.png "DDD")

Pipe `Point`/`Vector`/`Matrix`/`Mesh` objects to `Out-3d` (alias `o3d`) to visualize them as an
animated, interactive scene rendered directly in your terminal using
[sixel graphics](https://en.wikipedia.org/wiki/Sixel) — no window, no GPU driver, same code path
on every OS. The scene auto-rotates 360° around the X axis, then 360° around the Y axis, and
repeats, until you take the camera over yourself.

```powershell
Install-Module DDD -Repository PSGallery
Import-Module DDD
$points | Out-3d
```

## Running from source

To build and try DDD without installing from PSGallery, run `make.ps1` from the repo root
(`-Release` builds the Release configuration; omit it for a Debug build):

```powershell
./DDD/make.ps1 -Release -KillPrev
```

This builds the module and opens a new PowerShell window with it imported. In that window, run
a quick smoke test to confirm `Out-3d` is working:

```powershell
New-Point 1 0 0 | Out-3d
```

You should see a single point rendered as a sixel image, auto-rotating in the terminal. Press
`Esc` to exit. If a prior build is still loaded, pass `-KillPrev` to `make.ps1` to close it
before opening the new window.

Run the unit tests with `-Test` (add `-Release` to run them against the Release build):

```powershell
./DDD/make.ps1 -Test -Release
```

## Controls

An on-screen instructions line (bottom-left) and an FPS counter (top-right) are shown by default
(hide them with `-HideInstructions`/`-HideFps`, or toggle either live with `H`/`F`):

| Keys            | Action                                             |
|-----------------|----------------------------------------------------|
| `↑`/`↓`         | Rotate X                                           |
| `←`/`→`         | Rotate Y                                           |
| `[`/`]`         | Roll Z                                             |
| `+`/`-`         | Zoom in/out                                        |
| `T`             | Resume the auto-rotate turntable                   |
| `P`             | Toggle orthographic ⟷ perspective                  |
| `M`             | Cycle mesh render mode: points ⟷ wireframe ⟷ solid |
| `N`             | Toggle face normal indicators                      |
| `F`             | Toggle FPS overlay                                 |
| `H`             | Toggle the instructions overlay                    |
| `Esc` or Ctrl+C | Quit                                               |

Tapping a rotation key nudges the view by a fixed step; holding it down rotates continuously via
your OS's normal keyboard auto-repeat. The auto-rotate turntable runs until the first rotation
keypress, then hands control to you until you press `T` to resume it. `Out-3d` also takes
`-Perspective`, `-HideFps`, `-HideInstructions`, `-RenderMode <Points|Wireframe|Solid>`, and
`-ShowNormals` switches to set the initial state non-interactively (e.g. for a demo script).
`Mesh` render mode only affects `Mesh` objects (vertices/faces) - `Point`/`Vector`/`Matrix`
objects always render the same way. Solid mode culls faces pointing away from the camera and
shades the rest with a fixed camera-relative light. The render surface fills the current terminal
window. `Out-3d` draws in the terminal's alternate screen buffer — the same full-screen mode
`vim`/`less`/`htop` use — so exiting (`Esc` or Ctrl+C) snaps the terminal back to exactly what it
showed before `Out-3d` ran, with no scrolling or leftover output.

## Meshes

Build a `Mesh` with `New-Mesh`, add vertices (each an optional `Color` and/or `Normal` beyond
its `Point` position) with `.AddVertex(...)`, and add triangular faces by vertex index with
`.AddFace(...)`:

```powershell
$mesh = New-Mesh
$a = $mesh.AddVertex((New-Vertex (New-Point 1 0 0) (New-Color 255 0 0)))
$b = $mesh.AddVertex((New-Point 0 1 0))
$c = $mesh.AddVertex((New-Point 0 0 0))
$mesh.AddFace($a, $b, $c)
$mesh | Out-3d -RenderMode Solid
```

### Primitives

Parametric shapes, all built on the `Mesh` model above. Every parameter has a default (unit-sized
lengths/radii, a true cone via `-TopRadius 0`), so every cmdlet below also works with no
arguments at all - e.g. `New-Box` alone gives a 1x1x1 box at the origin:

| Cmdlet         | Parameters                                         |
|----------------|----------------------------------------------------|
| `New-Box`      | `-Width -Height -Depth -Center`                    |
| `New-Cube`     | `-Size -Center`                                    |
| `New-Sphere`   | `-Radius -Segments -Center`                        |
| `New-Cylinder` | `-Radius -Height -Segments -Center`                |
| `New-Cone`     | `-BaseRadius -TopRadius -Height -Segments -Center` |
| `New-Torus`    | `-MajorRadius -MinorRadius -Segments -Center`      |
| `New-Plane`    | `-Width -Depth -Center`                            |

`New-Cone -TopRadius 0` gives a true cone; equal `-BaseRadius`/`-TopRadius` gives a frustum.
`New-Cylinder` is a `New-Cone` with equal radii under the hood. All faces wind outward, so
backface culling and shading behave correctly in `-RenderMode Solid`:

```powershell
New-Torus -MajorRadius 3 -MinorRadius 1 -Segments 32 | Out-3d -RenderMode Solid
```

`Import-Ply -Path <file>` reads a `.ply` file (Stanford Polygon File Format, ASCII or binary,
either endianness) into a `Mesh` - DDD's native mesh file format, chosen over inventing a new one
since `.ply` already covers vertices, triangular faces, and optional per-vertex normals/colors,
and is supported by most other 3D tools. `Export-Ply -Path <file>` writes a `Mesh` back out the
same way (pipeline or `-Mesh`), always as ASCII. A face with more than 3 vertices in an imported
file is fan-triangulated.

```powershell
$mesh | Export-Ply -Path ./tetrahedron.ply
Import-Ply -Path ./tetrahedron.ply | Out-3d -RenderMode Solid
```

`Import-Gltf -Path <file>` and `Export-Gltf -Path <file>` read/write binary `.glb` (glTF 2.0) -
DDD's scene interchange format for multi-object work `.ply` doesn't cover (multiple meshes,
materials, lights, cameras). Covers mesh geometry (positions, normals, per-vertex color, triangle
indices) with each mesh's own materials, lights (`KHR_lights_punctual` -
directional/point/spot), and a camera (`New-Camera`); baked animation import is still planned
(see `PLAN.md`). `Export-Gltf` takes any mix of `Mesh`, `Light`, and (at most one) `Camera`
objects on the pipeline; `Import-Gltf` writes back out whatever combination the file contains.
Importing a file authored elsewhere bakes each mesh-carrying node's translation/rotation/scale
into that mesh's own vertices - DDD has no live scene-graph, so a `Mesh` piped out of
`Import-Gltf` is always already in its final position. Only `.glb` (single self-contained file)
is supported, not loose `.gltf` + `.bin` + textures.

`New-Camera -Position <point> -LookAt <point>` describes a directed shot - position, aim point,
up, field of view, near/far planes, perspective vs. orthographic - independent of glTF; `Camera`
isn't wired into `Out-3d`'s rendering yet (still the auto-fit interactive turntable either way),
so today it's purely a value that round-trips through `Export-Gltf`/`Import-Gltf` for hand-editing
a shot in an external glTF-aware tool. glTF only encodes a camera's *orientation* (via its node's
rotation), not "look at this point," so `LookAt` is lossy across a round trip - re-imported as a
point synthesized one unit along the decoded direction, not the original target.

A `Mesh`'s `Material`, if set, round-trips as glTF's PBR `metallicFactor`/`roughnessFactor` -
DDD still shades per-face with the same ambient/diffuse/specular model either way (see
[Lights and materials](#lights-and-materials) above), so this is an approximation, not a real
BRDF: metals get near-zero diffuse response and a strong, tight highlight; non-metals stay
diffuse-dominant with a subtle one. `Ambient` has no glTF equivalent (real PBR ambient comes from
image-based lighting, which DDD doesn't do), so round-tripping a `Material` through glTF is lossy
for anything not already expressed purely as metallic/roughness. `Light` doesn't carry a color
(see above), so an imported light's `color` field is discarded, and export always omits it.

```powershell
$mesh | Export-Gltf -Path ./tetrahedron.glb
Import-Gltf -Path ./tetrahedron.glb | Out-3d -RenderMode Solid

# multiple meshes, a light, and a camera, round-tripped together
$box = New-Box -Center (New-Point -2 0 0)
$sphere = New-Sphere -Center (New-Point 2 0 0)
$camera = New-Camera -Position (New-Point 0 2 6) -LookAt (New-Point 0 0 0)
$box, $sphere, (New-Light -Position (New-Point 3 3 3)), $camera | Export-Gltf -Path ./scene.glb
Import-Gltf -Path ./scene.glb | Out-3d -RenderMode Solid
```

### Reference meshes

Classic, instantly-recognizable test meshes from computer graphics history, bundled as embedded
binary `.ply` assets and loaded through the same reader as `Import-Ply` - no arguments, no
network access:

```powershell
New-Teapot | Out-3d -RenderMode Solid          # the Utah teapot (Martin Newell, 1975)
New-Suzanne | Out-3d -RenderMode Solid         # Blender's default test mesh
New-StanfordBunny | Out-3d -RenderMode Solid   # Stanford Computer Graphics Laboratory, 1994
```

### Lights and materials

`-RenderMode Solid` shades faces with ambient + diffuse + (optional) specular lighting. With no
`Light` piped in, a fixed camera-relative "headlamp" lights whichever faces point toward you,
regardless of scene rotation. Pipe one or more `New-Light` objects alongside your mesh and they
behave like real lights fixed in the room instead - rotating with the scene as it turns:

```powershell
New-Light -Direction (New-Vector 0 1 0)        # directional, "toward the light" convention
New-Light -Position (New-Point 0 3 2)          # point light, at a position
New-Light -LookAt $mesh                        # directional, aimed at $mesh's bounding-box center
```

`New-Material` controls how a `Mesh` responds to light - assign it to `$mesh.Material`:

```powershell
$mesh = New-Sphere
$mesh.Material = New-Material -Color (New-Color 100 150 255) -Specular 0.6 -Shininess 32
$mesh, (New-Light -Position (New-Point 2 3 2)) | Out-3d -RenderMode Solid
```

A `Mesh` with no `Material` uses a default that reproduces the old fixed headlamp look exactly.
Lights don't carry a color, only intensity, so shading always stays a single brightness scalar
applied to a face's own color - sixel output only exact-matches a small fixed palette, and a
colored-light tint would need a much larger one.

`New-CornellBox` builds the classic Cornell Box lighting test scene (red/green side walls, two
blocks) - purpose-built for seeing how convincing your lights/materials look:

```powershell
(New-CornellBox), (New-Light -Position (New-Point 0 1.9 0) -Intensity 0.9) | Out-3d -RenderMode Solid
```

(Both sides need parentheses when a statement starts with a bare command name - PowerShell parses
`Cmd, ...` at the start of a statement as arguments to `Cmd`, not an array. Starting with a
variable, e.g. `$mesh, (New-Light ...)`, doesn't have this problem.)

## Graphing

Turn plain PowerShell numbers into a chart, no manual geometry:

```powershell
New-ScatterPlot -Y 3,7,2,9,5 | Out-3d
New-BarChart -Y 3,-2,5 | Out-3d -RenderMode Solid
```

`-Y` is the only required parameter - `-X` defaults to the data's index (0, 1, 2, ...) when
omitted, or takes an explicit array the same length as `-Y`. Data always lands on the Z=0 plane;
`Out-3d` already draws X/Y axis lines through the scene automatically, which double as a chart's
axes for free. `New-BarChart` also takes `-BarWidth` (default `0.6`). Numeric tick labels aren't
implemented yet, and line graphs/3D surfaces (`New-LineGraph`, `New-Surface`) don't exist yet
either - see `PLAN.md`.

## Terminal requirements

`Out-3d` needs a terminal that understands sixel graphics. Known-good options:
Windows Terminal (with the sixel experimental feature enabled), WezTerm, iTerm2, mlterm, and
xterm started with `-ti vt340`. Running it in a terminal without sixel support produces a clear
error rather than garbled output.

[DDD Blog Posts](http://www.davidlenihan.com/category/ddd/)
