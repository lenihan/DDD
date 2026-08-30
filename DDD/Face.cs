using System;

namespace DDD
{
    [Serializable()]
    public struct Face : IEquatable<Face>
    {
        public int A;
        public int B;
        public int C;

        public Face(int a, int b, int c)
        {
            A = a;
            B = b;
            C = c;
        }
        public Face(Face f)
        {
            A = f.A;
            B = f.B;
            C = f.C;
        }
        public bool Equals(Face f) => (A == f.A) && (B == f.B) && (C == f.C);
        public override bool Equals(object? obj)
        {
            if ((obj is null) || !GetType().Equals(obj.GetType()))
            {
                return false;
            }
            else
            {
                return Equals((Face)obj);
            }
        }
        public override int GetHashCode() => A.GetHashCode() ^ B.GetHashCode() ^ C.GetHashCode();
        public override string ToString()
        {
            return String.Format(System.Globalization.CultureInfo.InvariantCulture, "Face: ({0} {1} {2})\n", A, B, C);
        }
        public static bool operator ==(Face a, Face b) => a.Equals(b);
        public static bool operator !=(Face a, Face b) => !(a == b);
    }
}
