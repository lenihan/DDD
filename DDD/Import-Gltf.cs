using System;
using System.Collections.Generic;
using System.IO;
using System.Management.Automation;

namespace DDD
{
    [Cmdlet(VerbsData.Import, "Gltf")]
    [OutputType(typeof(Mesh))]
    [OutputType(typeof(Light))]
    public class ImportGltfCommand : Cmdlet
    {
        [Parameter(Mandatory = true, Position = 0)]
        public string Path { get; set; } = "";

        protected override void EndProcessing()
        {
            try
            {
                List<object> objects = GltfFormat.Read(Path);
                foreach (object obj in objects)
                {
                    WriteObject(obj);
                }
            }
            catch (Exception ex) when (ex is FormatException or IOException)
            {
                ErrorRecord error = new ErrorRecord(ex, "GltfReadError", ErrorCategory.InvalidData, Path);
                ThrowTerminatingError(error);
            }
        }
    }
}
