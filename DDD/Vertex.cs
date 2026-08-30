using System;

namespace DDD
{
    [Serializable()]
    public struct Vertex : IEquatable<Vertex>
    {
        public Point Position;
        public Vector? Normal;
        public Color? Color;

        public Vertex(Point position)
        {
            Position = position;
            Normal = null;
            Color = null;
        }
        public Vertex(Point position, Vector normal)
        {
            Position = position;
            Normal = normal;
            Color = null;
        }
        public Vertex(Point position, Color color)
        {
            Position = position;
            Normal = null;
            Color = color;
        }
        public Vertex(Point position, Vector normal, Color color)
        {
            Position = position;
            Normal = normal;
            Color = color;
        }
        public Vertex(Vertex v)
        {
            Position = v.Position;
            Normal = v.Normal;
            Color = v.Color;
        }
        public bool Equals(Vertex v) => Position == v.Position && Normal == v.Normal && Color == v.Color;
        public override bool Equals(object? obj)
        {
            if ((obj is null) || !GetType().Equals(obj.GetType()))
            {
                return false;
            }
            else
            {
                return Equals((Vertex)obj);
            }
        }
        public override int GetHashCode() => Position.GetHashCode() ^ (Normal?.GetHashCode() ?? 0) ^ (Color?.GetHashCode() ?? 0);
        public override string ToString()
        {
            return String.Format(System.Globalization.CultureInfo.InvariantCulture,
                "Vertex: Position={0} Normal={1} Color={2}\n", Position, Normal, Color);
        }
        public static bool operator ==(Vertex a, Vertex b) => a.Equals(b);
        public static bool operator !=(Vertex a, Vertex b) => !(a == b);
    }
}
