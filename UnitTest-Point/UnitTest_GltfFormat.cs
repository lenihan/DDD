using System;
using System.Collections.Generic;
using System.IO;
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
            DDD.Mesh roundTripped = DDD.GltfFormat.Parse(glb).OfType<DDD.Mesh>().Single();

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
            DDD.Mesh roundTripped = DDD.GltfFormat.Parse(glb).OfType<DDD.Mesh>().Single();

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
            DDD.Mesh roundTripped = DDD.GltfFormat.Parse(glb).OfType<DDD.Mesh>().Single();

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
            DDD.Mesh roundTripped = DDD.GltfFormat.Parse(glb).OfType<DDD.Mesh>().Single();

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
            DDD.Mesh roundTripped = DDD.GltfFormat.Parse(glb).OfType<DDD.Mesh>().Single();

            Assert.IsTrue(roundTripped.Material.HasValue);
            DDD.Material rt = roundTripped.Material!.Value;
            Assert.AreEqual(original.Diffuse, rt.Diffuse, 1e-6);
            Assert.AreEqual(original.Specular, rt.Specular, 1e-6);
            Assert.AreEqual(original.Shininess, rt.Shininess, 1e-6);
        }

        static void AssertVectorsEqual(DDD.Vector expected, DDD.Vector actual, double tolerance)
        {
            Assert.AreEqual(expected.X, actual.X, tolerance, "X");
            Assert.AreEqual(expected.Y, actual.Y, tolerance, "Y");
            Assert.AreEqual(expected.Z, actual.Z, tolerance, "Z");
        }

        [TestMethod]
        public void RoundTripsAPointLight()
        {
            var light = new DDD.Light(new DDD.Point(1, 2, 3), 1.5);
            var objects = new List<object> { light };

            byte[] glb = DDD.GltfFormat.Serialize(objects);
            DDD.Light roundTripped = DDD.GltfFormat.Parse(glb).OfType<DDD.Light>().Single();

            Assert.AreEqual(DDD.LightKind.Point, roundTripped.Kind);
            Assert.AreEqual(new DDD.Point(1, 2, 3), roundTripped.Position);
            Assert.AreEqual(1.5, roundTripped.Intensity, 1e-9);
        }

        [TestMethod]
        public void RoundTripsADirectionalLightAlreadyAlignedWithLocalMinusZ()
        {
            // direction = (0,0,-1) exactly matches glTF's local -Z convention, so
            // QuaternionFromDirection takes the identity shortcut - no floating-point rotation
            // math involved, so this should round-trip exactly, not just within tolerance.
            var light = new DDD.Light(new DDD.Vector(0, 0, -1), 0.75);
            var objects = new List<object> { light };

            byte[] glb = DDD.GltfFormat.Serialize(objects);
            DDD.Light roundTripped = DDD.GltfFormat.Parse(glb).OfType<DDD.Light>().Single();

            Assert.AreEqual(DDD.LightKind.Directional, roundTripped.Kind);
            Assert.AreEqual(new DDD.Vector(0, 0, -1), roundTripped.Direction);
            Assert.AreEqual(0.75, roundTripped.Intensity, 1e-9);
        }

        [TestMethod]
        public void RoundTripsADirectionalLightAtAGenericAngle()
        {
            // A direction with no special relationship to -Z - exercises the general shortest-
            // arc quaternion path in both QuaternionFromDirection and RotateByQuaternion.
            var light = new DDD.Light(new DDD.Vector(1, 2, 3), 1.0);
            var objects = new List<object> { light };

            byte[] glb = DDD.GltfFormat.Serialize(objects);
            DDD.Light roundTripped = DDD.GltfFormat.Parse(glb).OfType<DDD.Light>().Single();

            AssertVectorsEqual(DDD.Vector.Normalize(new DDD.Vector(1, 2, 3)), roundTripped.Direction, 1e-9);
        }

        [TestMethod]
        public void RoundTripsADirectionalLightExactlyOppositeLocalMinusZ()
        {
            // direction = (0,0,1) is the 180-degree edge case QuaternionFromDirection special-
            // cases (dot < -0.999999), since the general shortest-arc formula is singular there.
            var light = new DDD.Light(new DDD.Vector(0, 0, 1), 1.0);
            var objects = new List<object> { light };

            byte[] glb = DDD.GltfFormat.Serialize(objects);
            DDD.Light roundTripped = DDD.GltfFormat.Parse(glb).OfType<DDD.Light>().Single();

            AssertVectorsEqual(new DDD.Vector(0, 0, 1), roundTripped.Direction, 1e-9);
        }

        [TestMethod]
        public void RoundTripsASpotLight()
        {
            var light = new DDD.Light(new DDD.Point(1, 0, 0), new DDD.Vector(0, -1, 0),
                outerConeAngleDegrees: 45, innerConeAngleDegrees: 10, intensity: 2.0);
            var objects = new List<object> { light };

            byte[] glb = DDD.GltfFormat.Serialize(objects);
            DDD.Light roundTripped = DDD.GltfFormat.Parse(glb).OfType<DDD.Light>().Single();

            Assert.AreEqual(DDD.LightKind.Spot, roundTripped.Kind);
            Assert.AreEqual(new DDD.Point(1, 0, 0), roundTripped.Position);
            AssertVectorsEqual(new DDD.Vector(0, -1, 0), roundTripped.Direction, 1e-9);
            Assert.AreEqual(45.0, roundTripped.OuterConeAngleDegrees, 1e-6);
            Assert.AreEqual(10.0, roundTripped.InnerConeAngleDegrees, 1e-6);
            Assert.AreEqual(2.0, roundTripped.Intensity, 1e-9);
        }

        [TestMethod]
        public void RoundTripsMultipleMeshesEachWithItsOwnMaterial()
        {
            DDD.Mesh a = Triangle();
            // metallic=1, roughness=0 boundary case - see the comment on the single-mesh material
            // tests above for why this round-trips exactly rather than needing tolerance.
            a.Material = new DDD.Material(new DDD.Color(255, 0, 0), ambient: 0.2, diffuse: 0.0,
                specular: 0.8, shininess: 128.0);
            DDD.Mesh b = DDD.Primitives.Box(2, 2, 2); // no material

            byte[] glb = DDD.GltfFormat.Serialize(new List<object> { a, b });
            List<DDD.Mesh> roundTripped = DDD.GltfFormat.Parse(glb).OfType<DDD.Mesh>().ToList();

            Assert.AreEqual(2, roundTripped.Count);
            DDD.Mesh rtA = roundTripped[0];
            DDD.Mesh rtB = roundTripped[1];

            Assert.AreEqual(3, rtA.Vertices.Count);
            Assert.IsTrue(rtA.Material.HasValue);
            Assert.AreEqual(a.Material.Value.Color, rtA.Material!.Value.Color);
            Assert.AreEqual(0.8, rtA.Material!.Value.Specular, 1e-9);

            Assert.IsFalse(rtB.Material.HasValue);
            Assert.AreEqual(b.Vertices.Count, rtB.Vertices.Count);
            Assert.AreEqual(b.Faces.Count, rtB.Faces.Count);
        }

        static byte[] BuildFloatBuffer(params float[][] vec3s)
        {
            using var stream = new MemoryStream();
            foreach (float[] v in vec3s)
            {
                foreach (float f in v)
                {
                    stream.Write(BitConverter.GetBytes(f));
                }
            }
            return stream.ToArray();
        }

        // Packs a JSON string + binary chunk into a minimal .glb container, independently of
        // GltfFormat's own writer (which never emits a non-identity transform on a mesh node -
        // see its own comment on why) - so ImportBakesANodesTranslationRotationAndScaleIntoMeshVertices
        // below exercises the reader against bytes it didn't produce itself, the way a real
        // external tool's export would.
        static byte[] BuildGlb(string json, byte[] bin)
        {
            byte[] jsonBytes = Encoding.UTF8.GetBytes(json);
            int jsonPadded = (jsonBytes.Length + 3) / 4 * 4;
            int binPadded = (bin.Length + 3) / 4 * 4;
            int totalLength = 12 + 8 + jsonPadded + 8 + binPadded;

            using var stream = new MemoryStream();
            using var writer = new BinaryWriter(stream);
            writer.Write(0x46546C67u); // "glTF"
            writer.Write(2u);
            writer.Write((uint)totalLength);

            writer.Write((uint)jsonPadded);
            writer.Write(0x4E4F534Au); // "JSON"
            writer.Write(jsonBytes);
            for (int i = jsonBytes.Length; i < jsonPadded; i++) writer.Write((byte)0x20);

            writer.Write((uint)binPadded);
            writer.Write(0x004E4942u); // "BIN\0"
            writer.Write(bin);
            for (int i = bin.Length; i < binPadded; i++) writer.Write((byte)0);

            return stream.ToArray();
        }

        [TestMethod]
        public void ImportBakesANodesTranslationRotationAndScaleIntoMeshVertices()
        {
            // scale (2,1,1) then translate (10,0,0): local (1,0,0) -> (2,0,0) -> (12,0,0).
            // Normal uses the reciprocal scale (see TransformNormal): local (1,1,0)/sqrt(2)
            // rescaled by (1/2,1,1) and renormalized lands on the clean value (1,2,0)/sqrt(5).
            float[] localPosition = { 1.0f, 0.0f, 0.0f };
            float[] localNormal = { (float)(1.0 / Math.Sqrt(2)), (float)(1.0 / Math.Sqrt(2)), 0.0f };
            byte[] bin = BuildFloatBuffer(localPosition, localNormal);

            string json = $$"""
            {
              "asset": { "version": "2.0" },
              "scene": 0,
              "scenes": [ { "nodes": [0] } ],
              "nodes": [ { "mesh": 0, "translation": [10.0, 0.0, 0.0], "scale": [2.0, 1.0, 1.0] } ],
              "meshes": [ { "primitives": [ { "attributes": { "POSITION": 0, "NORMAL": 1 }, "mode": 4 } ] } ],
              "accessors": [
                { "bufferView": 0, "componentType": 5126, "count": 1, "type": "VEC3" },
                { "bufferView": 1, "componentType": 5126, "count": 1, "type": "VEC3" }
              ],
              "bufferViews": [
                { "buffer": 0, "byteOffset": 0, "byteLength": 12 },
                { "buffer": 0, "byteOffset": 12, "byteLength": 12 }
              ],
              "buffers": [ { "byteLength": {{bin.Length}} } ]
            }
            """;

            byte[] glb = BuildGlb(json, bin);
            DDD.Mesh mesh = DDD.GltfFormat.Parse(glb).OfType<DDD.Mesh>().Single();

            Assert.AreEqual(1, mesh.Vertices.Count);
            DDD.Point position = mesh.Vertices[0].Position;
            AssertVectorsEqual(new DDD.Vector(12, 0, 0), new DDD.Vector(position.X, position.Y, position.Z), 1e-6);

            Assert.IsTrue(mesh.Vertices[0].Normal.HasValue);
            AssertVectorsEqual(DDD.Vector.Normalize(new DDD.Vector(1, 2, 0)), mesh.Vertices[0].Normal!.Value, 1e-6);
        }

        [TestMethod]
        public void RoundTripsAMeshAndALightTogether()
        {
            var objects = new List<object> { Triangle(), new DDD.Light(new DDD.Point(0, 5, 0)) };

            byte[] glb = DDD.GltfFormat.Serialize(objects);
            List<object> roundTripped = DDD.GltfFormat.Parse(glb);

            Assert.AreEqual(1, roundTripped.OfType<DDD.Mesh>().Count());
            Assert.AreEqual(1, roundTripped.OfType<DDD.Light>().Count());
        }

        [TestMethod]
        public void SerializeWritesTheExtensionsUsedDeclarationOnlyWhenThereAreLights()
        {
            byte[] withLight = DDD.GltfFormat.Serialize(new List<object> { new DDD.Light(new DDD.Vector(0, 1, 0)) });
            using JsonDocument withLightDoc = JsonDocument.Parse(ExtractJsonChunk(withLight));
            string[] extensionsUsed = withLightDoc.RootElement.GetProperty("extensionsUsed").EnumerateArray().Select(e => e.GetString()!).ToArray();
            string[] expected = { "KHR_lights_punctual" };
            CollectionAssert.AreEqual(expected, extensionsUsed);

            byte[] meshOnly = DDD.GltfFormat.Serialize(Triangle());
            using JsonDocument meshOnlyDoc = JsonDocument.Parse(ExtractJsonChunk(meshOnly));
            Assert.IsFalse(meshOnlyDoc.RootElement.TryGetProperty("extensionsUsed", out _));
        }

        [TestMethod]
        public void RoundTripsAPerspectiveCameraAlignedWithLocalMinusZ()
        {
            // Looking straight down -Z with the default up exactly matches glTF's local -Z/+Y
            // convention, so QuaternionFromBasis lands on the identity quaternion - no floating-
            // point rotation math involved, so this round-trips exactly, not just within tolerance.
            var camera = new DDD.Camera(new DDD.Point(0, 0, 5), new DDD.Point(0, 0, 0),
                perspective: true, fovYDegrees: 50.0, nearPlane: 0.5, farPlane: 200.0);

            byte[] glb = DDD.GltfFormat.Serialize(new List<object> { camera });
            DDD.Camera roundTripped = DDD.GltfFormat.Parse(glb).OfType<DDD.Camera>().Single();

            Assert.IsTrue(roundTripped.Perspective);
            Assert.AreEqual(new DDD.Point(0, 0, 5), roundTripped.Position);
            Assert.AreEqual(new DDD.Point(0, 0, 4), roundTripped.LookAt); // synthesized 1 unit along forward - see DefaultLookAtDistance
            Assert.AreEqual(new DDD.Vector(0, 1, 0), roundTripped.Up);
            Assert.AreEqual(50.0, roundTripped.FovYDegrees, 1e-6);
            Assert.AreEqual(0.5, roundTripped.NearPlane, 1e-9);
            Assert.AreEqual(200.0, roundTripped.FarPlane, 1e-9);
        }

        [TestMethod]
        public void RoundTripsAPerspectiveCameraLookingAlongTheXAxis()
        {
            // forward=(-1,0,0), up=(0,1,0) - exercises QuaternionFromBasis's general (non-
            // identity) path, still landing on exact fractions of a right angle, so tight
            // tolerance rather than an exact match is enough.
            var camera = new DDD.Camera(new DDD.Point(5, 0, 0), new DDD.Point(0, 0, 0));

            byte[] glb = DDD.GltfFormat.Serialize(new List<object> { camera });
            DDD.Camera roundTripped = DDD.GltfFormat.Parse(glb).OfType<DDD.Camera>().Single();

            Assert.AreEqual(new DDD.Point(5, 0, 0), roundTripped.Position);
            DDD.Vector forward = DDD.Vector.Normalize(roundTripped.LookAt - roundTripped.Position);
            AssertVectorsEqual(new DDD.Vector(-1, 0, 0), forward, 1e-9);
            AssertVectorsEqual(new DDD.Vector(0, 1, 0), roundTripped.Up, 1e-9);
        }

        [TestMethod]
        public void RoundTripsAnOrthographicCamera()
        {
            var camera = new DDD.Camera(new DDD.Point(0, 0, 5), new DDD.Point(0, 0, 0),
                perspective: false, orthographicHeight: 3.5, nearPlane: 0.1, farPlane: 50.0);

            byte[] glb = DDD.GltfFormat.Serialize(new List<object> { camera });
            DDD.Camera roundTripped = DDD.GltfFormat.Parse(glb).OfType<DDD.Camera>().Single();

            Assert.IsFalse(roundTripped.Perspective);
            Assert.AreEqual(3.5, roundTripped.OrthographicHeight, 1e-9);
            Assert.AreEqual(0.1, roundTripped.NearPlane, 1e-9);
            Assert.AreEqual(50.0, roundTripped.FarPlane, 1e-9);
        }

        [TestMethod]
        public void RoundTripsACameraLookingStraightUpWithoutThrowing()
        {
            // forward=(0,1,0) is parallel to the default up (0,1,0) - the degenerate case
            // QuaternionFromBasis falls back for. Roll is undefined here, so only forward (not
            // Up) is checked.
            var camera = new DDD.Camera(new DDD.Point(0, 0, 0), new DDD.Point(0, 5, 0));

            byte[] glb = DDD.GltfFormat.Serialize(new List<object> { camera });
            DDD.Camera roundTripped = DDD.GltfFormat.Parse(glb).OfType<DDD.Camera>().Single();

            DDD.Vector forward = DDD.Vector.Normalize(roundTripped.LookAt - roundTripped.Position);
            AssertVectorsEqual(new DDD.Vector(0, 1, 0), forward, 1e-9);
        }

        [TestMethod]
        public void RoundTripsAMeshLightAndCameraTogether()
        {
            var objects = new List<object>
            {
                Triangle(),
                new DDD.Light(new DDD.Point(0, 5, 0)),
                new DDD.Camera(new DDD.Point(0, 0, 5), new DDD.Point(0, 0, 0)),
            };

            byte[] glb = DDD.GltfFormat.Serialize(objects);
            List<object> roundTripped = DDD.GltfFormat.Parse(glb);

            Assert.AreEqual(1, roundTripped.OfType<DDD.Mesh>().Count());
            Assert.AreEqual(1, roundTripped.OfType<DDD.Light>().Count());
            Assert.AreEqual(1, roundTripped.OfType<DDD.Camera>().Count());
        }

        [TestMethod]
        public void SerializeWritesTheExpectedPerspectiveCameraFields()
        {
            var camera = new DDD.Camera(new DDD.Point(0, 0, 5), new DDD.Point(0, 0, 0),
                perspective: true, fovYDegrees: 60.0, nearPlane: 0.25, farPlane: 500.0);

            byte[] glb = DDD.GltfFormat.Serialize(new List<object> { camera });
            using JsonDocument doc = JsonDocument.Parse(ExtractJsonChunk(glb));

            JsonElement gltfCamera = doc.RootElement.GetProperty("cameras")[0];
            Assert.AreEqual("perspective", gltfCamera.GetProperty("type").GetString());
            JsonElement perspective = gltfCamera.GetProperty("perspective");
            Assert.AreEqual(60.0 * Math.PI / 180.0, perspective.GetProperty("yfov").GetDouble(), 1e-9);
            Assert.AreEqual(0.25, perspective.GetProperty("znear").GetDouble(), 1e-9);
            Assert.AreEqual(500.0, perspective.GetProperty("zfar").GetDouble(), 1e-9);

            JsonElement node = doc.RootElement.GetProperty("nodes").EnumerateArray().Single(n => n.TryGetProperty("camera", out _));
            Assert.AreEqual(0, node.GetProperty("camera").GetInt32());
        }
    }
}
