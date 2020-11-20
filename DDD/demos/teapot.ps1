# load a teapot in OBJ file format
Install-Module DDD -Repository PSGallery
Import-Module DDD

$url = "https://graphics.cs.utah.edu/courses/cs6620/fall2019/prj05/teapot.obj"
$obj = (Invoke-WebRequest $url).ToString()
$windowsLineEnding = "`r`n"
$lines = $obj -split $windowsLineEnding

# regular expression to match a vertex: "v <X_FLOAT> <Y_FLOAT> <Z_FLOAT>"
$V = "^v"  # https://en.wikipedia.org/wiki/Wavefront_.obj_file#Geometric_vertex
$WS = "\s+"  # whitespace
$FLOAT = "[\d\.\-]+"
# use "Named Captures" to get x, y, and z: https://docs.microsoft.com/en-us/powershell/module/microsoft.powershell.core/about/about_regular_expressions?view=powershell-7#named-captures
$X_FLOAT = "(?<x>$FLOAT)"
$Y_FLOAT = "(?<y>$FLOAT)"
$Z_FLOAT = "(?<z>$FLOAT)"
$vertRegex = "$V$WS$X_FLOAT$WS$Y_FLOAT$WS$Z_FLOAT"

# parse .obj and output verts
$teapot = $lines | ForEach-Object {
    if ($_ -match $vertRegex) {New-Point $Matches.x $Matches.y $Matches.z}
} 
$teapot | Out-3d