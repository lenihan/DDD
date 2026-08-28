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
- [x] Fix the 10 real pre-existing warnings that `Nullable`/analyzers surfaced (nullable
      annotations on `Point`/`Vector`/`Matrix` constructors and `Equals(object?)` overrides,
      `Out-3d.cs`'s `InputObject` field, matching test-file locals), suppress `CA1707` for the
      legacy `DDD_UnitTest` namespace in `.editorconfig` (same pattern as the existing CA1051
      suppression), and set `TreatWarningsAsErrors` in `Directory.Build.props` so this stays
      clean — confirmed 0 warnings / 0 errors after the fix, `dotnet test` still 95/95
- [x] Bump `actions/checkout` (v4→v7) and `actions/setup-dotnet` (v4→v6) in `ci.yml` — clears the
      11th CI warning (a platform-level "Node.js 20 is deprecated" notice, unrelated to our code)

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
- [x] Clear error when terminal doesn't support sixel — narrowed to a `TERM=dumb` denylist
      after real-world testing showed the original allowlist (env vars like `WT_SESSION`) false-
      negatived on a genuinely working Windows Terminal window spawned via `Start-Process`

## 6. Tests
- [x] `UnitTest-Point` still green after modernization — 95/95 passing
- [x] `SixelEncoder` tests (known framebuffer → known sixel bytes) — `UnitTest_SixelEncoder.cs`
- [x] Rasterizer projection math tests — `UnitTest_Rasterizer.cs`

## 7. CI, packaging, docs
- [x] `.github/workflows/ci.yml`: 6-way matrix — every OS (Windows/Linux/macOS) × every CPU
      architecture GitHub offers hosted runners for (amd64/arm64): `windows-latest` (amd64),
      `windows-11-arm` (arm64), `ubuntu-latest` (amd64), `ubuntu-24.04-arm` (arm64),
      `macos-latest` (arm64/Apple Silicon), `macos-15-intel` (amd64). The `-arm`/`-intel` labels
      are free-for-public-repos only — `lenihan/DDD` is public, so this costs nothing, but note
      for later: if the repo ever goes private, those two ARM labels stop working entirely
      (workflow fails outright), not just start costing money.
- [x] Update `README.md` (accurate cross-platform claim, sixel terminal requirements)
- [x] `make.ps1` path fix (covered in milestone 2)

**No literal 32-bit x86 coverage** — GitHub doesn't offer 32-bit hosted runners on any OS (nobody
ships 32-bit CI images anymore); "amd64" *is* the 64-bit x86 family, so this isn't a gap against
what "x86" usually means today. `DDD.dll` is `AnyCPU` managed IL either way (see PE-header check
below), so this isn't expected to be architecture-sensitive even for the untested case.

## Verification
- [x] `dotnet build DDD.sln` succeeds (only `DDD` + `UnitTest-Point` remain) — clean build,
      **0 warnings**, 0 errors, `TreatWarningsAsErrors` enabled (was 18 warnings locally / 66
      across the CI matrix before the cleanup above)
- [x] `dotnet test` passes via MTP, including new encoder/rasterizer tests — **95/95 passed**
- [x] Manual smoke test: ran `Out-3d` against real `Point` objects in an isolated child process,
      captured stdout for 3s, force-killed it. Zero stderr output; captured 20 complete sixel
      frames with correct DCS header, raster attributes, and palette color percentages
      (hand-verified against the `Rasterizer` palette's RGB values). Confirms the full
      `Out-3d` → `UISixel` → `Rasterizer` → `SixelEncoder` pipeline works end-to-end.
- [x] Manual: actually *view* a demo's turntable animation in a real sixel-capable terminal —
      **confirmed by David**: renders correctly in Windows Terminal on Windows, and in WSL
      (Ubuntu, ARM64) running PowerShell 7.6.5 against the same `DDD.dll`, same Windows Terminal
      window. Proves the "single artifact, no rebuild" cross-platform goal, not just the theory.
- [ ] Manual: same demo in a non-sixel terminal gives a clear error, not garbled output — not
      yet explicitly tried
- [ ] macOS: untested — no Mac available to verify. Should work by the same reasoning (managed-
      only assembly, same runtime model), but needs iTerm2 or WezTerm (Terminal.app has no sixel
      support) and an actual human check before calling it confirmed
- [x] Architecture-independence spot-check: `DDD.dll` (built on this ARM64 machine) reports PE
      machine type "Intel i386" via `file` — the standard placeholder for `AnyCPU`/pure-IL
      assemblies, confirming no architecture-specific bytes regardless of which SDK built it
- [x] CI matrix green across all 6 OS×architecture combinations — **confirmed on merge to
      master (commit `dbbd97f`)**: windows-amd64, windows-arm64, linux-amd64, linux-arm64,
      macos-amd64, macos-arm64 all passed.
      https://github.com/lenihan/DDD/actions/runs/33167330418
