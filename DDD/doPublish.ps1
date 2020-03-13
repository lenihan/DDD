Param([Switch]$PowerShellGallery)

$version = '0.0.3'
$name = 'David Lenihan'
$moduleName = 'DDD'
$description = 'Cross-platform (Windows, Linux, Mac) 3D tools for PowerShell.'
$reqAssemblies = 'DDD.dll'
$tags = "'3D', 'ply', 'obj', 'Point', 'Matrix', 'Vector'"
$scripts = "'functions.ps1', 'aliases.ps1'"
$cmdlets = "'Out-3d'"

$root = $PSScriptRoot
dotnet build $root\DDD.csproj --configuration Release
$null = mkdir $root\publish\DDD -Force
try {
    Copy-Item $root\bin\Release\netcoreapp3.1\DDD.dll $root\publish\DDD -ErrorAction Stop
} 
catch {
    Write-Host $_.Exception.Message -ForegroundColor Red
    Write-Host "Try 'exit' to free file." -ForegroundColor Yellow
    return
}
New-ModuleManifest -Path $root\publish\DDD\DDD.psd1
$manifest = Get-Content $root\publish\DDD\DDD.psd1
$manifest   -replace "^# RootModule = .*$",         "RootModule = '$moduleName'"                        `
            -replace "^ModuleVersion = .*$",        "ModuleVersion = '$version'"                        `
            -replace "^Author = .*$",               "Author = '$name'"                                  `
            -replace "^CompanyName = .*$",          "CompanyName = '$name'"                             `
            -replace "^Copyright = .*$",            "Copyright = '(c) $name. All rights reserved.'"     `
            -replace "^# Description = .*$",        "Description = '$description'"                      `
            -replace "^# RequiredAssemblies = .*$", "RequiredAssemblies = @('$reqAssemblies')"          `
            -replace "^# ScriptsToProcess = .*$",   "ScriptsToProcess = @($scripts)"                    `
            -replace "^CmdletsToExport = .*$",      "CmdletsToExport = @($cmdlets)"                     `
            -replace "# Tags = .*$",                "Tags = @($tags)"                                   `
    | Set-Content $root\publish\DDD\DDD.psd1

if ($PowerShellGallery) {
    Publish-Module -Name $root\publish\DDD -NuGetApiKey oy2ig7ftcymwygzfh7oaeychbiaumxyuld27f2zetouyca
}
else {
    # Publish locally for testing
    Write-Host "# Creating new pwsh. Exit to release files in use." -ForegroundColor Green
    pwsh -NoExit -Command "& {Write-Host '# Import-Module -Force $root\publish\DDD' -ForegroundColor Green; Import-Module -Force $root\publish\DDD}"
}