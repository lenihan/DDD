using System;
using System.Collections.Generic;
using System.Linq;

namespace DDD
{
    internal static class Rasterizer
    {
        const double FitFraction = 0.38;
        const double FovYDegrees = 40.0;

        static readonly (byte R, byte G, byte B) Background = (0, 0, 0);
        static readonly (byte R, byte G, byte B) AxisX = (220, 60, 60);
        static readonly (byte R, byte G, byte B) AxisY = (60, 200, 90);
        static readonly (byte R, byte G, byte B) AxisZ = (70, 120, 230);
        static readonly (byte R, byte G, byte B) PointColor = (240, 240, 240);
        static readonly (byte R, byte G, byte B) VectorColor = (240, 210, 60);
        static readonly (byte R, byte G, byte B) BoundingBoxColor = (110, 110, 110);
        static readonly (byte R, byte G, byte B) WireframeColor = (230, 230, 230);
        static readonly (byte R, byte G, byte B) NormalColor = (240, 100, 220);
        static readonly Color DefaultFaceColor = new Color(200, 200, 200);

        // Camera-relative "headlamp" light used as a placeholder until New-Light/New-Material
        // exist (see PLAN.md). A constant view direction in rotated/local space is an
        // approximation for perspective mode (the true per-face view vector varies slightly with
        // position), but it's cheap and visually indistinguishable at the FOV/zoom ranges this
        // renderer supports.
        static readonly Vector ViewDirection = new Vector(0, 0, 1);
        const double AmbientFloor = 0.2;
        const int ShadingLevelCount = 6;
        const double NormalLengthFraction = 0.15;

        // Sixel output only exact-matches a fixed palette (see SixelEncoder) - continuous
        // shading has to be quantized to a small, known set of brightness levels so every pixel
        // this renders lands on an exact palette entry that BuildPalette also produced.
        static readonly double[] ShadingLevels = Enumerable.Range(0, ShadingLevelCount)
            .Select(i => AmbientFloor + i * (1.0 - AmbientFloor) / (ShadingLevelCount - 1))
            .ToArray();

        public static IReadOnlyList<(byte R, byte G, byte B)> Palette { get; } = new[]
        {
            Background, AxisX, AxisY, AxisZ, PointColor, VectorColor, BoundingBoxColor
        };

        // The fixed Palette above only covers the scene furniture (axes, bounding box) and
        // point/vector markers. Mesh faces can carry arbitrary per-vertex Color, so the palette
        // actually used for a given scene has to be built from what that scene contains.
        public static IReadOnlyList<(byte R, byte G, byte B)> BuildPalette(List<object> objects)
        {
            var palette = new List<(byte R, byte G, byte B)>(Palette) { WireframeColor, NormalColor };
            var seen = new HashSet<(byte R, byte G, byte B)>(palette);

            void AddColor((byte R, byte G, byte B) color)
            {
                if (seen.Add(color)) palette.Add(color);
            }

            foreach (object obj in objects)
            {
                if (obj is not Mesh mesh) continue;

                foreach (Vertex vertex in mesh.Vertices)
                {
                    if (vertex.Color is Color vertexColor)
                    {
                        AddColor((vertexColor.R, vertexColor.G, vertexColor.B));
                    }
                }
                foreach (Face face in mesh.Faces)
                {
                    Color baseColor = ResolveFaceBaseColor(mesh, face);
                    foreach (double level in ShadingLevels)
                    {
                        AddColor(ShadedColor(baseColor, level));
                    }
                }
            }

            return palette;
        }

        public static Framebuffer Render(List<object> objects, Point boundingBoxMin, Point boundingBoxMax,
            double angleXDegrees, double angleYDegrees, int width, int height,
            double angleZDegrees = 0.0, bool perspective = false, double zoom = 1.0,
            RenderMode renderMode = RenderMode.Wireframe, bool showNormals = false)
        {
            var framebuffer = new Framebuffer(width, height);
            framebuffer.Clear(Background.R, Background.G, Background.B);

            Point center = new Point((boundingBoxMin.X + boundingBoxMax.X) / 2.0,
                                      (boundingBoxMin.Y + boundingBoxMax.Y) / 2.0,
                                      (boundingBoxMin.Z + boundingBoxMax.Z) / 2.0);

            // Fold the world origin into the fit too, so a scene offset far from (0,0,0)
            // doesn't clip the reference axes drawn through it.
            double radius = Math.Max(Distance(center, boundingBoxMax), Distance(center, new Point(0.0, 0.0, 0.0)));
            radius = Math.Max(radius, 1e-6);

            Matrix rotation = Matrix.RotateY(angleYDegrees) * Matrix.RotateX(angleXDegrees) * Matrix.RotateZ(angleZDegrees);

            double fovYRadians = FovYDegrees * Math.PI / 180.0;
            double halfFovTan = Math.Tan(fovYRadians / 2.0);

            // Orthographic scale, or a perspective camera whose distance is derived from the
            // same fit convention so switching modes at zoom=1 doesn't jump in apparent size.
            double orthoScale = Math.Min(width, height) * FitFraction / radius * zoom;
            double cameraDistance = Math.Max(radius / (FitFraction * halfFovTan) / zoom, radius * 1.05);
            double focalLengthPixels = (height / 2.0) / halfFovTan;

            Point RotateLocal(Point p)
            {
                Point local = new Point(p.X - center.X, p.Y - center.Y, p.Z - center.Z);
                return rotation * local;
            }

            (int X, int Y, bool Visible) Project(Point p)
            {
                Point rotated = RotateLocal(p);

                if (perspective)
                {
                    double viewZ = cameraDistance - rotated.Z;
                    if (viewZ <= 1e-6) return (0, 0, false);
                    double s = focalLengthPixels / viewZ;
                    int px = (int)Math.Round(width / 2.0 + rotated.X * s);
                    int py = (int)Math.Round(height / 2.0 - rotated.Y * s);
                    return (px, py, true);
                }
                else
                {
                    int sx = (int)Math.Round(width / 2.0 + rotated.X * orthoScale);
                    int sy = (int)Math.Round(height / 2.0 - rotated.Y * orthoScale);
                    return (sx, sy, true);
                }
            }

            double axisLength = radius * 1.15;
            DrawSegment(framebuffer, Project, center, center + new Vector(1, 0, 0) * axisLength, AxisX);
            DrawSegment(framebuffer, Project, center, center + new Vector(0, 1, 0) * axisLength, AxisY);
            DrawSegment(framebuffer, Project, center, center + new Vector(0, 0, 1) * axisLength, AxisZ);

            DrawBoundingBox(framebuffer, Project, boundingBoxMin, boundingBoxMax, BoundingBoxColor);

            foreach (object obj in objects)
            {
                if (obj is Point point)
                {
                    DrawMarker(framebuffer, Project(point), PointColor);
                }
                else if (obj is Vector vector)
                {
                    DrawSegment(framebuffer, Project, new Point(0, 0, 0), new Point(vector.X, vector.Y, vector.Z), VectorColor);
                }
                else if (obj is Matrix matrix)
                {
                    Point origin = matrix * new Point(0, 0, 0);
                    DrawSegment(framebuffer, Project, origin, matrix * new Point(1, 0, 0), AxisX);
                    DrawSegment(framebuffer, Project, origin, matrix * new Point(0, 1, 0), AxisY);
                    DrawSegment(framebuffer, Project, origin, matrix * new Point(0, 0, 1), AxisZ);
                }
                else if (obj is Mesh mesh)
                {
                    DrawMesh(framebuffer, Project, RotateLocal, mesh, renderMode, showNormals, radius);
                }
            }

            return framebuffer;
        }

        static Color ResolveFaceBaseColor(Mesh mesh, Face face)
        {
            return mesh.Vertices[face.A].Color
                ?? mesh.Vertices[face.B].Color
                ?? mesh.Vertices[face.C].Color
                ?? DefaultFaceColor;
        }

        static double QuantizeIntensity(double intensity)
        {
            double closest = ShadingLevels[0];
            double closestDiff = Math.Abs(intensity - closest);
            foreach (double level in ShadingLevels)
            {
                double diff = Math.Abs(intensity - level);
                if (diff < closestDiff)
                {
                    closest = level;
                    closestDiff = diff;
                }
            }
            return closest;
        }

        static (byte R, byte G, byte B) ShadedColor(Color baseColor, double intensity) =>
        (
            (byte)Math.Round(baseColor.R * intensity),
            (byte)Math.Round(baseColor.G * intensity),
            (byte)Math.Round(baseColor.B * intensity)
        );

        static void DrawMesh(Framebuffer framebuffer, Func<Point, (int X, int Y, bool Visible)> project,
            Func<Point, Point> rotateLocal, Mesh mesh, RenderMode renderMode, bool showNormals, double radius)
        {
            if (renderMode == RenderMode.Points)
            {
                foreach (Vertex vertex in mesh.Vertices)
                {
                    (byte R, byte G, byte B) color = vertex.Color is Color vertexColor
                        ? (vertexColor.R, vertexColor.G, vertexColor.B)
                        : PointColor;
                    DrawMarker(framebuffer, project(vertex.Position), color);
                }
            }
            else if (renderMode == RenderMode.Wireframe)
            {
                foreach (Face face in mesh.Faces)
                {
                    Point a = mesh.Vertices[face.A].Position;
                    Point b = mesh.Vertices[face.B].Position;
                    Point c = mesh.Vertices[face.C].Position;
                    DrawSegment(framebuffer, project, a, b, WireframeColor);
                    DrawSegment(framebuffer, project, b, c, WireframeColor);
                    DrawSegment(framebuffer, project, c, a, WireframeColor);
                }
            }
            else // Solid
            {
                foreach (Face face in mesh.Faces)
                {
                    Point a = mesh.Vertices[face.A].Position;
                    Point b = mesh.Vertices[face.B].Position;
                    Point c = mesh.Vertices[face.C].Position;

                    Point ra = rotateLocal(a);
                    Point rb = rotateLocal(b);
                    Point rc = rotateLocal(c);

                    Vector normal = Vector.Cross(rb - ra, rc - ra);
                    double normalLength = normal.Length();
                    if (normalLength < 1e-12) continue; // degenerate face

                    double rawIntensity = Vector.Dot(normal, ViewDirection) / normalLength;
                    if (rawIntensity <= 0) continue; // backface cull

                    double intensity = QuantizeIntensity(AmbientFloor + (1 - AmbientFloor) * rawIntensity);
                    Color baseColor = ResolveFaceBaseColor(mesh, face);
                    (byte R, byte G, byte B) color = ShadedColor(baseColor, intensity);

                    var sa = project(a);
                    var sb = project(b);
                    var sc = project(c);
                    if (!sa.Visible || !sb.Visible || !sc.Visible) continue;

                    framebuffer.FillTriangle(sa.X, sa.Y, sb.X, sb.Y, sc.X, sc.Y, color.R, color.G, color.B);
                }
            }

            if (showNormals)
            {
                double length = radius * NormalLengthFraction;
                foreach (Face face in mesh.Faces)
                {
                    Point a = mesh.Vertices[face.A].Position;
                    Point b = mesh.Vertices[face.B].Position;
                    Point c = mesh.Vertices[face.C].Position;
                    Point centroid = new Point((a.X + b.X + c.X) / 3.0, (a.Y + b.Y + c.Y) / 3.0, (a.Z + b.Z + c.Z) / 3.0);

                    Vector normal = Vector.Cross(b - a, c - a);
                    if (normal.Length() < 1e-12) continue;

                    Point tip = centroid + Vector.Normalize(normal) * length;
                    DrawSegment(framebuffer, project, centroid, tip, NormalColor);
                }
            }
        }

        static double Distance(Point a, Point b)
        {
            double dx = a.X - b.X, dy = a.Y - b.Y, dz = a.Z - b.Z;
            return Math.Sqrt(dx * dx + dy * dy + dz * dz);
        }

        static void DrawMarker(Framebuffer framebuffer, (int X, int Y, bool Visible) at, (byte R, byte G, byte B) color)
        {
            if (!at.Visible) return;
            framebuffer.SetPixel(at.X, at.Y, color.R, color.G, color.B);
            framebuffer.SetPixel(at.X + 1, at.Y, color.R, color.G, color.B);
            framebuffer.SetPixel(at.X - 1, at.Y, color.R, color.G, color.B);
            framebuffer.SetPixel(at.X, at.Y + 1, color.R, color.G, color.B);
            framebuffer.SetPixel(at.X, at.Y - 1, color.R, color.G, color.B);
        }

        static void DrawSegment(Framebuffer framebuffer, Func<Point, (int X, int Y, bool Visible)> project, Point from, Point to, (byte R, byte G, byte B) color)
        {
            var a = project(from);
            var b = project(to);
            if (!a.Visible || !b.Visible) return;
            framebuffer.DrawLine(a.X, a.Y, b.X, b.Y, color.R, color.G, color.B);
        }

        static void DrawBoundingBox(Framebuffer framebuffer, Func<Point, (int X, int Y, bool Visible)> project, Point min, Point max, (byte R, byte G, byte B) color)
        {
            var corners = new[]
            {
                new Point(min.X, min.Y, min.Z), new Point(max.X, min.Y, min.Z),
                new Point(max.X, max.Y, min.Z), new Point(min.X, max.Y, min.Z),
                new Point(min.X, min.Y, max.Z), new Point(max.X, min.Y, max.Z),
                new Point(max.X, max.Y, max.Z), new Point(min.X, max.Y, max.Z),
            };
            var edges = new (int A, int B)[]
            {
                (0, 1), (1, 2), (2, 3), (3, 0),
                (4, 5), (5, 6), (6, 7), (7, 4),
                (0, 4), (1, 5), (2, 6), (3, 7),
            };
            foreach (var edge in edges)
            {
                DrawSegment(framebuffer, project, corners[edge.A], corners[edge.B], color);
            }
        }
    }
}
