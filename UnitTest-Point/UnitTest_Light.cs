using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DDD_UnitTest
{
    [TestClass]
    public class Light
    {
        [TestMethod]
        public void PositionConstructorMakesAPointLight()
        {
            DDD.Light light = new DDD.Light(new DDD.Point(1, 2, 3), 1.5);
            Assert.AreEqual(DDD.LightKind.Point, light.Kind);
            Assert.AreEqual(new DDD.Point(1, 2, 3), light.Position);
            Assert.AreEqual(1.5, light.Intensity);
        }

        [TestMethod]
        public void DirectionConstructorMakesADirectionalLightAndNormalizesTheDirection()
        {
            DDD.Light light = new DDD.Light(new DDD.Vector(0, 2, 0), 2.0);
            Assert.AreEqual(DDD.LightKind.Directional, light.Kind);
            Assert.AreEqual(new DDD.Vector(0, 1, 0), light.Direction);
            Assert.AreEqual(2.0, light.Intensity);
        }

        [TestMethod]
        public void IntensityDefaultsToOne()
        {
            DDD.Light light = new DDD.Light(new DDD.Vector(0, 0, 1));
            Assert.AreEqual(1.0, light.Intensity);
        }

        [TestMethod]
        public void TestEquals()
        {
            DDD.Light a = new DDD.Light(new DDD.Vector(0, 0, 1), 1.0);
            DDD.Light b = new DDD.Light(new DDD.Vector(0, 0, 1), 1.0);
            DDD.Light c = new DDD.Light(new DDD.Point(1, 0, 0), 1.0);

            Assert.IsTrue(a.Equals(b));
            Assert.IsTrue(!a.Equals(c));
            Assert.IsTrue(!a.Equals((object)123));

            Assert.IsTrue(a == b);
            Assert.IsTrue(a != c);
        }
    }
}
