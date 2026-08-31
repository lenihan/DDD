using System;
using System.Linq;
using System.Text;
using System.Text.Json;
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

        // Independently walks the .glb container to pull out the JSON chunk text, rather than
        // reusing GltfFormat's own reader - so the material-export tests below check Serialize's
        // output against a genuinely separate parser, not against GltfFormat checking itself.
        static string ExtractJsonChunk(byte[] glb)
        {
            int offset = 12;
            while (offset + 8 <= glb.Length)
            {
                int chunkLength = BitConverter.ToInt32(glb, offset);
                uint chunkType = BitConverter.ToUInt32(glb, offset + 4);
                offset += 8;
                if (chunkType == 0x4E4F534A) // "JSON"
                {
                    return Encoding.UTF8.GetString(glb, offset, chunkLength);
                }
                offset += chunkLength;
            }
            throw new InvalidOperationException("No JSON chunk found in .glb.");
        }

        [TestMethod]
        public void SerializeWritesTheExpectedPbrMetallicRoughnessAndEmissiveFields()
        {
            DDD.Mesh mesh = Triangle();
            // metallic=1, roughness=0 (a fully metallic, mirror-smooth material) - the boundary
            // case for MaterialToGltf's reverse mapping, chosen because the arithmetic involved
            // (0.8-0.2, 0.6/0.6, 128-1 over itself) lands on exact floating-point values, unlike
            // an interior point like metallic=0.5.
            mesh.Material = new DDD.Material(new DDD.Color(255, 0, 0), ambient: 0.2, diffuse: 0.0,
                specular: 0.8, shininess: 128.0, emissive: new DDD.Color(0, 255, 0));

            byte[] glb = DDD.GltfFormat.Serialize(mesh);
            using JsonDocument doc = JsonDocument.Parse(ExtractJsonChunk(glb));

            JsonElement material = doc.RootElement.GetProperty("materials")[0];
            JsonElement pbr = material.GetProperty("pbrMetallicRoughness");

            Assert.AreEqual(1.0, pbr.GetProperty("metallicFactor").GetDouble(), 1e-9);
            Assert.AreEqual(0.0, pbr.GetProperty("roughnessFactor").GetDouble(), 1e-9);

            double[] baseColor = pbr.GetProperty("baseColorFactor").EnumerateArray().Select(e => e.GetDouble()).ToArray();
            double[] expectedBaseColor = { 1.0, 0.0, 0.0, 1.0 };
            CollectionAssert.AreEqual(expectedBaseColor, baseColor);

            double[] emissive = material.GetProperty("emissiveFactor").EnumerateArray().Select(e => e.GetDouble()).ToArray();
            double[] expectedEmissive = { 0.0, 1.0, 0.0 };
            CollectionAssert.AreEqual(expectedEmissive, emissive);
        }

        [TestMethod]
        public void SerializeOmitsEmissiveFactorWhenTheMaterialHasNone()
        {
            DDD.Mesh mesh = Triangle();
            mesh.Material = new DDD.Material(new DDD.Color(200, 200, 200)); // default Emissive = black/none

            byte[] glb = DDD.GltfFormat.Serialize(mesh);
            using JsonDocument doc = JsonDocument.Parse(ExtractJsonChunk(glb));
            JsonElement material = doc.RootElement.GetProperty("materials")[0];

            Assert.IsFalse(material.TryGetProperty("emissiveFactor", out _));
        }

        [TestMethod]
        public void SerializeOmitsMaterialsEntirelyWhenTheMeshHasNoMaterial()
        {
            DDD.Mesh mesh = Triangle(); // no Material assigned

            byte[] glb = DDD.GltfFormat.Serialize(mesh);
            using JsonDocument doc = JsonDocument.Parse(ExtractJsonChunk(glb));

            Assert.IsFalse(doc.RootElement.TryGetProperty("materials", out _));
            JsonElement primitive = doc.RootElement.GetProperty("meshes")[0].GetProperty("primitives")[0];
            Assert.IsFalse(primitive.TryGetProperty("material", out _));
        }

        [TestMethod]
        public void ParseRoundTripsAMaterialThatWasDerivedFromMetallicRoughness()
        {
            // Boundary case (metallic=1, roughness=0) - see the comment on the export test above
            // for why this specific case is safe to compare with floating-point tolerance instead
            // of needing anything fuzzier.
            DDD.Mesh mesh = Triangle();
            var original = new DDD.Material(new DDD.Color(255, 0, 0), ambient: 0.2, diffuse: 0.0,
                specular: 0.8, shininess: 128.0, emissive: new DDD.Color(0, 255, 0));
            mesh.Material = original;

            byte[] glb = DDD.GltfFormat.Serialize(mesh);
            DDD.Mesh roundTripped = DDD.GltfFormat.Parse(glb);

            Assert.IsTrue(roundTripped.Material.HasValue);
            DDD.Material rt = roundTripped.Material!.Value;
            Assert.AreEqual(original.Color, rt.Color);
            Assert.AreEqual(original.Ambient, rt.Ambient, 1e-9);
            Assert.AreEqual(original.Diffuse, rt.Diffuse, 1e-9);
            Assert.AreEqual(original.Specular, rt.Specular, 1e-9);
            Assert.AreEqual(original.Shininess, rt.Shininess, 1e-9);
            Assert.AreEqual(original.Emissive, rt.Emissive);
        }

        [TestMethod]
        public void ParseRoundTripsAMidRangeMetallicRoughnessMaterialWithinTolerance()
        {
            // metallic=0.5, roughness=0.5: an interior point, where the reverse-mapping
            // arithmetic doesn't land on exact binary fractions, so this specifically checks the
            // round trip holds up to floating-point tolerance rather than exactly.
            DDD.Mesh mesh = Triangle();
            var original = new DDD.Material(new DDD.Color(128, 64, 32), ambient: 0.2, diffuse: 0.4,
                specular: 0.5, shininess: 64.5);
            mesh.Material = original;

            byte[] glb = DDD.GltfFormat.Serialize(mesh);
            DDD.Mesh roundTripped = DDD.GltfFormat.Parse(glb);

            Assert.IsTrue(roundTripped.Material.HasValue);
            DDD.Material rt = roundTripped.Material!.Value;
            Assert.AreEqual(original.Diffuse, rt.Diffuse, 1e-6);
            Assert.AreEqual(original.Specular, rt.Specular, 1e-6);
            Assert.AreEqual(original.Shininess, rt.Shininess, 1e-6);
        }
    }
}
