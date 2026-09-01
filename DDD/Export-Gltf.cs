using System;
using System.Collections.Generic;
using System.IO;
using System.Management.Automation;

namespace DDD
{
    [Cmdlet(VerbsData.Export, "Gltf")]
    public class ExportGltfCommand : Cmdlet
    {
        readonly List<object> _objects = new List<object>();

        [Parameter(Mandatory = true, ValueFromPipeline = true, Position = 0)]
        public object[]? InputObject { get; set; }

        [Parameter(Mandatory = true, Position = 1)]
        public string Path { get; set; } = "";

        protected override void ProcessRecord()
        {
            if (InputObject != null)
            {
                _objects.AddRange(InputObject);
            }
        }

        protected override void EndProcessing()
        {
            if (_objects.Count == 0) return;

            try
            {
                GltfFormat.Write(_objects, Path);
            }
            catch (IOException ex)
            {
                ErrorRecord error = new ErrorRecord(ex, "GltfWriteError", ErrorCategory.WriteError, Path);
                ThrowTerminatingError(error);
            }
        }
    }
}
