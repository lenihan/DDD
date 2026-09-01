using System;
using System.Management.Automation;

namespace DDD
{
    [Cmdlet(VerbsCommon.New, "ScatterPlot")]
    [OutputType(typeof(Point))]
    public class NewScatterPlotCommand : Cmdlet
    {
        [Parameter(Mandatory = true, Position = 0)]
        public double[] Y { get; set; } = Array.Empty<double>();

        // Defaults to the data's index (0, 1, 2, ...) when omitted.
        [Parameter(Position = 1)]
        public double[]? X { get; set; }

        protected override void EndProcessing()
        {
            try
            {
                foreach (Point point in Graphing.ScatterPlot(Y, X))
                {
                    WriteObject(point);
                }
            }
            catch (ArgumentException ex)
            {
                ErrorRecord error = new ErrorRecord(ex, "ScatterPlotLengthMismatch", ErrorCategory.InvalidArgument, null);
                ThrowTerminatingError(error);
            }
        }
    }
}
