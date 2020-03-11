using System;

namespace DDD
{
    [Serializable()]
    public struct Vector : IEquatable<Vector>
    {
        public double X;
        public double Y;
        public double Z;

        public Vector(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }
        public Vector(Vector v)
        {
            X = v.X;
            Y = v.Y;
            Z = v.Z;
        }
        public Vector(double[] arr)
        {
            X = 0;
            Y = 0;
            Z = 0;
            if (arr == null) return;
            if (arr.Length > 0) X = arr[0];
            if (arr.Length > 1) Y = arr[1];
            if (arr.Length > 2) Z = arr[2];
        }
        public Vector(string str)
        {
            char[] delimiterChars = { ' ', ',', '\t' };
            char[] trimChars = { ' ', '(', ')', '[', ']', '{', '}', '<', '>', };

            X = 0;
            Y = 0;
            Z = 0;
            if (str == null) return;
            string[] values = str.Trim(trimChars).Split(delimiterChars, StringSplitOptions.RemoveEmptyEntries);
            if (values.Length > 0) X = double.Parse(values[0], System.Globalization.CultureInfo.InvariantCulture);
            if (values.Length > 1) Y = double.Parse(values[1], System.Globalization.CultureInfo.InvariantCulture);
            if (values.Length > 2) Z = double.Parse(values[2], System.Globalization.CultureInfo.InvariantCulture);
        }
        public bool Equals(Vector v) => v == null ? false : (X == v.X) && (Y == v.Y) && (Z == v.Z);
        public override bool Equals(object obj)
        {
            //Check for null and compare run-time types.
            if ((obj == null) || !GetType().Equals(obj.GetType()))
            {
                return false;
            }
            else
            {
                Vector v = (Vector)obj;
                return (X == v.X) && (Y == v.Y) && (Z == v.Z);
            }
        }
        public override int GetHashCode() => (int)X ^ (int)Y ^ (int)Z;
        public override string ToString()
        {
            // https://ss64.com/ps/syntax-f-operator.html
            // {0:#,0.##}
            //   0:    Index
            //   #,0   Group integers in 3's with commas, do not hide zero
            //   .##   Show at most 2 decimals, or nothing if no decimal point
            return String.Format(System.Globalization.CultureInfo.InvariantCulture, "[{0:#,0.##} {1:#,0.##} {2:#,0.##}]\n", X, Y, Z);
        }
        public static bool operator ==(Vector v1, Vector v2)
        {
            if ((object)v1 == null) return (object)v2 == null;
            return v1.Equals(v2);
        }
        public static bool operator !=(Vector v1, Vector v2) => !(v1 == v2);
        public double Length() => Math.Sqrt(X * X +
                                            Y * Y +
                                            Z * Z);
        public Vector Normalize()
        {
            double length = Length();
            X /= length;
            Y /= length;
            Z /= length;
            return this;
        }
        public static Vector operator +(Vector v1, Vector v2) => new Vector(v1.X + v2.X,
                                                                            v1.Y + v2.Y,
                                                                            v1.Z + v2.Z);
        public static Vector Add(Vector v1, Vector v2) => v1 + v2;
        public static Point operator +(Point p, Vector v) => v + p;
        public static Point Add(Point p, Vector v) => p + v;
        public static Point operator +(Vector v, Point p) => new Point(v.X + p.X,
                                                                       v.Y + p.Y,
                                                                       v.Z + p.Z);
        public static Point Add(Vector v, Point p) => v + p;
        public static Vector operator -(Vector v1, Vector v2) => new Vector(v1.X - v2.X,
                                                                            v1.Y - v2.Y,
                                                                            v1.Z - v2.Z);

        public static Vector Subtract(Vector v1, Vector v2) => v1 - v2;
        public static Vector operator *(Vector v, double s) => new Vector(v.X * s,
                                                                          v.Y * s,
                                                                          v.Z * s);
        public static Vector Multiply(Vector v, double s) => v * s; 
        public static Vector operator *(double s, Vector v) => v * s;
        public static Vector Multiply(double s, Vector v) => s * v; 
        public static Vector operator /(Vector v, double s) => new Vector(v.X / s,
                                                                          v.Y / s,
                                                                          v.Z / s);
        public static Vector Divide(Vector v, double s) => v / s;

        public static Vector operator -(Vector v) => new Vector(-v.X,
                                                                -v.Y,
                                                                -v.Z);
        public static Vector Negate(Vector v) => -v;

        public static double Dot(Vector a, Vector b)
        {
            return a.X * b.X +
                   a.Y * b.Y +
                   a.Z * b.Z;
        }
        public static Vector Cross(Vector a, Vector b)
        {
            return new Vector(a.Y * b.Z - a.Z * b.Y,
                              a.Z * b.X - a.X * b.Z,
                              a.X * b.Y - a.Y * b.X);
        }
        public static Vector Normalize(Vector v)
        {
            Vector u = new Vector(v);
            return u.Normalize();
        }
    }
}
