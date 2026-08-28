# DDD
Cross-platform (Windows, Linux, Mac) 3D tools for PowerShell.

![alt text](DDD.png "DDD")

Pipe `Point`/`Vector`/`Matrix` objects to `Out-3d` (alias `o3d`) to visualize them as an
animated turntable rendered directly in your terminal using [sixel graphics](https://en.wikipedia.org/wiki/Sixel) —
no window, no GPU driver, same code path on every OS. The scene auto-rotates 360° around the
X axis, then 360° around the Y axis, and repeats; press Ctrl+C to stop.

```powershell
Install-Module DDD -Repository PSGallery
Import-Module DDD
$points | Out-3d
```

## Terminal requirements

`Out-3d` needs a terminal that understands sixel graphics. Known-good options:
Windows Terminal (with the sixel experimental feature enabled), WezTerm, iTerm2, mlterm, and
xterm started with `-ti vt340`. Running it in a terminal without sixel support produces a clear
error rather than garbled output.

[DDD Blog Posts](http://www.davidlenihan.com/category/ddd/)
