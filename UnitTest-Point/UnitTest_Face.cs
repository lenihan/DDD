using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DDD_UnitTest
{
    [TestClass]
    public class Face
    {
        [TestMethod]
        public void ConstructorWithABC()
        {
            DDD.Face f = new DDD.Face(0, 1, 2);
            Assert.IsTrue(f.ToString() == "Face: (0 1 2)\n");
        }
        [TestMethod]
        public void ConstructorWithFace()
        {
            DDD.Face f = new DDD.Face(new DDD.Face(3, 4, 5));
            Assert.IsTrue(f.ToString() == "Face: (3 4 5)\n");
        }
        [TestMethod]
        public void TestEquals()
        {
            DDD.Face f1 = new DDD.Face(0, 1, 2);
            DDD.Face f2 = new DDD.Face(0, 1, 2);
            DDD.Face f3 = new DDD.Face(2, 1, 0);

            Assert.IsTrue(f1.Equals(f2));
            Assert.IsTrue(!f1.Equals(f3));
            Assert.IsTrue(!f1.Equals((object)123));

            Assert.IsTrue(f1 == f2);
            Assert.IsTrue(f1 != f3);
        }
        [TestMethod]
        public void TestGetHashCode()
        {
            DDD.Face f1 = new DDD.Face(0, 1, 2);
            DDD.Face f2 = new DDD.Face(0, 1, 2);
            Assert.IsTrue(f1.GetHashCode() == f2.GetHashCode());
        }
    }
}
