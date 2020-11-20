# create a sphere
Install-Module DDD -Repository PSGallery
Import-Module DDD
$points = for($degY = 0; $degY -lt 360; $degY += 10) {
    for($degZ = 0; $degZ -lt 360; $degZ += 10) {
        $point = New-Point 1 0 0
        $rotZ = New-RotateZMatrix $degZ
        $rotY = New-RotateYMatrix $degY
        $rotZ * $rotY * $point
    } 
} 
$points | Out-3d