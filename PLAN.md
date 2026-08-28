# DDD Production-Readiness Plan

Goal: replace the Windows-only OpenGL window in `Out-3d` with an in-process sixel renderer
(turntable animation, no interaction) so the module is genuinely cross-platform as a single
PowerShell module artifact, and modernize the build/test tooling to current (2026) .NET
practice. All work happens on the `sixel` branch.

`setup.ps1` (repo root) installs the pinned .NET SDK on Windows/Linux/WSL/macOS and sanity-builds
the solution — run it first on a new machine.

## 1. Retire the OpenGL/Win32 path and dead prototypes
- [x] Delete `DDD/UIWindows.cs`, `DDD/UIWindowsPinvoke.cs`, `DDD/UIWindowsPinvoke.txt`
- [x] Delete `Win32-OpenGL/`, `Win32-OpenGL-DLL/`, `DotNetCore-OpenGL/`, `CPP2CSBindings/`,
      `Prototype-CSWindow-Windows/`
- [x] Remove their entries from `DDD.sln`

## 2. Modernize build tooling & project files
- [x] Add `global.json` pinning the .NET SDK (also opts `dotnet test` into MTP mode via the
      `test.runner` key — required on the .NET 10 SDK)
- [x] Add `Directory.Build.props` (net10.0, Nullable, ImplicitUsings, LangVersion latest,
      EnableNETAnalyzers/AnalysisLevel)
- [x] Add `Directory.Packages.props` (central package management)
- [x] `DDD/DDD.csproj`: drop `Microsoft.CodeAnalysis.FxCopAnalyzers`, bump
      `System.Management.Automation` to 7.6.5
- [x] `UnitTest-Point/UnitTest.csproj`: migrate to `MSTest.Sdk` / Microsoft.Testing.Platform
- [x] Update `DDD/make.ps1` output paths (`netcoreapp3.1` → `net10.0`)
- [x] `dotnet test` passes after the modernization pass — verified (see Verification)

## 3. Software rasterizer (replaces immediate-mode OpenGL)
- [x] Minimal RGB framebuffer (no `System.Drawing`) — `DDD/Framebuffer.cs`
- [x] Bresenham line drawing + point plotting
- [x] Turntable camera (view/projection using existing `Point`/`Vector`/`Matrix`) — `DDD/Rasterizer.cs`
- [x] Auto-fit framing from bounding box
- [x] Draw axes, points, vectors, matrix gizmos, bbox wireframe

## 4. Sixel encoder
- [x] `DDD/SixelEncoder.cs`: framebuffer → sixel escape sequence, palette-quantized
- [x] Unit-testable independent of Console I/O

## 5. New `UI` implementation + `Out-3d.cs` wiring
- [x] Redesign `UI.cs` interface for looping render
- [x] `DDD/UISixel.cs`: turntable loop (X 360°, then Y 360°, repeat), Ctrl+C clean exit
- [x] Remove Windows-only check in `Out-3d.cs` `EndProcessing`
- [x] Clear error when terminal doesn't support sixel (best-effort env-var heuristic)

## 6. Tests
- [x] `UnitTest-Point` still green after modernization — 95/95 passing
- [x] `SixelEncoder` tests (known framebuffer → known sixel bytes) — `UnitTest_SixelEncoder.cs`
- [x] Rasterizer projection math tests — `UnitTest_Rasterizer.cs`

## 7. CI, packaging, docs
- [x] `.github/workflows/ci.yml`: build+test matrix on windows/ubuntu/macos, net10.0
- [x] Update `README.md` (accurate cross-platform claim, sixel terminal requirements)
- [x] `make.ps1` path fix (covered in milestone 2)

## Verification
- [x] `dotnet build DDD.sln` succeeds (only `DDD` + `UnitTest-Point` remain) — clean build,
      18 pre-existing nullable/CA warnings on old code, no errors
- [x] `dotnet test` passes via MTP, including new encoder/rasterizer tests — **95/95 passed**
- [x] Manual smoke test: ran `Out-3d` against real `Point` objects in an isolated child process,
      captured stdout for 3s, force-killed it. Zero stderr output; captured 20 complete sixel
      frames with correct DCS header, raster attributes, and palette color percentages
      (hand-verified against the `Rasterizer` palette's RGB values). Confirms the full
      `Out-3d` → `UISixel` → `Rasterizer` → `SixelEncoder` pipeline works end-to-end.
- [ ] Manual: actually *view* a demo's turntable animation in a real sixel-capable terminal
      (e.g. Windows Terminal with sixel enabled) — **not done; needs a human with such a
      terminal open, this environment has no visual terminal to check against**
- [ ] Manual: same demo in a non-sixel terminal gives a clear error, not garbled output — not
      yet explicitly tried (the `EnsureSixelSupported` heuristic treated the redirected-output
      smoke test above as "supported" via its `Console.IsOutputRedirected` escape hatch, so this
      specific path is still unverified)
- [ ] CI matrix green on all three OSes — **not yet run; needs the branch pushed / a PR opened**
