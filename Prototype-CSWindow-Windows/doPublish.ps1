Param([Switch]$PowerShellGallery)

# How to write a PowerShell module manifest
# https://docs.microsoft.com/en-us/powershell/scripting/developer/module/how-to-write-a-powershell-module-manifest?view=powershell-7

$root           = $PSScriptRoot
$moduleName     = split-path $PSScriptRoot -Leaf
$version        = '0.0.1'
$name           = 'David Lenihan'
$description    = 'Cross-platform (Windows, Linux, Mac) 3D tools for PowerShell.'
$reqAssemblies  = @("$moduleName.exe")
$tags           = @()
$scripts        = @()
$cmdlets        = @('Out-3d')
$aliases        = @('o3d')

$cmd = "dotnet build '$root\$moduleName.csproj' --configuration Release"
Write-Host "# $cmd" -ForegroundColor Green
Invoke-Expression $cmd
if ($LASTEXITCODE -ne 0) {
    Write-Host "# Fix errors and re-run." -ForegroundColor Green
    return
}

$cmd = "mkdir '$root\publish\$moduleName' -Force | Out-Null"
Write-Host "# $cmd" -ForegroundColor Green
Invoke-Expression $cmd

try {
    $cmd = "Copy-Item '$root\bin\Release\netcoreapp3.1\*' '$root\publish\$moduleName' -Recurse -Force -ErrorAction Stop"
    Write-Host "# $cmd" -ForegroundColor Green
    Invoke-Expression $cmd    
} 
catch {
    Write-Host $_.Exception.Message -ForegroundColor Red
    Write-Host "Kill process to free file." -ForegroundColor Yellow
    return
}

# $cmd = "Copy-Item '$root\assets\functions.ps1' '$root\publish\DDD'"
# Write-Host "# $cmd" -ForegroundColor Green
# Invoke-Expression $cmd

# $cmd = "Copy-Item '$root\assets\aliases.ps1' '$root\publish\DDD'"
# Write-Host "# $cmd" -ForegroundColor Green
# Invoke-Expression $cmd

# $manifestArgs = [ordered]@{
#     Path               = "$root\publish\DDD\DDD.psd1"
#     RootModule         = $moduleName                                             
#     ModuleVersion      = $version                                                
#     Author             = $name                                                   
#     CompanyName        = $name                                                   
#     Copyright          = "Copyright = '(c) $name. All rights reserved.'"         
#     Description        = $description                                            
#     RequiredAssemblies = $reqAssemblies                                          
#     ScriptsToProcess   = $scripts                                                
#     CmdletsToExport    = $cmdlets                                                
#     Tags               = $tags
#     AliasesToExport    = $aliases
# }
# Write-Host '# $manifestArgs contents: ' -ForegroundColor Green
# $manifestArgs

# $cmd = "New-ModuleManifest @manifestArgs"
# Write-Host "# $cmd" -ForegroundColor Green
# Invoke-Expression $cmd

# if ($PowerShellGallery) {
#     $cmd = "Publish-Module -Name '$root\publish\DDD' -NuGetApiKey oy2ig7ftcymwygzfh7oaeychbiaumxyuld27f2zetouyca"
#     Write-Host "# $cmd" -ForegroundColor Green
#     Invoke-Expression $cmd   
# }
# else {
#     $cmd = "Start-Process -FilePath 'pwsh' -ArgumentList '-NoExit -Command Import-Module -Force -Verbose $root\publish\DDD' -PassThru"
#     Write-Host "# $cmd" -ForegroundColor Green
#     $process = Invoke-Expression $cmd
#     $global:pwshId = $process.Id
#     Write-Host "# To run again..." -ForegroundColor Green
#     Write-Host "# kill `$pwshId; $($MyInvocation.InvocationName)" -ForegroundColor Green
# }