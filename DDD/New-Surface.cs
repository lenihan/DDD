using System;
using System.Management.Automation;

namespace DDD
{
    [Cmdlet(VerbsCommon.New, "Surface")]
    [OutputType(typeof(Mesh))]
    public class NewSurfaceCommand : Cmdlet
    {
        // A jagged array of rows, e.g. @(@(0,0,0),@(0,1,0),@(0,0,0)) - heights[z][x].
        [Parameter(Mandatory = true, Position = 0)]
        public double[][] Heights { get; set; } = Array.Empty<double[]>();

        [Parameter()]
        public double XSpacing { get; set; } = 1.0;

        [Parameter()]
        public double ZSpacing { get; set; } = 1.0;

        protected override void EndProcessing()
        {
            try
            {
                WriteObject(Graphing.Surface(Heights, XSpacing, ZSpacing));
            }
            catch (ArgumentException ex)
            {
                ErrorRecord error = new ErrorRecord(ex, "SurfaceInvalidGrid", ErrorCategory.InvalidArgument, null);
                ThrowTerminatingError(error);
            }
        }
    }
}
