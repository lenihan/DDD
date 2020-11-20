Add-Type -Assembly 'System.Drawing'
Import-Module 'DDD'

Invoke-WebRequest 'https://upload.wikimedia.org/wikipedia/commons/5/57/Heightmap.png' -OutFile 'heightmap.png'
$bmp = [System.Drawing.Bitmap]::FromFile('./heightmap.png')

$scale = 50

$points = for ($y = 0; $y -lt $bmp.Height; $y++) {
    for ($x = 0; $x -lt $bmp.Width; $x++) {
        New-Point $x $y ($bmp.GetPixel($x,$y).GetBrightness() * $scale)
    }
}

$points | Out-3d