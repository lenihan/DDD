using System;
using System.IO;
using System.Management.Automation;

namespace DDD
{
    [Cmdlet(VerbsData.Import, "Gltf")]
    [OutputType(typeof(Mesh))]
    public class ImportGltfCommand : Cmdlet
    {
        [Parameter(Mandatory = true, Position = 0)]
        public string Path { get; set; } = "";

        protected override void EndProcessing()
        {
            try
            {
                Mesh mesh = GltfFormat.Read(Path);
                WriteObject(mesh);
            }
            catch (Exception ex) when (ex is FormatException or IOException)
            {
                ErrorRecord error = new ErrorRecord(ex, "GltfReadError", ErrorCategory.InvalidData, Path);
                ThrowTerminatingError(error);
            }
        }
    }
}
