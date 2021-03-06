using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DDD_UnitTest
{
    [TestClass]
    public class Point
    {
        [TestMethod]
        public void ConstructorNoArgs()
        {
            DDD.Point p = new DDD.Point();
            Assert.IsTrue(p.ToString() == "(0 0 0)\n");
        }
        [TestMethod]
        public void ConstructorWithXYZ()
        {
            DDD.Point p = new DDD.Point(4, 2, 1);
            Assert.IsTrue(p.ToString() == "(4 2 1)\n");
        }
        [TestMethod]
        public void ConstructorWithPoint()
        {
            DDD.Point p = new DDD.Point(new DDD.Point(1, 2, 3));
            Assert.IsTrue(p.ToString() == "(1 2 3)\n");
        }
        [TestMethod]
        public void ConstructorWithNullArray()
        {
            double[] arr = null;
            DDD.Point p = new DDD.Point(arr);
            Assert.IsTrue(p.ToString() == "(0 0 0)\n");
        }
        [TestMethod]
        public void ConstructorWithEmptyArray()
        {
            double[] arr = System.Array.Empty<double>();
            DDD.Point p = new DDD.Point(arr);
            Assert.IsTrue(p.ToString() == "(0 0 0)\n");
        }
        [TestMethod]
        public void ConstructorWith1ElementArray()
        {
            double[] arr = { 10 };
            DDD.Point p = new DDD.Point(arr);
            Assert.IsTrue(p.ToString() == "(10 0 0)\n");
        }
        [TestMethod]
        public void ConstructorWith2ElementArray()
        {
            double[] arr = { 10, 20 };
            DDD.Point p = new DDD.Point(arr);
            Assert.IsTrue(p.ToString() == "(10 20 0)\n");
        }
        [TestMethod]
        public void ConstructorWith3ElementArray()
        {
            double[] arr = { 30, 20, 10 };
            DDD.Point p = new DDD.Point(arr);
            Assert.IsTrue(p.ToString() == "(30 20 10)\n");
        }
        [TestMethod]
        public void ConstructorWith4ElementArray()
        {
            double[] arr = { 40, 30, 20, 10 };
            DDD.Point p = new DDD.Point(arr);
            Assert.IsTrue(p.ToString() == "(40 30 20)\n");
        }
        [TestMethod]
        public void ConstructorWithNullString()
        {
            string str = null;
            DDD.Point p = new DDD.Point(str);
            Assert.IsTrue(p.ToString() == "(0 0 0)\n");
        }
        [TestMethod]
        public void ConstructorWithEmptyString()
        {
            string str = "";
            DDD.Point p = new DDD.Point(str);
            Assert.IsTrue(p.ToString() == "(0 0 0)\n");
        }
        [TestMethod]
        public void ConstructorWith1ElementString()
        {
            string str = "1";
            DDD.Point p = new DDD.Point(str);
            Assert.IsTrue(p.ToString() == "(1 0 0)\n");
        }
        [TestMethod]
        public void ConstructorWith2ElementString()
        {
            string str = "1 10";
            DDD.Point p = new DDD.Point(str);
            Assert.IsTrue(p.ToString() == "(1 10 0)\n");
        }
        [TestMethod]
        public void ConstructorWith3ElementString()
        {
            string str = "1 2 3";
            DDD.Point p = new DDD.Point(str);
            Assert.IsTrue(p.ToString() == "(1 2 3)\n");
        }
        [TestMethod]
        public void ConstructorWith4ElementString()
        {
            string str = "3 2 1 9";
            DDD.Point p = new DDD.Point(str);
            Assert.IsTrue(p.ToString() == "(3 2 1)\n");
        }
        [TestMethod]
        public void ConstructorWithtStringSpaceDelimiters()
        {
            string str = "11 22 33";
            DDD.Point p = new DDD.Point(str);
            Assert.IsTrue(p.ToString() == "(11 22 33)\n");
        }
        [TestMethod]
        public void ConstructorWithtStringCommaDelimiters()
        {
            string str = "11,22,33";
            DDD.Point p = new DDD.Point(str);
            Assert.IsTrue(p.ToString() == "(11 22 33)\n");
        }
        [TestMethod]
        public void ConstructorWithtStringTabDelimiters()
        {
            string str = "11\t22\t33";
            DDD.Point p = new DDD.Point(str);
            Assert.IsTrue(p.ToString() == "(11 22 33)\n");
        }
        [TestMethod]
        public void ConstructorWithtStringMultipleDelimiters()
        {
            string str = "11\t\t ,22,,  \t33";
            DDD.Point p = new DDD.Point(str);
            Assert.IsTrue(p.ToString() == "(11 22 33)\n");
        }
        [TestMethod]
        public void ConstructorWithtStringTrimCharacters()
        {
            string str = "  <{([   11 22 33   ])}>  ";
            DDD.Point p = new DDD.Point(str);
            Assert.IsTrue(p.ToString() == "(11 22 33)\n");
        }
        
        [TestMethod]
        public void TestEquals()
        {
            DDD.Point? p1 = new DDD.Point(1, 2, 3);
            DDD.Point? p2 = null;
            DDD.Point? p3 = new DDD.Point(1, 2, 3);
            DDD.Point? p4 = new DDD.Point(4, 5, 6);
            DDD.Point? p5 = null;

            Assert.IsTrue(p1.Equals(p3));
            Assert.IsTrue(!p1.Equals(p4));
            
            Assert.IsTrue(!p1.Equals((object)p2));
            Assert.IsTrue(!p1.Equals((object)123));
            Assert.IsTrue(p1.Equals((object)p3));
            Assert.IsTrue(!p1.Equals((object)p4));

            Assert.IsTrue(p2 == p5);
            Assert.IsTrue(p1 == p3);
            
            Assert.IsTrue(p1 != p2);
            Assert.IsTrue(p1 != p4);
        }
        [TestMethod]
        public void TestGetHashCode()
        {
            DDD.Point p = new DDD.Point(2.3, -3.112, 4.123);
            int ans = 1370492436; // from previous run
            Assert.IsTrue(p.GetHashCode() == ans);
        }
        [TestMethod]
        public void TestToString()
        {
            DDD.Point p = new DDD.Point(123456789, 123456789.12345, 1000000000000);
            Assert.IsTrue(p.ToString() == "(123,456,789 123,456,789.12 1,000,000,000,000)\n");
        }
        [TestMethod]
        public void TestAdd()
        {
            DDD.Point p1 = new DDD.Point(1, 2, 3);
            DDD.Point p2 = new DDD.Point(3, 2, 1);
            Assert.IsTrue((p1 + p2).ToString() == "(4 4 4)\n");
            Assert.IsTrue(DDD.Point.Add(p1,p2).ToString() == "(4 4 4)\n");            
        }
        [TestMethod]
        public void TestSubtract()
        {
            DDD.Point p1 = new DDD.Point(1, 2, 3);
            DDD.Point p2 = new DDD.Point(3, 2, 1);
            Assert.IsTrue((p1 - p2).ToString() == "[-2 0 2]\n");
            Assert.IsTrue(DDD.Point.Subtract(p1, p2).ToString() == "[-2 0 2]\n");
        }
        [TestMethod]
        public void TestMultiplyPre()
        {
            DDD.Point p = new DDD.Point(1, 2, 3);
            Assert.IsTrue((p * 2).ToString() == "(2 4 6)\n");
            Assert.IsTrue(DDD.Point.Multiply(p, 2).ToString() == "(2 4 6)\n");
        }
        [TestMethod]
        public void TestMultiplyPost()
        {
            DDD.Point p = new DDD.Point(1, 2, 3);
            Assert.IsTrue((2 * p).ToString() == "(2 4 6)\n");
            Assert.IsTrue(DDD.Point.Multiply(2, p).ToString() == "(2 4 6)\n");
        }
        [TestMethod]
        public void TestDivide()
        {
            DDD.Point p = new DDD.Point(10, 20, 30);
            Assert.IsTrue((p / 10).ToString() == "(1 2 3)\n");
            Assert.IsTrue(DDD.Point.Divide(p, 10).ToString() == "(1 2 3)\n");
        }
    }
}
