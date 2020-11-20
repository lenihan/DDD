$text = "PowerShell!"


import-module DDD

Add-Type -AssemblyName System.Drawing

$bmp = [Drawing.Bitmap]::new(($w = 10*$text.Length), ($h=20))
$g = [Drawing.Graphics]::FromImage($bmp)
$g.DrawString($text, [Drawing.Font]::new("Fixedsys", 10), [Drawing.Brushes]::Black, 0, 0)
    
$allPoints = foreach ($x in 0..($w-1))
{
  foreach ($y in 0..($h-1))
  {
    if ($bmp.GetPixel($x, $y).A -gt 0) {
      new-point $x ($h-$y) 0
    }
  }
}
$allPoints | out-3d