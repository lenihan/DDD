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
function New-RotateXMatrix ($degrees) {
    return [DDD.Matrix]::RotateX($degrees)
}
function New-RotateYMatrix ($degrees) {
    return [DDD.Matrix]::RotateY($degrees)
}
function New-RotateZMatrix ($degrees) {
    return [DDD.Matrix]::RotateZ($degrees)
}
function New-ScaleMatrix ($xScale, $yScale, $zScale) {
    return [DDD.Matrix]::Scale($xScale, $yScale, $zScale)
}


# static functions
function Get-CrossProduct ($vectorA, $vectorB) {
    return [DDD.Vector]::Cross($vectorA, $vectorB)
}
function Get-DotProduct ($vectorA, $vectorB) {
    return [DDD.Vector]::Dot($vectorA, $vectorB)
}
