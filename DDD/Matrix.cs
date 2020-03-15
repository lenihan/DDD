using System;

namespace DDD
{
    [Serializable()]
    public struct Matrix : IEquatable<Matrix>
    {
        // Storing Matrix in row major: mXY. For example, m23 is the second row, third column
        public double M11, M12, M13, M14;
        public double M21, M22, M23, M24;
        public double M31, M32, M33, M34;
        public double M41, M42, M43, M44;

        // API inspired by DirectX 9.0: https://msdn.microsoft.com/en-us/library/windows/desktop/bb281696(v=vs.85).aspx
        public Matrix(double m11, double m12, double m13, double m14,
                      double m21, double m22, double m23, double m24,
                      double m31, double m32, double m33, double m34,
                      double m41, double m42, double m43, double m44)
        {
            M11 = m11; M12 = m12; M13 = m13; M14 = m14;
            M21 = m21; M22 = m22; M23 = m23; M24 = m24;
            M31 = m31; M32 = m32; M33 = m33; M34 = m34;
            M41 = m41; M42 = m42; M43 = m43; M44 = m44;
        }
        public Matrix(double m11, double m12, double m13,
                      double m21, double m22, double m23,
                      double m31, double m32, double m33)
        {
            M11 = m11; M12 = m12; M13 = m13; M14 = 0;
            M21 = m21; M22 = m22; M23 = m23; M24 = 0;
            M31 = m31; M32 = m32; M33 = m33; M34 = 0;
            M41 = 0;   M42 = 0;   M43 = 0;   M44 = 1;
        }
        public Matrix(Matrix mat)
        {
            M11 = mat.M11; M12 = mat.M12; M13 = mat.M13; M14 = mat.M14;
            M21 = mat.M21; M22 = mat.M22; M23 = mat.M23; M24 = mat.M24;
            M31 = mat.M31; M32 = mat.M32; M33 = mat.M33; M34 = mat.M34;
            M41 = mat.M41; M42 = mat.M42; M43 = mat.M43; M44 = mat.M44;
        }
        public Matrix(double[] arr)
        {
            M11 = 0; M12 = 0; M13 = 0; M14 = 0;
            M21 = 0; M22 = 0; M23 = 0; M24 = 0;
            M31 = 0; M32 = 0; M33 = 0; M34 = 0;
            M41 = 0; M42 = 0; M43 = 0; M44 = 0;
            if (arr == null)
            {
                return;
            }
            else if (arr.Length == 16)
            {
                M11 = arr[0];  M12 = arr[1];  M13 = arr[2];  M14 = arr[3];
                M21 = arr[4];  M22 = arr[5];  M23 = arr[6];  M24 = arr[7];
                M31 = arr[8];  M32 = arr[9];  M33 = arr[10]; M34 = arr[11];
                M41 = arr[12]; M42 = arr[13]; M43 = arr[14]; M44 = arr[15];
            }
            else if (arr.Length == 9)
            {
                M11 = arr[0]; M12 = arr[1]; M13 = arr[2]; M14 = 0;
                M21 = arr[3]; M22 = arr[4]; M23 = arr[5]; M24 = 0;
                M31 = arr[6]; M32 = arr[7]; M33 = arr[8]; M34 = 0;
                M41 = 0; M42 = 0; M43 = 0; M44 = 1;
            }
        }
        public Matrix(Vector xAxis, Vector yAxis, Vector zAxis)
        {
            M11 = xAxis.X; M12 = yAxis.X; M13 = zAxis.X; M14 = 0;
            M21 = xAxis.Y; M22 = yAxis.Y; M23 = zAxis.Y; M24 = 0;
            M31 = xAxis.Z; M32 = yAxis.Z; M33 = zAxis.Z; M34 = 0;
            M41 = 0; M42 = 0; M43 = 0; M44 = 1;
        }
        public bool Equals(Matrix m) => m == null ? false : (M11 == m.M11) && (M12 == m.M12) && (M13 == m.M13) && (M14 == m.M14) &&
                                                            (M21 == m.M21) && (M22 == m.M22) && (M23 == m.M23) && (M24 == m.M24) &&
                                                            (M31 == m.M31) && (M32 == m.M32) && (M33 == m.M33) && (M34 == m.M34) &&
                                                            (M41 == m.M41) && (M42 == m.M42) && (M43 == m.M43) && (M44 == m.M44);
        public override bool Equals(object obj)
        {
            if ((obj is null) || !GetType().Equals(obj.GetType()))
            {
                return false;
            }
            else
            {
                return Equals((Matrix)obj);
            }
        }
        public override int GetHashCode()
        {
            int row1 = M11.GetHashCode() ^ M12.GetHashCode() ^ M13.GetHashCode() ^ M14.GetHashCode();
            int row2 = M21.GetHashCode() ^ M22.GetHashCode() ^ M23.GetHashCode() ^ M24.GetHashCode();
            int row3 = M31.GetHashCode() ^ M32.GetHashCode() ^ M33.GetHashCode() ^ M34.GetHashCode();
            int row4 = M41.GetHashCode() ^ M42.GetHashCode() ^ M43.GetHashCode() ^ M44.GetHashCode();
            return row1 ^ row2 ^ row3 ^ row4;
        }

        public override string ToString()
        {
            // https://ss64.com/ps/syntax-f-operator.html
            // {0,16:#,0.##}
            //   0:    Index
            //   ,16   Use at least 16 characters, right justified. Chose 16 to fit 1,234,567,890.12
            //   #,0   Group integers in 3's with commas, do not hide zero
            //   .##   Show at most 2 decimals, or nothing if no decimal point
            string row1 = String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0,16:#,0.##} {1,16:#,0.##} {2,16:#,0.##} {3,16:#,0.##}\n", M11, M12, M13, M14);
            string row2 = String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0,16:#,0.##} {1,16:#,0.##} {2,16:#,0.##} {3,16:#,0.##}\n", M21, M22, M23, M24);
            string row3 = String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0,16:#,0.##} {1,16:#,0.##} {2,16:#,0.##} {3,16:#,0.##}\n", M31, M32, M33, M34);
            string row4 = String.Format(System.Globalization.CultureInfo.InvariantCulture, "{0,16:#,0.##} {1,16:#,0.##} {2,16:#,0.##} {3,16:#,0.##}\n", M41, M42, M43, M44);
            return row1 + row2 + row3 + row4;
        }
        public static bool operator ==(Matrix? a, Matrix? b) => a is null ? b is null : a.Equals(b);
        public static bool operator !=(Matrix? a, Matrix? b) => !(a == b);
        public double Determinate() => Determinate4x4(M11, M12, M13, M14,
                                                      M21, M22, M23, M24,
                                                      M31, M32, M33, M34,
                                                      M41, M42, M43, M44);
        public Matrix Invert()
        {
            // https://en.wikipedia.org/wiki/Invertible_matrix
            double det = Determinate();
            Matrix adj = Adjugate();
            Matrix inv = (1 / det) * adj;
            M11 = inv.M11; M12 = inv.M12; M13 = inv.M13; M14 = inv.M14;
            M21 = inv.M21; M22 = inv.M22; M23 = inv.M23; M24 = inv.M24;
            M31 = inv.M31; M32 = inv.M32; M33 = inv.M33; M34 = inv.M34;
            M41 = inv.M41; M42 = inv.M42; M43 = inv.M43; M44 = inv.M44;
            return this;
        }
        public Matrix Transpose()
        {
            double a = M11; double b = M12; double c = M13; double d = M14;
            double e = M21; double f = M22; double g = M23; double h = M24;
            double i = M31; double j = M32; double k = M33; double l = M34;
            double m = M41; double n = M42; double o = M43; double p = M44;

            M11 = a; M12 = e; M13 = i; M14 = m;
            M21 = b; M22 = f; M23 = j; M24 = n;
            M31 = c; M32 = g; M33 = k; M34 = o;
            M41 = d; M42 = h; M43 = l; M44 = p;
            return this;
        }
        public Matrix Adjugate()
        {
            // https://en.wikipedia.org/wiki/Adjugate_matrix
            double a = M11; double b = M12; double c = M13; double d = M14;
            double e = M21; double f = M22; double g = M23; double h = M24;
            double i = M31; double j = M32; double k = M33; double l = M34;
            double m = M41; double n = M42; double o = M43; double p = M44;

            double detA = Determinant3x3(f, g, h,
                                         j, k, l,
                                         n, o, p);
            double detB = Determinant3x3(e, g, h,
                                         i, k, l,
                                         m, o, p);
            double detC = Determinant3x3(e, f, h,
                                         i, j, l,
                                         m, n, p);
            double detD = Determinant3x3(e, f, g,
                                         i, j, k,
                                         m, n, o);
            double detE = Determinant3x3(b, c, d,
                                         j, k, l,
                                         n, o, p);
            double detF = Determinant3x3(a, c, d,
                                         i, k, l,
                                         m, o, p);
            double detG = Determinant3x3(a, b, d,
                                         i, j, l,
                                         m, n, p);
            double detH = Determinant3x3(a, b, c,
                                         i, j, k,
                                         m, n, o);
            double detI = Determinant3x3(b, c, d,
                                         f, g, h,
                                         n, o, p);
            double detJ = Determinant3x3(a, c, d,
                                         e, g, h,
                                         m, o, p);
            double detK = Determinant3x3(a, b, d,
                                         e, f, h,
                                         m, n, p);
            double detL = Determinant3x3(a, b, c,
                                         e, f, g,
                                         m, n, o);
            double detM = Determinant3x3(b, c, d,
                                         f, g, h,
                                         j, k, l);
            double detN = Determinant3x3(a, c, d,
                                         e, g, h,
                                         i, k, l);
            double detO = Determinant3x3(a, b, d,
                                         e, f, h,
                                         i, j, l);
            double detP = Determinant3x3(a, b, c,
                                         e, f, g,
                                         i, j, k);
            Matrix mat = new Matrix(+detA, -detB, +detC, -detD,
                                    -detE, +detF, -detG, +detH,
                                    +detI, -detJ, +detK, -detL,
                                    -detM, +detN, -detO, +detP);
            return mat.Transpose();
        }
        public static Matrix operator *(Matrix a, Matrix b)
        {
            // https://en.wikipedia.org/wiki/Matrix_multiplication#Definition
            // row 1
            double c11 = a.M11 * b.M11 + a.M12 * b.M21 + a.M13 * b.M31 + a.M14 * b.M41;
            double c12 = a.M11 * b.M12 + a.M12 * b.M22 + a.M13 * b.M32 + a.M14 * b.M42;
            double c13 = a.M11 * b.M13 + a.M12 * b.M23 + a.M13 * b.M33 + a.M14 * b.M43;
            double c14 = a.M11 * b.M14 + a.M12 * b.M24 + a.M13 * b.M34 + a.M14 * b.M44;

            // row 2                                                                 
            double c21 = a.M21 * b.M11 + a.M22 * b.M21 + a.M23 * b.M31 + a.M24 * b.M41;
            double c22 = a.M21 * b.M12 + a.M22 * b.M22 + a.M23 * b.M32 + a.M24 * b.M42;
            double c23 = a.M21 * b.M13 + a.M22 * b.M23 + a.M23 * b.M33 + a.M24 * b.M43;
            double c24 = a.M21 * b.M14 + a.M22 * b.M24 + a.M23 * b.M34 + a.M24 * b.M44;

            // row 3                                                                 
            double c31 = a.M31 * b.M11 + a.M32 * b.M21 + a.M33 * b.M31 + a.M34 * b.M41;
            double c32 = a.M31 * b.M12 + a.M32 * b.M22 + a.M33 * b.M32 + a.M34 * b.M42;
            double c33 = a.M31 * b.M13 + a.M32 * b.M23 + a.M33 * b.M33 + a.M34 * b.M43;
            double c34 = a.M31 * b.M14 + a.M32 * b.M24 + a.M33 * b.M34 + a.M34 * b.M44;

            // row 4                                                                 
            double c41 = a.M41 * b.M11 + a.M42 * b.M21 + a.M43 * b.M31 + a.M44 * b.M41;
            double c42 = a.M41 * b.M12 + a.M42 * b.M22 + a.M43 * b.M32 + a.M44 * b.M42;
            double c43 = a.M41 * b.M13 + a.M42 * b.M23 + a.M43 * b.M33 + a.M44 * b.M43;
            double c44 = a.M41 * b.M14 + a.M42 * b.M24 + a.M43 * b.M34 + a.M44 * b.M44;

            return new Matrix(c11, c12, c13, c14,
                              c21, c22, c23, c24,
                              c31, c32, c33, c34,
                              c41, c42, c43, c44);
        }
        public static Matrix Multiply(Matrix a, Matrix b) => a * b;

        public static Matrix operator *(Matrix m, double s)
        {
            double n11 = m.M11 * s; double n12 = m.M12 * s; double n13 = m.M13 * s; double n14 = m.M14 * s;
            double n21 = m.M21 * s; double n22 = m.M22 * s; double n23 = m.M23 * s; double n24 = m.M24 * s;
            double n31 = m.M31 * s; double n32 = m.M32 * s; double n33 = m.M33 * s; double n34 = m.M34 * s;
            double n41 = m.M41 * s; double n42 = m.M42 * s; double n43 = m.M43 * s; double n44 = m.M44 * s;
            return new Matrix(n11, n12, n13, n14,
                              n21, n22, n23, n24,
                              n31, n32, n33, n34,
                              n41, n42, n43, n44);
        }
        public static Matrix Multiply(Matrix m, double s) => m * s;
        public static Matrix operator *(double s, Matrix m) => m * s;
        public static Matrix Multiply(double s, Matrix m) => s * m;
        public static Vector operator *(Matrix m, Vector v)
        {
            double x = m.M11 * v.X + m.M12 * v.Y + m.M13 * v.Z + m.M14 * 1;
            double y = m.M21 * v.X + m.M22 * v.Y + m.M23 * v.Z + m.M24 * 1;
            double z = m.M31 * v.X + m.M32 * v.Y + m.M33 * v.Z + m.M34 * 1;
            return new Vector(x, y, z);
        }
        public static Vector Multiply(Matrix m, Vector v) => m * v;
        public static Point operator *(Matrix m, Point p)
        {
            double x = m.M11 * p.X + m.M12 * p.Y + m.M13 * p.Z + m.M14 * 1;
            double y = m.M21 * p.X + m.M22 * p.Y + m.M23 * p.Z + m.M24 * 1;
            double z = m.M31 * p.X + m.M32 * p.Y + m.M33 * p.Z + m.M34 * 1;
            return new Point(x, y, z);
        }
        public static Point Multiply(Matrix m, Point p) => m * p;
        public static Matrix operator +(Matrix a, Matrix b)
        {
            return new Matrix(a.M11 + b.M11, a.M12 + b.M12, a.M13 + b.M13, a.M14 + b.M14,
                              a.M21 + b.M21, a.M22 + b.M22, a.M23 + b.M23, a.M24 + b.M24,
                              a.M31 + b.M31, a.M32 + b.M32, a.M33 + b.M33, a.M34 + b.M34,
                              a.M41 + b.M41, a.M42 + b.M42, a.M43 + b.M43, a.M44 + b.M44);
        }
        public static Matrix Add(Matrix a, Matrix b) => a + b;
        public static Matrix Zero() => new Matrix(0, 0, 0, 0,
                                                  0, 0, 0, 0,
                                                  0, 0, 0, 0,
                                                  0, 0, 0, 0);
        public static Matrix Identity() => new Matrix(1, 0, 0, 0,
                                                      0, 1, 0, 0,
                                                      0, 0, 1, 0,
                                                      0, 0, 0, 1);
        public static Matrix Translate(double x, double y, double z)
        {
            // https://en.wikipedia.org/wiki/Translation_(geometry)
            Matrix m = Identity();
            m.M14 += x;
            m.M24 += y;
            m.M34 += z;
            return m;
        }
        public static Matrix Translate(Vector v) => Translate(v.X, v.Y, v.Z);
        public static Matrix Scale(double x, double y, double z)
        {
            // https://en.wikipedia.org/wiki/Scaling_(geometry)
            Matrix m = Identity();
            m.M11 *= x; m.M12 *= y; m.M13 *= z;
            m.M21 *= x; m.M22 *= y; m.M23 *= z;
            m.M31 *= x; m.M32 *= y; m.M33 *= z;
            m.M41 *= x; m.M42 *= y; m.M43 *= z;
            return m;
        }
        public static Matrix RotateX(double degreesCCW)
        {
            // https://en.wikipedia.org/wiki/Rotation_matrix
            double radians = degreesCCW * Math.PI / 180.0;
            Matrix m = Identity();
            m.M22 = Math.Cos(radians); m.M23 = -Math.Sin(radians);
            m.M32 = Math.Sin(radians); m.M33 =  Math.Cos(radians);
            return m;
        }
        public static Matrix RotateY(double degreesCCW)
        {
            // https://en.wikipedia.org/wiki/Rotation_matrix
            double radians = degreesCCW * Math.PI / 180.0;
            Matrix m = Identity();
            m.M11 =  Math.Cos(radians); m.M13 = Math.Sin(radians);
            m.M31 = -Math.Sin(radians); m.M33 = Math.Cos(radians);
            return m;
        }
        public static Matrix RotateZ(double degreesCCW)
        {
            // https://en.wikipedia.org/wiki/Rotation_matrix
            double radians = degreesCCW * Math.PI / 180.0;
            Matrix m = Identity();
            m.M11 = Math.Cos(radians); m.M12 = -Math.Sin(radians);
            m.M21 = Math.Sin(radians); m.M22 =  Math.Cos(radians);
            return m;
        }
        public static double Determinant2x2(double a, double b,
                                            double c, double d)
        {
            // https://en.wikipedia.org/wiki/Determinant
            return a * d - b * c;
        }
        public static double Determinant3x3(double a, double b, double c,
                                            double d, double e, double f,
                                            double g, double h, double i)
        {
            // https://en.wikipedia.org/wiki/Determinant
            double detA = Determinant2x2(e, f,
                                         h, i);
            double detB = Determinant2x2(d, f,
                                         g, i);
            double detC = Determinant2x2(d, e,
                                         g, h);
            return a * detA - b * detB + c * detC;
        }
        public static double Determinate4x4(double a, double b, double c, double d,
                                            double e, double f, double g, double h,
                                            double i, double j, double k, double l,
                                            double m, double n, double o, double p)
        {
            // https://en.wikipedia.org/wiki/Determinant
            double detA = Determinant3x3(f, g, h,
                                         j, k, l,
                                         n, o, p);
            double detB = Determinant3x3(e, g, h,
                                         i, k, l,
                                         m, o, p);
            double detC = Determinant3x3(e, f, h,
                                         i, j, l,
                                         m, n, p);
            double detD = Determinant3x3(e, f, g,
                                         i, j, k,
                                         m, n, o);
            return a * detA - b * detB + c * detC - d * detD;
        }
    }
}
