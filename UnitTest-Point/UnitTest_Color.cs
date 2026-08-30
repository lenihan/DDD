using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DDD_UnitTest
{
    [TestClass]
    public class Color
    {
        [TestMethod]
        public void ConstructorWithRGB()
        {
            DDD.Color c = new DDD.Color(255, 128, 0);
            Assert.IsTrue(c.ToString() == "#FF8000\n");
        }
        [TestMethod]
        public void ConstructorWithColor()
        {
            DDD.Color c = new DDD.Color(new DDD.Color(10, 20, 30));
            Assert.IsTrue(c.ToString() == "#0A141E\n");
        }
        [TestMethod]
        public void TestEquals()
        {
            DDD.Color c1 = new DDD.Color(1, 2, 3);
            DDD.Color c2 = new DDD.Color(1, 2, 3);
            DDD.Color c3 = new DDD.Color(4, 5, 6);

            Assert.IsTrue(c1.Equals(c2));
            Assert.IsTrue(!c1.Equals(c3));
            Assert.IsTrue(!c1.Equals((object)123));
            Assert.IsTrue(c1.Equals((object)c2));

            Assert.IsTrue(c1 == c2);
            Assert.IsTrue(c1 != c3);
        }
        [TestMethod]
        public void TestGetHashCode()
        {
            DDD.Color c1 = new DDD.Color(1, 2, 3);
            DDD.Color c2 = new DDD.Color(1, 2, 3);
            Assert.IsTrue(c1.GetHashCode() == c2.GetHashCode());
        }
    }
}
