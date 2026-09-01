using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace DDD
{
    // Stanford Polygon File Format (.ply) - DDD's native mesh interchange format (see PLAN.md:
    // chosen over inventing a DDD-specific format, since it already covers vertices/faces/
    // optional per-vertex normal/color). Reading supports ASCII and binary (both little- and
    // big-endian); writing is ASCII-only, since hand-authored meshes don't need binary's smaller
    // footprint - binary reading exists to load bundled reference-mesh assets (see 1d).
    //
    // Known scope limits: only "vertex" and "face" elements are understood - a file with an
    // extra element (e.g. "edge") between or after them will misparse, since there's no general
    // per-element skip logic. Unrecognized vertex properties (e.g. texture coordinates, alpha)
    // are silently ignored rather than rejected.
    internal static class PlyFormat
    {
        static readonly Color DefaultExportColor = new Color(255, 255, 255);

        readonly record struct VertexProperty(string Type, string Name);

        sealed class Header
        {
            public string Format = "";
            public int VertexCount;
            public int FaceCount;
            public List<VertexProperty> VertexProperties = new();
            public string FaceCountType = "uchar";
            public string FaceIndexType = "int";
            public int BodyStart; // byte offset right after "end_header\n"
        }

        readonly record struct VertexIndices(int X, int Y, int Z, int Nx, int Ny, int Nz, int Red, int Green, int Blue);

        public static Mesh Read(string path) => Parse(File.ReadAllBytes(path));

        public static Mesh ReadEmbeddedResource(string logicalName)
        {
            using Stream? stream = typeof(PlyFormat).Assembly.GetManifestResourceStream(logicalName);
            if (stream is null)
            {
                throw new FileNotFoundException($"Embedded .ply resource '{logicalName}' was not found.");
            }
            using var memoryStream = new MemoryStream();
            stream.CopyTo(memoryStream);
            return Parse(memoryStream.ToArray());
        }

        public static void Write(Mesh mesh, string path) => File.WriteAllText(path, Serialize(mesh));

        // Convenience/test entry point for hand-written ASCII .ply content, one line per element.
        public static Mesh Parse(IEnumerable<string> lines) =>
            Parse(Encoding.ASCII.GetBytes(string.Join('\n', lines) + '\n'));

        public static Mesh Parse(byte[] bytes)
        {
            Header header = ParseHeader(bytes);

            return header.Format switch
            {
                "ascii" => ParseAsciiBody(bytes, header),
                "binary_little_endian" => ParseBinaryBody(bytes, header, bigEndian: false),
                "binary_big_endian" => ParseBinaryBody(bytes, header, bigEndian: true),
                _ => throw new FormatException($"Unsupported .ply format '{header.Format}'."),
            };
        }

        static Header ParseHeader(byte[] bytes)
        {
            int headerEnd = FindHeaderEnd(bytes);
            string headerText = Encoding.ASCII.GetString(bytes, 0, headerEnd);
            string[] headerLines = headerText.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            if (headerLines.Length == 0 || headerLines[0] != "ply")
            {
                throw new FormatException("Not a .ply file (expected 'ply' as the first line).");
            }
            if (headerLines.Length < 2)
            {
                throw new FormatException("Malformed .ply header: missing 'format' line.");
            }

            string[] formatTokens = headerLines[1].Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (formatTokens.Length < 2 || formatTokens[0] != "format")
            {
                throw new FormatException("Malformed .ply header: expected a 'format' line.");
            }
            string format = formatTokens[1];
            if (format != "ascii" && format != "binary_little_endian" && format != "binary_big_endian")
            {
                throw new FormatException($"Unsupported .ply format '{format}'.");
            }

            var header = new Header { Format = format, BodyStart = headerEnd };
            bool inVertexElement = false, inFaceElement = false;

            for (int i = 2; i < headerLines.Length; i++)
            {
                string[] tokens = headerLines[i].Split(' ', StringSplitOptions.RemoveEmptyEntries);
                if (tokens.Length == 0 || tokens[0] == "comment") continue;

                if (tokens[0] == "element")
                {
                    inVertexElement = tokens[1] == "vertex";
                    inFaceElement = tokens[1] == "face";
                    int count = int.Parse(tokens[2], CultureInfo.InvariantCulture);
                    if (inVertexElement) header.VertexCount = count;
                    else if (inFaceElement) header.FaceCount = count;
                }
                else if (tokens[0] == "property" && inVertexElement)
                {
                    header.VertexProperties.Add(new VertexProperty(tokens[1], tokens[2])); // property <type> <name>
                }
                else if (tokens[0] == "property" && inFaceElement && tokens.Length >= 4 && tokens[1] == "list")
                {
                    header.FaceCountType = tokens[2];
                    header.FaceIndexType = tokens[3];
                }
            }

            return header;
        }

        static int FindHeaderEnd(byte[] bytes)
        {
            byte[] marker = Encoding.ASCII.GetBytes("end_header");
            for (int i = 0; i <= bytes.Length - marker.Length; i++)
            {
                bool match = true;
                for (int k = 0; k < marker.Length; k++)
                {
                    if (bytes[i + k] != marker[k]) { match = false; break; }
                }
                if (!match) continue;

                int j = i + marker.Length;
                while (j < bytes.Length && bytes[j] != (byte)'\n') j++;
                return Math.Min(j + 1, bytes.Length);
            }
            throw new FormatException("Malformed .ply file: 'end_header' not found.");
        }

        static VertexIndices ResolveVertexIndices(List<VertexProperty> properties)
        {
            int IndexOfName(string name) => properties.FindIndex(p => p.Name == name);

            int x = IndexOfName("x"), y = IndexOfName("y"), z = IndexOfName("z");
            if (x < 0 || y < 0 || z < 0)
            {
                throw new FormatException(".ply vertex element is missing x/y/z properties.");
            }

            return new VertexIndices(x, y, z,
                IndexOfName("nx"), IndexOfName("ny"), IndexOfName("nz"),
                IndexOfName("red"), IndexOfName("green"), IndexOfName("blue"));
        }

        static Vertex BuildVertex(double[] values, VertexIndices idx)
        {
            Point position = new Point(values[idx.X], values[idx.Y], values[idx.Z]);
            bool hasNormal = idx.Nx >= 0 && idx.Ny >= 0 && idx.Nz >= 0;
            bool hasColor = idx.Red >= 0 && idx.Green >= 0 && idx.Blue >= 0;

            Vector normal = hasNormal ? new Vector(values[idx.Nx], values[idx.Ny], values[idx.Nz]) : default;
            Color color = hasColor
                ? new Color((byte)values[idx.Red], (byte)values[idx.Green], (byte)values[idx.Blue])
                : default;

            return (hasNormal, hasColor) switch
            {
                (true, true) => new Vertex(position, normal, color),
                (true, false) => new Vertex(position, normal),
                (false, true) => new Vertex(position, color),
                _ => new Vertex(position),
            };
        }

        static void AddFanTriangulatedFace(Mesh mesh, int[] indices)
        {
            if (indices.Length < 3)
            {
                throw new FormatException("Malformed .ply face: fewer than 3 vertex indices.");
            }
            // Fan-triangulate faces with more than 3 vertices (e.g. quads) - our Mesh only
            // stores triangles.
            for (int k = 1; k < indices.Length - 1; k++)
            {
                mesh.AddFace(indices[0], indices[k], indices[k + 1]);
            }
        }

        static Mesh ParseAsciiBody(byte[] bytes, Header header)
        {
            string bodyText = Encoding.ASCII.GetString(bytes, header.BodyStart, bytes.Length - header.BodyStart);
            string[] bodyLines = bodyText.Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            int lineIndex = 0;

            string NextLine()
            {
                if (lineIndex >= bodyLines.Length)
                {
                    throw new FormatException("Unexpected end of .ply file.");
                }
                return bodyLines[lineIndex++];
            }

            VertexIndices idx = ResolveVertexIndices(header.VertexProperties);
            var mesh = new Mesh();

            for (int i = 0; i < header.VertexCount; i++)
            {
                string[] tokens = NextLine().Split(' ', StringSplitOptions.RemoveEmptyEntries);
                var values = new double[header.VertexProperties.Count];
                for (int p = 0; p < values.Length; p++)
                {
                    values[p] = double.Parse(tokens[p], CultureInfo.InvariantCulture);
                }
                mesh.AddVertex(BuildVertex(values, idx));
            }

            for (int i = 0; i < header.FaceCount; i++)
            {
                int[] tokens = NextLine().Split(' ', StringSplitOptions.RemoveEmptyEntries)
                    .Select(t => int.Parse(t, CultureInfo.InvariantCulture)).ToArray();
                AddFanTriangulatedFace(mesh, tokens.Skip(1).ToArray()); // tokens[0] is the list count
            }

            return mesh;
        }

        static Mesh ParseBinaryBody(byte[] bytes, Header header, bool bigEndian)
        {
            int offset = header.BodyStart;
            VertexIndices idx = ResolveVertexIndices(header.VertexProperties);
            var mesh = new Mesh();

            for (int i = 0; i < header.VertexCount; i++)
            {
                var values = new double[header.VertexProperties.Count];
                for (int p = 0; p < values.Length; p++)
                {
                    values[p] = ReadBinaryNumber(bytes, ref offset, header.VertexProperties[p].Type, bigEndian);
                }
                mesh.AddVertex(BuildVertex(values, idx));
            }

            for (int i = 0; i < header.FaceCount; i++)
            {
                int listCount = (int)ReadBinaryNumber(bytes, ref offset, header.FaceCountType, bigEndian);
                var indices = new int[listCount];
                for (int k = 0; k < listCount; k++)
                {
                    indices[k] = (int)ReadBinaryNumber(bytes, ref offset, header.FaceIndexType, bigEndian);
                }
                AddFanTriangulatedFace(mesh, indices);
            }

            return mesh;
        }

        static double ReadBinaryNumber(byte[] bytes, ref int offset, string type, bool bigEndian)
        {
            ReadOnlySpan<byte> span = bytes.AsSpan(offset);
            double value;
            int size;
            switch (type)
            {
                case "char": case "int8": value = (sbyte)bytes[offset]; size = 1; break;
                case "uchar": case "uint8": value = bytes[offset]; size = 1; break;
                case "short": case "int16":
                    value = bigEndian ? BinaryPrimitives.ReadInt16BigEndian(span) : BinaryPrimitives.ReadInt16LittleEndian(span);
                    size = 2; break;
                case "ushort": case "uint16":
                    value = bigEndian ? BinaryPrimitives.ReadUInt16BigEndian(span) : BinaryPrimitives.ReadUInt16LittleEndian(span);
                    size = 2; break;
                case "int": case "int32":
                    value = bigEndian ? BinaryPrimitives.ReadInt32BigEndian(span) : BinaryPrimitives.ReadInt32LittleEndian(span);
                    size = 4; break;
                case "uint": case "uint32":
                    value = bigEndian ? BinaryPrimitives.ReadUInt32BigEndian(span) : BinaryPrimitives.ReadUInt32LittleEndian(span);
                    size = 4; break;
                case "float": case "float32":
                    value = bigEndian ? BinaryPrimitives.ReadSingleBigEndian(span) : BinaryPrimitives.ReadSingleLittleEndian(span);
                    size = 4; break;
                case "double": case "float64":
                    value = bigEndian ? BinaryPrimitives.ReadDoubleBigEndian(span) : BinaryPrimitives.ReadDoubleLittleEndian(span);
                    size = 8; break;
                default:
                    throw new FormatException($"Unsupported .ply property type '{type}'.");
            }
            offset += size;
            return value;
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
