using System.Management.Automation;

namespace DDD
{
    // Assign the result to a Mesh's Material property, e.g. $mesh.Material = New-Material -Specular 0.5
    [Cmdlet(VerbsCommon.New, "Material")]
    [OutputType(typeof(Material))]
    public class NewMaterialCommand : Cmdlet
    {
        [Parameter()]
        public Color Color { get; set; } = new Color(200, 200, 200);

        [Parameter()]
        public double Ambient { get; set; } = 0.2;

        [Parameter()]
        public double Diffuse { get; set; } = 0.8;

        [Parameter()]
        public double Specular { get; set; }

        [Parameter()]
        public double Shininess { get; set; } = 16.0;

        protected override void EndProcessing()
        {
            WriteObject(new Material(Color, Ambient, Diffuse, Specular, Shininess));
        }
    }
}
