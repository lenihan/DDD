using System;

namespace DDD
{
    // Assigned to Mesh.Material (optional - a Mesh with none uses Rasterizer's default, which
    // reproduces the old fixed "headlamp" look exactly: Ambient + Diffuse sum to 1.0, so a face
    // facing a light dead-on still renders at full, unclamped brightness).
    [Serializable()]
    public struct Material : IEquatable<Material>
    {
        public Color Color;
        public double Ambient;
        public double Diffuse;
        public double Specular;
        public double Shininess;

        public Material(Color color, double ambient = 0.2, double diffuse = 0.8, double specular = 0.0, double shininess = 16.0)
        {
            Color = color;
            Ambient = ambient;
            Diffuse = diffuse;
            Specular = specular;
            Shininess = shininess;
        }
        public bool Equals(Material m) => Color == m.Color && Ambient == m.Ambient && Diffuse == m.Diffuse
            && Specular == m.Specular && Shininess == m.Shininess;
        public override bool Equals(object? obj)
        {
            if ((obj is null) || !GetType().Equals(obj.GetType()))
            {
                return false;
            }
            else
            {
                return Equals((Material)obj);
            }
        }
        public override int GetHashCode() => Color.GetHashCode() ^ Ambient.GetHashCode() ^ Diffuse.GetHashCode()
            ^ Specular.GetHashCode() ^ Shininess.GetHashCode();
        public override string ToString()
        {
            return String.Format(System.Globalization.CultureInfo.InvariantCulture,
                "Material: Color={0} Ambient={1:#,0.##} Diffuse={2:#,0.##} Specular={3:#,0.##} Shininess={4:#,0.##}\n",
                Color, Ambient, Diffuse, Specular, Shininess);
        }
        public static bool operator ==(Material a, Material b) => a.Equals(b);
        public static bool operator !=(Material a, Material b) => !(a == b);
    }
}
