$version = '0.0.3'
$name = 'David Lenihan'
$moduleName = 'DDD'
$description = 'Cross-platform (Windows, Linux, Mac) 3D tools for PowerShell.'
$reqAssemblies = 'DDD.dll'
$tags = "'3D', 'ply', 'obj', 'Point', 'Matrix', 'Vector'"
$scripts = "'functions.ps1', 'aliases.ps1'"

$root = $PSScriptRoot
dotnet build $root\DDD.csproj --configuration Release
$null = mkdir $root\publish\DDD -Force
Copy-Item $root\bin\Release\netcoreapp3.1\DDD.dll $root\publish\DDD
New-ModuleManifest -Path $root\publish\DDD\DDD.psd1
$manifest = Get-Content $root\publish\DDD\DDD.psd1
$manifest   -replace "^# RootModule = .*$",         "RootModule = '$moduleName'"                        `
            -replace "^ModuleVersion = .*$",        "ModuleVersion = '$version'"                        `
            -replace "^Author = .*$",               "Author = '$name'"                                  `
            -replace "^CompanyName = .*$",          "CompanyName = '$name'"                             `
            -replace "^Copyright = .*$",            "Copyright = '(c) $name. All rights reserved.'"     `
            -replace "^# Description = .*$",        "Description = '$description'"                      `
            -replace "^# RequiredAssemblies = .*$", "RequiredAssemblies = @('$reqAssemblies')"          `
            -replace "^# ScriptsToProcess = .*$",   "ScriptsToProcess =  @($scripts)"                   `
            -replace "# Tags = .*$",                "Tags = @($tags)"                                   | Set-Content $root\publish\DDD\DDD.psd1

Publish-Module -Name $root\publish\DDD -NuGetApiKey oy2ig7ftcymwygzfh7oaeychbiaumxyuld27f2zetouyca
