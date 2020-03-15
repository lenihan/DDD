using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DDD_UnitTest
{
    [TestClass]
    public class Vector
    {
        [TestMethod]
        public void ConstructorNoArgs()
        {
            DDD.Vector v = new DDD.Vector();
            Assert.IsTrue(v.ToString() == "[0 0 0]\n");
        }
        [TestMethod]
        public void ConstructorWithXYZ()
        {
            DDD.Vector v = new DDD.Vector(4, 2, 1);
            Assert.IsTrue(v.ToString() == "[4 2 1]\n");
        }
        [TestMethod]
        public void ConstructorWithVector()
        {
            DDD.Vector v = new DDD.Vector(new DDD.Vector(1, 2, 3));
            Assert.IsTrue(v.ToString() == "[1 2 3]\n");
        }
        [TestMethod]
        public void ConstructorWithNullArray()
        {
            double[] arr = null;
            DDD.Vector v = new DDD.Vector(arr);
            Assert.IsTrue(v.ToString() == "[0 0 0]\n");
        }
        static public void ConstructorWithEmptyArray()
        {
            double[] arr = System.Array.Empty<double>();
            DDD.Vector v = new DDD.Vector(arr);
            Assert.IsTrue(v.ToString() == "[0 0 0]\n");
        }
        [TestMethod]
        public void ConstructorWith1ElementArray()
        {
            double[] arr = { 10 };
            DDD.Vector v = new DDD.Vector(arr);
            Assert.IsTrue(v.ToString() == "[10 0 0]\n");
        }
        [TestMethod]
        public void ConstructorWith2ElementArray()
        {
            double[] arr = { 10, 20 };
            DDD.Vector v = new DDD.Vector(arr);
            Assert.IsTrue(v.ToString() == "[10 20 0]\n");
        }
        [TestMethod]
        public void ConstructorWith3ElementArray()
        {
            double[] arr = { 30, 20, 10 };
            DDD.Vector v = new DDD.Vector(arr);
            Assert.IsTrue(v.ToString() == "[30 20 10]\n");
        }
        [TestMethod]
        public void ConstructorWith4ElementArray()
        {
            double[] arr = { 40, 30, 20, 10 };
            DDD.Vector v = new DDD.Vector(arr);
            Assert.IsTrue(v.ToString() == "[40 30 20]\n");
        }
        [TestMethod]
        public void ConstructorWithNullString()
        {
            string str = null;
            DDD.Vector v = new DDD.Vector(str);
            Assert.IsTrue(v.ToString() == "[0 0 0]\n");
        }
        [TestMethod]
        public void ConstructorWithEmptyString()
        {
            string str = "";
            DDD.Vector v = new DDD.Vector(str);
            Assert.IsTrue(v.ToString() == "[0 0 0]\n");
        }
        [TestMethod]
        public void ConstructorWith1ElementString()
        {
            string str = "1";
            DDD.Vector v = new DDD.Vector(str);
            Assert.IsTrue(v.ToString() == "[1 0 0]\n");
        }
        [TestMethod]
        public void ConstructorWith2ElementString()
        {
            string str = "1 10";
            DDD.Vector v = new DDD.Vector(str);
            Assert.IsTrue(v.ToString() == "[1 10 0]\n");
        }
        [TestMethod]
        public void ConstructorWith3ElementString()
        {
            string str = "1 2 3";
            DDD.Vector v = new DDD.Vector(str);
            Assert.IsTrue(v.ToString() == "[1 2 3]\n");
        }
        [TestMethod]
        public void ConstructorWith4ElementString()
        {
            string str = "3 2 1 9";
            DDD.Vector v = new DDD.Vector(str);
            Assert.IsTrue(v.ToString() == "[3 2 1]\n");
        }
        [TestMethod]
        public void ConstructorWithtStringSpaceDelimiters()
        {
            string str = "11 22 33";
            DDD.Vector v = new DDD.Vector(str);
            Assert.IsTrue(v.ToString() == "[11 22 33]\n");
        }
        [TestMethod]
        public void ConstructorWithtStringCommaDelimiters()
        {
            string str = "11,22,33";
            DDD.Vector v = new DDD.Vector(str);
            Assert.IsTrue(v.ToString() == "[11 22 33]\n");
        }
        [TestMethod]
        public void ConstructorWithtStringTabDelimiters()
        {
            string str = "11\t22\t33";
            DDD.Vector v = new DDD.Vector(str);
            Assert.IsTrue(v.ToString() == "[11 22 33]\n");
        }
        [TestMethod]
        public void ConstructorWithtStringMultipleDelimiters()
        {
            string str = "11\t\t ,22,,  \t33";
            DDD.Vector v = new DDD.Vector(str);
            Assert.IsTrue(v.ToString() == "[11 22 33]\n");
        }
        [TestMethod]
        public void ConstructorWithtStringTrimCharacters()
        {
            string str = "  <{([   11 22 33   ])}>  ";
            DDD.Vector v = new DDD.Vector(str);
            Assert.IsTrue(v.ToString() == "[11 22 33]\n");
        }
        [TestMethod]
        public void TestEquals()
        {
            DDD.Vector? v1 = new DDD.Vector(1, 2, 3);
            DDD.Vector? v2 = null;
            DDD.Vector? v3 = new DDD.Vector(1, 2, 3);
            DDD.Vector? v4 = new DDD.Vector(4, 5, 6);
            DDD.Vector? v5 = null;

            Assert.IsTrue(v1.Equals(v3));
            Assert.IsTrue(!v1.Equals(v4));

            Assert.IsTrue(!v1.Equals((object)v2));
            Assert.IsTrue(!v1.Equals((object)123));
            Assert.IsTrue(v1.Equals((object)v3));
            Assert.IsTrue(!v1.Equals((object)v4));

            Assert.IsTrue(v2 == v5);
            Assert.IsTrue(v1 == v3);

            Assert.IsTrue(v1 != v2);
            Assert.IsTrue(v1 != v4);
        }
        [TestMethod]
        public void TestGetHashCode()
        {
            DDD.Vector p = new DDD.Vector(2.3, -3.112, 4.123);
            int ans = 1370492436; // from previous run
            Assert.IsTrue(p.GetHashCode() == ans);
        }
        [TestMethod]
        public void TestToString()
        {
            DDD.Vector v = new DDD.Vector(123456789, 123456789.12345, 1000000000000);
            Assert.IsTrue(v.ToString() == "[123,456,789 123,456,789.12 1,000,000,000,000]\n");
        }
        [TestMethod]
        public void TestLength()
        {
            DDD.Vector v = new DDD.Vector(1, 2, 2);
            Assert.IsTrue(v.Length() == 3);
        }
        [TestMethod]
        public void TestNormalize()
        {
            DDD.Vector v = new DDD.Vector(1, 2, 3);
            var vn = v.Normalize();
            var str = vn.ToString();
            Assert.IsTrue(str == "[0.27 0.53 0.8]\n");
            Assert.IsTrue(v.ToString() == "[0.27 0.53 0.8]\n");
            Assert.IsTrue(v.X == 0.2672612419124244);
            Assert.IsTrue(v.Y == 0.53452248382484879);
            Assert.IsTrue(v.Z == 0.80178372573727319);
        }
        [TestMethod]
        public void TestNormalizeStatic()
        {

            DDD.Vector v = new DDD.Vector(1, 2, 3);
            Assert.IsTrue(DDD.Vector.Normalize(v).ToString() == "[0.27 0.53 0.8]\n");
        }
        [TestMethod]
        public void TestAddVectorVector()
        {
            DDD.Vector v1 = new DDD.Vector(1, 2, 3);
            DDD.Vector v2 = new DDD.Vector(6, -2, 8);
            Assert.IsTrue((v1 + v2).ToString() == "[7 0 11]\n");
            Assert.IsTrue(DDD.Vector.Add(v1, v2).ToString() == "[7 0 11]\n");
        }
        [TestMethod]
        public void TestAddVectorPoint()
        {
            DDD.Vector v = new DDD.Vector(1, 2, 3);
            DDD.Point p = new DDD.Point(1, 2, 3);
            Assert.IsTrue((v + p).ToString() == "(2 4 6)\n");
            Assert.IsTrue(DDD.Vector.Add(v, p).ToString() == "(2 4 6)\n");
        }
        [TestMethod]
        public void TestAddPointVector()
        {
            DDD.Vector v = new DDD.Vector(1, 2, 3);
            DDD.Point p = new DDD.Point(1, 2, 3);
            Assert.IsTrue((p + v).ToString() == "(2 4 6)\n");
            Assert.IsTrue(DDD.Vector.Add(p, v).ToString() == "(2 4 6)\n");
        }
        [TestMethod]
        public void TestSubtractVectorVector()
        {
            DDD.Vector v1 = new DDD.Vector(4, 5, 6);
            DDD.Vector v2 = new DDD.Vector(3, 2, 1);
            Assert.IsTrue((v1 - v2).ToString() == "[1 3 5]\n");
            Assert.IsTrue(DDD.Vector.Subtract(v1, v2).ToString() == "[1 3 5]\n");
        }
        [TestMethod]
        public void TestMultiplyVectorScaler()
        {
            DDD.Vector v = new DDD.Vector(1, 2, 3);
            Assert.IsTrue((v * 2).ToString() == "[2 4 6]\n");
            Assert.IsTrue(DDD.Vector.Multiply(v, 2).ToString() == "[2 4 6]\n");
        }
        [TestMethod]
        public void TestMultiplyScalerVector()
        {
            DDD.Vector v = new DDD.Vector(1, 2, 3);
            Assert.IsTrue((2 * v).ToString() == "[2 4 6]\n");
            Assert.IsTrue(DDD.Vector.Multiply(2, v).ToString() == "[2 4 6]\n");
        }
        [TestMethod]
        public void TestDivideVectorScaler()
        {
            DDD.Vector v = new DDD.Vector(1, 2, 3);
            Assert.IsTrue((v / 2).ToString() == "[0.5 1 1.5]\n");
            Assert.IsTrue(DDD.Vector.Divide(v, 2).ToString() == "[0.5 1 1.5]\n");
        }
        [TestMethod]
        public void TestNegate()
        {
            DDD.Vector v = new DDD.Vector(1, 2, 3);
            Assert.IsTrue((-v).ToString() == "[-1 -2 -3]\n");
            Assert.IsTrue(DDD.Vector.Negate(v).ToString() == "[-1 -2 -3]\n");
        }
        [TestMethod]
        public void TestDot()
        {
            DDD.Vector v1 = new DDD.Vector(1, 2, 3);
            DDD.Vector v2 = new DDD.Vector(10, 11, 12);
            // Verified by http://onlinemschool.com/math/assistance/vector/multiply/
            Assert.IsTrue(DDD.Vector.Dot(v1, v2) == 68);
        }
        [TestMethod]
        public void TestCross()
        {
            DDD.Vector v1 = new DDD.Vector(1, 2, 3);
            DDD.Vector v2 = new DDD.Vector(10, 11, 12);
            // Verified by https://www.symbolab.com/solver/Vector-cross-product-calculator
            Assert.IsTrue(DDD.Vector.Cross(v1, v2).ToString() == "[-9 18 -9]\n");
        }
    }
}
