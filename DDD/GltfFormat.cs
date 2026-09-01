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
    // Covers mesh geometry (POSITION/NORMAL/COLOR_0 + indices, TRIANGLES mode) with scene-graph
    // flattening (every mesh-carrying node's translation/rotation/scale is baked into that
    // mesh's own vertices on import, so DDD - which has no live scene-graph concept - gets back
    // a flat list of already-placed Mesh objects), scalar-only PBR materials, KHR_lights_punctual
    // lights (no per-light color - see the note on Light), and cameras (DDD's own Camera type,
    // from PLAN.md 1f). Animation baking is still follow-up work (PLAN.md 1h lists the full
    // scope and what's deliberately excluded, e.g. textures and skinning).
    //
    // Known scope limits: only each mesh's first primitive is read (a multi-primitive mesh isn't
    // split into separate DDD Meshes); only TRIANGLES mode is supported (others are rejected, not
    // converted); a glTF mesh not reachable from any node is skipped, matching what an actual
    // glTF viewer would show; export writes at most one Camera (the first found) alongside any
    // number of Meshes and Lights; import reads only the first camera-carrying node found, for
    // the same reason.
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
            public List<GltfMaterial>? Materials { get; set; }
            public List<GltfCamera>? Cameras { get; set; }
            public List<string>? ExtensionsUsed { get; set; }
            public GltfExtensions? Extensions { get; set; }
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
            public int? Camera { get; set; }
            public double[]? Translation { get; set; }
            public double[]? Rotation { get; set; } // quaternion [x, y, z, w]
            public double[]? Scale { get; set; }
            public GltfExtensions? Extensions { get; set; }
        }
        // Reused at both document root (Lights: the palette of lights in the file) and node
        // level (Light: which one this node carries) - glTF only ever populates one or the
        // other depending on where the extension object appears.
        sealed class GltfExtensions
        {
            [JsonPropertyName("KHR_lights_punctual")]
            public GltfLightsPunctualExtension? KhrLightsPunctual { get; set; }
        }
        sealed class GltfLightsPunctualExtension
        {
            public List<GltfLight>? Lights { get; set; }
            public int? Light { get; set; }
        }
        sealed class GltfLight
        {
            public string Type { get; set; } = "directional"; // "directional" | "point" | "spot"
            public double Intensity { get; set; } = 1.0;
            public GltfSpot? Spot { get; set; }
        }
        sealed class GltfSpot
        {
            public double InnerConeAngle { get; set; }
            public double OuterConeAngle { get; set; } = Math.PI / 4;
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
        sealed class GltfMaterial
        {
            public GltfPbrMetallicRoughness? PbrMetallicRoughness { get; set; }
            public double[]? EmissiveFactor { get; set; }
        }
        sealed class GltfPbrMetallicRoughness
        {
            public double[]? BaseColorFactor { get; set; }
            public double? MetallicFactor { get; set; }
            public double? RoughnessFactor { get; set; }
        }
        sealed class GltfCamera
        {
            public string Type { get; set; } = "perspective"; // "perspective" | "orthographic"
            public GltfPerspectiveCamera? Perspective { get; set; }
            public GltfOrthographicCamera? Orthographic { get; set; }
        }
        sealed class GltfPerspectiveCamera
        {
            public double Yfov { get; set; }
            public double Znear { get; set; }
            public double? Zfar { get; set; }
        }
        sealed class GltfOrthographicCamera
        {
            public double Xmag { get; set; }
            public double Ymag { get; set; }
            public double Znear { get; set; }
            public double Zfar { get; set; }
        }

        // --- Public API ---

        // Reads every object PLAN.md 1h currently supports out of the file: every mesh-carrying
        // node's mesh (transform baked in) plus any KHR_lights_punctual lights plus the first
        // camera-carrying node's camera (if present). At least one must be present, or this throws.
        public static List<object> Read(string path) => Parse(System.IO.File.ReadAllBytes(path));

        public static void Write(Mesh mesh, string path) => Write(new List<object> { mesh }, path);
        public static void Write(List<object> objects, string path) => System.IO.File.WriteAllBytes(path, Serialize(objects));

        public static List<object> Parse(byte[] glb)
        {
            (GltfDocument document, byte[] binaryChunk) = ReadGlbContainer(glb);
            var results = new List<object>();

            if (document.Meshes != null && document.Nodes != null)
            {
                foreach (GltfNode node in document.Nodes)
                {
                    if (node.Mesh is int meshIndex && meshIndex < document.Meshes.Count)
                    {
                        results.Add(ParseMesh(document, binaryChunk, document.Meshes[meshIndex], node));
                    }
                }
            }

            List<GltfLight>? gltfLights = document.Extensions?.KhrLightsPunctual?.Lights;
            if (document.Nodes != null && gltfLights != null)
            {
                foreach (GltfNode node in document.Nodes)
                {
                    if (node.Extensions?.KhrLightsPunctual?.Light is int lightIndex && lightIndex < gltfLights.Count)
                    {
                        results.Add(LightFromGltf(gltfLights[lightIndex], node));
                    }
                }
            }

            if (document.Nodes != null && document.Cameras != null)
            {
                foreach (GltfNode node in document.Nodes)
                {
                    if (node.Camera is int cameraIndex && cameraIndex < document.Cameras.Count)
                    {
                        results.Add(CameraFromGltf(document.Cameras[cameraIndex], node));
                        break; // at most one Camera is read - see the scope note at the top of this file
                    }
                }
            }

            if (results.Count == 0)
            {
                throw new FormatException("glTF document has no meshes, lights, or cameras to import.");
            }
            return results;
        }

        static Mesh ParseMesh(GltfDocument document, byte[] binaryChunk, GltfMesh gltfMesh, GltfNode node)
        {
            GltfPrimitive primitive = gltfMesh.Primitives.FirstOrDefault()
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
                Point localPosition = new Point(positions[i][0], positions[i][1], positions[i][2]);
                Point position = TransformPosition(node, localPosition);
                bool hasNormal = normals != null;
                bool hasColor = colors != null;
                Vector normal = hasNormal
                    ? TransformNormal(node, new Vector(normals![i][0], normals[i][1], normals[i][2]))
                    : default;
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

            if (primitive.Material is int materialIndex && document.Materials != null && materialIndex < document.Materials.Count)
            {
                mesh.Material = MaterialFromGltf(document.Materials[materialIndex]);
            }

            return mesh;
        }

        public static byte[] Serialize(Mesh mesh) => Serialize(new List<object> { mesh });

        // Writes at most one Mesh (the first found, if any - "flattened scene graph" for
        // multiple meshes is separate future work per PLAN.md 1h) plus any number of Lights.
        public static byte[] Serialize(List<object> objects)
        {
            List<Mesh> meshObjects = objects.OfType<Mesh>().ToList();
            List<Light> lightObjects = objects.OfType<Light>().ToList();
            Camera? camera = objects.OfType<Camera>().Cast<Camera?>().FirstOrDefault();

            using var binary = new System.IO.MemoryStream();
            var bufferViews = new List<GltfBufferView>();
            var accessors = new List<GltfAccessor>();
            var meshes = new List<GltfMesh>();
            var nodes = new List<GltfNode>();
            var sceneNodeIndices = new List<int>();
            List<GltfMaterial>? materials = null;

            foreach (Mesh mesh in meshObjects)
            {
                bool hasNormal = mesh.Vertices.Any(v => v.Normal.HasValue);
                bool hasColor = mesh.Vertices.Any(v => v.Color.HasValue);
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

                int? materialIndex = null;
                if (mesh.Material is Material material)
                {
                    materials ??= new List<GltfMaterial>();
                    materials.Add(MaterialToGltf(material));
                    materialIndex = materials.Count - 1;
                }

                meshes.Add(new GltfMesh
                {
                    Primitives = new List<GltfPrimitive>
                    {
                        new GltfPrimitive { Attributes = attributes, Indices = indicesAccessorIndex, Material = materialIndex, Mode = ModeTriangles },
                    },
                });
                // DDD meshes already carry their final world-space vertex positions - no
                // per-node transform to write, unlike the nodes ParseMesh reads back (which bake
                // whatever transform an external tool's node carries into the vertices instead).
                nodes.Add(new GltfNode { Mesh = meshes.Count - 1 });
                sceneNodeIndices.Add(nodes.Count - 1);
            }

            List<GltfLight>? gltfLights = null;
            if (lightObjects.Count > 0)
            {
                gltfLights = new List<GltfLight>();
                foreach (Light light in lightObjects)
                {
                    (GltfLight gltfLight, GltfNode node) = LightToGltf(light, gltfLights.Count);
                    gltfLights.Add(gltfLight);
                    nodes.Add(node);
                    sceneNodeIndices.Add(nodes.Count - 1);
                }
            }

            List<GltfCamera>? cameras = null;
            if (camera is Camera cam)
            {
                (GltfCamera gltfCamera, GltfNode node) = CameraToGltf(cam);
                cameras = new List<GltfCamera> { gltfCamera };
                node.Camera = 0;
                nodes.Add(node);
                sceneNodeIndices.Add(nodes.Count - 1);
            }

            var document = new GltfDocument
            {
                Scene = 0,
                Scenes = new List<GltfScene> { new GltfScene { Nodes = sceneNodeIndices.ToArray() } },
                Nodes = nodes.Count > 0 ? nodes : null,
                Meshes = meshes.Count > 0 ? meshes : null,
                Accessors = accessors.Count > 0 ? accessors : null,
                BufferViews = bufferViews.Count > 0 ? bufferViews : null,
                Buffers = binary.Length > 0 ? new List<GltfBuffer> { new GltfBuffer { ByteLength = (int)binary.Length } } : null,
                Materials = materials,
                Cameras = cameras,
                ExtensionsUsed = gltfLights != null ? new List<string> { "KHR_lights_punctual" } : null,
                Extensions = gltfLights != null ? new GltfExtensions { KhrLightsPunctual = new GltfLightsPunctualExtension { Lights = gltfLights } } : null,
            };

            return WriteGlbContainer(document, binary.ToArray());
        }

        // --- Material conversion ---

        // metallic/roughness -> ambient/diffuse/specular/shininess is a per-face flat-shading
        // approximation, not a real Cook-Torrance BRDF (see PLAN.md 1h). Metals have near-zero
        // diffuse response and a strong, tight specular highlight; dielectrics (metallic=0) are
        // diffuse-dominant with only a subtle specular highlight. Roughness controls how tight
        // (smooth) or broad (rough) that highlight is, via Shininess.
        const double MaxShininess = 128.0;

        static Material MaterialFromGltf(GltfMaterial gltfMaterial)
        {
            GltfPbrMetallicRoughness? pbr = gltfMaterial.PbrMetallicRoughness;
            double[] baseColor = pbr?.BaseColorFactor ?? new[] { 1.0, 1.0, 1.0, 1.0 };
            double metallic = pbr?.MetallicFactor ?? 1.0;
            double roughness = pbr?.RoughnessFactor ?? 1.0;
            double[] emissiveFactor = gltfMaterial.EmissiveFactor ?? new[] { 0.0, 0.0, 0.0 };

            Color color = new Color(ToByte(baseColor[0]), ToByte(baseColor[1]), ToByte(baseColor[2]));
            Color emissive = new Color(ToByte(emissiveFactor[0]), ToByte(emissiveFactor[1]), ToByte(emissiveFactor[2]));

            double diffuse = 0.8 * (1.0 - metallic);
            double specular = 0.2 + metallic * 0.6;
            double shininess = 1.0 + (1.0 - roughness) * (MaxShininess - 1.0);

            return new Material(color, ambient: 0.2, diffuse, specular, shininess, emissive);
        }

        static GltfMaterial MaterialToGltf(Material material)
        {
            // Best-effort reverse of MaterialFromGltf - lossy for a Material that wasn't
            // originally derived from metallic/roughness (e.g. hand-authored via New-Material
            // with arbitrary Ambient/Diffuse/Specular), and Ambient has no glTF equivalent at
            // all (real-time PBR ambient normally comes from image-based lighting, which DDD
            // doesn't do), so it's simply dropped on export.
            double metallic = Math.Clamp((material.Specular - 0.2) / 0.6, 0.0, 1.0);
            double roughness = Math.Clamp(1.0 - (material.Shininess - 1.0) / (MaxShininess - 1.0), 0.0, 1.0);
            bool hasEmissive = material.Emissive != default;

            return new GltfMaterial
            {
                PbrMetallicRoughness = new GltfPbrMetallicRoughness
                {
                    BaseColorFactor = new[] { material.Color.R / 255.0, material.Color.G / 255.0, material.Color.B / 255.0, 1.0 },
                    MetallicFactor = metallic,
                    RoughnessFactor = roughness,
                },
                EmissiveFactor = hasEmissive
                    ? new[] { material.Emissive.R / 255.0, material.Emissive.G / 255.0, material.Emissive.B / 255.0 }
                    : null,
            };
        }

        // --- Light conversion ---

        // A glTF light has no position/direction of its own - it inherits both from whatever
        // node carries it, via that node's translation and rotation. Directional/Spot lights
        // point along their node's local -Z axis by convention, so placing or reading one
        // requires encoding/decoding a rotation quaternion; RotateByQuaternion/
        // QuaternionFromDirection below are exactly that, with no other use in this file.
        // No per-light color: KHR_lights_punctual's "color" field is read but discarded, and
        // export always writes white - see the "No per-light color" note on Light.

        static Light LightFromGltf(GltfLight gltfLight, GltfNode node)
        {
            double[] translation = node.Translation ?? new[] { 0.0, 0.0, 0.0 };
            Point position = new Point(translation[0], translation[1], translation[2]);

            double[] rotation = node.Rotation ?? new[] { 0.0, 0.0, 0.0, 1.0 };
            Vector direction = RotateByQuaternion(rotation, new Vector(0, 0, -1));

            const double RadiansToDegrees = 180.0 / Math.PI;
            return gltfLight.Type switch
            {
                "point" => new Light(position, gltfLight.Intensity),
                "spot" => new Light(position, direction,
                    outerConeAngleDegrees: (gltfLight.Spot?.OuterConeAngle ?? Math.PI / 4) * RadiansToDegrees,
                    innerConeAngleDegrees: (gltfLight.Spot?.InnerConeAngle ?? 0.0) * RadiansToDegrees,
                    intensity: gltfLight.Intensity),
                _ => new Light(direction, gltfLight.Intensity), // "directional"
            };
        }

        static (GltfLight Light, GltfNode Node) LightToGltf(Light light, int lightIndex)
        {
            const double DegreesToRadians = Math.PI / 180.0;
            GltfLight gltfLight = light.Kind switch
            {
                LightKind.Point => new GltfLight { Type = "point", Intensity = light.Intensity },
                LightKind.Spot => new GltfLight
                {
                    Type = "spot",
                    Intensity = light.Intensity,
                    Spot = new GltfSpot
                    {
                        InnerConeAngle = light.InnerConeAngleDegrees * DegreesToRadians,
                        OuterConeAngle = light.OuterConeAngleDegrees * DegreesToRadians,
                    },
                },
                _ => new GltfLight { Type = "directional", Intensity = light.Intensity },
            };

            var node = new GltfNode
            {
                Extensions = new GltfExtensions { KhrLightsPunctual = new GltfLightsPunctualExtension { Light = lightIndex } },
            };
            if (light.Kind != LightKind.Directional)
            {
                node.Translation = new[] { light.Position.X, light.Position.Y, light.Position.Z };
            }
            if (light.Kind != LightKind.Point)
            {
                node.Rotation = QuaternionFromDirection(light.Direction);
            }

            return (gltfLight, node);
        }

        // Shortest-arc rotation quaternion [x, y, z, w] that takes local -Z to `direction`.
        static double[] QuaternionFromDirection(Vector direction)
        {
            Vector from = new Vector(0, 0, -1);
            Vector to = Vector.Normalize(direction);
            double dot = Vector.Dot(from, to);

            if (dot > 0.999999)
            {
                return new[] { 0.0, 0.0, 0.0, 1.0 }; // already aligned
            }
            if (dot < -0.999999)
            {
                // 180-degree rotation: any axis perpendicular to `from` works.
                Vector axis = Vector.Cross(new Vector(1, 0, 0), from);
                if (axis.Length() < 1e-6)
                {
                    axis = Vector.Cross(new Vector(0, 1, 0), from);
                }
                axis = Vector.Normalize(axis);
                return new[] { axis.X, axis.Y, axis.Z, 0.0 };
            }

            Vector axisV = Vector.Cross(from, to);
            double w = 1.0 + dot;
            double len = Math.Sqrt(axisV.X * axisV.X + axisV.Y * axisV.Y + axisV.Z * axisV.Z + w * w);
            return new[] { axisV.X / len, axisV.Y / len, axisV.Z / len, w / len };
        }

        static Vector RotateByQuaternion(double[] q, Vector v)
        {
            double qx = q[0], qy = q[1], qz = q[2], qw = q[3];
            double x = v.X, y = v.Y, z = v.Z;
            double ix = qw * x + qy * z - qz * y;
            double iy = qw * y + qz * x - qx * z;
            double iz = qw * z + qx * y - qy * x;
            double iw = -qx * x - qy * y - qz * z;
            double rx = ix * qw + iw * -qx + iy * -qz - iz * -qy;
            double ry = iy * qw + iw * -qy + iz * -qx - ix * -qz;
            double rz = iz * qw + iw * -qz + ix * -qy - iy * -qx;
            return new Vector(rx, ry, rz);
        }

        // --- Node transform baking (scene-graph flattening) ---

        // DDD has no live scene-graph concept - a Mesh's vertices are always already in their
        // final world position. Importing a glTF mesh means baking whatever TRS transform its
        // node carries into the vertices once, here, rather than keeping node/mesh as separate
        // concepts the way glTF (and DDD's own Serialize, which always writes identity nodes for
        // meshes - see the comment where meshes are written) doesn't need to.
        static readonly double[] IdentityQuaternion = { 0.0, 0.0, 0.0, 1.0 };
        static readonly double[] ZeroTranslation = { 0.0, 0.0, 0.0 };
        static readonly double[] UnitScale = { 1.0, 1.0, 1.0 };

        static Point TransformPosition(GltfNode node, Point local)
        {
            double[] scale = NodeScale(node);
            Vector scaled = new Vector(local.X * scale[0], local.Y * scale[1], local.Z * scale[2]);
            double[] rotation = node.Rotation ?? IdentityQuaternion;
            Vector rotated = RotateByQuaternion(rotation, scaled);
            double[] translation = node.Translation ?? ZeroTranslation;
            return new Point(rotated.X + translation[0], rotated.Y + translation[1], rotated.Z + translation[2]);
        }

        // A normal needs the inverse-transpose of the position transform, not the position
        // transform itself, or a non-uniform scale skews it off the surface it's meant to be
        // perpendicular to. For a diagonal scale matrix (the only kind a glTF TRS node can
        // express), the inverse-transpose is exactly the reciprocal scale - not an approximation.
        static Vector TransformNormal(GltfNode node, Vector local)
        {
            double[] scale = NodeScale(node);
            Vector rescaled = new Vector(
                scale[0] != 0 ? local.X / scale[0] : local.X,
                scale[1] != 0 ? local.Y / scale[1] : local.Y,
                scale[2] != 0 ? local.Z / scale[2] : local.Z);
            double[] rotation = node.Rotation ?? IdentityQuaternion;
            return Vector.Normalize(RotateByQuaternion(rotation, rescaled));
        }

        static double[] NodeScale(GltfNode node) => node.Scale ?? UnitScale;

        // --- Camera conversion ---

        // A glTF camera, like a light, has no position/orientation of its own - both come from
        // whatever node carries it, looking down its local -Z axis with local +Y as up (same
        // convention as lights). Unlike a light, a camera's "up" is meaningful (roll changes the
        // image), so placing/reading one needs a full look-rotation quaternion built from a
        // forward+up basis (QuaternionFromBasis/QuaternionFromMatrix below), not just the
        // shortest-arc rotation QuaternionFromDirection used for lights.
        //
        // glTF has no "look at this point" concept, only orientation - Camera.LookAt is
        // synthesized on import by walking a fixed, arbitrary distance along the decoded forward
        // direction. Lossy for anything not originally exported by DDD itself, same category as
        // Ambient/Light color being dropped elsewhere in this file.
        const double DefaultLookAtDistance = 1.0;

        static Camera CameraFromGltf(GltfCamera gltfCamera, GltfNode node)
        {
            double[] translation = node.Translation ?? new[] { 0.0, 0.0, 0.0 };
            Point position = new Point(translation[0], translation[1], translation[2]);

            double[] rotation = node.Rotation ?? new[] { 0.0, 0.0, 0.0, 1.0 };
            Vector forward = RotateByQuaternion(rotation, new Vector(0, 0, -1));
            Vector up = RotateByQuaternion(rotation, new Vector(0, 1, 0));
            Point lookAt = position + forward * DefaultLookAtDistance;

            bool perspective = gltfCamera.Type != "orthographic";
            const double RadiansToDegrees = 180.0 / Math.PI;
            const double DefaultYfovRadians = 40.0 * Math.PI / 180.0;

            double fovYDegrees = (gltfCamera.Perspective?.Yfov ?? DefaultYfovRadians) * RadiansToDegrees;
            double orthographicHeight = gltfCamera.Orthographic?.Ymag ?? 1.0;
            double nearPlane = perspective ? (gltfCamera.Perspective?.Znear ?? 0.01) : (gltfCamera.Orthographic?.Znear ?? 0.01);
            double farPlane = perspective ? (gltfCamera.Perspective?.Zfar ?? 1000.0) : (gltfCamera.Orthographic?.Zfar ?? 1000.0);

            return new Camera(position, lookAt, perspective, fovYDegrees, orthographicHeight, up, nearPlane, farPlane);
        }

        static (GltfCamera Camera, GltfNode Node) CameraToGltf(Camera camera)
        {
            const double DegreesToRadians = Math.PI / 180.0;
            GltfCamera gltfCamera = camera.Perspective
                ? new GltfCamera
                {
                    Type = "perspective",
                    Perspective = new GltfPerspectiveCamera
                    {
                        Yfov = camera.FovYDegrees * DegreesToRadians,
                        Znear = camera.NearPlane,
                        Zfar = camera.FarPlane,
                    },
                }
                : new GltfCamera
                {
                    // DDD tracks only a single OrthographicHeight (no separate width/aspect), so
                    // xmag and ymag are written equal - a square view volume regardless of the
                    // aspect ratio it's actually rendered at.
                    Type = "orthographic",
                    Orthographic = new GltfOrthographicCamera
                    {
                        Xmag = camera.OrthographicHeight,
                        Ymag = camera.OrthographicHeight,
                        Znear = camera.NearPlane,
                        Zfar = camera.FarPlane,
                    },
                };

            Vector toLookAt = camera.LookAt - camera.Position;
            Vector forward = toLookAt.Length() > 1e-9 ? Vector.Normalize(toLookAt) : new Vector(0, 0, -1);
            Vector up = camera.Up.Length() > 1e-9 ? camera.Up : new Vector(0, 1, 0);

            var node = new GltfNode
            {
                Translation = new[] { camera.Position.X, camera.Position.Y, camera.Position.Z },
                Rotation = QuaternionFromBasis(forward, up),
            };
            return (gltfCamera, node);
        }

        // Look-rotation quaternion [x, y, z, w] that takes local -Z to `forward` and local +Y as
        // close to `up` as an orthogonal basis allows (Gram-Schmidt: up is projected perpendicular
        // to forward, not used as-is). Falls back to an arbitrary reference axis when forward is
        // (near-)parallel to up - e.g. a camera looking straight up or down - where roll is
        // undefined anyway.
        static double[] QuaternionFromBasis(Vector forward, Vector up)
        {
            Vector f = Vector.Normalize(forward);
            Vector right = Vector.Cross(f, up);
            if (right.Length() < 1e-6)
            {
                Vector fallbackUp = Math.Abs(f.Y) > 0.999 ? new Vector(0, 0, 1) : new Vector(0, 1, 0);
                right = Vector.Cross(f, fallbackUp);
            }
            right = Vector.Normalize(right);
            Vector trueUp = Vector.Cross(right, f);

            // Rotation matrix columns are where each local axis maps to in world space:
            // local +X -> right, local +Y -> trueUp, local -Z -> f (so local +Z -> -f).
            return QuaternionFromMatrix(
                right.X, trueUp.X, -f.X,
                right.Y, trueUp.Y, -f.Y,
                right.Z, trueUp.Z, -f.Z);
        }

        // Standard trace-based rotation-matrix-to-quaternion conversion (Shepperd's method),
        // taking a row-major 3x3 rotation matrix and returning [x, y, z, w].
        static double[] QuaternionFromMatrix(
            double m00, double m01, double m02,
            double m10, double m11, double m12,
            double m20, double m21, double m22)
        {
            double trace = m00 + m11 + m22;
            if (trace > 0)
            {
                double s = 0.5 / Math.Sqrt(trace + 1.0);
                return new[] { (m21 - m12) * s, (m02 - m20) * s, (m10 - m01) * s, 0.25 / s };
            }
            if (m00 > m11 && m00 > m22)
            {
                double s = 2.0 * Math.Sqrt(1.0 + m00 - m11 - m22);
                return new[] { 0.25 * s, (m01 + m10) / s, (m02 + m20) / s, (m21 - m12) / s };
            }
            if (m11 > m22)
            {
                double s = 2.0 * Math.Sqrt(1.0 + m11 - m00 - m22);
                return new[] { (m01 + m10) / s, 0.25 * s, (m12 + m21) / s, (m02 - m20) / s };
            }
            else
            {
                double s = 2.0 * Math.Sqrt(1.0 + m22 - m00 - m11);
                return new[] { (m02 + m20) / s, (m12 + m21) / s, 0.25 * s, (m10 - m01) / s };
            }
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
