using System.Management.Automation;

namespace DDD
{
    // Blender's default test mesh (a stylized monkey head), bundled as a binary .ply asset
    // (see PLAN.md 1d).
    [Cmdlet(VerbsCommon.New, "Suzanne")]
    [OutputType(typeof(Mesh))]
    public class NewSuzanneCommand : Cmdlet
    {
        protected override void EndProcessing()
        {
            WriteObject(PlyFormat.ReadEmbeddedResource("DDD.Assets.suzanne.ply"));
        }
    }
}
