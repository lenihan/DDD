using System;
using System.Collections.Generic;

namespace DDD
{
    internal static class BitmapFont
    {
        public const int GlyphWidth = 5;
        public const int GlyphHeight = 7;
        public const int GlyphSpacingPixels = 1;

        static readonly Dictionary<char, string[]> Glyphs = new()
        {
            [' '] = new[] { ".....", ".....", ".....", ".....", ".....", ".....", "....." },
            ['0'] = new[] { ".###.", "#...#", "#..##", "#.#.#", "##..#", "#...#", ".###." },
            ['1'] = new[] { "..#..", ".##..", "..#..", "..#..", "..#..", "..#..", ".###." },
            ['2'] = new[] { ".###.", "#...#", "....#", "...#.", "..#..", ".#...", "#####" },
            ['3'] = new[] { ".###.", "#...#", "....#", "..##.", "....#", "#...#", ".###." },
            ['4'] = new[] { "...#.", "..##.", ".#.#.", "#..#.", "#####", "...#.", "...#." },
            ['5'] = new[] { "#####", "#....", "####.", "....#", "....#", "#...#", ".###." },
            ['6'] = new[] { "..##.", ".#...", "#....", "####.", "#...#", "#...#", ".###." },
            ['7'] = new[] { "#####", "....#", "...#.", "..#..", ".#...", ".#...", ".#..." },
            ['8'] = new[] { ".###.", "#...#", "#...#", ".###.", "#...#", "#...#", ".###." },
            ['9'] = new[] { ".###.", "#...#", "#...#", ".####", "....#", "...#.", ".##.." },
            ['A'] = new[] { ".###.", "#...#", "#...#", "#####", "#...#", "#...#", "#...#" },
            ['B'] = new[] { "####.", "#...#", "#...#", "####.", "#...#", "#...#", "####." },
            ['C'] = new[] { ".###.", "#...#", "#....", "#....", "#....", "#...#", ".###." },
            ['D'] = new[] { "####.", "#...#", "#...#", "#...#", "#...#", "#...#", "####." },
            ['E'] = new[] { "#####", "#....", "#....", "####.", "#....", "#....", "#####" },
            ['F'] = new[] { "#####", "#....", "#....", "####.", "#....", "#....", "#...." },
            ['G'] = new[] { ".###.", "#...#", "#....", "#.###", "#...#", "#...#", ".###." },
            ['H'] = new[] { "#...#", "#...#", "#...#", "#####", "#...#", "#...#", "#...#" },
            ['I'] = new[] { ".###.", "..#..", "..#..", "..#..", "..#..", "..#..", ".###." },
            ['J'] = new[] { "..###", "...#.", "...#.", "...#.", "...#.", "#..#.", ".##.." },
            ['K'] = new[] { "#...#", "#..#.", "#.#..", "##...", "#.#..", "#..#.", "#...#" },
            ['L'] = new[] { "#....", "#....", "#....", "#....", "#....", "#....", "#####" },
            ['M'] = new[] { "#...#", "##.##", "#.#.#", "#...#", "#...#", "#...#", "#...#" },
            ['N'] = new[] { "#...#", "##..#", "#.#.#", "#..##", "#...#", "#...#", "#...#" },
            ['O'] = new[] { ".###.", "#...#", "#...#", "#...#", "#...#", "#...#", ".###." },
            ['P'] = new[] { "####.", "#...#", "#...#", "####.", "#....", "#....", "#...." },
            ['Q'] = new[] { ".###.", "#...#", "#...#", "#...#", "#.#.#", "#..#.", ".##.#" },
            ['R'] = new[] { "####.", "#...#", "#...#", "####.", "#.#..", "#..#.", "#...#" },
            ['S'] = new[] { ".####", "#....", "#....", ".###.", "....#", "....#", "####." },
            ['T'] = new[] { "#####", "..#..", "..#..", "..#..", "..#..", "..#..", "..#.." },
            ['U'] = new[] { "#...#", "#...#", "#...#", "#...#", "#...#", "#...#", ".###." },
            ['V'] = new[] { "#...#", "#...#", "#...#", "#...#", "#...#", ".#.#.", "..#.." },
            ['W'] = new[] { "#...#", "#...#", "#...#", "#.#.#", "#.#.#", "##.##", "#...#" },
            ['X'] = new[] { "#...#", "#...#", ".#.#.", "..#..", ".#.#.", "#...#", "#...#" },
            ['Y'] = new[] { "#...#", "#...#", ".#.#.", "..#..", "..#..", "..#..", "..#.." },
            ['Z'] = new[] { "#####", "....#", "...#.", "..#..", ".#...", "#....", "#####" },
            [':'] = new[] { ".....", "..#..", ".....", ".....", "..#..", ".....", "....." },
            ['.'] = new[] { ".....", ".....", ".....", ".....", ".....", "..#..", "....." },
            ['+'] = new[] { ".....", "..#..", "..#..", "#####", "..#..", "..#..", "....." },
            ['-'] = new[] { ".....", ".....", ".....", "#####", ".....", ".....", "....." },
            ['/'] = new[] { "....#", "...#.", "...#.", "..#..", ".#...", ".#...", "#...." },
            ['['] = new[] { ".###.", ".#...", ".#...", ".#...", ".#...", ".#...", ".###." },
            [']'] = new[] { ".###.", "...#.", "...#.", "...#.", "...#.", "...#.", ".###." },
        };

        public static void DrawText(Framebuffer framebuffer, string text, int x, int y, byte r, byte g, byte b, int scale = 1)
        {
            int cursorX = x;
            foreach (char c in text)
            {
                if (Glyphs.TryGetValue(char.ToUpperInvariant(c), out string[]? rows))
                {
                    DrawGlyph(framebuffer, rows, cursorX, y, r, g, b, scale);
                }
                cursorX += (GlyphWidth + GlyphSpacingPixels) * scale;
            }
        }

        public static int MeasureWidth(string text, int scale = 1)
        {
            if (text.Length == 0) return 0;
            return text.Length * (GlyphWidth + GlyphSpacingPixels) * scale - GlyphSpacingPixels * scale;
        }

        // Greedily packs double-space-separated tokens onto as few lines as fit within
        // maxWidthPixels, splitting only when the text doesn't already fit on one line. A single
        // token wider than maxWidthPixels still gets its own (overflowing) line rather than being
        // split mid-token - clipped at the framebuffer edge, same as any oversized draw.
        public static string[] Wrap(string text, int maxWidthPixels, int scale = 1)
        {
            string[] tokens = text.Split("  ");
            var lines = new List<string>();
            string currentLine = "";

            foreach (string token in tokens)
            {
                string candidate = currentLine.Length == 0 ? token : currentLine + "  " + token;
                if (currentLine.Length == 0 || MeasureWidth(candidate, scale) <= maxWidthPixels)
                {
                    currentLine = candidate;
                }
                else
                {
                    lines.Add(currentLine);
                    currentLine = token;
                }
            }
            if (currentLine.Length > 0) lines.Add(currentLine);

            return lines.ToArray();
        }

        static void DrawGlyph(Framebuffer framebuffer, string[] rows, int x, int y, byte r, byte g, byte b, int scale)
        {
            for (int row = 0; row < rows.Length; row++)
            {
                string line = rows[row];
                for (int col = 0; col < line.Length; col++)
                {
                    if (line[col] != '#') continue;

                    for (int sy = 0; sy < scale; sy++)
                    {
                        for (int sx = 0; sx < scale; sx++)
                        {
                            framebuffer.SetPixel(x + col * scale + sx, y + row * scale + sy, r, g, b);
                        }
                    }
                }
            }
        }
    }
}
