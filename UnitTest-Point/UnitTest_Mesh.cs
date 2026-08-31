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
        [TestMethod]
        public void MaterialDefaultsToNullAndCanBeAssigned()
        {
            DDD.Mesh mesh = new DDD.Mesh();
            Assert.IsNull(mesh.Material);

            var material = new DDD.Material(new DDD.Color(1, 2, 3));
            mesh.Material = material;
            Assert.AreEqual(material, mesh.Material);
        }
        [TestMethod]
        public void BoundingBoxCenterOfAnEmptyMeshIsTheOrigin()
        {
            DDD.Mesh mesh = new DDD.Mesh();
            Assert.AreEqual(new DDD.Point(0, 0, 0), mesh.BoundingBoxCenter());
        }
        [TestMethod]
        public void BoundingBoxCenterOfASingleVertexIsThatVertex()
        {
            DDD.Mesh mesh = new DDD.Mesh();
            mesh.AddVertex(new DDD.Point(3, -1, 4));
            Assert.AreEqual(new DDD.Point(3, -1, 4), mesh.BoundingBoxCenter());
        }
        [TestMethod]
        public void BoundingBoxCenterIsTheMidpointOfTheExtremes()
        {
            DDD.Mesh mesh = new DDD.Mesh();
            mesh.AddVertex(new DDD.Point(-2, 0, 10));
            mesh.AddVertex(new DDD.Point(4, 6, -2));
            mesh.AddVertex(new DDD.Point(0, 3, 4)); // interior point, doesn't move the center
            Assert.AreEqual(new DDD.Point(1, 3, 4), mesh.BoundingBoxCenter());
        }
    }
}
