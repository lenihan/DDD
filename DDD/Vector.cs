using System;

namespace DDD
{
    [Serializable()]
    public struct Vector
    {
        public double X;
        public double Y;
        public double Z;

        public Vector(Vector v)
        {
            X = v.X;
            Y = v.Y;
            Z = v.Z;
        }
        public Vector(double x, double y, double z)
        {
            X = x;
            Y = y;
            Z = z;
        }
        public Vector(double[] arr)
        {
            X = arr[0];
            Y = arr[1];
            Z = arr[2];
        }
        public override string ToString()
        {
            // https://ss64.com/ps/syntax-f-operator.html
            // {0:#,0.##}
            //   0:    Index
            //   #,0   Group integers in 3's with commas, do not hide zero
            //   .##   Show at most 2 decimals, or nothing if no decimal point
            return String.Format("[{0:#,0.##} {1:#,0.##} {2:#,0.##}]\n", X, Y, Z);
        }
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
        public static Point operator +(Vector v, Point p) => new Point(v.X + p.X,
                                                                       v.Y + p.Y,
                                                                       v.Z + p.Z);
        public static Point operator +(Point p, Vector v) => v + p;
        public static Vector operator -(Vector v1, Vector v2) => new Vector(v1.X - v2.X,
                                                                            v1.Y - v2.Y,
                                                                            v1.Z - v2.Z);
        public static Vector operator *(Vector v, double s) => new Vector(v.X * s,
                                                                          v.Y * s,
                                                                          v.Z * s);
        public static Vector operator *(double s, Vector v) => v * s;
        public static Vector operator /(Vector v, double s) => new Vector(v.X / s,
                                                                          v.Y / s,
                                                                          v.Z / s);

        public static Vector operator -(Vector v) => new Vector(-v.X, 
                                                                -v.Y, 
                                                                -v.Z);
        public static double Dot(Vector a, Vector b)
        {
            return a.X * b.X +
                   a.Y * b.Y +
                   a.Z * b.Z;
        }
        public static Vector Cross(Vector a, Vector b)
        {
            return new Vector(a.Y *b.Z - a.Z *b.Y,
                              a.Z *b.X - a.X *b.Z,
                              a.X *b.Y - a.Y *b.X);
        }
        public static Vector Normalize(Vector v)
        {
            Vector u = new Vector(v);
            return u.Normalize();
        }
    }
}
