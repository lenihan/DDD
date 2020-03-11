using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DDD_UnitTest
{
    [TestClass]
    public class Matrix
    {
        [TestMethod]
        public void ConstructorNoArgs()
        {
            DDD.Matrix m = new DDD.Matrix();
            string ans = "               0                0                0                0\n" +
                         "               0                0                0                0\n" +
                         "               0                0                0                0\n" +
                         "               0                0                0                0\n";
            Assert.IsTrue(m.ToString() == ans);
        }
        [TestMethod]
        public void ConstructorWithValues()
        {
            DDD.Matrix m = new DDD.Matrix(  0,  1,  2,  3,
                                            4,  5,  6,  7,
                                            8,  9, 10, 11,
                                           12, 13, 14, 15);
            string ans = "               0                1                2                3\n" +
                         "               4                5                6                7\n" +
                         "               8                9               10               11\n" +
                         "              12               13               14               15\n";
            Assert.IsTrue(m.ToString() == ans);
        }
        [TestMethod]
        public void ConstructorWithMatrix()
        {
            DDD.Matrix m1 = new DDD.Matrix( 0, 1,   2,  3,
                                            4, 5,   6,  7,
                                            8, 9,  10, 11,
                                           12, 13, 14, 15);
            DDD.Matrix m2 = new DDD.Matrix(m1);
            string ans = "               0                1                2                3\n" +
                         "               4                5                6                7\n" +
                         "               8                9               10               11\n" +
                         "              12               13               14               15\n";
            Assert.IsTrue(m2.ToString() == ans);
        }

        [TestMethod]
        public void ConstructorWithNullArray()
        {
            double[] arr = null;
            DDD.Matrix m = new DDD.Matrix(arr);
            string ans = "               0                0                0                0\n" +
                         "               0                0                0                0\n" +
                         "               0                0                0                0\n" +
                         "               0                0                0                0\n";
            Assert.IsTrue(m.ToString() == ans);
        }
        public void ConstructorWithEmptyArray()
        {
            double[] arr = { };
            DDD.Matrix m = new DDD.Matrix(arr);
            string ans = "               0                0                0                0\n" +
                         "               0                0                0                0\n" +
                         "               0                0                0                0\n" +
                         "               0                0                0                0\n";
            Assert.IsTrue(m.ToString() == ans);
        }
        [TestMethod]
        public void ConstructorWith3ElementArray()
        {
            double[] arr = { 1, 2, 3 };
            DDD.Matrix m = new DDD.Matrix(arr);
            string ans = "               0                0                0                0\n" +
                         "               0                0                0                0\n" +
                         "               0                0                0                0\n" +
                         "               0                0                0                0\n";
            Assert.IsTrue(m.ToString() == ans);
        }
        [TestMethod]
        public void ConstructorWith9ElementArray()
        {
            double[] arr = { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            DDD.Matrix m = new DDD.Matrix(arr);
            string ans = "               1                2                3                0\n" +
                         "               4                5                6                0\n" +
                         "               7                8                9                0\n" +
                         "               0                0                0                1\n";
            Assert.IsTrue(m.ToString() == ans);
        }
        [TestMethod]
        public void ConstructorWith16ElementArray()
        {
            double[] arr = { 16, 15, 14, 13, 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1 };
            DDD.Matrix m = new DDD.Matrix(arr);
            string ans = "              16               15               14               13\n" +
                         "              12               11               10                9\n" +
                         "               8                7                6                5\n" +
                         "               4                3                2                1\n";
            Assert.IsTrue(m.ToString() == ans);
        }
        [TestMethod]
        public void ConstructorWith17ElementArray()
        {
            double[] arr = { 17, 16, 15, 14, 13, 12, 11, 10, 9, 8, 7, 6, 5, 4, 3, 2, 1 };
            DDD.Matrix m = new DDD.Matrix(arr);
            string ans = "               0                0                0                0\n" +
                         "               0                0                0                0\n" +
                         "               0                0                0                0\n" +
                         "               0                0                0                0\n";
            Assert.IsTrue(m.ToString() == ans);
        }
        [TestMethod]
        public void ConstructorWith3Vectors()
        {
            DDD.Vector vx = new DDD.Vector(1, 2, 3);
            DDD.Vector vy = new DDD.Vector(4, 5, 6);
            DDD.Vector vz = new DDD.Vector(7, 8, 9);
            DDD.Matrix m = new DDD.Matrix(vx, vy, vz);
            string ans = "               1                4                7                0\n" +
                         "               2                5                8                0\n" +
                         "               3                6                9                0\n" +
                         "               0                0                0                1\n";
            Assert.IsTrue(m.ToString() == ans);
        }
        [TestMethod]
        public void TestEquals()
        {
            DDD.Matrix? m1 = DDD.Matrix.Identity();
            DDD.Matrix? m2 = null;
            DDD.Matrix? m3 = DDD.Matrix.Identity();
            DDD.Matrix? m4 = DDD.Matrix.Zero();
            DDD.Matrix? m5 = null;

            Assert.IsTrue(m1.Equals(m3));
            Assert.IsTrue(!m1.Equals(m4));

            Assert.IsTrue(!m1.Equals((object)m2));
            Assert.IsTrue(!m1.Equals((object)123));
            Assert.IsTrue(m1.Equals((object)m3));
            Assert.IsTrue(!m1.Equals((object)m4));

            Assert.IsTrue(m2 == m5);
            Assert.IsTrue(m1 == m3);

            Assert.IsTrue(m1 != m2);
            Assert.IsTrue(m1 != m4);
        }
        [TestMethod]
        public void TestGetHashCode()
        {
            DDD.Matrix m = new DDD.Matrix(1, 2, 3, 4,
                                          5, 6, 7, 8,
                                          9, 10, 11, 12,
                                         13, 14, 15, 16);
            int ans = 16; // 1 ^ 2 ^ 3 ^ 4 ^ 5 ^ 6 ^ 7 ^ 8 ^ 9 ^ 10 ^ 11 ^ 12 ^ 13 ^ 14 ^ 15 ^ 16;
            Assert.IsTrue(m.GetHashCode() == ans);
        }
        [TestMethod]
        public void TestToString()
        {
            DDD.Matrix m = new DDD.Matrix((double)1/3,    (double)-1/3, (double)+1/3, 123.456789,
                                          (double)1/2,    (double)-1/2, (double)+1/2,          8,
                                                  0.0,            -0.0,         +0.0, 1234567890,
                                         -.1234567890, -00001234567890,      000.000,      -01.0);
            string ans = "            0.33            -0.33             0.33           123.46\n" +
                         "             0.5             -0.5              0.5                8\n" +
                         "               0               -0                0    1,234,567,890\n" +
                         "           -0.12   -1,234,567,890                0               -1\n";
            Assert.IsTrue(m.ToString() == ans);
        }
        [TestMethod]
        public void TranslateXYZ()
        {
            DDD.Matrix m = DDD.Matrix.Translate(1, 2, 3);
            string ans = "               1                0                0                1\n" +
                         "               0                1                0                2\n" +
                         "               0                0                1                3\n" +
                         "               0                0                0                1\n";
            Assert.IsTrue(m.ToString() == ans);
        }
        [TestMethod]
        public void TranslateVector()
        {
            DDD.Vector v = new DDD.Vector(10, 20, 30);
            DDD.Matrix m = DDD.Matrix.Translate(v);
            string ans = "               1                0                0               10\n" +
                         "               0                1                0               20\n" +
                         "               0                0                1               30\n" +
                         "               0                0                0                1\n";
            Assert.IsTrue(m.ToString() == ans);
        }
        [TestMethod]
        public void ScaleXYZ()
        {
            DDD.Matrix m = DDD.Matrix.Scale(2, 4, 8);
            string ans = "               2                0                0                0\n" +
                         "               0                4                0                0\n" +
                         "               0                0                8                0\n" +
                         "               0                0                0                1\n";
            Assert.IsTrue(m.ToString() == ans);
        }
        [TestMethod]
        public void RotateX()
        {
            DDD.Matrix m = DDD.Matrix.RotateX(90);
            string ans = "               1                0                0                0\n" +
                         "               0                0               -1                0\n" +
                         "               0                1                0                0\n" +
                         "               0                0                0                1\n";
            Assert.IsTrue(m.ToString() == ans);
        }
        [TestMethod]
        public void RotateY()
        {
            DDD.Matrix m = DDD.Matrix.RotateY(90);
            string ans = "               0                0                1                0\n" +
                         "               0                1                0                0\n" +
                         "              -1                0                0                0\n" +
                         "               0                0                0                1\n";
            Assert.IsTrue(m.ToString() == ans);
        }
        [TestMethod]
        public void RotateZ()
        {
            DDD.Matrix m = DDD.Matrix.RotateZ(90);
            string ans = "               0               -1                0                0\n" +
                         "               1                0                0                0\n" +
                         "               0                0                1                0\n" +
                         "               0                0                0                1\n";
            Assert.IsTrue(m.ToString() == ans);
        }
        [TestMethod]
        public void Determinate()
        {
            DDD.Matrix m = new DDD.Matrix( 1,  2,  3,  4,
                                           5,  6,  7,  8,
                                           9, 10, 11, 12,
                                          13, 14, 15, 16);
            Assert.IsTrue(m.Determinate() == 0);
        }
        [TestMethod]
        public void InvertBad()
        {
            DDD.Matrix m = new DDD.Matrix(7, 6, 5, 4,
                                          1, 2, 3, 4,
                                          9, 8, 7, 6,
                                          2, 4, 6, 8);
            m.Invert();   // determinate is zero (ie not invertable), divide by zero gives NaN
            string ans = "             NaN              NaN              NaN              NaN\n" +
                         "             NaN              NaN              NaN              NaN\n" +
                         "             NaN              NaN              NaN              NaN\n" +
                         "             NaN              NaN              NaN              NaN\n";
            Assert.IsTrue(m.ToString() == ans);
        }
        [TestMethod]
        public void InvertGood()
        {
            // Note: Invert() tests both Adjugate() and Transpose()
            DDD.Matrix m = new DDD.Matrix(7, 3, 5, 4,
                                          3, 2, 3, 1,
                                          9, 3, 7, 6,
                                          1, 4, 6, 8);
            m.Invert();   // Verified numbers are correct via http://matrix.reshish.com/inverse.php
            Assert.IsTrue(m.M11 == 0.34090909090909094);
            Assert.IsTrue(m.M12 == -0.22727272727272729);
            Assert.IsTrue(m.M13 == -0.068181818181818177);
            Assert.IsTrue(m.M14 == -0.090909090909090912);
            Assert.IsTrue(m.M21 == 1.0340909090909092);
            Assert.IsTrue(m.M22 == -0.022727272727272728);
            Assert.IsTrue(m.M23 == -0.80681818181818188);
            Assert.IsTrue(m.M24 == 0.090909090909090912);
            Assert.IsTrue(m.M31 == -1.125);
            Assert.IsTrue(m.M32 == 0.75);
            Assert.IsTrue(m.M33 == 0.625);
            Assert.IsTrue(m.M34 == 0);
            Assert.IsTrue(m.M41 == 0.28409090909090912);
            Assert.IsTrue(m.M42 == -0.52272727272727271);
            Assert.IsTrue(m.M43 == -0.056818181818181823);
            Assert.IsTrue(m.M44 == 0.090909090909090912);
        }
        [TestMethod]
        public void MultiplyMatrixByMatrix()
        {
            DDD.Matrix m1 = new DDD.Matrix( 1,  2,  3,  4,
                                            5,  6,  7,  8,
                                            9, 10, 11, 12,
                                           13, 14, 15, 16);
            DDD.Matrix m2 = new DDD.Matrix(  3, 11,  -3,   2,
                                            -2,  2,  99,  18,
                                            59, 17, 121, 112,
                                             3,  4,  -5, 106);
            // answer verified by http://matrix.reshish.com/multCalculation.php
            string ans = "             188               82              538              798\n" +
                         "             440              218            1,386            1,750\n" +
                         "             692              354            2,234            2,702\n" +
                         "             944              490            3,082            3,654\n";
            Assert.IsTrue((m1 * m2).ToString() == ans);
            Assert.IsTrue(DDD.Matrix.Multiply(m1, m2).ToString() == ans);
        }
        [TestMethod]
        public void MultiplyMatrixByScaler()
        {
            DDD.Matrix m = DDD.Matrix.Identity();
            string ans = "               3                0                0                0\n" +
                         "               0                3                0                0\n" +
                         "               0                0                3                0\n" +
                         "               0                0                0                3\n";
            Assert.IsTrue((m * 3.0).ToString() == ans);
            Assert.IsTrue(DDD.Matrix.Multiply(m, 3.0).ToString() == ans);
        }
        [TestMethod]
        public void MultiplyScalerByMatrix()
        {
            DDD.Matrix m = DDD.Matrix.Identity();
            string ans = "               3                0                0                0\n" +
                         "               0                3                0                0\n" +
                         "               0                0                3                0\n" +
                         "               0                0                0                3\n";
            Assert.IsTrue((3.0 * m).ToString() == ans);
            Assert.IsTrue(DDD.Matrix.Multiply(3.0, m).ToString() == ans);
        }
        [TestMethod]
        public void MultiplyMatrixByVector()
        {
            DDD.Matrix m = DDD.Matrix.Identity();
            DDD.Vector v = new DDD.Vector(1, 2, 3);
            string ans = "[1 2 3]\n";
            Assert.IsTrue((m * v).ToString() == ans);
            Assert.IsTrue(DDD.Matrix.Multiply(m, v).ToString() == ans);
        }
        [TestMethod]
        public void MultiplyMatrixByPoint()
        {
            DDD.Matrix m = DDD.Matrix.Identity();
            DDD.Point p = new DDD.Point(3, 2, 1);
            string ans = "(3 2 1)\n";
            Assert.IsTrue((m * p).ToString() == ans);
            Assert.IsTrue(DDD.Matrix.Multiply(m, p).ToString() == ans);
        }
        [TestMethod]
        public void AddMatrixToMatrix()
        {
            var m1 = new DDD.Matrix( 1,  2,  3,  4,
                                     5,  6,  7,  8,
                                     9, 10, 11, 12,
                                    13, 14, 15, 16);
            var m2 = new DDD.Matrix(2,  4,   6,  8,
                                    10, 12, 14, 16,
                                    18, 20, 22, 24,
                                    26, 28, 30, 32);
            string ans = "               3                6                9               12\n" +
                         "              15               18               21               24\n" +
                         "              27               30               33               36\n" +
                         "              39               42               45               48\n";
            Assert.IsTrue((m1 + m2).ToString() == ans);
            Assert.IsTrue(DDD.Matrix.Add(m1, m2).ToString() == ans);
        }
        [TestMethod]
        public void Zero()
        {
            string ans = "               0                0                0                0\n" +
                         "               0                0                0                0\n" +
                         "               0                0                0                0\n" +
                         "               0                0                0                0\n";
            Assert.IsTrue(DDD.Matrix.Zero().ToString() == ans);
        }
        [TestMethod]
        public void Identity()
        {
            string ans = "               1                0                0                0\n" +
                         "               0                1                0                0\n" +
                         "               0                0                1                0\n" +
                         "               0                0                0                1\n";
            Assert.IsTrue(DDD.Matrix.Identity().ToString() == ans);
        }
    }
}
