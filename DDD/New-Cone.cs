using System.Management.Automation;

namespace DDD
{
    [Cmdlet(VerbsCommon.New, "Cone")]
    [OutputType(typeof(Mesh))]
    public class NewConeCommand : Cmdlet
    {
        [Parameter()]
        public double BaseRadius { get; set; } = 1.0;

        // Defaults to a true cone (a point at the top); pass -TopRadius to get a frustum.
        [Parameter()]
        public double TopRadius { get; set; }

        [Parameter()]
        public double Height { get; set; } = 1.0;

        [Parameter()]
        public int Segments { get; set; } = 16;

        [Parameter()]
        public Point Center { get; set; } = new Point(0, 0, 0);

        protected override void EndProcessing()
        {
            WriteObject(Primitives.Cone(BaseRadius, TopRadius, Height, Segments, Center));
        }
    }
}
