using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DDD_UnitTest
{
    [TestClass]
    public class Framebuffer
    {
        [TestMethod]
        public void FillTriangleFillsARightTriangleInclusiveOfItsVertices()
        {
            var framebuffer = new DDD.Framebuffer(10, 10);
            framebuffer.FillTriangle(0, 0, 4, 0, 0, 4, 255, 0, 0);

            Assert.AreEqual(((byte)255, (byte)0, (byte)0), framebuffer.GetPixel(0, 0));
            Assert.AreEqual(((byte)255, (byte)0, (byte)0), framebuffer.GetPixel(4, 0));
            Assert.AreEqual(((byte)255, (byte)0, (byte)0), framebuffer.GetPixel(0, 4));
            Assert.AreEqual(((byte)255, (byte)0, (byte)0), framebuffer.GetPixel(1, 1));

            Assert.AreEqual(((byte)0, (byte)0, (byte)0), framebuffer.GetPixel(4, 4));
            Assert.AreEqual(((byte)0, (byte)0, (byte)0), framebuffer.GetPixel(9, 9));
        }

        [TestMethod]
        public void FillTriangleDrawsNothingForADegenerateZeroAreaTriangle()
        {
            var framebuffer = new DDD.Framebuffer(10, 10);
            framebuffer.FillTriangle(1, 1, 5, 1, 3, 1, 255, 0, 0);

            for (int y = 0; y < 10; y++)
            {
                for (int x = 0; x < 10; x++)
                {
                    Assert.AreEqual(((byte)0, (byte)0, (byte)0), framebuffer.GetPixel(x, y), $"Mismatch at ({x},{y})");
                }
            }
        }
    }
}
