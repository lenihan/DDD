using System.Management.Automation;

namespace DDD
{
    // The Stanford bunny (Stanford Computer Graphics Laboratory, 1994), decimated reconstruction
    // (bun_zipper_res4), bundled as a binary .ply asset (see PLAN.md 1d).
    [Cmdlet(VerbsCommon.New, "StanfordBunny")]
    [OutputType(typeof(Mesh))]
    public class NewStanfordBunnyCommand : Cmdlet
    {
        protected override void EndProcessing()
        {
            WriteObject(PlyFormat.ReadEmbeddedResource("DDD.Assets.bunny.ply"));
        }
    }
}
