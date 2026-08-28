using System;
using System.Collections.Generic;
using System.Text;

namespace DDD
{
    // DEC sixel graphics: https://vt100.net/docs/vt3xx-gp/chapter14.html
    // Each sixel character encodes a 6-pixel-tall vertical strip via bits 0-5 of (value - 63).
    internal static class SixelEncoder
    {
        public static string Encode(Framebuffer framebuffer, IReadOnlyList<(byte R, byte G, byte B)> palette)
        {
            var paletteIndex = new Dictionary<(byte R, byte G, byte B), int>();
            for (int i = 0; i < palette.Count; i++)
            {
                paletteIndex[palette[i]] = i;
            }

            var sb = new StringBuilder();
            sb.Append("P0;0;8q");
            sb.Append('"').Append("1;1;").Append(framebuffer.Width).Append(';').Append(framebuffer.Height);

            for (int i = 0; i < palette.Count; i++)
            {
                (byte r, byte g, byte b) = palette[i];
                sb.Append('#').Append(i).Append(";2;")
                  .Append(ToPercent(r)).Append(';')
                  .Append(ToPercent(g)).Append(';')
                  .Append(ToPercent(b));
            }

            int width = framebuffer.Width;
            int bandCount = (framebuffer.Height + 5) / 6;
            for (int band = 0; band < bandCount; band++)
            {
                int y0 = band * 6;
                int rowsInBand = Math.Min(6, framebuffer.Height - y0);

                var bitsByColor = new int[palette.Count][];
                for (int x = 0; x < width; x++)
                {
                    for (int row = 0; row < rowsInBand; row++)
                    {
                        var pixel = framebuffer.GetPixel(x, y0 + row);
                        int colorIndex = paletteIndex.TryGetValue(pixel, out int idx) ? idx : 0;
                        bitsByColor[colorIndex] ??= new int[width];
                        bitsByColor[colorIndex][x] |= 1 << row;
                    }
                }

                bool firstColorInBand = true;
                for (int colorIndex = 0; colorIndex < palette.Count; colorIndex++)
                {
                    int[]? bits = bitsByColor[colorIndex];
                    if (bits is null) continue;

                    if (!firstColorInBand) sb.Append('$');
                    firstColorInBand = false;
                    sb.Append('#').Append(colorIndex);
                    AppendRun(sb, bits, width);
                }
                sb.Append('-');
            }

            sb.Append("\\");
            return sb.ToString();
        }

        static int ToPercent(byte channel) => (int)Math.Round(channel * 100.0 / 255.0);

        static void AppendRun(StringBuilder sb, int[] bits, int width)
        {
            char? runChar = null;
            int runLength = 0;

            for (int x = 0; x < width; x++)
            {
                char c = (char)(63 + bits[x]);
                if (c == runChar)
                {
                    runLength++;
                    continue;
                }
                Flush(sb, runChar, runLength);
                runChar = c;
                runLength = 1;
            }
            Flush(sb, runChar, runLength);
        }

        static void Flush(StringBuilder sb, char? runChar, int runLength)
        {
            if (runChar is null || runLength == 0) return;
            if (runLength > 3)
            {
                sb.Append('!').Append(runLength).Append(runChar.Value);
            }
            else
            {
                for (int i = 0; i < runLength; i++) sb.Append(runChar.Value);
            }
        }
    }
}
