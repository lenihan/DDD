using System;
using System.IO;
using System.Management.Automation;

namespace DDD
{
    [Cmdlet(VerbsData.Export, "Ply")]
    public class ExportPlyCommand : Cmdlet
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
                PlyFormat.Write(Mesh, Path);
            }
            catch (IOException ex)
            {
                ErrorRecord error = new ErrorRecord(ex, "PlyWriteError", ErrorCategory.WriteError, Path);
                ThrowTerminatingError(error);
            }
        }
    }
}
