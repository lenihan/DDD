# create a coil
Install-Module DDD -Repository PSGallery
Import-Module DDD
0..3600 | ForEach-Object {
    $deg = $_ 
    $rad = $deg*[Math]::PI/180 
    $x = [Math]::Sin($rad)*360
    $y = $deg
    $z = [Math]::Cos($rad)*360
    New-Point $x $y $z
} | Out-3d