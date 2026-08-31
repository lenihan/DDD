using System;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DDD_UnitTest
{
    [TestClass]
    public class GltfFormat
    {
        static DDD.Mesh Triangle()
        {
            var mesh = new DDD.Mesh();
            int a = mesh.AddVertex(new DDD.Vertex(new DDD.Point(1, 0, 0), new DDD.Vector(0, 0, 1), new DDD.Color(255, 0, 0)));
            int b = mesh.AddVertex(new DDD.Vertex(new DDD.Point(0, 1, 0), new DDD.Vector(0, 0, 1), new DDD.Color(0, 255, 0)));
            int c = mesh.AddVertex(new DDD.Vertex(new DDD.Point(0, 0, 0), new DDD.Vector(0, 0, 1), new DDD.Color(0, 0, 255)));
            mesh.AddFace(a, b, c);
            return mesh;
        }

        [TestMethod]
        public void SerializeProducesAValidGlbHeader()
        {
            byte[] glb = DDD.GltfFormat.Serialize(Triangle());

            Assert.IsTrue(glb.Length >= 12);
            Assert.AreEqual("glTF", Encoding.ASCII.GetString(glb, 0, 4));
            uint version = BitConverter.ToUInt32(glb, 4);
            Assert.AreEqual(2u, version);
            uint declaredLength = BitConverter.ToUInt32(glb, 8);
            Assert.AreEqual((uint)glb.Length, declaredLength);
        }

        [TestMethod]
        public void SerializeThenParseRoundTripsPositionsNormalsColorsAndFaces()
        {
            DDD.Mesh mesh = Triangle();

            byte[] glb = DDD.GltfFormat.Serialize(mesh);
            DDD.Mesh roundTripped = DDD.GltfFormat.Parse(glb);

            Assert.AreEqual(3, roundTripped.Vertices.Count);
            for (int i = 0; i < mesh.Vertices.Count; i++)
            {
                Assert.AreEqual(mesh.Vertices[i].Position, roundTripped.Vertices[i].Position, $"Position mismatch at {i}");
                Assert.AreEqual(mesh.Vertices[i].Normal, roundTripped.Vertices[i].Normal, $"Normal mismatch at {i}");
                Assert.AreEqual(mesh.Vertices[i].Color, roundTripped.Vertices[i].Color, $"Color mismatch at {i}");
            }
            Assert.AreEqual(1, roundTripped.Faces.Count);
            Assert.AreEqual(mesh.Faces[0], roundTripped.Faces[0]);
        }

        [TestMethod]
        public void RoundTripsAMeshWithNoNormalsOrColors()
        {
            var mesh = new DDD.Mesh();
            int a = mesh.AddVertex(new DDD.Point(1, 0, 0));
            int b = mesh.AddVertex(new DDD.Point(0, 1, 0));
            int c = mesh.AddVertex(new DDD.Point(0, 0, 0));
            mesh.AddFace(a, b, c);

            byte[] glb = DDD.GltfFormat.Serialize(mesh);
            DDD.Mesh roundTripped = DDD.GltfFormat.Parse(glb);

            Assert.AreEqual(3, roundTripped.Vertices.Count);
            Assert.IsNull(roundTripped.Vertices[0].Normal);
            Assert.IsNull(roundTripped.Vertices[0].Color);
            Assert.AreEqual(new DDD.Point(1, 0, 0), roundTripped.Vertices[0].Position);
            Assert.AreEqual(1, roundTripped.Faces.Count);
        }

        [TestMethod]
        public void RoundTripsAMeshWithManyTriangles()
        {
            // Exercise a mesh bigger than one triangle - fan-like geometry with shared vertices,
            // more representative of a real primitive than the single-triangle tests above.
            DDD.Mesh mesh = DDD.Primitives.Box(2, 2, 2);

            byte[] glb = DDD.GltfFormat.Serialize(mesh);
            DDD.Mesh roundTripped = DDD.GltfFormat.Parse(glb);

            Assert.AreEqual(mesh.Vertices.Count, roundTripped.Vertices.Count);
            Assert.AreEqual(mesh.Faces.Count, roundTripped.Faces.Count);
            for (int i = 0; i < mesh.Vertices.Count; i++)
            {
                Assert.AreEqual(mesh.Vertices[i].Position, roundTripped.Vertices[i].Position, $"Position mismatch at {i}");
            }
            for (int i = 0; i < mesh.Faces.Count; i++)
            {
                Assert.AreEqual(mesh.Faces[i], roundTripped.Faces[i], $"Face mismatch at {i}");
            }
        }

        [TestMethod]
        public void ThrowsOnBadMagicNumber()
        {
            byte[] bogus = Encoding.ASCII.GetBytes("not-a-glb-file-at-all");
            Assert.ThrowsExactly<FormatException>(() => DDD.GltfFormat.Parse(bogus));
        }

        [TestMethod]
        public void ThrowsOnUnsupportedVersion()
        {
            byte[] glb = DDD.GltfFormat.Serialize(Triangle());
            byte[] tampered = (byte[])glb.Clone();
            tampered[4] = 99; // corrupt the version field (bytes 4-7, little-endian)
            Assert.ThrowsExactly<FormatException>(() => DDD.GltfFormat.Parse(tampered));
        }
    }
}
