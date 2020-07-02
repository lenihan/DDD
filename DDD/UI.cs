using System;
using System.Collections.Generic;
// Defining input from the pipeline
// https://docs.microsoft.com/en-us/powershell/scripting/developer/cmdlet/adding-parameters-that-process-pipeline-input?view=powershell-7

// Samples here:
// C:\Program Files (x86)\Microsoft SDKs\Windows\v7.0\Samples\sysmgmt\WindowsPowerShell\

namespace DDD
{
    class UI
    {
        public void ShowWindow(List<object> Objects, Point BoundingBoxMin, Point BoundingBoxMax, string Title)
        {
            Console.WriteLine($"TODO: Implmented ShowWindow({Objects}, {BoundingBoxMin}, {BoundingBoxMax}, {Title})");
        }
    }
}