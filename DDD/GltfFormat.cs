using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace DDD
{
    // glTF 2.0, binary container (.glb) only - DDD's scene interchange format for multi-object
    // work (see PLAN.md 1h: .ply covers a single mesh well, but has no concept of multiple
    // positioned objects, materials, lights, or cameras). Loose .gltf + .bin + external textures
    // is not supported; a .glb is a single self-contained file, matching the instinct that
    // bundled the 1d reference meshes as binary .ply rather than several loose files.
    //
    // This first slice covers mesh geometry only (POSITION/NORMAL/COLOR_0 + indices, TRIANGLES
    // mode) - materials, lights, cameras, scene-graph flattening, and animation baking are
    // follow-up work (PLAN.md 1h lists the full scope and what's deliberately excluded, e.g.
    // textures and skinning).
    //
    // Known scope limits for this slice: only the first mesh's first primitive is read on
    // import; only TRIANGLES mode is supported (others are rejected, not converted).
    //
    // Nested DTO type names are prefixed "Gltf" (GltfMesh, GltfNode, ...) purely to avoid
    // colliding with DDD's own Mesh/etc. types in this namespace - a nested type would otherwise
    // shadow the outer one and silently break every public Mesh reference here. Property *names*
    // on those DTOs are NOT prefixed (Mesh, Scene, Buffer, ...), since JsonNamingPolicy.CamelCase
    // maps them straight onto glTF's actual JSON field names ("mesh", "scene", "buffer", ...).
    internal static class GltfFormat
    {
        const uint GlbMagic = 0x46546C67; // "glTF"
        const uint GlbVersion = 2;
        const uint ChunkTypeJson = 0x4E4F534A; // "JSON"
        const uint ChunkTypeBin = 0x004E4942; // "BIN\0"

        const int ComponentByte = 5120;
        const int ComponentUnsignedByte = 5121;
        const int ComponentShort = 5122;
        const int ComponentUnsignedShort = 5123;
        const int ComponentUnsignedInt = 5125;
        const int ComponentFloat = 5126;

        const int TargetArrayBuffer = 34962;
        const int TargetElementArrayBuffer = 34963;
        const int ModeTriangles = 4;

        static readonly JsonSerializerOptions JsonOptions = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        };

        // --- glTF JSON document model (only the fields this slice reads/writes) ---

        sealed class GltfDocument
        {
            public GltfAsset Asset { get; set; } = new();
            public int? Scene { get; set; }
            public List<GltfScene>? Scenes { get; set; }
            public List<GltfNode>? Nodes { get; set; }
            public List<GltfMesh>? Meshes { get; set; }
            public List<GltfAccessor>? Accessors { get; set; }
            public List<GltfBufferView>? BufferViews { get; set; }
            public List<GltfBuffer>? Buffers { get; set; }
        }
        sealed class GltfAsset
        {
            public string Version { get; set; } = "2.0";
        }
        sealed class GltfScene
        {
            public int[]? Nodes { get; set; }
        }
        sealed class GltfNode
        {
            public int? Mesh { get; set; }
        }
        sealed class GltfMesh
        {
            public List<GltfPrimitive> Primitives { get; set; } = new();
        }
        sealed class GltfPrimitive
        {
            public Dictionary<string, int> Attributes { get; set; } = new();
            public int? Indices { get; set; }
            public int? Material { get; set; }
            public int Mode { get; set; } = ModeTriangles;
        }
        sealed class GltfAccessor
        {
            public int BufferView { get; set; }
            public int ByteOffset { get; set; }
            public int ComponentType { get; set; }
            public bool Normalized { get; set; }
            public int Count { get; set; }
            public string Type { get; set; } = "";
            public double[]? Min { get; set; }
            public double[]? Max { get; set; }
        }
        sealed class GltfBufferView
        {
            public int Buffer { get; set; }
            public int ByteOffset { get; set; }
            public int ByteLength { get; set; }
            public int? Target { get; set; }
        }
        sealed class GltfBuffer
        {
            public int ByteLength { get; set; }
        }

        // --- Public API ---

        public static Mesh Read(string path) => Parse(System.IO.File.ReadAllBytes(path));

        public static void Write(Mesh mesh, string path) => System.IO.File.WriteAllBytes(path, Serialize(mesh));

        public static Mesh Parse(byte[] glb)
        {
            (GltfDocument document, byte[] binaryChunk) = ReadGlbContainer(glb);

            if (document.Meshes is null || document.Meshes.Count == 0)
            {
                throw new FormatException("glTF document has no meshes.");
            }
            GltfPrimitive primitive = document.Meshes[0].Primitives.FirstOrDefault()
                ?? throw new FormatException("glTF mesh has no primitives.");
            if (primitive.Mode != ModeTriangles)
            {
                throw new FormatException($"Only TRIANGLES-mode primitives are supported (got mode {primitive.Mode}).");
            }
            if (!primitive.Attributes.TryGetValue("POSITION", out int positionIndex))
            {
                throw new FormatException("glTF primitive is missing a POSITION attribute.");
            }

            double[][] positions = ReadAccessorVectors(document, binaryChunk, positionIndex);
            double[][]? normals = primitive.Attributes.TryGetValue("NORMAL", out int normalIndex)
                ? ReadAccessorVectors(document, binaryChunk, normalIndex)
                : null;
            double[][]? colors = primitive.Attributes.TryGetValue("COLOR_0", out int colorIndex)
                ? ReadAccessorVectors(document, binaryChunk, colorIndex)
                : null;

            var mesh = new Mesh();
            for (int i = 0; i < positions.Length; i++)
            {
                Point position = new Point(positions[i][0], positions[i][1], positions[i][2]);
                bool hasNormal = normals != null;
                bool hasColor = colors != null;
                Vector normal = hasNormal ? new Vector(normals![i][0], normals[i][1], normals[i][2]) : default;
                Color color = hasColor
                    ? new Color(ToByte(colors![i][0]), ToByte(colors[i][1]), ToByte(colors[i][2]))
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

            if (primitive.Indices is int indicesAccessorIndex)
            {
                int[] indices = ReadAccessorScalars(document, binaryChunk, indicesAccessorIndex);
                for (int i = 0; i + 2 < indices.Length; i += 3)
                {
                    mesh.AddFace(indices[i], indices[i + 1], indices[i + 2]);
                }
            }

            return mesh;
        }

        public static byte[] Serialize(Mesh mesh)
        {
            bool hasNormal = mesh.Vertices.Any(v => v.Normal.HasValue);
            bool hasColor = mesh.Vertices.Any(v => v.Color.HasValue);

            using var binary = new System.IO.MemoryStream();
            var bufferViews = new List<GltfBufferView>();
            var accessors = new List<GltfAccessor>();
            var attributes = new Dictionary<string, int>();

            var positions = mesh.Vertices.Select(v => new[] { v.Position.X, v.Position.Y, v.Position.Z }).ToArray();
            attributes["POSITION"] = WriteVec3FloatAccessor(binary, bufferViews, accessors, positions, TargetArrayBuffer, includeBounds: true);

            if (hasNormal)
            {
                var normals = mesh.Vertices.Select(v =>
                {
                    Vector n = v.Normal ?? default;
                    return new[] { n.X, n.Y, n.Z };
                }).ToArray();
                attributes["NORMAL"] = WriteVec3FloatAccessor(binary, bufferViews, accessors, normals, TargetArrayBuffer, includeBounds: false);
            }
            if (hasColor)
            {
                var colors = mesh.Vertices.Select(v =>
                {
                    Color c = v.Color ?? default;
                    return new[] { c.R, c.G, c.B };
                }).ToArray();
                attributes["COLOR_0"] = WriteVec3UnsignedByteAccessor(binary, bufferViews, accessors, colors);
            }

            int[] indices = mesh.Faces.SelectMany(f => new[] { f.A, f.B, f.C }).ToArray();
            int indicesAccessorIndex = WriteScalarUnsignedIntAccessor(binary, bufferViews, accessors, indices, TargetElementArrayBuffer);

            int[] sceneNodeIndices = { 0 };
            var document = new GltfDocument
            {
                Scene = 0,
                Scenes = new List<GltfScene> { new GltfScene { Nodes = sceneNodeIndices } },
                Nodes = new List<GltfNode> { new GltfNode { Mesh = 0 } },
                Meshes = new List<GltfMesh>
                {
                    new GltfMesh
                    {
                        Primitives = new List<GltfPrimitive>
                        {
                            new GltfPrimitive { Attributes = attributes, Indices = indicesAccessorIndex, Mode = ModeTriangles },
                        },
                    },
                },
                Accessors = accessors,
                BufferViews = bufferViews,
                Buffers = new List<GltfBuffer> { new GltfBuffer { ByteLength = (int)binary.Length } },
            };

            return WriteGlbContainer(document, binary.ToArray());
        }

        // --- .glb container framing ---

        static (GltfDocument Document, byte[] BinaryChunk) ReadGlbContainer(byte[] glb)
        {
            if (glb.Length < 12 || BinaryPrimitives.ReadUInt32LittleEndian(glb) != GlbMagic)
            {
                throw new FormatException("Not a .glb file (bad magic number).");
            }
            uint version = BinaryPrimitives.ReadUInt32LittleEndian(glb.AsSpan(4));
            if (version != GlbVersion)
            {
                throw new FormatException($"Only glTF binary version 2 is supported (got version {version}).");
            }

            int offset = 12;
            string? jsonText = null;
            byte[] binaryChunk = Array.Empty<byte>();

            while (offset + 8 <= glb.Length)
            {
                uint chunkLength = BinaryPrimitives.ReadUInt32LittleEndian(glb.AsSpan(offset));
                uint chunkType = BinaryPrimitives.ReadUInt32LittleEndian(glb.AsSpan(offset + 4));
                offset += 8;

                if (chunkType == ChunkTypeJson)
                {
                    jsonText = Encoding.UTF8.GetString(glb, offset, (int)chunkLength);
                }
                else if (chunkType == ChunkTypeBin)
                {
                    binaryChunk = glb.AsSpan(offset, (int)chunkLength).ToArray();
                }
                offset += (int)chunkLength;
            }

            if (jsonText is null)
            {
                throw new FormatException(".glb file has no JSON chunk.");
            }
            GltfDocument document = JsonSerializer.Deserialize<GltfDocument>(jsonText, JsonOptions)
                ?? throw new FormatException(".glb JSON chunk did not parse to a document.");
            return (document, binaryChunk);
        }

        static byte[] WriteGlbContainer(GltfDocument document, byte[] binaryChunk)
        {
            byte[] jsonBytes = JsonSerializer.SerializeToUtf8Bytes(document, JsonOptions);
            int jsonPadded = Align4(jsonBytes.Length);
            int binPadded = Align4(binaryChunk.Length);

            int totalLength = 12 + 8 + jsonPadded + 8 + binPadded;
            var glb = new byte[totalLength];
            int offset = 0;

            BinaryPrimitives.WriteUInt32LittleEndian(glb.AsSpan(offset), GlbMagic); offset += 4;
            BinaryPrimitives.WriteUInt32LittleEndian(glb.AsSpan(offset), GlbVersion); offset += 4;
            BinaryPrimitives.WriteUInt32LittleEndian(glb.AsSpan(offset), (uint)totalLength); offset += 4;

            BinaryPrimitives.WriteUInt32LittleEndian(glb.AsSpan(offset), (uint)jsonPadded); offset += 4;
            BinaryPrimitives.WriteUInt32LittleEndian(glb.AsSpan(offset), ChunkTypeJson); offset += 4;
            jsonBytes.CopyTo(glb.AsSpan(offset));
            for (int i = jsonBytes.Length; i < jsonPadded; i++) glb[offset + i] = 0x20; // space-pad JSON
            offset += jsonPadded;

            BinaryPrimitives.WriteUInt32LittleEndian(glb.AsSpan(offset), (uint)binPadded); offset += 4;
            BinaryPrimitives.WriteUInt32LittleEndian(glb.AsSpan(offset), ChunkTypeBin); offset += 4;
            binaryChunk.CopyTo(glb.AsSpan(offset));
            // Remaining bytes (zero-pad for BIN) are already zero from array initialization.

            return glb;
        }

        static int Align4(int length) => (length + 3) / 4 * 4;

        // --- Accessor read helpers ---

        static double[][] ReadAccessorVectors(GltfDocument document, byte[] binaryChunk, int accessorIndex)
        {
            GltfAccessor accessor = document.Accessors![accessorIndex];
            GltfBufferView bufferView = document.BufferViews![accessor.BufferView];
            int componentCount = accessor.Type switch
            {
                "SCALAR" => 1,
                "VEC2" => 2,
                "VEC3" => 3,
                "VEC4" => 4,
                _ => throw new FormatException($"Unsupported accessor type '{accessor.Type}'."),
            };

            int offset = bufferView.ByteOffset + accessor.ByteOffset;
            int componentSize = ComponentSize(accessor.ComponentType);
            var result = new double[accessor.Count][];
            for (int i = 0; i < accessor.Count; i++)
            {
                var vector = new double[componentCount];
                for (int c = 0; c < componentCount; c++)
                {
                    vector[c] = ReadComponent(binaryChunk, offset, accessor.ComponentType, accessor.Normalized);
                    offset += componentSize;
                }
                result[i] = vector;
            }
            return result;
        }

        static int[] ReadAccessorScalars(GltfDocument document, byte[] binaryChunk, int accessorIndex)
        {
            double[][] vectors = ReadAccessorVectors(document, binaryChunk, accessorIndex);
            return vectors.Select(v => (int)v[0]).ToArray();
        }

        static int ComponentSize(int componentType) => componentType switch
        {
            ComponentByte or ComponentUnsignedByte => 1,
            ComponentShort or ComponentUnsignedShort => 2,
            ComponentUnsignedInt or ComponentFloat => 4,
            _ => throw new FormatException($"Unsupported accessor component type {componentType}."),
        };

        static double ReadComponent(byte[] bytes, int offset, int componentType, bool normalized)
        {
            ReadOnlySpan<byte> span = bytes.AsSpan(offset);
            switch (componentType)
            {
                case ComponentUnsignedByte:
                    return normalized ? bytes[offset] / 255.0 : bytes[offset];
                case ComponentByte:
                    return normalized ? Math.Max((sbyte)bytes[offset] / 127.0, -1.0) : (sbyte)bytes[offset];
                case ComponentUnsignedShort:
                    {
                        ushort v = BinaryPrimitives.ReadUInt16LittleEndian(span);
                        return normalized ? v / 65535.0 : v;
                    }
                case ComponentShort:
                    {
                        short v = BinaryPrimitives.ReadInt16LittleEndian(span);
                        return normalized ? Math.Max(v / 32767.0, -1.0) : v;
                    }
                case ComponentUnsignedInt:
                    return BinaryPrimitives.ReadUInt32LittleEndian(span);
                case ComponentFloat:
                    return BinaryPrimitives.ReadSingleLittleEndian(span);
                default:
                    throw new FormatException($"Unsupported accessor component type {componentType}.");
            }
        }

        static byte ToByte(double normalized) => (byte)Math.Round(Math.Clamp(normalized, 0.0, 1.0) * 255.0);

        // --- Accessor write helpers ---

        static int WriteVec3FloatAccessor(System.IO.MemoryStream binary, List<GltfBufferView> bufferViews, List<GltfAccessor> accessors,
            double[][] vectors, int target, bool includeBounds)
        {
            int byteOffset = (int)binary.Position;
            Span<byte> buf = stackalloc byte[4];
            foreach (double[] v in vectors)
            {
                for (int c = 0; c < 3; c++)
                {
                    BinaryPrimitives.WriteSingleLittleEndian(buf, (float)v[c]);
                    binary.Write(buf);
                }
            }

            double[]? min = null, max = null;
            if (includeBounds && vectors.Length > 0)
            {
                min = new[] { vectors.Min(v => v[0]), vectors.Min(v => v[1]), vectors.Min(v => v[2]) };
                max = new[] { vectors.Max(v => v[0]), vectors.Max(v => v[1]), vectors.Max(v => v[2]) };
            }

            bufferViews.Add(new GltfBufferView { Buffer = 0, ByteOffset = byteOffset, ByteLength = (int)binary.Position - byteOffset, Target = target });
            accessors.Add(new GltfAccessor
            {
                BufferView = bufferViews.Count - 1,
                ComponentType = ComponentFloat,
                Count = vectors.Length,
                Type = "VEC3",
                Min = min,
                Max = max,
            });
            return accessors.Count - 1;
        }

        static int WriteVec3UnsignedByteAccessor(System.IO.MemoryStream binary, List<GltfBufferView> bufferViews, List<GltfAccessor> accessors, byte[][] vectors)
        {
            int byteOffset = (int)binary.Position;
            foreach (byte[] v in vectors)
            {
                binary.WriteByte(v[0]);
                binary.WriteByte(v[1]);
                binary.WriteByte(v[2]);
            }
            PadTo4(binary);

            bufferViews.Add(new GltfBufferView { Buffer = 0, ByteOffset = byteOffset, ByteLength = (int)binary.Position - byteOffset, Target = TargetArrayBuffer });
            accessors.Add(new GltfAccessor
            {
                BufferView = bufferViews.Count - 1,
                ComponentType = ComponentUnsignedByte,
                Normalized = true,
                Count = vectors.Length,
                Type = "VEC3",
            });
            return accessors.Count - 1;
        }

        static int WriteScalarUnsignedIntAccessor(System.IO.MemoryStream binary, List<GltfBufferView> bufferViews, List<GltfAccessor> accessors, int[] scalars, int target)
        {
            int byteOffset = (int)binary.Position;
            Span<byte> buf = stackalloc byte[4];
            foreach (int s in scalars)
            {
                BinaryPrimitives.WriteUInt32LittleEndian(buf, (uint)s);
                binary.Write(buf);
            }

            bufferViews.Add(new GltfBufferView { Buffer = 0, ByteOffset = byteOffset, ByteLength = (int)binary.Position - byteOffset, Target = target });
            accessors.Add(new GltfAccessor
            {
                BufferView = bufferViews.Count - 1,
                ComponentType = ComponentUnsignedInt,
                Count = scalars.Length,
                Type = "SCALAR",
            });
            return accessors.Count - 1;
        }

        static void PadTo4(System.IO.MemoryStream binary)
        {
            while (binary.Position % 4 != 0) binary.WriteByte(0);
        }
    }
}
