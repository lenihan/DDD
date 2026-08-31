using System;
using System.IO;
using System.Management.Automation;

namespace DDD
{
    [Cmdlet(VerbsData.Export, "Gltf")]
    public class ExportGltfCommand : Cmdlet
    {
        [Parameter(Mandatory = true, ValueFromPipeline = true, Position = 0)]
        public Mesh? Mesh { get; set; }

        [Parameter(Mandatory = true, Position = 1)]
        public string Path { get; set; } = "";

        protected override void ProcessRecord()
        {
            if (Mesh == null) return;

            try
            {
                GltfFormat.Write(Mesh, Path);
            }
            catch (IOException ex)
            {
                ErrorRecord error = new ErrorRecord(ex, "GltfWriteError", ErrorCategory.WriteError, Path);
                ThrowTerminatingError(error);
            }
        }
    }
}
