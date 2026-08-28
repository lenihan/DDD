using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DDD_UnitTest
{
    [TestClass]
    public class SixelEncoder
    {
        static readonly (byte R, byte G, byte B) Black = (0, 0, 0);
        static readonly (byte R, byte G, byte B) White = (255, 255, 255);

        [TestMethod]
        public void EncodesSinglePixel()
        {
            var framebuffer = new DDD.Framebuffer(1, 1);
            framebuffer.Clear(Black.R, Black.G, Black.B);
            framebuffer.SetPixel(0, 0, White.R, White.G, White.B);

            var palette = new[] { Black, White };
            string sixel = DDD.SixelEncoder.Encode(framebuffer, palette);

            Assert.AreEqual("P0;0;8q\"1;1;1;1#0;2;0;0;0#1;2;100;100;100#1@-\\", sixel);
        }

        [TestMethod]
        public void RunLengthEncodesRepeatedColumnsAndSeparatesColorsWithinABand()
        {
            (byte R, byte G, byte B) fg = (10, 20, 30);
            var framebuffer = new DDD.Framebuffer(5, 1);
            framebuffer.Clear(Black.R, Black.G, Black.B);
            framebuffer.SetPixel(0, 0, fg.R, fg.G, fg.B);
            framebuffer.SetPixel(1, 0, fg.R, fg.G, fg.B);
            framebuffer.SetPixel(2, 0, fg.R, fg.G, fg.B);
            framebuffer.SetPixel(3, 0, fg.R, fg.G, fg.B);
            // pixel (4,0) is left as the cleared background color.

            var palette = new[] { Black, fg };
            string sixel = DDD.SixelEncoder.Encode(framebuffer, palette);

            Assert.AreEqual("P0;0;8q\"1;1;5;1#0;2;0;0;0#1;2;4;8;12#0!4?@$#1!4@?-\\", sixel);
        }
    }
}
