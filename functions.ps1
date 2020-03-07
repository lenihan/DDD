# Function wrappers around constructors
function New-Point {
    New-Object -TypeName 'DDD.Point' -ArgumentList $args
}
function New-PointArray {
    New-Object -TypeName 'DDD.Point[]' -ArgumentList $args
}

function New-Vector {
    New-Object -TypeName 'DDD.Vector' -ArgumentList $args
}
function New-VectorArray {
    New-Object -TypeName 'DDD.Vector[]' -ArgumentList $args
}
function New-Matrix {
    New-Object -TypeName 'DDD.Matrix' -ArgumentList $args
}
function New-MatrixArray {
    New-Object -TypeName 'DDD.Matrix[]' -ArgumentList $args
}
function New-IdentityMatrix {
    return [DDD.Matrix]::Identity()
}
function New-IdentityMatrix {
    return [DDD.Matrix]::Zero()
}
function New-RotateXMatrix ($DegreesCCW) {
    return [DDD.Matrix]::RotateX($DegreesCCW)
}
function New-RotateYMatrix ($DegreesCCW) {
    return [DDD.Matrix]::RotateY($DegreesCCW)
}
function New-RotateZMatrix ($DegreesCCW) {
    return [DDD.Matrix]::RotateZ($DegreesCCW)
}
function New-ScaleMatrix ($X, $Y, $Z) {
    return [DDD.Matrix]::Scale($X, $Y, $Z)
}
function New-TranslateMatrix ($X, $Y, $Z) {
    return [DDD.Matrix]::Translate($X, $Y, $Z)
}
function New-TranslateMatrix ($Vector) {
    return [DDD.Matrix]::Translate($Vector)
}


# static functions
function Get-CrossProduct ($vectorA, $vectorB) {
    return [DDD.Vector]::Cross($vectorA, $vectorB)
}
function Get-DotProduct ($vectorA, $vectorB) {
    return [DDD.Vector]::Dot($vectorA, $vectorB)
}
