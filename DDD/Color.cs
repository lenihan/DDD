using System;

namespace DDD
{
    [Serializable()]
    public struct Color : IEquatable<Color>
    {
        public byte R;
        public byte G;
        public byte B;

        public Color(byte r, byte g, byte b)
        {
            R = r;
            G = g;
            B = b;
        }
        public Color(Color c)
        {
            R = c.R;
            G = c.G;
            B = c.B;
        }
        public bool Equals(Color c) => (R == c.R) && (G == c.G) && (B == c.B);
        public override bool Equals(object? obj)
        {
            if ((obj is null) || !GetType().Equals(obj.GetType()))
            {
                return false;
            }
            else
            {
                return Equals((Color)obj);
            }
        }
        public override int GetHashCode() => R.GetHashCode() ^ G.GetHashCode() ^ B.GetHashCode();
        public override string ToString()
        {
            return String.Format(System.Globalization.CultureInfo.InvariantCulture, "#{0:X2}{1:X2}{2:X2}\n", R, G, B);
        }
        public static bool operator ==(Color a, Color b) => a.Equals(b);
        public static bool operator !=(Color a, Color b) => !(a == b);
    }
}
