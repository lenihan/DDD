using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DDD_UnitTest
{
    [TestClass]
    public class PlyFormat
    {
        // Hand-builds a binary .ply byte buffer (header text + tightly-packed binary body) so the
        // binary reader can be verified against a known-correct byte layout, the same way the
        // ASCII tests use hand-written text. Vertices are (float x,y,z [,uchar red,green,blue]);
        // faces are (uchar count, int32 indices...).
        static byte[] BuildBinaryPly(bool bigEndian, (float X, float Y, float Z, byte R, byte G, byte B)[] vertices,
            bool includeColor, int[][] faces)
        {
            string endian = bigEndian ? "binary_big_endian" : "binary_little_endian";
            var header = new StringBuilder();
            header.Append("ply\n");
            header.Append("format ").Append(endian).Append(" 1.0\n");
            header.Append("element vertex ").Append(vertices.Length).Append('\n');
            header.Append("property float x\n");
            header.Append("property float y\n");
            header.Append("property float z\n");
            if (includeColor)
            {
                header.Append("property uchar red\n");
                header.Append("property uchar green\n");
                header.Append("property uchar blue\n");
            }
            header.Append("element face ").Append(faces.Length).Append('\n');
            header.Append("property list uchar int vertex_indices\n");
            header.Append("end_header\n");

            var bytes = new List<byte>(Encoding.ASCII.GetBytes(header.ToString()));

            byte[] FloatBytes(float f)
            {
                var buf = new byte[4];
                if (bigEndian) BinaryPrimitives.WriteSingleBigEndian(buf, f);
                else BinaryPrimitives.WriteSingleLittleEndian(buf, f);
                return buf;
            }
            byte[] Int32Bytes(int i)
            {
                var buf = new byte[4];
                if (bigEndian) BinaryPrimitives.WriteInt32BigEndian(buf, i);
                else BinaryPrimitives.WriteInt32LittleEndian(buf, i);
                return buf;
            }

            foreach (var v in vertices)
            {
                bytes.AddRange(FloatBytes(v.X));
                bytes.AddRange(FloatBytes(v.Y));
                bytes.AddRange(FloatBytes(v.Z));
                if (includeColor)
                {
                    bytes.Add(v.R);
                    bytes.Add(v.G);
                    bytes.Add(v.B);
                }
            }

            foreach (int[] face in faces)
            {
                bytes.Add((byte)face.Length);
                foreach (int index in face)
                {
                    bytes.AddRange(Int32Bytes(index));
                }
            }

            return bytes.ToArray();
        }

        [TestMethod]
        public void ParsesABinaryLittleEndianTriangle()
        {
            var vertices = new[]
            {
                (1f, 0f, 0f, (byte)0, (byte)0, (byte)0),
                (0f, 1f, 0f, (byte)0, (byte)0, (byte)0),
                (0f, 0f, 0f, (byte)0, (byte)0, (byte)0),
            };
            int[][] faces = { new[] { 0, 1, 2 } };
            byte[] bytes = BuildBinaryPly(bigEndian: false, vertices, includeColor: false, faces);

            DDD.Mesh mesh = DDD.PlyFormat.Parse(bytes);

            Assert.AreEqual(3, mesh.Vertices.Count);
            Assert.AreEqual(new DDD.Point(1, 0, 0), mesh.Vertices[0].Position);
            Assert.AreEqual(new DDD.Point(0, 1, 0), mesh.Vertices[1].Position);
            Assert.AreEqual(new DDD.Point(0, 0, 0), mesh.Vertices[2].Position);
            Assert.AreEqual(1, mesh.Faces.Count);
            Assert.AreEqual(new DDD.Face(0, 1, 2), mesh.Faces[0]);
        }

        [TestMethod]
        public void ParsesABinaryBigEndianTriangleWithColor()
        {
            var vertices = new[]
            {
                (1f, 0f, 0f, (byte)255, (byte)0, (byte)0),
                (0f, 1f, 0f, (byte)0, (byte)255, (byte)0),
                (0f, 0f, 0f, (byte)0, (byte)0, (byte)255),
            };
            int[][] faces = { new[] { 0, 1, 2 } };
            byte[] bytes = BuildBinaryPly(bigEndian: true, vertices, includeColor: true, faces);

            DDD.Mesh mesh = DDD.PlyFormat.Parse(bytes);

            Assert.AreEqual(new DDD.Point(1, 0, 0), mesh.Vertices[0].Position);
            Assert.AreEqual(new DDD.Color(255, 0, 0), mesh.Vertices[0].Color);
            Assert.AreEqual(new DDD.Color(0, 255, 0), mesh.Vertices[1].Color);
            Assert.AreEqual(new DDD.Color(0, 0, 255), mesh.Vertices[2].Color);
        }

        [TestMethod]
        public void FanTriangulatesABinaryQuadFace()
        {
            var vertices = new[]
            {
                (0f, 0f, 0f, (byte)0, (byte)0, (byte)0),
                (1f, 0f, 0f, (byte)0, (byte)0, (byte)0),
                (1f, 1f, 0f, (byte)0, (byte)0, (byte)0),
                (0f, 1f, 0f, (byte)0, (byte)0, (byte)0),
            };
            int[][] faces = { new[] { 0, 1, 2, 3 } };
            byte[] bytes = BuildBinaryPly(bigEndian: false, vertices, includeColor: false, faces);

            DDD.Mesh mesh = DDD.PlyFormat.Parse(bytes);

            Assert.AreEqual(2, mesh.Faces.Count);
            Assert.AreEqual(new DDD.Face(0, 1, 2), mesh.Faces[0]);
            Assert.AreEqual(new DDD.Face(0, 2, 3), mesh.Faces[1]);
        }

        [TestMethod]
        public void ParsesAMinimalTriangleWithNoOptionalProperties()
        {
            string[] lines =
            {
                "ply",
                "format ascii 1.0",
                "element vertex 3",
                "property float x",
                "property float y",
                "property float z",
                "element face 1",
                "property list uchar int vertex_indices",
                "end_header",
                "0 0 0",
                "1 0 0",
                "0 1 0",
                "3 0 1 2",
            };

            DDD.Mesh mesh = DDD.PlyFormat.Parse(lines);

            Assert.AreEqual(3, mesh.Vertices.Count);
            Assert.AreEqual(new DDD.Point(0, 0, 0), mesh.Vertices[0].Position);
            Assert.AreEqual(new DDD.Point(1, 0, 0), mesh.Vertices[1].Position);
            Assert.AreEqual(new DDD.Point(0, 1, 0), mesh.Vertices[2].Position);
            Assert.IsNull(mesh.Vertices[0].Normal);
            Assert.IsNull(mesh.Vertices[0].Color);

            Assert.AreEqual(1, mesh.Faces.Count);
            Assert.AreEqual(new DDD.Face(0, 1, 2), mesh.Faces[0]);
        }

        [TestMethod]
        public void ParsesVertexNormalsAndColors()
        {
            string[] lines =
            {
                "ply",
                "format ascii 1.0",
                "element vertex 1",
                "property float x",
                "property float y",
                "property float z",
                "property float nx",
                "property float ny",
                "property float nz",
                "property uchar red",
                "property uchar green",
                "property uchar blue",
                "element face 0",
                "property list uchar int vertex_indices",
                "end_header",
                "1 2 3 0 1 0 255 128 0",
            };

            DDD.Mesh mesh = DDD.PlyFormat.Parse(lines);

            Assert.AreEqual(new DDD.Point(1, 2, 3), mesh.Vertices[0].Position);
            Assert.AreEqual(new DDD.Vector(0, 1, 0), mesh.Vertices[0].Normal);
            Assert.AreEqual(new DDD.Color(255, 128, 0), mesh.Vertices[0].Color);
        }

        [TestMethod]
        public void IgnoresUnrecognizedVertexPropertiesRegardlessOfColumnOrder()
        {
            string[] lines =
            {
                "ply",
                "format ascii 1.0",
                "element vertex 1",
                "property float x",
                "property float y",
                "property float z",
                "property float confidence",
                "property uchar red",
                "property uchar green",
                "property uchar blue",
                "property float nx",
                "property float ny",
                "property float nz",
                "element face 0",
                "property list uchar int vertex_indices",
                "end_header",
                "1 2 3 0.9 10 20 30 0 0 1",
            };

            DDD.Mesh mesh = DDD.PlyFormat.Parse(lines);

            Assert.AreEqual(new DDD.Point(1, 2, 3), mesh.Vertices[0].Position);
            Assert.AreEqual(new DDD.Color(10, 20, 30), mesh.Vertices[0].Color);
            Assert.AreEqual(new DDD.Vector(0, 0, 1), mesh.Vertices[0].Normal);
        }

        [TestMethod]
        public void FanTriangulatesAFaceWithMoreThanThreeVertices()
        {
            string[] lines =
            {
                "ply",
                "format ascii 1.0",
                "element vertex 4",
                "property float x",
                "property float y",
                "property float z",
                "element face 1",
                "property list uchar int vertex_indices",
                "end_header",
                "0 0 0",
                "1 0 0",
                "1 1 0",
                "0 1 0",
                "4 0 1 2 3",
            };

            DDD.Mesh mesh = DDD.PlyFormat.Parse(lines);

            Assert.AreEqual(2, mesh.Faces.Count);
            Assert.AreEqual(new DDD.Face(0, 1, 2), mesh.Faces[0]);
            Assert.AreEqual(new DDD.Face(0, 2, 3), mesh.Faces[1]);
        }

        [TestMethod]
        public void ThrowsWhenFormatIsNotAscii()
        {
            string[] lines = { "ply", "format binary_little_endian 1.0" };
            Assert.ThrowsExactly<FormatException>(() => DDD.PlyFormat.Parse(lines));
        }

        [TestMethod]
        public void ThrowsWhenFirstLineIsNotPly()
        {
            string[] lines = { "not a ply file" };
            Assert.ThrowsExactly<FormatException>(() => DDD.PlyFormat.Parse(lines));
        }

        [TestMethod]
        public void SerializeThenParseRoundTripsPositionsNormalsColorsAndFaces()
        {
            var mesh = new DDD.Mesh();
            int a = mesh.AddVertex(new DDD.Vertex(new DDD.Point(0, 0, 0), new DDD.Vector(0, 0, 1), new DDD.Color(255, 0, 0)));
            int b = mesh.AddVertex(new DDD.Vertex(new DDD.Point(1, 0, 0), new DDD.Vector(0, 0, 1), new DDD.Color(0, 255, 0)));
            int c = mesh.AddVertex(new DDD.Vertex(new DDD.Point(0, 1, 0), new DDD.Vector(0, 0, 1), new DDD.Color(0, 0, 255)));
            mesh.AddFace(a, b, c);

            string serialized = DDD.PlyFormat.Serialize(mesh);
            DDD.Mesh roundTripped = DDD.PlyFormat.Parse(serialized.Split('\n'));

            Assert.AreEqual(mesh.Vertices.Count, roundTripped.Vertices.Count);
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
        public void SerializeOmitsNormalAndColorColumnsWhenNoVertexHasThem()
        {
            var mesh = new DDD.Mesh();
            mesh.AddVertex(new DDD.Point(1, 2, 3));

            string ply = DDD.PlyFormat.Serialize(mesh);
            string[] lines = ply.Split('\n');
            int endHeaderIndex = Array.IndexOf(lines, "end_header");

            Assert.IsFalse(ply.Contains("nx", StringComparison.Ordinal));
            Assert.IsFalse(ply.Contains("red", StringComparison.Ordinal));
            Assert.AreEqual("1 2 3", lines[endHeaderIndex + 1]);
        }

        [TestMethod]
        public void SerializeDefaultsAMissingColorToWhiteWhenAnyVertexHasOne()
        {
            var mesh = new DDD.Mesh();
            mesh.AddVertex(new DDD.Vertex(new DDD.Point(0, 0, 0), new DDD.Color(10, 20, 30)));
            mesh.AddVertex(new DDD.Point(1, 1, 1)); // no color

            string ply = DDD.PlyFormat.Serialize(mesh);
            string[] lines = ply.Split('\n');
            int endHeaderIndex = Array.IndexOf(lines, "end_header");

            Assert.AreEqual("0 0 0 10 20 30", lines[endHeaderIndex + 1]);
            Assert.AreEqual("1 1 1 255 255 255", lines[endHeaderIndex + 2]);
        }

        [TestMethod]
        public void ReadsTheBundledTeapotResource()
        {
            DDD.Mesh mesh = DDD.PlyFormat.ReadEmbeddedResource("DDD.Assets.teapot.ply");
            Assert.AreEqual(1177, mesh.Vertices.Count);
            Assert.AreEqual(2256, mesh.Faces.Count);
        }

        [TestMethod]
        public void ReadsTheBundledSuzanneResource()
        {
            DDD.Mesh mesh = DDD.PlyFormat.ReadEmbeddedResource("DDD.Assets.suzanne.ply");
            Assert.AreEqual(507, mesh.Vertices.Count);
            Assert.AreEqual(968, mesh.Faces.Count);
        }

        [TestMethod]
        public void ReadsTheBundledBunnyResource()
        {
            DDD.Mesh mesh = DDD.PlyFormat.ReadEmbeddedResource("DDD.Assets.bunny.ply");
            Assert.AreEqual(453, mesh.Vertices.Count);
            Assert.AreEqual(948, mesh.Faces.Count);
        }

        [TestMethod]
        public void ReadEmbeddedResourceThrowsForAnUnknownResourceName()
        {
            Assert.ThrowsExactly<System.IO.FileNotFoundException>(
                () => DDD.PlyFormat.ReadEmbeddedResource("DDD.Assets.does-not-exist.ply"));
        }
    }
}
