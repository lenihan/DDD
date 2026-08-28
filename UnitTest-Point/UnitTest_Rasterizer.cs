using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DDD_UnitTest
{
    [TestClass]
    public class Rasterizer
    {
        [TestMethod]
        public void ProjectsAPointOnThePositiveXAxisToTheRightOfCenter()
        {
            var objects = new List<object> { new DDD.Point(1, 0, 0) };
            DDD.Point min = new DDD.Point(-1, -1, -1);
            DDD.Point max = new DDD.Point(1, 1, 1);

            // No rotation, 100x100 framebuffer: center is (50,50).
            // radius = distance from bbox center to bbox max = sqrt(3).
            // scale = min(width,height) * 0.38 / radius.
            // projected x = round(50 + 1 * scale) = 72, y stays 50.
            DDD.Framebuffer framebuffer = DDD.Rasterizer.Render(objects, min, max, 0.0, 0.0, 100, 100);

            var pixel = framebuffer.GetPixel(72, 50);
            Assert.AreEqual(((byte)240, (byte)240, (byte)240), pixel);
        }
    }
}
