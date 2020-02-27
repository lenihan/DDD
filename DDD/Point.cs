﻿using System;

namespace DDD
{
    [Serializable()]
    public struct Point
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
            if (arr.Length > 0) { X = arr[0]; }
            if (arr.Length > 1) { Y = arr[1]; }
            if (arr.Length > 2) { Z = arr[2]; }
        }
        public Point(string str)
        {
            char[] delimiterChars = { ' ', ',', '\t' };
            char[] trimChars = { ' ', '(', ')', '[', ']', '{', '}', '<', '>', };

            string[] values = str.Trim(trimChars).Split(delimiterChars, StringSplitOptions.RemoveEmptyEntries);
            X = 0;
            Y = 0;
            Z = 0;
            if (values.Length > 0) { X = double.Parse(values[0]); }
            if (values.Length > 1) { Y = double.Parse(values[1]); }
            if (values.Length > 2) { Z = double.Parse(values[2]); }
        }
        public override string ToString()
        {
            // https://ss64.com/ps/syntax-f-operator.html
            // {0:#,0.##}
            //   0:    Index
            //   #,0   Group integers in 3's with commas, do not hide zero
            //   .##   Show at most 2 decimals, or nothing if no decimal point
            return String.Format("({0:#,0.##} {1:#,0.##} {2:#,0.##})\n", X, Y, Z);
        }
        public static Point operator +(Point p1, Point p2) => new Point(p1.X + p2.X,
                                                                        p1.Y + p2.Y,
                                                                        p1.Z + p2.Z);
        public static Vector operator -(Point p1, Point p2) => new Vector(p1.X - p2.X,
                                                                          p1.Y - p2.Y,
                                                                          p1.Z - p2.Z);
        public static Point operator *(double s, Point p) => new Point(s * p.X,
                                                                       s * p.Y,
                                                                       s * p.Z);
        public static Point operator *(Point p, double s) => s * p;
        public static Point operator /(Point p, double s) => new Point(p.X / s,
                                                                       p.Y / s,
                                                                       p.Z / s);

    }
}
