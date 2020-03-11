using System;

namespace DDD
{
    [Serializable()]
    public struct Point : IEquatable<Point>
    {
        public double X;
        public double Y;
        public double Z;

        public Point(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }
        public Point(Point p)
        {
            X = p.X;
            Y = p.Y;
            Z = p.Z;
        }
        public Point(double[] arr)
        {
            X = 0;
            Y = 0;
            Z = 0;
            if (arr == null) return;
            if (arr.Length > 0) X = arr[0];
            if (arr.Length > 1) Y = arr[1];
            if (arr.Length > 2) Z = arr[2];
        }
        public Point(string str)
        {
            char[] delimiterChars = { ' ', ',', '\t' };
            char[] trimChars      = { ' ', '(', ')', '[', ']', '{', '}', '<', '>', };
            X = 0;
            Y = 0;
            Z = 0;
            if (str == null) return;
            string[] values = str.Trim(trimChars).Split(delimiterChars, StringSplitOptions.RemoveEmptyEntries);
            if (values.Length > 0) X = double.Parse(values[0], System.Globalization.CultureInfo.InvariantCulture);
            if (values.Length > 1) Y = double.Parse(values[1], System.Globalization.CultureInfo.InvariantCulture);
            if (values.Length > 2) Z = double.Parse(values[2], System.Globalization.CultureInfo.InvariantCulture);
        }
        public bool Equals(Point p) => p == null ? false : (X == p.X) && (Y == p.Y) && (Z == p.Z);
        public override bool Equals(object obj)
        {
            if ((obj is null) || !GetType().Equals(obj.GetType()))
            {
                return false;
            }
            else
            {
                return Equals((Point)obj);
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
            return String.Format(System.Globalization.CultureInfo.InvariantCulture, "({0:#,0.##} {1:#,0.##} {2:#,0.##})\n", X, Y, Z);
        }
        public static bool operator ==(Point? a, Point? b) => a is null ? b is null : a.Equals(b);
        public static bool operator !=(Point? a, Point? b) => !(a == b);
        public static Point operator +(Point a, Point b) => new Point(a.X + b.X,
                                                                      a.Y + b.Y,
                                                                      a.Z + b.Z);
        public static Point Add(Point a, Point b) => a + b;
        public static Vector operator -(Point a, Point b) => new Vector(a.X - b.X,
                                                                        a.Y - b.Y,
                                                                        a.Z - b.Z);
        public static Vector Subtract(Point a, Point b) => a - b;
        public static Point operator *(double s, Point p) => new Point(s * p.X,
                                                                       s * p.Y,
                                                                       s * p.Z);
        public static Point Multiply(double s, Point p) => s * p;
        public static Point operator *(Point p, double s) => s * p;
        public static Point Multiply(Point p, double s) => p * s;
        public static Point operator /(Point p, double s) => new Point(p.X / s,
                                                                       p.Y / s,
                                                                       p.Z / s);
        public static Point Divide(Point p, double s) => p / s;
    }
}
