using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DDD_UnitTest
{
    [TestClass]
    public class Mesh
    {
        [TestMethod]
        public void NewMeshHasNoVerticesOrFaces()
        {
            DDD.Mesh mesh = new DDD.Mesh();
            Assert.IsTrue(mesh.Vertices.Count == 0);
            Assert.IsTrue(mesh.Faces.Count == 0);
        }
        [TestMethod]
        public void AddVertexReturnsItsIndex()
        {
            DDD.Mesh mesh = new DDD.Mesh();
            int i0 = mesh.AddVertex(new DDD.Point(0, 0, 0));
            int i1 = mesh.AddVertex(new DDD.Point(1, 0, 0));
            int i2 = mesh.AddVertex(new DDD.Vertex(new DDD.Point(0, 1, 0), new DDD.Color(255, 0, 0)));

            Assert.IsTrue(i0 == 0);
            Assert.IsTrue(i1 == 1);
            Assert.IsTrue(i2 == 2);
            Assert.IsTrue(mesh.Vertices.Count == 3);
            Assert.IsTrue(mesh.Vertices[2].Color == new DDD.Color(255, 0, 0));
        }
        [TestMethod]
        public void AddFaceByIndicesMatchesAddFaceByFace()
        {
            DDD.Mesh mesh = new DDD.Mesh();
            mesh.AddVertex(new DDD.Point(0, 0, 0));
            mesh.AddVertex(new DDD.Point(1, 0, 0));
            mesh.AddVertex(new DDD.Point(0, 1, 0));

            mesh.AddFace(0, 1, 2);
            mesh.AddFace(new DDD.Face(0, 1, 2));

            Assert.IsTrue(mesh.Faces.Count == 2);
            Assert.IsTrue(mesh.Faces[0] == mesh.Faces[1]);
        }
        [TestMethod]
        public void TestToString()
        {
            DDD.Mesh mesh = new DDD.Mesh();
            mesh.AddVertex(new DDD.Point(0, 0, 0));
            mesh.AddVertex(new DDD.Point(1, 0, 0));
            mesh.AddVertex(new DDD.Point(0, 1, 0));
            mesh.AddFace(0, 1, 2);

            Assert.IsTrue(mesh.ToString() == "Mesh: 3 vertices, 1 faces\n");
        }
    }
}
