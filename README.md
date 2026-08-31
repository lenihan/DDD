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

`Import-Ply -Path <file>` reads an ASCII `.ply` file (Stanford Polygon File Format) into a
`Mesh` - DDD's native mesh file format, chosen over inventing a new one since `.ply` already
covers vertices, triangular faces, and optional per-vertex normals/colors, and is supported by
most other 3D tools. `Export-Ply -Path <file>` writes a `Mesh` back out the same way (pipeline
or `-Mesh`). A face with more than 3 vertices in an imported file is fan-triangulated. Binary
`.ply` is not supported - only the ASCII variant.

```powershell
$mesh | Export-Ply -Path ./tetrahedron.ply
Import-Ply -Path ./tetrahedron.ply | Out-3d -RenderMode Solid
```

## Terminal requirements

`Out-3d` needs a terminal that understands sixel graphics. Known-good options:
Windows Terminal (with the sixel experimental feature enabled), WezTerm, iTerm2, mlterm, and
xterm started with `-ti vt340`. Running it in a terminal without sixel support produces a clear
error rather than garbled output.

[DDD Blog Posts](http://www.davidlenihan.com/category/ddd/)
