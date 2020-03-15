using System.Management.Automation;             //Windows PowerShell namespace

namespace DDD
{
    [Cmdlet(VerbsData.Out, "3d")]
    public class Out3dCommand : Cmdlet
    {
        // // Declare the parameters for the cmdlet.
        // [Parameter(Mandatory = true)]
        // public string Name
        // {
        //     get { return name; }
        //     set { name = value; }
        // }
        // private string name;

        // // Override the ProcessRecord method to process
        // // the supplied user name and write out a
        // // greeting to the user by calling the WriteObject
        // // method.
        // protected override void ProcessRecord()
        // {
        //     WriteObject("Hello " + name + "!");
        // }
        
        [Parameter(
            Position = 0,
            ValueFromPipeline = true,
            ValueFromPipelineByPropertyName = true)]
        [ValidateNotNullOrEmpty]
        public DDD.Point[] Points
        {
            get { return this.Points; }
            set { this.Points = value; }
        }
        protected override void ProcessRecord()
        {
            try 
            {
                WriteObject("HOWDY!!!");
                // WriteObject(Points);
                // WriteObject(Points, false);
                // WriteObject(Points, true);
            }
            catch (System.Exception e)
            {
                System.Console.WriteLine($"Exception: {e}");
            }
        }
    }
}
