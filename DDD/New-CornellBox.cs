using System.Management.Automation;

namespace DDD
{
    // The classic Cornell Box lighting test scene - a room with red/green side walls and two
    // blocks, purpose-built for validating how convincing New-Light/New-Material shading looks.
    [Cmdlet(VerbsCommon.New, "CornellBox")]
    [OutputType(typeof(Mesh))]
    public class NewCornellBoxCommand : Cmdlet
    {
        protected override void EndProcessing()
        {
            WriteObject(Primitives.CornellBox());
        }
    }
}
