using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DDD_UnitTest
{
    [TestClass]
    public class Rasterizer
    {
        [TestMethod]
        public void ProjectsAPointOnThePositiveXAxisToTheRightOfCenter()
        {
            var objects = new List<object> { new DDD.Point(1, 0, 0) };
            DDD.Point min = new DDD.Point(-1, -1, -1);
            DDD.Point max = new DDD.Point(1, 1, 1);

            // No rotation, 100x100 framebuffer: center is (50,50).
            // radius = distance from bbox center to bbox max = sqrt(3).
            // scale = min(width,height) * 0.38 / radius.
            // projected x = round(50 + 1 * scale) = 72, y stays 50.
            DDD.Framebuffer framebuffer = DDD.Rasterizer.Render(objects, min, max, 0.0, 0.0, 100, 100);

            var pixel = framebuffer.GetPixel(72, 50);
            Assert.AreEqual(((byte)240, (byte)240, (byte)240), pixel);
        }

        [TestMethod]
        public void ZoomScalesTheOrthographicProjectionLinearly()
        {
            var objects = new List<object> { new DDD.Point(1, 0, 0) };
            DDD.Point min = new DDD.Point(-1, -1, -1);
            DDD.Point max = new DDD.Point(1, 1, 1);

            // Same setup as the base ortho test, but zoom=2 should exactly double the offset
            // from center: scale = min(width,height) * 0.38 / radius * zoom.
            // projected x = round(50 + 1 * scale) = round(50 + 43.878644...) = 94.
            DDD.Framebuffer framebuffer = DDD.Rasterizer.Render(objects, min, max, 0.0, 0.0, 100, 100, zoom: 2.0);

            var pixel = framebuffer.GetPixel(94, 50);
            Assert.AreEqual(((byte)240, (byte)240, (byte)240), pixel);
        }

        [TestMethod]
        public void RollRotatesAroundTheZAxis()
        {
            var objects = new List<object> { new DDD.Point(0, 1, 0) };
            DDD.Point min = new DDD.Point(-1, -1, -1);
            DDD.Point max = new DDD.Point(1, 1, 1);

            // With angleX = angleY = 0, RotateY(0)*RotateX(0) is the identity, so the
            // composed rotation is exactly RotateZ(90). RotateZ(90) sends (0,1,0) -> (-1,0,0)
            // (M11=0, M12=-1, M21=1, M22=0 at 90 degrees). Same scale as the base ortho test
            // (same bbox): projected x = round(50 + (-1) * 21.939322...) = 28, y stays 50.
            DDD.Framebuffer framebuffer = DDD.Rasterizer.Render(objects, min, max, 0.0, 0.0, 100, 100, angleZDegrees: 90.0);

            var pixel = framebuffer.GetPixel(28, 50);
            Assert.AreEqual(((byte)240, (byte)240, (byte)240), pixel);
        }

        [TestMethod]
        public void PerspectiveProjectionMatchesTheWorkedExample()
        {
            var objects = new List<object> { new DDD.Point(1, 0, 0) };
            DDD.Point min = new DDD.Point(-1, -1, -1);
            DDD.Point max = new DDD.Point(1, 1, 1);

            // radius = sqrt(3). fovY = 40deg, halfFovTan = tan(20deg) ~= 0.363970.
            // cameraDistance = max(radius / (0.38 * halfFovTan), radius * 1.05) ~= 12.5232.
            // focalLengthPixels = (height/2) / halfFovTan ~= 137.379.
            // No rotation: rotated = (1,0,0), viewZ = cameraDistance ~= 12.5232.
            // scale = focalLengthPixels / viewZ ~= 10.970.
            // projected x = round(50 + 1 * 10.970) = 61, y stays 50.
            DDD.Framebuffer framebuffer = DDD.Rasterizer.Render(objects, min, max, 0.0, 0.0, 100, 100, perspective: true);

            var pixel = framebuffer.GetPixel(61, 50);
            Assert.AreEqual(((byte)240, (byte)240, (byte)240), pixel);
        }

        [TestMethod]
        public void ClipsPointsBehindThePerspectiveCamera()
        {
            DDD.Point min = new DDD.Point(-1, -1, -1);
            DDD.Point max = new DDD.Point(1, 1, 1);

            // cameraDistance for this bbox is ~12.5232 (see the worked-example test above), so
            // a point at Z=20 sits behind the camera (viewZ <= 0) and must be clipped entirely -
            // the resulting framebuffer should be pixel-identical to rendering no objects at all.
            var withBehindPoint = new List<object> { new DDD.Point(0.5, 0.5, 20) };
            var empty = new List<object>();

            DDD.Framebuffer withPoint = DDD.Rasterizer.Render(withBehindPoint, min, max, 0.0, 0.0, 100, 100, perspective: true);
            DDD.Framebuffer withoutPoint = DDD.Rasterizer.Render(empty, min, max, 0.0, 0.0, 100, 100, perspective: true);

            for (int y = 0; y < 100; y++)
            {
                for (int x = 0; x < 100; x++)
                {
                    Assert.AreEqual(withoutPoint.GetPixel(x, y), withPoint.GetPixel(x, y), $"Mismatch at ({x},{y})");
                }
            }
        }

        [TestMethod]
        public void MeshPointsModeProjectsEachVertexLikeABarePoint()
        {
            var mesh = new DDD.Mesh();
            mesh.AddVertex(new DDD.Point(1, 0, 0));
            var objects = new List<object> { mesh };
            DDD.Point min = new DDD.Point(-1, -1, -1);
            DDD.Point max = new DDD.Point(1, 1, 1);

            // Same worked example as ProjectsAPointOnThePositiveXAxisToTheRightOfCenter: a
            // vertex with no Color falls back to the same PointColor a bare Point uses.
            DDD.Framebuffer framebuffer = DDD.Rasterizer.Render(objects, min, max, 0.0, 0.0, 100, 100,
                renderMode: DDD.RenderMode.Points);

            var pixel = framebuffer.GetPixel(72, 50);
            Assert.AreEqual(((byte)240, (byte)240, (byte)240), pixel);
        }

        [TestMethod]
        public void MeshWireframeModeDrawsEachFacesEdges()
        {
            var mesh = new DDD.Mesh();
            int a = mesh.AddVertex(new DDD.Point(1, 0, 0));   // projects to (72, 50)
            int b = mesh.AddVertex(new DDD.Point(0, 1, 0));   // projects to (50, 28)
            int c = mesh.AddVertex(new DDD.Point(0, 0, 0));   // projects to (50, 50)
            mesh.AddFace(a, b, c);
            var objects = new List<object> { mesh };
            DDD.Point min = new DDD.Point(-1, -1, -1);
            DDD.Point max = new DDD.Point(1, 1, 1);

            DDD.Framebuffer framebuffer = DDD.Rasterizer.Render(objects, min, max, 0.0, 0.0, 100, 100,
                renderMode: DDD.RenderMode.Wireframe);

            // Edge endpoints are always included by DrawLine, so checking the three projected
            // vertices is enough to confirm all three edges were drawn.
            var wireColor = ((byte)230, (byte)230, (byte)230);
            Assert.AreEqual(wireColor, framebuffer.GetPixel(72, 50));
            Assert.AreEqual(wireColor, framebuffer.GetPixel(50, 28));
            Assert.AreEqual(wireColor, framebuffer.GetPixel(50, 50));
        }

        [TestMethod]
        public void MeshSolidModeFillsAFrontFacingFaceAtFullIntensity()
        {
            var mesh = new DDD.Mesh();
            int a = mesh.AddVertex(new DDD.Point(1, 0, 0));
            int b = mesh.AddVertex(new DDD.Point(0, 1, 0));
            int c = mesh.AddVertex(new DDD.Point(0, 0, 0));
            mesh.AddFace(a, b, c); // winding gives face normal (0,0,1) - facing the camera

            var objects = new List<object> { mesh };
            DDD.Point min = new DDD.Point(-1, -1, -1);
            DDD.Point max = new DDD.Point(1, 1, 1);

            DDD.Framebuffer framebuffer = DDD.Rasterizer.Render(objects, min, max, 0.0, 0.0, 100, 100,
                renderMode: DDD.RenderMode.Solid);

            // Facing the camera dead-on quantizes to the top shading level (1.0), leaving the
            // default face color (200,200,200) unchanged.
            var faceColor = ((byte)200, (byte)200, (byte)200);
            Assert.AreEqual(faceColor, framebuffer.GetPixel(72, 50)); // vertex a
            Assert.AreEqual(faceColor, framebuffer.GetPixel(50, 50)); // vertex c
            Assert.AreEqual(((byte)0, (byte)0, (byte)0), framebuffer.GetPixel(10, 10)); // outside the face
        }

        [TestMethod]
        public void MeshSolidModeCullsABackFacingFace()
        {
            var mesh = new DDD.Mesh();
            int a = mesh.AddVertex(new DDD.Point(1, 0, 0));
            int b = mesh.AddVertex(new DDD.Point(0, 1, 0));
            int c = mesh.AddVertex(new DDD.Point(0, 0, 0));
            mesh.AddFace(a, c, b); // reversed winding - normal (0,0,-1), facing away from the camera

            var withFace = new List<object> { mesh };
            var empty = new List<object>();
            DDD.Point min = new DDD.Point(-1, -1, -1);
            DDD.Point max = new DDD.Point(1, 1, 1);

            DDD.Framebuffer withFaceBuffer = DDD.Rasterizer.Render(withFace, min, max, 0.0, 0.0, 100, 100, renderMode: DDD.RenderMode.Solid);
            DDD.Framebuffer emptyBuffer = DDD.Rasterizer.Render(empty, min, max, 0.0, 0.0, 100, 100, renderMode: DDD.RenderMode.Solid);

            for (int y = 0; y < 100; y++)
            {
                for (int x = 0; x < 100; x++)
                {
                    Assert.AreEqual(emptyBuffer.GetPixel(x, y), withFaceBuffer.GetPixel(x, y), $"Mismatch at ({x},{y})");
                }
            }
        }

        [TestMethod]
        public void ShowNormalsDrawsAnIndicatorLineFromTheFaceCentroid()
        {
            var mesh = new DDD.Mesh();
            int a = mesh.AddVertex(new DDD.Point(1, 0, 0));
            int b = mesh.AddVertex(new DDD.Point(0, 1, 0));
            int c = mesh.AddVertex(new DDD.Point(0, 0, 0));
            mesh.AddFace(a, b, c);
            var objects = new List<object> { mesh };
            DDD.Point min = new DDD.Point(-1, -1, -1);
            DDD.Point max = new DDD.Point(1, 1, 1);

            // The face normal (0,0,1) is purely along the (orthographically invisible) Z axis, so
            // the centroid (1/3, 1/3, 0) and the normal's tip project to the same pixel:
            // round(50 + (1/3) * 21.939322) = 57, round(50 - (1/3) * 21.939322) = 43.
            DDD.Framebuffer withNormals = DDD.Rasterizer.Render(objects, min, max, 0.0, 0.0, 100, 100,
                renderMode: DDD.RenderMode.Points, showNormals: true);
            DDD.Framebuffer withoutNormals = DDD.Rasterizer.Render(objects, min, max, 0.0, 0.0, 100, 100,
                renderMode: DDD.RenderMode.Points, showNormals: false);

            Assert.AreEqual(((byte)240, (byte)100, (byte)220), withNormals.GetPixel(57, 43));
            Assert.AreEqual(((byte)0, (byte)0, (byte)0), withoutNormals.GetPixel(57, 43));
        }
    }
}
