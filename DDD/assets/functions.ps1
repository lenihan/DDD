# function global:wrappers around constructors
function global:New-Point {
    New-Object -TypeName 'DDD.Point' -ArgumentList $args
}
function global:New-PointArray ([Parameter(Mandatory=$true)][ulong]$Size) {
    New-Object -TypeName 'DDD.Point[]' -ArgumentList $Size
}
function global:New-Vector {
    New-Object -TypeName 'DDD.Vector' -ArgumentList $args
}
function global:New-VectorArray ([Parameter(Mandatory=$true)][ulong]$Size) {
    New-Object -TypeName 'DDD.Vector[]' -ArgumentList $Size
}
function global:New-Matrix {
    New-Object -TypeName 'DDD.Matrix' -ArgumentList $args
}
function global:New-MatrixArray ([Parameter(Mandatory=$true)][ulong]$Size) {
    New-Object -TypeName 'DDD.Matrix[]' -ArgumentList $Size
}
function global:New-IdentityMatrix {
    return [DDD.Matrix]::Identity()
}
function global:New-ZeroMatrix {
    return [DDD.Matrix]::Zero()
}
function global:New-RotateXMatrix ([Parameter(Mandatory=$true)][double]$DegreesCCW) {
    return [DDD.Matrix]::RotateX($DegreesCCW)
}
function global:New-RotateYMatrix ([Parameter(Mandatory=$true)][double]$DegreesCCW) {
    return [DDD.Matrix]::RotateY($DegreesCCW)
}
function global:New-RotateZMatrix ([Parameter(Mandatory=$true)][double]$DegreesCCW) {
    return [DDD.Matrix]::RotateZ($DegreesCCW)
}
function global:New-ScaleMatrix (   [Parameter(Mandatory=$true)][double]$X, 
                                    [Parameter(Mandatory=$true)][double]$Y, 
                                    [Parameter(Mandatory=$true)][double]$Z) {
    return [DDD.Matrix]::Scale($X, $Y, $Z)
}
function global:New-TranslateMatrixXYZ ([Parameter(Mandatory=$true)][double]$X, 
                                        [Parameter(Mandatory=$true)][double]$Y, 
                                        [Parameter(Mandatory=$true)][double]$Z) {
    return [DDD.Matrix]::Translate($X, $Y, $Z)
}
function global:New-TranslateMatrixVector ([Parameter(Mandatory=$true)][DDD.Vector]$Vector) {
    return [DDD.Matrix]::Translate($Vector)
}


# static functions
function global:Get-CrossProduct (  [Parameter(Mandatory=$true)][DDD.Vector]$VectorA, 
                                    [Parameter(Mandatory=$true)][DDD.Vector]$VectorB) {
    return [DDD.Vector]::Cross($VectorA, $VectorB)
}
function global:Get-DotProduct ([Parameter(Mandatory=$true)][DDD.Vector]$VectorA, 
                                [Parameter(Mandatory=$true)][DDD.Vector]$VectorB) {
    return [DDD.Vector]::Dot($vectorA, $vectorB)
}
