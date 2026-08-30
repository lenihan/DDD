using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DDD_UnitTest
{
    [TestClass]
    public class Vertex
    {
        [TestMethod]
        public void ConstructorWithPositionOnlyLeavesNormalAndColorNull()
        {
            DDD.Vertex v = new DDD.Vertex(new DDD.Point(1, 2, 3));
            Assert.IsTrue(v.Position == new DDD.Point(1, 2, 3));
            Assert.IsTrue(v.Normal == null);
            Assert.IsTrue(v.Color == null);
        }
        [TestMethod]
        public void ConstructorWithPositionAndNormal()
        {
            DDD.Vertex v = new DDD.Vertex(new DDD.Point(1, 2, 3), new DDD.Vector(0, 1, 0));
            Assert.IsTrue(v.Normal == new DDD.Vector(0, 1, 0));
            Assert.IsTrue(v.Color == null);
        }
        [TestMethod]
        public void ConstructorWithPositionAndColor()
        {
            DDD.Vertex v = new DDD.Vertex(new DDD.Point(1, 2, 3), new DDD.Color(255, 0, 0));
            Assert.IsTrue(v.Normal == null);
            Assert.IsTrue(v.Color == new DDD.Color(255, 0, 0));
        }
        [TestMethod]
        public void ConstructorWithPositionNormalAndColor()
        {
            DDD.Vertex v = new DDD.Vertex(new DDD.Point(1, 2, 3), new DDD.Vector(0, 1, 0), new DDD.Color(255, 0, 0));
            Assert.IsTrue(v.Position == new DDD.Point(1, 2, 3));
            Assert.IsTrue(v.Normal == new DDD.Vector(0, 1, 0));
            Assert.IsTrue(v.Color == new DDD.Color(255, 0, 0));
        }
        [TestMethod]
        public void ConstructorWithVertex()
        {
            DDD.Vertex original = new DDD.Vertex(new DDD.Point(1, 2, 3), new DDD.Vector(0, 1, 0), new DDD.Color(255, 0, 0));
            DDD.Vertex copy = new DDD.Vertex(original);
            Assert.IsTrue(copy.Equals(original));
        }
        [TestMethod]
        public void TestEquals()
        {
            DDD.Vertex v1 = new DDD.Vertex(new DDD.Point(1, 2, 3), new DDD.Vector(0, 1, 0), new DDD.Color(255, 0, 0));
            DDD.Vertex v2 = new DDD.Vertex(new DDD.Point(1, 2, 3), new DDD.Vector(0, 1, 0), new DDD.Color(255, 0, 0));
            DDD.Vertex v3 = new DDD.Vertex(new DDD.Point(9, 9, 9));

            Assert.IsTrue(v1.Equals(v2));
            Assert.IsTrue(!v1.Equals(v3));
            Assert.IsTrue(!v1.Equals((object)123));

            Assert.IsTrue(v1 == v2);
            Assert.IsTrue(v1 != v3);
        }
    }
}
