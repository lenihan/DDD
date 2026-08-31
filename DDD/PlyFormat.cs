using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace DDD
{
    // Stanford Polygon File Format (.ply), ASCII variant only - DDD's native mesh interchange
    // format (see PLAN.md: chosen over inventing a DDD-specific format, since it already covers
    // vertices/faces/optional per-vertex normal/color). Binary .ply is not supported.
    //
    // Known scope limits: only "vertex" and "face" elements are understood - a file with an
    // extra element (e.g. "edge") between or after them will misparse, since there's no general
    // per-element skip logic. Unrecognized vertex properties (e.g. texture coordinates, alpha)
    // are silently ignored rather than rejected.
    internal static class PlyFormat
    {
        static readonly Color DefaultExportColor = new Color(255, 255, 255);

        public static Mesh Read(string path) => Parse(File.ReadLines(path));

        public static void Write(Mesh mesh, string path) => File.WriteAllText(path, Serialize(mesh));

        public static Mesh Parse(IEnumerable<string> lines)
        {
            using IEnumerator<string> enumerator = lines.GetEnumerator();

            string NextLine()
            {
                if (!enumerator.MoveNext())
                {
                    throw new FormatException("Unexpected end of .ply file.");
                }
                return enumerator.Current.Trim();
            }

            if (NextLine() != "ply")
            {
                throw new FormatException("Not a .ply file (expected 'ply' as the first line).");
            }

            string format = NextLine();
            if (format != "format ascii 1.0")
            {
                throw new FormatException($"Only 'format ascii 1.0' .ply files are supported (got '{format}').");
            }

            var vertexProperties = new List<string>();
            int vertexCount = 0;
            int faceCount = 0;
            bool inVertexElement = false;
            bool inFaceElement = false;

            string line;
            while ((line = NextLine()) != "end_header")
            {
                string[] tokens = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (tokens.Length == 0 || tokens[0] == "comment") continue;

                if (tokens[0] == "element")
                {
                    inVertexElement = tokens[1] == "vertex";
                    inFaceElement = tokens[1] == "face";
                    int count = int.Parse(tokens[2], CultureInfo.InvariantCulture);
                    if (inVertexElement) vertexCount = count;
                    else if (inFaceElement) faceCount = count;
                }
                else if (tokens[0] == "property" && inVertexElement)
                {
                    vertexProperties.Add(tokens[2]); // property <type> <name>
                }
            }

            int xIndex = vertexProperties.IndexOf("x");
            int yIndex = vertexProperties.IndexOf("y");
            int zIndex = vertexProperties.IndexOf("z");
            if (xIndex < 0 || yIndex < 0 || zIndex < 0)
            {
                throw new FormatException(".ply vertex element is missing x/y/z properties.");
            }
            int nxIndex = vertexProperties.IndexOf("nx");
            int nyIndex = vertexProperties.IndexOf("ny");
            int nzIndex = vertexProperties.IndexOf("nz");
            bool hasNormal = nxIndex >= 0 && nyIndex >= 0 && nzIndex >= 0;
            int redIndex = vertexProperties.IndexOf("red");
            int greenIndex = vertexProperties.IndexOf("green");
            int blueIndex = vertexProperties.IndexOf("blue");
            bool hasColor = redIndex >= 0 && greenIndex >= 0 && blueIndex >= 0;

            var mesh = new Mesh();

            for (int i = 0; i < vertexCount; i++)
            {
                string[] values = NextLine().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                Point position = new Point(
                    double.Parse(values[xIndex], CultureInfo.InvariantCulture),
                    double.Parse(values[yIndex], CultureInfo.InvariantCulture),
                    double.Parse(values[zIndex], CultureInfo.InvariantCulture));

                Vector normal = hasNormal
                    ? new Vector(
                        double.Parse(values[nxIndex], CultureInfo.InvariantCulture),
                        double.Parse(values[nyIndex], CultureInfo.InvariantCulture),
                        double.Parse(values[nzIndex], CultureInfo.InvariantCulture))
                    : default;
                Color color = hasColor
                    ? new Color(
                        byte.Parse(values[redIndex], CultureInfo.InvariantCulture),
                        byte.Parse(values[greenIndex], CultureInfo.InvariantCulture),
                        byte.Parse(values[blueIndex], CultureInfo.InvariantCulture))
                    : default;

                Vertex vertex = (hasNormal, hasColor) switch
                {
                    (true, true) => new Vertex(position, normal, color),
                    (true, false) => new Vertex(position, normal),
                    (false, true) => new Vertex(position, color),
                    _ => new Vertex(position),
                };

                mesh.AddVertex(vertex);
            }

            for (int i = 0; i < faceCount; i++)
            {
                string[] values = NextLine().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                int listCount = int.Parse(values[0], CultureInfo.InvariantCulture);
                if (listCount < 3)
                {
                    throw new FormatException("Malformed .ply face: fewer than 3 vertex indices.");
                }

                int[] indices = new int[listCount];
                for (int k = 0; k < listCount; k++)
                {
                    indices[k] = int.Parse(values[1 + k], CultureInfo.InvariantCulture);
                }

                // Fan-triangulate faces with more than 3 vertices (e.g. quads) - our Mesh only
                // stores triangles.
                for (int k = 1; k < listCount - 1; k++)
                {
                    mesh.AddFace(indices[0], indices[k], indices[k + 1]);
                }
            }

            return mesh;
        }

        public static string Serialize(Mesh mesh)
        {
            bool hasNormal = mesh.Vertices.Any(v => v.Normal.HasValue);
            bool hasColor = mesh.Vertices.Any(v => v.Color.HasValue);

            var sb = new StringBuilder();
            sb.Append("ply\n");
            sb.Append("format ascii 1.0\n");
            sb.Append("element vertex ").Append(mesh.Vertices.Count).Append('\n');
            sb.Append("property float x\n");
            sb.Append("property float y\n");
            sb.Append("property float z\n");
            if (hasNormal)
            {
                sb.Append("property float nx\n");
                sb.Append("property float ny\n");
                sb.Append("property float nz\n");
            }
            if (hasColor)
            {
                sb.Append("property uchar red\n");
                sb.Append("property uchar green\n");
                sb.Append("property uchar blue\n");
            }
            sb.Append("element face ").Append(mesh.Faces.Count).Append('\n');
            sb.Append("property list uchar int vertex_indices\n");
            sb.Append("end_header\n");

            foreach (Vertex vertex in mesh.Vertices)
            {
                sb.Append(FormatDouble(vertex.Position.X)).Append(' ')
                  .Append(FormatDouble(vertex.Position.Y)).Append(' ')
                  .Append(FormatDouble(vertex.Position.Z));

                if (hasNormal)
                {
                    // A missing per-vertex normal defaults to (0,0,0) here - degenerate, but
                    // every row in an ASCII .ply element must have the same columns.
                    Vector normal = vertex.Normal ?? default;
                    sb.Append(' ').Append(FormatDouble(normal.X))
                      .Append(' ').Append(FormatDouble(normal.Y))
                      .Append(' ').Append(FormatDouble(normal.Z));
                }
                if (hasColor)
                {
                    Color color = vertex.Color ?? DefaultExportColor;
                    sb.Append(' ').Append(color.R).Append(' ').Append(color.G).Append(' ').Append(color.B);
                }
                sb.Append('\n');
            }

            foreach (Face face in mesh.Faces)
            {
                sb.Append("3 ").Append(face.A).Append(' ').Append(face.B).Append(' ').Append(face.C).Append('\n');
            }

            return sb.ToString();
        }

        static string FormatDouble(double value) => value.ToString(CultureInfo.InvariantCulture);
    }
}
