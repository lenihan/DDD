using System;

namespace DDD
{
    public enum LightKind
    {
        Directional,
        Point
    }

    // A scene light, piped to Out-3d alongside Mesh/Point/Vector/Matrix objects. Directional
    // lights carry a direction (pointing TOWARD the light, same convention Rasterizer's camera
    // headlamp already used); point lights carry a position. Both rotate with the scene during
    // the turntable/manual rotation, so a light behaves as if it's fixed in the room while the
    // camera orbits - unlike the default headlamp used when no Light is present, which stays
    // fixed relative to the camera itself.
    //
    // No per-light color: shading intensity is a single scalar multiplied uniformly across a
    // face's base color (see Rasterizer's palette quantization) - a colored-light tint would
    // need a much larger, per-channel-quantized palette, out of scope for this pass.
    [Serializable()]
    public struct Light : IEquatable<Light>
    {
        public LightKind Kind;
        public Point Position;
        public Vector Direction;
        public double Intensity;

        public Light(Point position, double intensity = 1.0)
        {
            Kind = LightKind.Point;
            Position = position;
            Direction = default;
            Intensity = intensity;
        }
        public Light(Vector direction, double intensity = 1.0)
        {
            Kind = LightKind.Directional;
            Position = default;
            Direction = Vector.Normalize(direction);
            Intensity = intensity;
        }
        public bool Equals(Light l) => Kind == l.Kind && Position == l.Position && Direction == l.Direction && Intensity == l.Intensity;
        public override bool Equals(object? obj)
        {
            if ((obj is null) || !GetType().Equals(obj.GetType()))
            {
                return false;
            }
            else
            {
                return Equals((Light)obj);
            }
        }
        public override int GetHashCode() => Kind.GetHashCode() ^ Position.GetHashCode() ^ Direction.GetHashCode() ^ Intensity.GetHashCode();
        public override string ToString()
        {
            return Kind == LightKind.Point
                ? String.Format(System.Globalization.CultureInfo.InvariantCulture, "Light: Point Position={0} Intensity={1:#,0.##}\n", Position, Intensity)
                : String.Format(System.Globalization.CultureInfo.InvariantCulture, "Light: Directional Direction={0} Intensity={1:#,0.##}\n", Direction, Intensity);
        }
        public static bool operator ==(Light a, Light b) => a.Equals(b);
        public static bool operator !=(Light a, Light b) => !(a == b);
    }
}
