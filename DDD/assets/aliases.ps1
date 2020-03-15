# VERBS - from https://docs.microsoft.com/en-us/powershell/scripting/developer/cmdlet/approved-verbs-for-windows-powershell-commands?view=powershell-7
# New       = n     
# Get       = g     
# Out       = o     

# NOUNS
# Point     = pt
# Vector    = vec
# Matrix    = mat
# Array     = arr
# Identity  = i
# Zero      = z
# Rotation  = rot
# Scale     = sc
# Translate = tr

Set-Alias -Name npt -Value New-Point
Set-Alias -Name nptarr -Value New-PointArray

Set-Alias -Name nvec -Value New-Vector
Set-Alias -Name nvecarr -Value New-VectorArray

Set-Alias -Name nmat -Value New-Matrix
Set-Alias -Name nmatarr -Value New-MatrixArray

Set-Alias -Name nimat -Value New-IdentityMatrix
Set-Alias -Name nzmat -Value New-ZeroMatrix

Set-Alias -Name nrotxmat -Value New-RotateXMatrix
Set-Alias -Name nrotymat -Value New-RotateYMatrix
Set-Alias -Name nrotzmat -Value New-RotateZMatrix

Set-Alias -Name nscmat -Value New-ScaleMatrix

Set-Alias -Name ntrmatxyz -Value New-TranslateMatrixXYZ
Set-Alias -Name ntrmatvec -Value New-TranslateMatrixVector

Set-Alias -Name gcp -Value Get-CrossProduct
Set-Alias -Name cross -Value Get-CrossProduct

Set-Alias -Name gdp -Value Get-DotProduct
Set-Alias -Name dot -Value Get-DotProduct

Set-Alias -Name o3d -Value Out-3d