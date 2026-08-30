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

        [TestMethod]
        public void WrapKeepsTextOnOneLineWhenItAlreadyFits()
        {
            // MeasureWidth("AB  CD") == 6*6-1 == 35, so it fits comfortably under 100.
            string[] lines = DDD.BitmapFont.Wrap("AB  CD", 100);
            string[] expected = { "AB  CD" };
            CollectionAssert.AreEqual(expected, lines);
        }

        [TestMethod]
        public void WrapSplitsOntoANewLineOnlyWhenTheNextTokenWouldOverflow()
        {
            // MeasureWidth("AB  CD") == 35 (fits in 40); MeasureWidth("AB  CD  EF") == 10*6-1 == 59
            // (doesn't fit in 40), so EF has to start a new line.
            string[] lines = DDD.BitmapFont.Wrap("AB  CD  EF", 40);
            string[] expected = { "AB  CD", "EF" };
            CollectionAssert.AreEqual(expected, lines);
        }

        [TestMethod]
        public void WrapNeverSplitsASingleTokenEvenIfItOverflows()
        {
            // A single token wider than maxWidthPixels still gets its own line rather than being
            // dropped or split mid-token.
            string[] lines = DDD.BitmapFont.Wrap("ABCDEFGHIJ", 10);
            string[] expected = { "ABCDEFGHIJ" };
            CollectionAssert.AreEqual(expected, lines);
        }
    }
}
