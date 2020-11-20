# create a sphere
Install-Module DDD -Repository PSGallery
Import-Module DDD
$allPoints = @() # empty array
for($degY = 0; $degY -lt 360; $degY += 10) {
    for($degZ = 0; $degZ -lt 360; $degZ += 10) {
        $x = 1
        $y = 0
        $z = 0
        $pt = New-Point $x $y $z
        $rotZ = New-RotateZMatrix $degZ
        $rotY = New-RotateYMatrix $degY
        $allPoints += $rotZ * $rotY * $pt
    } 
} 
$allPoints | Out-3d