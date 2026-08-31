using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DDD_UnitTest
{
    [TestClass]
    public class PlyFormat
    {
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
    }
}
