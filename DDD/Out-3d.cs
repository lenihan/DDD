using System.Management.Automation;             //Windows PowerShell namespace

namespace DDD
{
    [Cmdlet(VerbsData.Out, "3d")]
    public class Out3dCommand : Cmdlet
    {
        // Declare the parameters for the cmdlet.
        [Parameter(Mandatory = true)]
        public string Name
        {
            get { return name; }
            set { name = value; }
        }
        private string name;

        // Override the ProcessRecord method to process
        // the supplied user name and write out a
        // greeting to the user by calling the WriteObject
        // method.
        protected override void ProcessRecord()
        {
            WriteObject("Hello " + name + "!");
        }
    }
}
