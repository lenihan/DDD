using System;

namespace DDD
{
    // A directed camera shot, piped to Out-3d/exported through glTF alongside Mesh/Light objects -
    // independent of glTF (see PLAN.md 1f/1h), for framing precise video shots rather than only
    // the auto-fit interactive turntable every other DDD render uses today. LookAt is a world
    // point (not a raw direction) so it can be aimed at a mesh the same way New-Light -LookAt is:
    // "look at this thing," not "face this way."
    //
    // FovYDegrees only applies when Perspective is true; OrthographicHeight (half the view
    // volume's world-space height) only applies when it's false - both fields always exist so a
    // Camera can be toggled between modes without losing whichever one it isn't currently using.
    //
    // Up is a roll-disambiguating hint, the same convention as gluLookAt/any standard look-at
    // camera - only its component perpendicular to (LookAt - Position) actually matters. It's
    // stored as-given by a direct New-Camera call, but a round trip through glTF (see
    // GltfFormat) re-derives it via Gram-Schmidt against the view direction, so it comes back
    // exactly equal only when it was already perpendicular going in.
    [Serializable()]
    public struct Camera : IEquatable<Camera>
    {
        public Point Position;
        public Point LookAt;
        public Vector Up;
        public bool Perspective;
        public double FovYDegrees;
        public double OrthographicHeight;
        public double NearPlane;
        public double FarPlane;

        public Camera(Point position, Point lookAt, bool perspective = true, double fovYDegrees = 40.0,
            double orthographicHeight = 1.0, Vector? up = null, double nearPlane = 0.01, double farPlane = 1000.0)
        {
            Position = position;
            LookAt = lookAt;
            Perspective = perspective;
            FovYDegrees = fovYDegrees;
            OrthographicHeight = orthographicHeight;
            Up = up ?? new Vector(0, 1, 0);
            NearPlane = nearPlane;
            FarPlane = farPlane;
        }

        public bool Equals(Camera c) => Position == c.Position && LookAt == c.LookAt && Up == c.Up
            && Perspective == c.Perspective && FovYDegrees == c.FovYDegrees && OrthographicHeight == c.OrthographicHeight
            && NearPlane == c.NearPlane && FarPlane == c.FarPlane;
        public override bool Equals(object? obj)
        {
            if ((obj is null) || !GetType().Equals(obj.GetType()))
            {
                return false;
            }
            else
            {
                return Equals((Camera)obj);
            }
        }
        public override int GetHashCode() => Position.GetHashCode() ^ LookAt.GetHashCode() ^ Up.GetHashCode()
            ^ Perspective.GetHashCode() ^ FovYDegrees.GetHashCode() ^ OrthographicHeight.GetHashCode()
            ^ NearPlane.GetHashCode() ^ FarPlane.GetHashCode();
        public override string ToString() => Perspective
            ? String.Format(System.Globalization.CultureInfo.InvariantCulture,
                "Camera: Perspective Position={0} LookAt={1} FovY={2:#,0.##}\n", Position, LookAt, FovYDegrees)
            : String.Format(System.Globalization.CultureInfo.InvariantCulture,
                "Camera: Orthographic Position={0} LookAt={1} Height={2:#,0.##}\n", Position, LookAt, OrthographicHeight);
        public static bool operator ==(Camera a, Camera b) => a.Equals(b);
        public static bool operator !=(Camera a, Camera b) => !(a == b);
    }
}
