using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DDD_UnitTest
{
    [TestClass]
    public class Primitives
    {
        // Every primitive stores an analytically-correct Normal on each vertex. Rather than
        // assuming a shape is star-shaped from its center (true for Box/Sphere/Cylinder/Cone,
        // but NOT for Torus - a point on the inner rim's true outward normal points toward the
        // central axis, not away from it), check winding against the vertices' own stored
        // normals instead: a face's cross-product normal must point the same general direction
        // as the (summed) normals of the vertices it references. Works uniformly for every
        // shape, regardless of center offset.
        static void AssertAllFacesMatchStoredVertexNormals(DDD.Mesh mesh)
        {
            foreach (DDD.Face face in mesh.Faces)
            {
                DDD.Vertex va = mesh.Vertices[face.A];
                DDD.Vertex vb = mesh.Vertices[face.B];
                DDD.Vertex vc = mesh.Vertices[face.C];

                DDD.Vector faceNormal = DDD.Vector.Cross(vb.Position - va.Position, vc.Position - va.Position);
                if (faceNormal.Length() < 1e-9) continue; // degenerate (e.g. a cone's zero-radius apex ring)

                DDD.Vector referenceNormal = (va.Normal ?? default) + (vb.Normal ?? default) + (vc.Normal ?? default);
                Assert.IsTrue(DDD.Vector.Dot(faceNormal, referenceNormal) > 0,
                    $"Face ({face.A},{face.B},{face.C}) winding doesn't match its vertices' stored normals");
            }
        }

        [TestMethod]
        public void BoxHas24VerticesAnd12FacesAllCorrectlyWound()
        {
            DDD.Mesh mesh = DDD.Primitives.Box(2, 4, 6);

            Assert.AreEqual(24, mesh.Vertices.Count);
            Assert.AreEqual(12, mesh.Faces.Count);
            AssertAllFacesMatchStoredVertexNormals(mesh);
        }

        [TestMethod]
        public void BoxPlusXFaceHasTheExpectedCorners()
        {
            // width=2,height=4,depth=6 -> half extents (1,2,3). The +X face is added first.
            DDD.Mesh mesh = DDD.Primitives.Box(2, 4, 6);

            Assert.AreEqual(new DDD.Point(1, -2, -3), mesh.Vertices[0].Position);
            Assert.AreEqual(new DDD.Point(1, 2, -3), mesh.Vertices[1].Position);
            Assert.AreEqual(new DDD.Point(1, 2, 3), mesh.Vertices[2].Position);
            Assert.AreEqual(new DDD.Point(1, -2, 3), mesh.Vertices[3].Position);
            Assert.AreEqual(new DDD.Vector(1, 0, 0), mesh.Vertices[0].Normal);
            Assert.AreEqual(new DDD.Face(0, 1, 2), mesh.Faces[0]);
            Assert.AreEqual(new DDD.Face(0, 2, 3), mesh.Faces[1]);
        }

        [TestMethod]
        public void BoxRespectsCenter()
        {
            DDD.Mesh mesh = DDD.Primitives.Box(2, 2, 2, new DDD.Point(10, 0, 0));
            Assert.AreEqual(new DDD.Point(11, -1, -1), mesh.Vertices[0].Position);
            AssertAllFacesMatchStoredVertexNormals(mesh);
        }

        [TestMethod]
        public void PlaneHas4VerticesAnd2FacesFacingUp()
        {
            DDD.Mesh mesh = DDD.Primitives.Plane(4, 6);

            Assert.AreEqual(4, mesh.Vertices.Count);
            Assert.AreEqual(2, mesh.Faces.Count);
            Assert.AreEqual(new DDD.Point(-2, 0, -3), mesh.Vertices[0].Position);
            Assert.AreEqual(new DDD.Point(-2, 0, 3), mesh.Vertices[1].Position);
            Assert.AreEqual(new DDD.Point(2, 0, 3), mesh.Vertices[2].Position);
            Assert.AreEqual(new DDD.Point(2, 0, -3), mesh.Vertices[3].Position);
            Assert.AreEqual(new DDD.Vector(0, 1, 0), mesh.Vertices[0].Normal);
            AssertAllFacesMatchStoredVertexNormals(mesh);
        }

        [TestMethod]
        public void SphereHasTheExpectedVertexAndFaceCountsAndAllFacesCorrectlyWound()
        {
            // segments=8 -> lonSteps=8, latSteps=max(2,4)=4.
            // vertices = 2 poles + (latSteps-1)*lonSteps = 2 + 3*8 = 26
            // faces = caps 2*lonSteps + bands (latSteps-2)*lonSteps*2 = 16 + 2*8*2 = 48
            DDD.Mesh mesh = DDD.Primitives.Sphere(2.0, 8);

            Assert.AreEqual(26, mesh.Vertices.Count);
            Assert.AreEqual(48, mesh.Faces.Count);
            Assert.AreEqual(new DDD.Point(0, 2, 0), mesh.Vertices[0].Position); // north pole
            Assert.AreEqual(new DDD.Point(0, -2, 0), mesh.Vertices[1].Position); // south pole
            AssertAllFacesMatchStoredVertexNormals(mesh);
        }

        [TestMethod]
        public void SphereRespectsCenter()
        {
            var center = new DDD.Point(0, 5, 0);
            DDD.Mesh mesh = DDD.Primitives.Sphere(1.0, 8, center);
            Assert.AreEqual(new DDD.Point(0, 6, 0), mesh.Vertices[0].Position);
            AssertAllFacesMatchStoredVertexNormals(mesh);
        }

        [TestMethod]
        public void CylinderHasTheExpectedVertexAndFaceCountsAndAllFacesCorrectlyWound()
        {
            // segments=4 -> steps=4. vertices = 4*4 + 2 = 18. faces = 4*4 = 16.
            DDD.Mesh mesh = DDD.Primitives.Cylinder(1.0, 2.0, 4);

            Assert.AreEqual(18, mesh.Vertices.Count);
            Assert.AreEqual(16, mesh.Faces.Count);
            // i=0: theta=0 -> cos=1,sin=0. The side normal is purely horizontal for a true
            // cylinder (baseRadius == topRadius, so the side isn't slanted).
            Assert.AreEqual(new DDD.Point(1, -1, 0), mesh.Vertices[0].Position);
            Assert.AreEqual(new DDD.Point(1, 1, 0), mesh.Vertices[1].Position);
            Assert.AreEqual(new DDD.Vector(1, 0, 0), mesh.Vertices[0].Normal);
            AssertAllFacesMatchStoredVertexNormals(mesh);
        }

        [TestMethod]
        public void ConeHasTheExpectedVertexAndFaceCountsAndAllFacesCorrectlyWound()
        {
            // A true cone: topRadius=0. segments=8 -> steps=8. vertices=8*4+2=34, faces=8*4=32.
            DDD.Mesh mesh = DDD.Primitives.Cone(1.0, 0.0, 2.0, 8);

            Assert.AreEqual(34, mesh.Vertices.Count);
            Assert.AreEqual(32, mesh.Faces.Count);
            AssertAllFacesMatchStoredVertexNormals(mesh);
        }

        [TestMethod]
        public void FrustumHasTheExpectedVertexAndFaceCountsAndAllFacesCorrectlyWound()
        {
            // A genuine frustum: both radii non-zero and different.
            DDD.Mesh mesh = DDD.Primitives.Cone(2.0, 1.0, 3.0, 8);

            Assert.AreEqual(34, mesh.Vertices.Count);
            Assert.AreEqual(32, mesh.Faces.Count);
            AssertAllFacesMatchStoredVertexNormals(mesh);
        }

        [TestMethod]
        public void TorusHasTheExpectedVertexAndFaceCountsAndAllFacesCorrectlyWound()
        {
            // segments=8 -> steps=8. vertices=8*8=64. faces=8*8*2=128.
            DDD.Mesh mesh = DDD.Primitives.Torus(3.0, 1.0, 8);

            Assert.AreEqual(64, mesh.Vertices.Count);
            Assert.AreEqual(128, mesh.Faces.Count);
            AssertAllFacesMatchStoredVertexNormals(mesh);
        }

        [TestMethod]
        public void CornellBoxHasTheExpectedVertexAndFaceCountsAndAllFacesCorrectlyWound()
        {
            // 5 walls (no front wall) * 4 vertices/2 faces + 2 blocks * 24 vertices/12 faces.
            DDD.Mesh mesh = DDD.Primitives.CornellBox();

            Assert.AreEqual(5 * 4 + 2 * 24, mesh.Vertices.Count);
            Assert.AreEqual(5 * 2 + 2 * 12, mesh.Faces.Count);
            // Winding is self-consistent per face even though the walls (inward normals) and
            // blocks (outward normals) use opposite conventions - this check doesn't assume a
            // single global "outward" direction for the whole mesh.
            AssertAllFacesMatchStoredVertexNormals(mesh);
        }
    }
}
