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
        // Renders at full brightness regardless of any light, added on top of Ambient - used
        // only via its luminance today (see Rasterizer.ComputeIntensity), not as a true glow;
        // stored as a full Color anyway so glTF's emissiveFactor round-trips faithfully.
        public Color Emissive;

        public Material(Color color, double ambient = 0.2, double diffuse = 0.8, double specular = 0.0,
            double shininess = 16.0, Color emissive = default)
        {
            Color = color;
            Ambient = ambient;
            Diffuse = diffuse;
            Specular = specular;
            Shininess = shininess;
            Emissive = emissive;
        }
        public bool Equals(Material m) => Color == m.Color && Ambient == m.Ambient && Diffuse == m.Diffuse
            && Specular == m.Specular && Shininess == m.Shininess && Emissive == m.Emissive;
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
            ^ Specular.GetHashCode() ^ Shininess.GetHashCode() ^ Emissive.GetHashCode();
        public override string ToString()
        {
            return String.Format(System.Globalization.CultureInfo.InvariantCulture,
                "Material: Color={0} Ambient={1:#,0.##} Diffuse={2:#,0.##} Specular={3:#,0.##} Shininess={4:#,0.##} Emissive={5}\n",
                Color, Ambient, Diffuse, Specular, Shininess, Emissive);
        }
        public static bool operator ==(Material a, Material b) => a.Equals(b);
        public static bool operator !=(Material a, Material b) => !(a == b);
    }
}
