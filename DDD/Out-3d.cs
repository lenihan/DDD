using System;
using System.Collections.Generic;
using System.Management.Automation;             // Windows PowerShell namespace
using System.Runtime.InteropServices;
// Defining input from the pipeline
// https://docs.microsoft.com/en-us/powershell/scripting/developer/cmdlet/adding-parameters-that-process-pipeline-input?view=powershell-7

// Samples here:
// C:\Program Files (x86)\Microsoft SDKs\Windows\v7.0\Samples\sysmgmt\WindowsPowerShell\

namespace DDD
{
#pragma warning disable CA1303 // Do not pass literals as localized parameters
    [Cmdlet(VerbsData.Out, "3d")]
    [Alias("o3d")]
    public class Out3dCommand : Cmdlet
    {
        List<object> _objects = new List<object>();
        string _title = "";
        Point _bboxMin = new Point(0.0, 0.0, 0.0);
        Point _bboxMax = new Point(0.0, 0.0, 0.0);
        
        [Parameter(ValueFromPipeline = true, ValueFromPipelineByPropertyName = true)]
        public object[] InputObject;
        
        [Parameter()]
        public string Title
        {
            get {return _title;}
            set {_title = value;}
        }

        protected override void BeginProcessing()
        {
            // Console.WriteLine("BeginProcessing");
        }
        void UpdateBBox(Point p)
        {
            // bboxMin
            if (p.X < _bboxMin.X) _bboxMin.X = p.X;
            if (p.Y < _bboxMin.Y) _bboxMin.Y = p.Y;
            if (p.Z < _bboxMin.Z) _bboxMin.Z = p.Z;

            // bboxMax
            if (p.X > _bboxMax.X) _bboxMax.X = p.X;
            if (p.Y > _bboxMax.Y) _bboxMax.Y = p.Y;
            if (p.Z > _bboxMax.Z) _bboxMax.Z = p.Z;
        }
        void ProcessObject(object input)
        {
            if (input is Point p)
            {
                _objects.Add(input);
                UpdateBBox(p);
                // Console.WriteLine($"Got point: {p}");
            }
            else if (input is Matrix m)
            {
                _objects.Add(input);
                Point origin = m * new Point(0.0, 0.0, 0.0);
                Point xaxis = m * new Point(1.0, 0.0, 0.0);
                Point yaxis = m * new Point(0.0, 1.0, 0.0);
                Point zaxis = m * new Point(0.0, 0.0, 1.0);
                UpdateBBox(origin);
                UpdateBBox(xaxis);
                UpdateBBox(yaxis);
                UpdateBBox(zaxis);
                // Console.WriteLine($"Got matrix: {origin}");
            }
            else if (input is Vector v)
            {
                _objects.Add(input);
                UpdateBBox(new Point(v.X, v.Y, v.Z));
                // Console.WriteLine($"Got vector: {v}"); 
            }
            else
            {
                // Throw a terminating error for types that are not supported.
                ErrorRecord error = new ErrorRecord(
                    new FormatException("Invalid data type for Out-3d"),
                    "DataNotQualifiedFor3d",
                    ErrorCategory.InvalidType,
                    null);
                this.ThrowTerminatingError(error);
            }
        }
        protected override void ProcessRecord()
        {
            if (InputObject == null)
            {
                // Console.WriteLine("Got null");                
            }
            else if (InputObject.Length == 0)
            {
                // Console.WriteLine("Got empty array of type {0}", InputObject[0].GetType().ToString());
            }
            else if (InputObject.Length == 1) 
            {
                // Console.WriteLine("Got array length 1 of type {0}", InputObject[0].GetType().ToString());
                ProcessObject(InputObject[0]);
            }
            else 
            {   
                // Console.WriteLine("Got array length {0} containing...", InputObject.Length);
                foreach(object obj in InputObject) 
                {
                    // Console.WriteLine("    Type: {0}", obj.GetType().ToString());
                    ProcessObject(obj);
                }
            }
        }
        protected override void EndProcessing()
        {
            if (_objects.Count == 0) return;

// TODO: VERIFY EXCEPTIONS WORK
            try
            {
                // var ui = new UI();
                // if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)) ui = new UIWindows();
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                {
                    new UIWindows().ShowWindow(_objects, _bboxMin, _bboxMax, _title);
                }
            } 
            catch (System.InvalidOperationException ioex)
            {
                ErrorRecord er = new ErrorRecord(
                    new FormatException(ioex.Message),
                    "LastError",
                    ErrorCategory.NotSpecified,
                    null);
                ThrowTerminatingError(er);
            }

            // Console.WriteLine("EndProcessing - DONE");
        }
    }
#pragma warning restore CA1303 // Do not pass literals as localized parameters
}