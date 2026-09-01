using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DDD_UnitTest
{
    [TestClass]
    public class Material
    {
        [TestMethod]
        public void ConstructorDefaultsMatchTheOldFixedHeadlampLook()
        {
            // Ambient + Diffuse must sum to 1.0: a face lit dead-on by the default headlamp
            // should still render at exactly full brightness, same as before Material existed.
            DDD.Material material = new DDD.Material(new DDD.Color(200, 200, 200));
            Assert.AreEqual(0.2, material.Ambient);
            Assert.AreEqual(0.8, material.Diffuse);
            Assert.AreEqual(1.0, material.Ambient + material.Diffuse);
            Assert.AreEqual(0.0, material.Specular);
        }

        [TestMethod]
        public void TestEquals()
        {
            DDD.Material a = new DDD.Material(new DDD.Color(1, 2, 3), 0.1, 0.5, 0.2, 8.0);
            DDD.Material b = new DDD.Material(new DDD.Color(1, 2, 3), 0.1, 0.5, 0.2, 8.0);
            DDD.Material c = new DDD.Material(new DDD.Color(9, 9, 9), 0.1, 0.5, 0.2, 8.0);

            Assert.IsTrue(a.Equals(b));
            Assert.IsTrue(!a.Equals(c));
            Assert.IsTrue(!a.Equals((object)123));

            Assert.IsTrue(a == b);
            Assert.IsTrue(a != c);
        }
    }
}
