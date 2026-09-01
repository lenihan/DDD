using System;
using System.Management.Automation;

namespace DDD
{
    [Cmdlet(VerbsCommon.New, "BarChart")]
    [OutputType(typeof(Mesh))]
    public class NewBarChartCommand : Cmdlet
    {
        [Parameter(Mandatory = true, Position = 0)]
        public double[] Y { get; set; } = Array.Empty<double>();

        // Defaults to the data's index (0, 1, 2, ...) when omitted.
        [Parameter(Position = 1)]
        public double[]? X { get; set; }

        [Parameter()]
        public double BarWidth { get; set; } = 0.6;

        protected override void EndProcessing()
        {
            try
            {
                foreach (Mesh bar in Graphing.BarChart(Y, X, BarWidth))
                {
                    WriteObject(bar);
                }
            }
            catch (ArgumentException ex)
            {
                ErrorRecord error = new ErrorRecord(ex, "BarChartLengthMismatch", ErrorCategory.InvalidArgument, null);
                ThrowTerminatingError(error);
            }
        }
    }
}
