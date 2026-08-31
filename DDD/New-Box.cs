using System.Management.Automation;

namespace DDD
{
    [Cmdlet(VerbsCommon.New, "Box")]
    [OutputType(typeof(Mesh))]
    public class NewBoxCommand : Cmdlet
    {
        [Parameter()]
        public double Width { get; set; } = 1.0;

        [Parameter()]
        public double Height { get; set; } = 1.0;

        [Parameter()]
        public double Depth { get; set; } = 1.0;

        [Parameter()]
        public Point Center { get; set; } = new Point(0, 0, 0);

        protected override void EndProcessing()
        {
            WriteObject(Primitives.Box(Width, Height, Depth, Center));
        }
    }
}
