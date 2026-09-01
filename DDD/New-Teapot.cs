using System.Management.Automation;

namespace DDD
{
    // The Utah teapot (Martin Newell, 1975) - a triangulated version of the classic, public-
    // domain reference model, bundled as a binary .ply asset (see PLAN.md 1d).
    [Cmdlet(VerbsCommon.New, "Teapot")]
    [OutputType(typeof(Mesh))]
    public class NewTeapotCommand : Cmdlet
    {
        protected override void EndProcessing()
        {
            WriteObject(PlyFormat.ReadEmbeddedResource("DDD.Assets.teapot.ply"));
        }
    }
}
