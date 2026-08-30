# DDD
Cross-platform (Windows, Linux, Mac) 3D tools for PowerShell.

![alt text](DDD.png "DDD")

Pipe `Point`/`Vector`/`Matrix` objects to `Out-3d` (alias `o3d`) to visualize them as an
animated, interactive scene rendered directly in your terminal using
[sixel graphics](https://en.wikipedia.org/wiki/Sixel) — no window, no GPU driver, same code path
on every OS. The scene auto-rotates 360° around the X axis, then 360° around the Y axis, and
repeats, until you take the camera over yourself.

```powershell
Install-Module DDD -Repository PSGallery
Import-Module DDD
$points | Out-3d
```

## Controls

An on-screen instructions line is shown by default (hide it with `-HideInstructions`, or toggle
it live with `H`):

| Keys | Action |
|---|---|
| `↑`/`↓` | Rotate X |
| `←`/`→` | Rotate Y |
| `[`/`]` | Roll Z |
| `+`/`-` | Zoom in/out |
| `T` | Resume the auto-rotate turntable |
| `P` | Toggle orthographic ⟷ perspective |
| `F` | Toggle FPS overlay |
| `H` | Toggle the instructions overlay |
| `Esc` or Ctrl+C | Quit |

Tapping a rotation key nudges the view by a fixed step; holding it down rotates continuously via
your OS's normal keyboard auto-repeat. The auto-rotate turntable runs until the first rotation
keypress, then hands control to you until you press `T` to resume it. `Out-3d` also takes
`-Perspective`, `-ShowFps`, and `-HideInstructions` switches to set the initial state
non-interactively (e.g. for a demo script). The render surface fills the current terminal
window. `Out-3d` draws in the terminal's alternate screen buffer — the same full-screen mode
`vim`/`less`/`htop` use — so exiting (`Esc` or Ctrl+C) snaps the terminal back to exactly what it
showed before `Out-3d` ran, with no scrolling or leftover output.

## Terminal requirements

`Out-3d` needs a terminal that understands sixel graphics. Known-good options:
Windows Terminal (with the sixel experimental feature enabled), WezTerm, iTerm2, mlterm, and
xterm started with `-ti vt340`. Running it in a terminal without sixel support produces a clear
error rather than garbled output.

[DDD Blog Posts](http://www.davidlenihan.com/category/ddd/)
