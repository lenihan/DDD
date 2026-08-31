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

        // The standard face used by the lighting tests below: normal (0,0,1), facing the camera
        // dead-on, no scene rotation. Centroid is (1/3, 1/3, 0).
        static DDD.Mesh FrontFacingTriangle()
        {
            var mesh = new DDD.Mesh();
            int a = mesh.AddVertex(new DDD.Point(1, 0, 0));
            int b = mesh.AddVertex(new DDD.Point(0, 1, 0));
            int c = mesh.AddVertex(new DDD.Point(0, 0, 0));
            mesh.AddFace(a, b, c);
            return mesh;
        }

        [TestMethod]
        public void AnExplicitDirectionalLightAlignedWithTheCameraMatchesTheDefaultHeadlamp()
        {
            DDD.Mesh mesh = FrontFacingTriangle();
            var light = new DDD.Light(new DDD.Vector(0, 0, 1));
            var objects = new List<object> { mesh, light };
            DDD.Point min = new DDD.Point(-1, -1, -1);
            DDD.Point max = new DDD.Point(1, 1, 1);

            DDD.Framebuffer framebuffer = DDD.Rasterizer.Render(objects, min, max, 0.0, 0.0, 100, 100, renderMode: DDD.RenderMode.Solid);

            Assert.AreEqual(((byte)200, (byte)200, (byte)200), framebuffer.GetPixel(50, 50));
        }

        [TestMethod]
        public void ALightPerpendicularToTheFaceDimsItToJustTheAmbientTerm()
        {
            DDD.Mesh mesh = FrontFacingTriangle();
            // Perpendicular to the face normal (0,0,1): contributes zero diffuse.
            var light = new DDD.Light(new DDD.Vector(0, 1, 0));
            var objects = new List<object> { mesh, light };
            DDD.Point min = new DDD.Point(-1, -1, -1);
            DDD.Point max = new DDD.Point(1, 1, 1);

            // Default material: Ambient=0.2, Diffuse=0.8. diffuseFactor=0, so total=0.2 exactly,
            // an exact quantization level -> 200*0.2 = 40.
            DDD.Framebuffer framebuffer = DDD.Rasterizer.Render(objects, min, max, 0.0, 0.0, 100, 100, renderMode: DDD.RenderMode.Solid);

            Assert.AreEqual(((byte)40, (byte)40, (byte)40), framebuffer.GetPixel(50, 50));
        }

        [TestMethod]
        public void HighAmbientMaterialIgnoresLightDirection()
        {
            DDD.Mesh mesh = FrontFacingTriangle();
            mesh.Material = new DDD.Material(new DDD.Color(200, 200, 200), ambient: 1.0, diffuse: 0.0);
            // Same perpendicular light as the test above - would contribute zero diffuse, but
            // Ambient=1.0 alone already saturates brightness regardless of light direction.
            var light = new DDD.Light(new DDD.Vector(0, 1, 0));
            var objects = new List<object> { mesh, light };
            DDD.Point min = new DDD.Point(-1, -1, -1);
            DDD.Point max = new DDD.Point(1, 1, 1);

            DDD.Framebuffer framebuffer = DDD.Rasterizer.Render(objects, min, max, 0.0, 0.0, 100, 100, renderMode: DDD.RenderMode.Solid);

            Assert.AreEqual(((byte)200, (byte)200, (byte)200), framebuffer.GetPixel(50, 50));
        }

        [TestMethod]
        public void SpecularHighlightAddsBrightnessBeyondDiffuseAlone()
        {
            // A point light placed straight out along the face's own normal from its centroid
            // (1/3, 1/3, 10) makes lightDir exactly equal the face normal (0,0,1): diffuseFactor=1,
            // and the reflection vector also comes out to exactly (0,0,1) = ViewDirection, so
            // specularFactor=1 regardless of shininess.
            var light = new DDD.Light(new DDD.Point(1.0 / 3.0, 1.0 / 3.0, 10), 1.0);
            var objects = new List<object> { FrontFacingTriangle(), light };
            DDD.Point min = new DDD.Point(-1, -1, -1);
            DDD.Point max = new DDD.Point(1, 1, 1);

            DDD.Mesh diffuseOnly = FrontFacingTriangle();
            diffuseOnly.Material = new DDD.Material(new DDD.Color(200, 200, 200), ambient: 0.0, diffuse: 0.6, specular: 0.0);
            var diffuseObjects = new List<object> { diffuseOnly, light };

            DDD.Mesh withSpecular = FrontFacingTriangle();
            withSpecular.Material = new DDD.Material(new DDD.Color(200, 200, 200), ambient: 0.0, diffuse: 0.6, specular: 0.3);
            var specularObjects = new List<object> { withSpecular, light };

            // diffuse-only: total = 0.6*1 = 0.6 (exact level) -> 200*0.6 = 120
            DDD.Framebuffer diffuseFramebuffer = DDD.Rasterizer.Render(diffuseObjects, min, max, 0.0, 0.0, 100, 100, renderMode: DDD.RenderMode.Solid);
            // with specular: total = 0.6*1 + 0.3*1 = 0.9 -> quantizes to 0.8 (tie broken toward
            // the earlier level in ShadingLevels) -> 200*0.8 = 160
            DDD.Framebuffer specularFramebuffer = DDD.Rasterizer.Render(specularObjects, min, max, 0.0, 0.0, 100, 100, renderMode: DDD.RenderMode.Solid);

            Assert.AreEqual(((byte)120, (byte)120, (byte)120), diffuseFramebuffer.GetPixel(50, 50));
            Assert.AreEqual(((byte)160, (byte)160, (byte)160), specularFramebuffer.GetPixel(50, 50));
        }

        [TestMethod]
        public void MultipleLightsCombineAdditively()
        {
            DDD.Mesh oneLight = FrontFacingTriangle();
            oneLight.Material = new DDD.Material(new DDD.Color(200, 200, 200), ambient: 0.0, diffuse: 1.0, specular: 0.0);
            var lightA = new DDD.Light(new DDD.Vector(0, 0, 1), 0.5);
            var oneLightObjects = new List<object> { oneLight, lightA };

            DDD.Mesh twoLights = FrontFacingTriangle();
            twoLights.Material = new DDD.Material(new DDD.Color(200, 200, 200), ambient: 0.0, diffuse: 1.0, specular: 0.0);
            var lightB = new DDD.Light(new DDD.Vector(0, 0, 1), 0.5);
            var twoLightObjects = new List<object> { twoLights, lightA, lightB };

            DDD.Point min = new DDD.Point(-1, -1, -1);
            DDD.Point max = new DDD.Point(1, 1, 1);

            // One light: total = 1*1.0*0.5 = 0.5 -> quantizes to 0.4 -> 200*0.4 = 80
            DDD.Framebuffer oneLightFramebuffer = DDD.Rasterizer.Render(oneLightObjects, min, max, 0.0, 0.0, 100, 100, renderMode: DDD.RenderMode.Solid);
            // Two lights: total = 2 * (1*1.0*0.5) = 1.0 exactly -> 200*1.0 = 200
            DDD.Framebuffer twoLightFramebuffer = DDD.Rasterizer.Render(twoLightObjects, min, max, 0.0, 0.0, 100, 100, renderMode: DDD.RenderMode.Solid);

            Assert.AreEqual(((byte)80, (byte)80, (byte)80), oneLightFramebuffer.GetPixel(50, 50));
            Assert.AreEqual(((byte)200, (byte)200, (byte)200), twoLightFramebuffer.GetPixel(50, 50));
        }

        [TestMethod]
        public void APointLightBehindTheFaceGivesOnlyTheAmbientTerm()
        {
            DDD.Mesh mesh = FrontFacingTriangle();
            // Positioned behind the face (negative Z), opposite the normal (0,0,1): diffuseFactor
            // clamps to 0 via Math.Max, leaving only the default material's Ambient=0.2 term.
            var light = new DDD.Light(new DDD.Point(1.0 / 3.0, 1.0 / 3.0, -10), 1.0);
            var objects = new List<object> { mesh, light };
            DDD.Point min = new DDD.Point(-1, -1, -1);
            DDD.Point max = new DDD.Point(1, 1, 1);

            DDD.Framebuffer framebuffer = DDD.Rasterizer.Render(objects, min, max, 0.0, 0.0, 100, 100, renderMode: DDD.RenderMode.Solid);

            Assert.AreEqual(((byte)40, (byte)40, (byte)40), framebuffer.GetPixel(50, 50));
        }
    }
}
