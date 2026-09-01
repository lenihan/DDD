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

        // Used for a mesh with no Material assigned. Ambient + Diffuse sum to 1.0 so a face
        // lit dead-on still hits exactly full brightness, matching the old fixed headlamp look.
        static readonly Material DefaultMaterial = new Material(DefaultFaceColor, ambient: 0.2, diffuse: 0.8, specular: 0.0, shininess: 16.0);

        // Fixed camera direction, used two ways: (1) always for backface culling - a face
        // pointing away from the camera is skipped regardless of how it's lit; (2) as the
        // direction of the default "headlamp" light used when a scene has no explicit Light
        // object - fixed relative to the viewer regardless of scene rotation, unlike a real
        // Light (see EffectiveLight below). An approximation for perspective mode (the true
        // per-face view vector varies slightly with position), but cheap and visually
        // indistinguishable at the FOV/zoom ranges this renderer supports.
        static readonly Vector ViewDirection = new Vector(0, 0, 1);
        const int ShadingLevelCount = 6;
        const double NormalLengthFraction = 0.15;

        // Sixel output only exact-matches a fixed palette (see SixelEncoder) - continuous
        // shading has to be quantized to a small, known set of brightness levels so every pixel
        // this renders lands on an exact palette entry that BuildPalette also produced. Spans
        // the full [0,1] range uniformly - final intensity is always clamped to [0,1] before
        // quantizing, regardless of how many lights or what Material produced it.
        static readonly double[] ShadingLevels = Enumerable.Range(0, ShadingLevelCount)
            .Select(i => i / (double)(ShadingLevelCount - 1))
            .ToArray();

        // A Light rotated into the current frame's view space (or the synthetic default
        // headlamp, left unrotated since it's already camera-relative by definition). For a
        // Spot, Direction is the aim axis (same sense as Light.Direction for Spot - the
        // direction it shines toward), not "toward the light" like Directional's Direction.
        readonly record struct EffectiveLight(LightKind Kind, Vector Direction, Point Position, double Intensity,
            double InnerConeAngleDegrees, double OuterConeAngleDegrees);

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
                Material material = mesh.Material ?? DefaultMaterial;

                foreach (Vertex vertex in mesh.Vertices)
                {
                    if (vertex.Color is Color vertexColor)
                    {
                        AddColor((vertexColor.R, vertexColor.G, vertexColor.B));
                    }
                }
                foreach (Face face in mesh.Faces)
                {
                    Color baseColor = ResolveFaceBaseColor(mesh, face, material);
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

            // Lights rotate with the scene, like a fixture fixed in the room, unless the scene
            // has none - then fall back to the fixed camera-relative headlamp (see ViewDirection).
            List<Light> userLights = objects.OfType<Light>().ToList();
            List<EffectiveLight> lights = userLights.Count > 0
                ? userLights.Select(l => l.Kind switch
                    {
                        LightKind.Point => new EffectiveLight(LightKind.Point, default, RotateLocal(l.Position), l.Intensity, 0, 0),
                        LightKind.Spot => new EffectiveLight(LightKind.Spot, Vector.Normalize(rotation * l.Direction), RotateLocal(l.Position),
                            l.Intensity, l.InnerConeAngleDegrees, l.OuterConeAngleDegrees),
                        _ => new EffectiveLight(LightKind.Directional, Vector.Normalize(rotation * l.Direction), default, l.Intensity, 0, 0),
                    })
                  .ToList()
                : new List<EffectiveLight> { new EffectiveLight(LightKind.Directional, ViewDirection, default, 1.0, 0, 0) };

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
                    DrawMesh(framebuffer, Project, RotateLocal, mesh, renderMode, showNormals, radius, lights);
                }
            }

            return framebuffer;
        }

        static Color ResolveFaceBaseColor(Mesh mesh, Face face, Material material)
        {
            return mesh.Vertices[face.A].Color
                ?? mesh.Vertices[face.B].Color
                ?? mesh.Vertices[face.C].Color
                ?? material.Color;
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

        // Ambient + emissive + Lambertian diffuse + (optional) Phong specular, summed over every
        // active light, clamped to [0,1] and quantized. unitNormal and rotatedCentroid are
        // already in view space (post-rotation), matching how EffectiveLight positions/
        // directions were prepared in Render. Emissive contributes only its luminance (average
        // of R/G/B) - a genuine colored glow would need its own base color in the shading
        // formula, on top of the face's own color, which the palette-quantization scheme (see
        // BuildPalette) isn't built to combine.
        static double ComputeIntensity(Vector unitNormal, Point rotatedCentroid, Material material, IReadOnlyList<EffectiveLight> lights)
        {
            double emissiveLuminance = (material.Emissive.R + material.Emissive.G + material.Emissive.B) / (3.0 * 255.0);
            double total = material.Ambient + emissiveLuminance;
            foreach (EffectiveLight light in lights)
            {
                Vector lightDir = light.Kind == LightKind.Directional
                    ? light.Direction
                    : Vector.Normalize(light.Position - rotatedCentroid); // Point and Spot both have a Position

                double spotAttenuation = light.Kind == LightKind.Spot
                    ? SpotAttenuation(light, lightDir)
                    : 1.0;
                if (spotAttenuation <= 0) continue;

                double diffuseFactor = Math.Max(0, Vector.Dot(unitNormal, lightDir));
                total += diffuseFactor * material.Diffuse * light.Intensity * spotAttenuation;

                if (material.Specular > 0 && diffuseFactor > 0)
                {
                    Vector reflectDir = unitNormal * (2 * Vector.Dot(unitNormal, lightDir)) - lightDir;
                    double specularFactor = Math.Pow(Math.Max(0, Vector.Dot(reflectDir, ViewDirection)), material.Shininess);
                    total += specularFactor * material.Specular * light.Intensity * spotAttenuation;
                }
            }
            return QuantizeIntensity(Math.Clamp(total, 0.0, 1.0));
        }

        // Linear falloff by angle from the spot's aim axis - simpler to reason about (and to
        // hand-verify in tests) than glTF's own cosine-based smoothstep formula, at the cost of
        // not matching it exactly pixel-for-pixel. lightDir points FROM the surface TOWARD the
        // light, so -lightDir is the direction the light travels to reach the surface, which is
        // what's compared against the spot's own aim direction.
        static double SpotAttenuation(EffectiveLight light, Vector lightDir)
        {
            double cos = Math.Clamp(Vector.Dot(light.Direction, -lightDir), -1.0, 1.0);
            double angleDegrees = Math.Acos(cos) * 180.0 / Math.PI;

            if (angleDegrees >= light.OuterConeAngleDegrees) return 0.0;
            if (angleDegrees <= light.InnerConeAngleDegrees) return 1.0;

            double coneSpan = light.OuterConeAngleDegrees - light.InnerConeAngleDegrees;
            return coneSpan > 1e-9 ? 1.0 - (angleDegrees - light.InnerConeAngleDegrees) / coneSpan : 0.0;
        }

        static (byte R, byte G, byte B) ShadedColor(Color baseColor, double intensity) =>
        (
            (byte)Math.Round(baseColor.R * intensity),
            (byte)Math.Round(baseColor.G * intensity),
            (byte)Math.Round(baseColor.B * intensity)
        );

        static void DrawMesh(Framebuffer framebuffer, Func<Point, (int X, int Y, bool Visible)> project,
            Func<Point, Point> rotateLocal, Mesh mesh, RenderMode renderMode, bool showNormals, double radius,
            IReadOnlyList<EffectiveLight> lights)
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
                Material material = mesh.Material ?? DefaultMaterial;

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

                    // Backface culling is always camera-relative, regardless of how the face is
                    // lit - a face pointing away from the viewer is skipped either way.
                    double cullTest = Vector.Dot(normal, ViewDirection) / normalLength;
                    if (cullTest <= 0) continue;

                    Vector unitNormal = normal / normalLength;
                    Point rotatedCentroid = new Point((ra.X + rb.X + rc.X) / 3.0, (ra.Y + rb.Y + rc.Y) / 3.0, (ra.Z + rb.Z + rc.Z) / 3.0);
                    double intensity = ComputeIntensity(unitNormal, rotatedCentroid, material, lights);

                    Color baseColor = ResolveFaceBaseColor(mesh, face, material);
                    (byte R, byte G, byte B) color = ShadedColor(baseColor, intensity);

                    var sa = project(a);
                    var sb = project(b);
                    var sc = project(c);
                    if (!sa.Visible || !sb.Visible || !sc.Visible) continue;

                    // -rotated.Z: same camera-relative convention as ViewDirection/backface
                    // culling above, just negated so smaller means closer (see Framebuffer's
                    // depth-buffer comment).
                    framebuffer.FillTriangle(sa.X, sa.Y, -ra.Z, sb.X, sb.Y, -rb.Z, sc.X, sc.Y, -rc.Z, color.R, color.G, color.B);
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
