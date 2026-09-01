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
            framebuffer.FillTriangle(0, 0, 0.0, 4, 0, 0.0, 0, 4, 0.0, 255, 0, 0);

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
            framebuffer.FillTriangle(1, 1, 0.0, 5, 1, 0.0, 3, 1, 0.0, 255, 0, 0);

            for (int y = 0; y < 10; y++)
            {
                for (int x = 0; x < 10; x++)
                {
                    Assert.AreEqual(((byte)0, (byte)0, (byte)0), framebuffer.GetPixel(x, y), $"Mismatch at ({x},{y})");
                }
            }
        }

        [TestMethod]
        public void FillTriangleKeepsTheCloserFaceWhenTheFartherOneIsDrawnAfter()
        {
            var framebuffer = new DDD.Framebuffer(10, 10);
            framebuffer.FillTriangle(0, 0, 5.0, 9, 0, 5.0, 0, 9, 5.0, 0, 0, 255);    // far, drawn first
            framebuffer.FillTriangle(0, 0, 1.0, 9, 0, 1.0, 0, 9, 1.0, 255, 0, 0);    // near, drawn second

            Assert.AreEqual(((byte)255, (byte)0, (byte)0), framebuffer.GetPixel(1, 1));
        }

        [TestMethod]
        public void FillTriangleKeepsTheCloserFaceWhenItWasDrawnFirst()
        {
            var framebuffer = new DDD.Framebuffer(10, 10);
            framebuffer.FillTriangle(0, 0, 1.0, 9, 0, 1.0, 0, 9, 1.0, 255, 0, 0);    // near, drawn first
            framebuffer.FillTriangle(0, 0, 5.0, 9, 0, 5.0, 0, 9, 5.0, 0, 0, 255);    // far, drawn second

            // Draw order alone must not decide the winner - only depth does.
            Assert.AreEqual(((byte)255, (byte)0, (byte)0), framebuffer.GetPixel(1, 1));
        }

        [TestMethod]
        public void ClearResetsTheDepthBufferSoAFartherFillCanSucceedAgain()
        {
            var framebuffer = new DDD.Framebuffer(10, 10);
            framebuffer.FillTriangle(0, 0, 1.0, 9, 0, 1.0, 0, 9, 1.0, 255, 0, 0);
            framebuffer.Clear(0, 0, 0);
            framebuffer.FillTriangle(0, 0, 5.0, 9, 0, 5.0, 0, 9, 5.0, 0, 0, 255);

            Assert.AreEqual(((byte)0, (byte)0, (byte)255), framebuffer.GetPixel(1, 1));
        }
    }
}
