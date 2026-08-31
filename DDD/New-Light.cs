using System.Management.Automation;

namespace DDD
{
    [Cmdlet(VerbsCommon.New, "Light", DefaultParameterSetName = "Directional")]
    [OutputType(typeof(Light))]
    public class NewLightCommand : PSCmdlet
    {
        // Direction points TOWARD the light (same convention Rasterizer's camera headlamp uses)
        // for Directional; for Spot it means the opposite - the direction the light is AIMED
        // (shines toward), since "toward the light" isn't meaningful for a light with a real aim
        // axis. Default (Directional only): overhead.
        [Parameter(ParameterSetName = "Directional")]
        [Parameter(Mandatory = true, ParameterSetName = "Spot")]
        public Vector Direction { get; set; } = new Vector(0, 1, 0);

        [Parameter(Mandatory = true, ParameterSetName = "Point")]
        [Parameter(Mandatory = true, ParameterSetName = "Spot")]
        public Point Position { get; set; }

        // Half-angle of the cone (degrees, 0-90) - no light beyond it.
        [Parameter(Mandatory = true, ParameterSetName = "Spot")]
        public double OuterConeAngle { get; set; }

        // Where falloff begins (degrees, <= OuterConeAngle) - full intensity inside it.
        [Parameter(ParameterSetName = "Spot")]
        public double InnerConeAngle { get; set; }

        // Produces a directional light whose Direction is the mesh's own bounding-box center,
        // treated as a vector from the world origin - e.g. a mesh centered at (0,5,0) gives a
        // light that appears to come from directly overhead.
        [Parameter(Mandatory = true, ParameterSetName = "LookAt")]
        public Mesh? LookAt { get; set; }

        [Parameter()]
        public double Intensity { get; set; } = 1.0;

        protected override void EndProcessing()
        {
            Light light = ParameterSetName switch
            {
                "Point" => new Light(Position, Intensity),
                "Spot" => new Light(Position, Direction, OuterConeAngle, InnerConeAngle, Intensity),
                "LookAt" => new Light(DirectionToOrigin(LookAt!.BoundingBoxCenter()), Intensity),
                _ => new Light(Direction, Intensity),
            };
            WriteObject(light);
        }

        static Vector DirectionToOrigin(Point center)
        {
            var toCenter = new Vector(center.X, center.Y, center.Z);
            // A mesh centered exactly at the world origin has no meaningful direction to
            // compute - fall back to the same overhead default -Direction otherwise uses.
            return toCenter.Length() > 1e-9 ? Vector.Normalize(toCenter) : new Vector(0, 1, 0);
        }
    }
}
