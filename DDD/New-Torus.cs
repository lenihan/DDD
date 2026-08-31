using System.Management.Automation;

namespace DDD
{
    [Cmdlet(VerbsCommon.New, "Torus")]
    [OutputType(typeof(Mesh))]
    public class NewTorusCommand : Cmdlet
    {
        [Parameter()]
        public double MajorRadius { get; set; } = 1.0;

        [Parameter()]
        public double MinorRadius { get; set; } = 0.25;

        [Parameter()]
        public int Segments { get; set; } = 16;

        [Parameter()]
        public Point Center { get; set; } = new Point(0, 0, 0);

        protected override void EndProcessing()
        {
            WriteObject(Primitives.Torus(MajorRadius, MinorRadius, Segments, Center));
        }
    }
}
