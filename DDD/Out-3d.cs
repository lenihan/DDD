using System.Management.Automation;             //Windows PowerShell namespace
using System.Diagnostics;
// Defining input from the pipeline
// https://docs.microsoft.com/en-us/powershell/scripting/developer/cmdlet/adding-parameters-that-process-pipeline-input?view=powershell-7

// Samples here:
// C:\Program Files (x86)\Microsoft SDKs\Windows\v7.0\Samples\sysmgmt\WindowsPowerShell\

namespace DDD
{
    [Cmdlet(VerbsData.Out, "3d")]
    public class Out3dCommand : Cmdlet
    {
        private DDD.Point[] p;
        private DDD.Vector[] v;
        private DDD.Matrix[] m;
        [Parameter(
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true)]
        // [ValidateNotNullOrEmpty]
        public DDD.Point[] Point
        {
            get { return this.p; }
            set { this.p = value; }
        }
        [Parameter(
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true)]
        // [ValidateNotNullOrEmpty]
        public DDD.Vector[] Vector
        {
            get { return this.v; }
            set { this.v = value; }
        }
        [Parameter(
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true)]
        // [ValidateNotNullOrEmpty]
        public DDD.Matrix[] Matrix
        {
            get { return this.m; }
            set { this.m = value; }
        }
        protected override void ProcessRecord()
        {
            WriteObject("ProcessRecord");
            if (p != null) WriteObject(p);
            if (v != null) WriteObject(v);
            if (m != null) WriteObject(m);
        }        
        // private DDD.Point p;
        // [Parameter(
        //     Position = 0,
        //     ValueFromPipeline = true,
        //     ValueFromPipelineByPropertyName = true)]
        // // [ValidateNotNullOrEmpty]
        // public DDD.Point Point
        // {
        //     get { return this.p; }
        //     set { this.p = value; }
        // }
        // protected override void ProcessRecord()
        // {
        //     WriteObject(p);
        // }        



        // private string[] processNames;

        // [Parameter(
        //     Position = 0,
        //     ValueFromPipeline = true,
        //     ValueFromPipelineByPropertyName = true)]
        // // [ValidateNotNullOrEmpty]
        // public string[] Name
        // {
        //     get { return this.processNames; }
        //     set { this.processNames = value; }
        // }

        // protected override void ProcessRecord()
        // {
        //     // If no process names are passed to the cmdlet, get all processes.
        //     if (processNames == null)
        //     {
        //         // Write the processes to the pipeline making them available
        //         // to the next cmdlet. The second argument of this call tells
        //         // PowerShell to enumerate the array, and send one process at a
        //         // time to the pipeline.
        //         WriteObject(Process.GetProcesses(), true);
        //     }
        //     else
        //     {
        //         // If process names are passed to the cmdlet, get and write
        //         // the associated processes.
        //         foreach (string name in processNames)
        //         {
        //             WriteObject(Process.GetProcessesByName(name), true);
        //         } // End foreach (string name...).
        //     }
        // }
    }
}
