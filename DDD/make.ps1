Param(
    [Switch]$PowerShellGallery,
    [Switch]$Release,
    [Switch]$KillPrev,
    [Switch]$Test,
    [String]$ApiKey
)

$root           = $PSScriptRoot
$pidFile        = "$root\.ddd-pwsh-pid"
$version        = '0.1.0'
$name           = 'David Lenihan'
$moduleName     = 'DDD'
$description    = "Cross-platform (Windows, Linux, Mac) 3D tools for PowerShell.`nhttp://www.davidlenihan.com/category/ddd/"
$reqAssemblies  = @('DDD.dll')
$tags           = @('3D', 'ply', 'gltf', 'glb', 'obj', 'Point', 'Matrix', 'Vector')
$scripts        = @('functions.ps1', 'aliases.ps1')
$cmdlets        = @('Out-3d', 'Import-Ply', 'Export-Ply', 'Import-Gltf', 'Export-Gltf', 'New-Box', 'New-Sphere', 'New-Cylinder', 'New-Cone', 'New-Torus', 'New-Plane', 'New-Teapot', 'New-Suzanne', 'New-StanfordBunny', 'New-Light', 'New-Material', 'New-Camera', 'New-CornellBox', 'New-ScatterPlot', 'New-BarChart', 'New-Surface')
$aliases        = @('o3d')
$config         = $Release ? "Release" : "Debug"


if ($KillPrev) {
    if (Test-Path $pidFile) {
        $oldId = Get-Content $pidFile
        $proc = Get-Process -Id $oldId -ErrorAction SilentlyContinue
        if ($proc -and $proc.ProcessName -eq 'pwsh') {
            Stop-Process -Id $oldId -Force
        }
        Remove-Item $pidFile -ErrorAction SilentlyContinue
    }
}
# How to write a PowerShell module manifest
# https://docs.microsoft.com/en-us/powershell/scripting/developer/module/how-to-write-a-powershell-module-manifest?view=powershell-7


# Clean
$cmd = "dotnet clean '$root\DDD.csproj' --configuration $config"
Write-Host "# $cmd" -ForegroundColor Green
Invoke-Expression $cmd
if ($LASTEXITCODE -ne 0) {
    Write-Host "# Fix errors and re-run." -ForegroundColor Green
    return
}

# Build
#       Creates $root\bin\$config\net10.0\DDD.dll
#               $root\obj\$config\net10.0\DDD.dll
$cmd = "dotnet build '$root\DDD.csproj' --configuration $config"
Write-Host "# $cmd" -ForegroundColor Green
Invoke-Expression $cmd
if ($LASTEXITCODE -ne 0) {
    Write-Host "# Fix errors and re-run." -ForegroundColor Green
    return
}

if ($Test) {
    $cmd = "dotnet test '$root\..\UnitTest-Point\UnitTest.csproj' --configuration $config"
    Write-Host "# $cmd" -ForegroundColor Green
    Invoke-Expression $cmd
    if ($LASTEXITCODE -ne 0) {
        Write-Host "# Tests failed." -ForegroundColor Red
    }
    return
}

$cmd = "mkdir '$root\publish\DDD' -Force | Out-Null"
Write-Host "# $cmd" -ForegroundColor Green
Invoke-Expression $cmd

try {
    $cmd = "Copy-Item '$root\bin\$config\net10.0\DDD.dll' '$root\publish\DDD' -ErrorAction Stop"
    Write-Host "# $cmd" -ForegroundColor Green
    Invoke-Expression $cmd    
} 
catch {
    Write-Host $_.Exception.Message -ForegroundColor Red
    Write-Host "Kill process to free file." -ForegroundColor Yellow
    return
}

$cmd = "Copy-Item '$root\assets\functions.ps1' '$root\publish\DDD'"
Write-Host "# $cmd" -ForegroundColor Green
Invoke-Expression $cmd

$cmd = "Copy-Item '$root\assets\aliases.ps1' '$root\publish\DDD'"
Write-Host "# $cmd" -ForegroundColor Green
Invoke-Expression $cmd

$manifestArgs = [ordered]@{
    Path               = "$root\publish\DDD\DDD.psd1"
        RootModule         = $moduleName                                             
        ModuleVersion      = $version                                                
        Author             = $name                                                   
        CompanyName        = $name                                                   
        Copyright          = "Copyright = '(c) $name. All rights reserved.'"         
        Description        = $description                                            
        RequiredAssemblies = $reqAssemblies                                          
        ScriptsToProcess   = $scripts                                                
        CmdletsToExport    = $cmdlets                                                
        Tags               = $tags
        AliasesToExport    = $aliases
    }
    Write-Host '# $manifestArgs contents: ' -ForegroundColor Green
    $manifestArgs
    
    $cmd = "New-ModuleManifest @manifestArgs"
    Write-Host "# $cmd" -ForegroundColor Green
    Invoke-Expression $cmd
    
if ($PowerShellGallery) {
    if (!$ApiKey) {
        Write-Host "No API Key provided. To publish, pass the API Key via -ApiKey option.`nTo generate an API Key: https://www.powershellgallery.com/account/apikeys" -ForegroundColor Yellow
        return
    }
    $cmd = "Publish-Module -Name '$root\publish\DDD' -NuGetApiKey $ApiKey"
    Write-Host "# $cmd" -ForegroundColor Green
    Invoke-Expression $cmd   
}
else {
    $process = Start-Process -FilePath 'pwsh' -ArgumentList @(
        '-NoExit',
        '-NoProfile',
        '-Command',
        "Import-Module -Force -Verbose '$root\publish\DDD'"
    ) -PassThru

    $process.Id | Out-File $pidFile
    $global:pwshId = $process.Id   # still handy for same-session use
    Write-Host "# Launched pwsh (PID $($process.Id)) running DDD" -ForegroundColor Green
    Write-Host "# To run again..." -ForegroundColor Green
    Write-Host "# $($MyInvocation.InvocationName) -KillPrev" -ForegroundColor Green

}