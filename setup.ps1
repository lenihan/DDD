#!/usr/bin/env pwsh
<#
Installs the prerequisites needed to build, test, and run DDD: the .NET SDK version pinned in
global.json. Safe to re-run - it checks what's already installed before doing anything.
#>
[CmdletBinding()]
param(
    [switch]$SkipBuildCheck
)

$ErrorActionPreference = 'Stop'

if ($PSVersionTable.PSVersion.Major -lt 7) {
    Write-Error "setup.ps1 requires PowerShell 7+. You're running $($PSVersionTable.PSVersion). Install PowerShell 7 first: https://aka.ms/powershell"
    exit 1
}

$repoRoot = $PSScriptRoot
$globalJsonPath = Join-Path $repoRoot 'global.json'
if (-not (Test-Path $globalJsonPath)) {
    Write-Error "Couldn't find global.json next to setup.ps1 at $globalJsonPath"
    exit 1
}
$globalJson = Get-Content $globalJsonPath -Raw | ConvertFrom-Json
$pinnedSdkVersion = $globalJson.sdk.version          # e.g. "10.0.100"
$requiredChannel = ($pinnedSdkVersion -split '\.')[0..1] -join '.'   # e.g. "10.0"
$requiredMajor = ($pinnedSdkVersion -split '\.')[0]                  # e.g. "10"

function Write-Step($message) {
    Write-Host "==> $message" -ForegroundColor Cyan
}

function Test-DotNetSdkPresent {
    param([string]$Major)
    $dotnetCmd = Get-Command dotnet -ErrorAction SilentlyContinue
    if (-not $dotnetCmd) { return $false }
    try {
        $sdks = & dotnet --list-sdks 2>$null
    } catch {
        return $false
    }
    return [bool]($sdks | Where-Object { $_ -match "^$Major\." })
}

function Install-DotNetSdk-Windows {
    param([string]$Major)
    Write-Step "Installing .NET $Major SDK on Windows"
    $winget = Get-Command winget -ErrorAction SilentlyContinue
    if ($winget) {
        $wingetId = "Microsoft.DotNet.SDK.$Major"
        Write-Host "Running: winget install --id $wingetId --source winget --accept-package-agreements --accept-source-agreements"
        winget install --id $wingetId --source winget --accept-package-agreements --accept-source-agreements
        if ($LASTEXITCODE -ne 0) {
            Write-Warning "winget install exited with code $LASTEXITCODE. You may need to install manually: https://dotnet.microsoft.com/download/dotnet/$Major.0"
        }
        # winget installs to Program Files and updates PATH for new shells, but not this one.
        $candidatePath = 'C:\Program Files\dotnet'
        if ((Test-Path $candidatePath) -and ($env:PATH -notlike "*$candidatePath*")) {
            $env:PATH = "$candidatePath;$env:PATH"
        }
    }
    else {
        Write-Warning "winget isn't available on this machine. Install the .NET $Major SDK manually: https://dotnet.microsoft.com/download/dotnet/$Major.0"
    }
}

function Install-DotNetSdk-UnixLike {
    param([string]$Channel, [string]$OsLabel)
    Write-Step "Installing .NET $Channel SDK on $OsLabel"

    if ($OsLabel -eq 'macOS') {
        $brew = Get-Command brew -ErrorAction SilentlyContinue
        if ($brew) {
            Write-Host "Running: brew install --cask dotnet-sdk"
            Write-Warning "Homebrew's dotnet-sdk cask tracks the latest release, not a pinned channel - it should still satisfy .NET $Channel while it's current, but check 'dotnet --list-sdks' below."
            brew install --cask dotnet-sdk
            if ($LASTEXITCODE -eq 0) { return }
            Write-Warning "brew install failed (exit $LASTEXITCODE); falling back to the official install script."
        }
    }

    # Microsoft's official cross-distro/macOS install script: https://learn.microsoft.com/dotnet/core/tools/dotnet-install-script
    $installDir = Join-Path $HOME '.dotnet'
    $scriptPath = Join-Path ([System.IO.Path]::GetTempPath()) 'dotnet-install.sh'
    Write-Host "Downloading dotnet-install.sh..."
    Invoke-WebRequest -Uri 'https://dot.net/v1/dotnet-install.sh' -OutFile $scriptPath
    chmod +x $scriptPath
    Write-Host "Running: bash $scriptPath --channel $Channel --install-dir $installDir"
    bash $scriptPath --channel $Channel --install-dir $installDir
    if ($LASTEXITCODE -ne 0) {
        Write-Warning "dotnet-install.sh exited with code $LASTEXITCODE."
    }

    if ($env:PATH -notlike "*$installDir*") {
        $env:PATH = "${installDir}:$env:PATH"
    }

    $profileHint = if ($IsMacOS) { '~/.zprofile (or ~/.bash_profile)' } else { '~/.bashrc (or your shell profile)' }
    Write-Host "Add this to $profileHint so future shells can find dotnet:"
    Write-Host "  export PATH=`"$installDir`:`$PATH`""
}

Write-Step "Checking for .NET $requiredMajor SDK (repo is pinned to $pinnedSdkVersion via global.json)"

if (Test-DotNetSdkPresent -Major $requiredMajor) {
    Write-Host ".NET $requiredMajor SDK already installed." -ForegroundColor Green
}
elseif ($IsWindows) {
    Install-DotNetSdk-Windows -Major $requiredMajor
}
elseif ($IsMacOS) {
    Install-DotNetSdk-UnixLike -Channel $requiredChannel -OsLabel 'macOS'
}
elseif ($IsLinux) {
    $wslLabel = if ($env:WSL_DISTRO_NAME) { "WSL ($env:WSL_DISTRO_NAME)" } else { 'Linux' }
    Install-DotNetSdk-UnixLike -Channel $requiredChannel -OsLabel $wslLabel
}
else {
    Write-Error "Unrecognized OS - none of `$IsWindows/`$IsMacOS/`$IsLinux is true. Install the .NET $requiredMajor SDK manually: https://dotnet.microsoft.com/download/dotnet/$requiredMajor.0"
    exit 1
}

Write-Step "Verifying"
if (-not (Test-DotNetSdkPresent -Major $requiredMajor)) {
    Write-Error "The .NET $requiredMajor SDK still isn't visible to 'dotnet --list-sdks' after installation. Open a new shell and re-run this script, or install manually: https://dotnet.microsoft.com/download/dotnet/$requiredMajor.0"
    exit 1
}
Write-Host "dotnet --list-sdks:" -ForegroundColor Green
dotnet --list-sdks

if (-not $SkipBuildCheck) {
    Write-Step "Sanity-building the solution"
    Push-Location $repoRoot
    try {
        dotnet build DDD.sln --configuration Debug
        if ($LASTEXITCODE -ne 0) {
            Write-Warning "Build failed (exit $LASTEXITCODE). Prerequisites are installed, but the solution itself needs attention."
        }
        else {
            Write-Host "Build succeeded." -ForegroundColor Green
        }
    }
    finally {
        Pop-Location
    }
}

Write-Host ""
Write-Host "A sixel-capable terminal is also recommended to actually view Out-3d's output:" -ForegroundColor Yellow
Write-Host "  Windows Terminal (sixel enabled), WezTerm, iTerm2, mlterm, or xterm -ti vt340."
Write-Host "This script does not install one for you, since terminal choice is a personal preference."
