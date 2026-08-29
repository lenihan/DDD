using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DDD_UnitTest
{
    [TestClass]
    public class BitmapFont
    {
        [TestMethod]
        public void DrawsTheLetterIGlyphExactly()
        {
            var framebuffer = new DDD.Framebuffer(DDD.BitmapFont.GlyphWidth, DDD.BitmapFont.GlyphHeight);
            framebuffer.Clear(0, 0, 0);

            DDD.BitmapFont.DrawText(framebuffer, "I", 0, 0, 255, 255, 255);

            string[] expected =
            {
                ".###.",
                "..#..",
                "..#..",
                "..#..",
                "..#..",
                "..#..",
                ".###.",
            };

            for (int row = 0; row < expected.Length; row++)
            {
                for (int col = 0; col < expected[row].Length; col++)
                {
                    var pixel = framebuffer.GetPixel(col, row);
                    var expectedColor = expected[row][col] == '#'
                        ? ((byte)255, (byte)255, (byte)255)
                        : ((byte)0, (byte)0, (byte)0);
                    Assert.AreEqual(expectedColor, pixel, $"Mismatch at ({col},{row})");
                }
            }
        }

        [TestMethod]
        public void MeasureWidthAccountsForInterGlyphSpacing()
        {
            // 3 glyphs: 3 * glyphWidth + 2 * spacing (no trailing spacing after the last glyph).
            int expected = (3 * DDD.BitmapFont.GlyphWidth) + (2 * DDD.BitmapFont.GlyphSpacingPixels);
            Assert.AreEqual(expected, DDD.BitmapFont.MeasureWidth("ABC"));
        }
    }
}
