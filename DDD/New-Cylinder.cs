using System.Management.Automation;

namespace DDD
{
    [Cmdlet(VerbsCommon.New, "Cylinder")]
    [OutputType(typeof(Mesh))]
    public class NewCylinderCommand : Cmdlet
    {
        [Parameter()]
        public double Radius { get; set; } = 1.0;

        [Parameter()]
        public double Height { get; set; } = 1.0;

        [Parameter()]
        public int Segments { get; set; } = 16;

        [Parameter()]
        public Point Center { get; set; } = new Point(0, 0, 0);

        protected override void EndProcessing()
        {
            WriteObject(Primitives.Cylinder(Radius, Height, Segments, Center));
        }
    }
}
