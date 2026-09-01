using System.Management.Automation;

namespace DDD
{
    [Cmdlet(VerbsCommon.New, "Camera")]
    [OutputType(typeof(Camera))]
    public class NewCameraCommand : PSCmdlet
    {
        [Parameter(Mandatory = true, Position = 0)]
        public Point Position { get; set; }

        // Aimed at this point, the same "look at this thing" convention as New-Light -LookAt -
        // not a raw direction.
        [Parameter(Position = 1)]
        public Point LookAt { get; set; } = new Point(0, 0, 0);

        [Parameter()]
        public Vector Up { get; set; } = new Vector(0, 1, 0);

        [Parameter()]
        public SwitchParameter Orthographic { get; set; }

        // Only used when -Orthographic is not set.
        [Parameter()]
        public double FovY { get; set; } = 40.0;

        // Half the view volume's world-space height; only used when -Orthographic is set.
        [Parameter()]
        public double OrthographicHeight { get; set; } = 1.0;

        [Parameter()]
        public double NearPlane { get; set; } = 0.01;

        [Parameter()]
        public double FarPlane { get; set; } = 1000.0;

        protected override void EndProcessing()
        {
            Camera camera = new Camera(Position, LookAt, !Orthographic.IsPresent, FovY, OrthographicHeight, Up, NearPlane, FarPlane);
            WriteObject(camera);
        }
    }
}
