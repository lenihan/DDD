using System;
using System.Collections.Generic;

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

        public static IReadOnlyList<(byte R, byte G, byte B)> Palette { get; } = new[]
        {
            Background, AxisX, AxisY, AxisZ, PointColor, VectorColor, BoundingBoxColor
        };

        public static Framebuffer Render(List<object> objects, Point boundingBoxMin, Point boundingBoxMax,
            double angleXDegrees, double angleYDegrees, int width, int height,
            double angleZDegrees = 0.0, bool perspective = false, double zoom = 1.0)
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

            (int X, int Y, bool Visible) Project(Point p)
            {
                Point local = new Point(p.X - center.X, p.Y - center.Y, p.Z - center.Z);
                Point rotated = rotation * local;

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
            }

            return framebuffer;
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
