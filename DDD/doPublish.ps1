Param([Switch]$PowerShellGallery)

# How to write a PowerShell module manifest
# https://docs.microsoft.com/en-us/powershell/scripting/developer/module/how-to-write-a-powershell-module-manifest?view=powershell-7

$root           = $PSScriptRoot
$version        = '0.0.3'
$name           = 'David Lenihan'
$moduleName     = 'DDD'
$description    = 'Cross-platform (Windows, Linux, Mac) 3D tools for PowerShell.'
$reqAssemblies  = @('DDD.dll')
$tags           = @('3D', 'ply', 'obj', 'Point', 'Matrix', 'Vector')
$scripts        = @('functions.ps1', 'aliases.ps1')
$cmdlets        = @('Out-3d')

dotnet build $root\DDD.csproj --configuration Release
$null = mkdir $root\publish\DDD -Force
try {
    Copy-Item $root\bin\Release\netcoreapp3.1\DDD.dll $root\publish\DDD -ErrorAction Stop
} 
catch {
    Write-Host $_.Exception.Message -ForegroundColor Red
    Write-Host "Kill process to free file." -ForegroundColor Yellow
    return
}
Copy-Item $root\assets\functions.ps1 $root\publish\DDD
Copy-Item $root\assets\aliases.ps1 $root\publish\DDD
New-ModuleManifest                                                              `
    -Path               $root\publish\DDD\DDD.psd1                              `
    -RootModule         $moduleName                                             `
    -ModuleVersion      $version                                                `
    -Author             $name                                                   `
    -CompanyName        $name                                                   `
    -Copyright          "Copyright = '(c) $name. All rights reserved.'"         `
    -Description        $description                                            `
    -RequiredAssemblies $reqAssemblies                                          `
    -ScriptsToProcess   $scripts                                                `
    -CmdletsToExport    $cmdlets                                                `
    -Tags               $tags

if ($PowerShellGallery) {
    Publish-Module -Name $root\publish\DDD -NuGetApiKey oy2ig7ftcymwygzfh7oaeychbiaumxyuld27f2zetouyca
}
else {
    # Publish locally for testing
    Write-Host "# Import-Module -Force -Verbose $root\publish\DDD" -ForegroundColor Green
    # Creating new pwsh in a window. Exit to release files in use.
    Start-Process -FilePath "pwsh" -ArgumentList "-NoExit -Command Import-Module -Force -Verbose $root\publish\DDD"
}