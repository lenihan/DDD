using System;

namespace DDD
{
    public enum LightKind
    {
        Directional,
        Point,
        Spot
    }

    // A scene light, piped to Out-3d alongside Mesh/Point/Vector/Matrix objects. Directional
    // lights carry a direction (pointing TOWARD the light, same convention Rasterizer's camera
    // headlamp already used); point lights carry a position; spot lights carry both a position
    // and a direction, but Direction there means the direction the light is AIMED (shines
    // toward) - the opposite sense from Directional's Direction, since "toward the light" isn't
    // a meaningful idea for a light with a real aim axis. All rotate with the scene during the
    // turntable/manual rotation, so a light behaves as if it's fixed in the room while the
    // camera orbits - unlike the default headlamp used when no Light is present, which stays
    // fixed relative to the camera itself.
    //
    // No per-light color: shading intensity is a single scalar multiplied uniformly across a
    // face's base color (see Rasterizer's palette quantization) - a colored-light tint would
    // need a much larger, per-channel-quantized palette, out of scope for this pass. For the
    // same reason, no distance-based (inverse-square) falloff for Point/Spot - Intensity is a
    // flat scalar regardless of distance.
    [Serializable()]
    public struct Light : IEquatable<Light>
    {
        public LightKind Kind;
        public Point Position;
        public Vector Direction;
        public double Intensity;
        public double InnerConeAngleDegrees;
        public double OuterConeAngleDegrees;

        public Light(Point position, double intensity = 1.0)
        {
            Kind = LightKind.Point;
            Position = position;
            Direction = default;
            Intensity = intensity;
            InnerConeAngleDegrees = 0.0;
            OuterConeAngleDegrees = 0.0;
        }
        public Light(Vector direction, double intensity = 1.0)
        {
            Kind = LightKind.Directional;
            Position = default;
            Direction = Vector.Normalize(direction);
            Intensity = intensity;
            InnerConeAngleDegrees = 0.0;
            OuterConeAngleDegrees = 0.0;
        }
        // outerConeAngleDegrees is the half-angle of the cone (0-90); no light beyond it.
        // innerConeAngleDegrees (must be <= outer) is where falloff begins - full intensity
        // inside it, linearly falling to zero at the outer edge.
        public Light(Point position, Vector direction, double outerConeAngleDegrees, double innerConeAngleDegrees = 0.0, double intensity = 1.0)
        {
            Kind = LightKind.Spot;
            Position = position;
            Direction = Vector.Normalize(direction);
            Intensity = intensity;
            InnerConeAngleDegrees = innerConeAngleDegrees;
            OuterConeAngleDegrees = outerConeAngleDegrees;
        }
        public bool Equals(Light l) => Kind == l.Kind && Position == l.Position && Direction == l.Direction
            && Intensity == l.Intensity && InnerConeAngleDegrees == l.InnerConeAngleDegrees && OuterConeAngleDegrees == l.OuterConeAngleDegrees;
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
        public override int GetHashCode() => Kind.GetHashCode() ^ Position.GetHashCode() ^ Direction.GetHashCode() ^ Intensity.GetHashCode()
            ^ InnerConeAngleDegrees.GetHashCode() ^ OuterConeAngleDegrees.GetHashCode();
        public override string ToString() => Kind switch
        {
            LightKind.Point => String.Format(System.Globalization.CultureInfo.InvariantCulture,
                "Light: Point Position={0} Intensity={1:#,0.##}\n", Position, Intensity),
            LightKind.Spot => String.Format(System.Globalization.CultureInfo.InvariantCulture,
                "Light: Spot Position={0} Direction={1} Intensity={2:#,0.##} InnerCone={3:#,0.##} OuterCone={4:#,0.##}\n",
                Position, Direction, Intensity, InnerConeAngleDegrees, OuterConeAngleDegrees),
            _ => String.Format(System.Globalization.CultureInfo.InvariantCulture,
                "Light: Directional Direction={0} Intensity={1:#,0.##}\n", Direction, Intensity),
        };
        public static bool operator ==(Light a, Light b) => a.Equals(b);
        public static bool operator !=(Light a, Light b) => !(a == b);
    }
}
