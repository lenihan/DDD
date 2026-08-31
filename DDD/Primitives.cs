using System;

namespace DDD
{
    // Parametric mesh primitives (PLAN.md 1d). Two triangle-winding strategies are used:
    //  - Box/Plane: flat faces get their own non-shared vertices via AddQuad, whose corner
    //    parametrization is proven correct by construction (see AddQuad) - no per-shape
    //    winding derivation needed.
    //  - Sphere/Cylinder/Cone/Torus: vertices are shared for smooth normals, so faces are added
    //    via AddFaceFacingOutward, which picks the correct index order automatically from an
    //    approximate outward-direction hint rather than a hand-derived winding rule per shape.
    public static class Primitives
    {
        public static Mesh Box(double width, double height, double depth, Point center = default)
        {
            var mesh = new Mesh();
            double hw = width / 2.0, hh = height / 2.0, hd = depth / 2.0;
            var x = new Vector(1, 0, 0);
            var y = new Vector(0, 1, 0);
            var z = new Vector(0, 0, 1);

            AddQuad(mesh, center + x * hw, y, hh, z, hd, x);           // +X
            AddQuad(mesh, center + x * -hw, z, hd, y, hh, -x);         // -X
            AddQuad(mesh, center + y * hh, z, hd, x, hw, y);           // +Y
            AddQuad(mesh, center + y * -hh, x, hw, z, hd, -y);         // -Y
            AddQuad(mesh, center + z * hd, x, hw, y, hh, z);           // +Z
            AddQuad(mesh, center + z * -hd, y, hh, x, hw, -z);         // -Z

            return mesh;
        }

        public static Mesh Plane(double width, double depth, Point center = default)
        {
            // Cross(t1, t2) must equal normal (see AddQuad) - Cross(Z, X) == Y, not Cross(X, Z).
            var mesh = new Mesh();
            AddQuad(mesh, center, new Vector(0, 0, 1), depth / 2.0, new Vector(1, 0, 0), width / 2.0, new Vector(0, 1, 0));
            return mesh;
        }

        public static Mesh Sphere(double radius, int segments = 16, Point center = default)
        {
            int lonSteps = Math.Max(3, segments);
            int latSteps = Math.Max(2, lonSteps / 2);

            var mesh = new Mesh();
            int north = mesh.AddVertex(new Vertex(center + new Vector(0, radius, 0), new Vector(0, 1, 0)));
            int south = mesh.AddVertex(new Vertex(center + new Vector(0, -radius, 0), new Vector(0, -1, 0)));

            var rings = new int[latSteps - 1][];
            for (int row = 1; row < latSteps; row++)
            {
                double phi = row * Math.PI / latSteps;
                double y = radius * Math.Cos(phi);
                double ringRadius = radius * Math.Sin(phi);

                rings[row - 1] = new int[lonSteps];
                for (int col = 0; col < lonSteps; col++)
                {
                    double theta = col * 2.0 * Math.PI / lonSteps;
                    var offset = new Vector(ringRadius * Math.Cos(theta), y, ringRadius * Math.Sin(theta));
                    Vector normal = Vector.Normalize(offset);
                    rings[row - 1][col] = mesh.AddVertex(new Vertex(center + offset, normal));
                }
            }

            for (int col = 0; col < lonSteps; col++)
            {
                int next = (col + 1) % lonSteps;
                AddFaceFacingOutward(mesh, north, rings[0][col], rings[0][next]);
                AddFaceFacingOutward(mesh, south, rings[latSteps - 2][next], rings[latSteps - 2][col]);
            }

            for (int row = 0; row < latSteps - 2; row++)
            {
                for (int col = 0; col < lonSteps; col++)
                {
                    int next = (col + 1) % lonSteps;
                    AddFaceFacingOutward(mesh, rings[row][col], rings[row + 1][next], rings[row + 1][col]);
                    AddFaceFacingOutward(mesh, rings[row][col], rings[row][next], rings[row + 1][next]);
                }
            }

            return mesh;
        }

        public static Mesh Cylinder(double radius, double height, int segments = 16, Point center = default) =>
            Frustum(radius, radius, height, segments, center);

        public static Mesh Cone(double baseRadius, double topRadius, double height, int segments = 16, Point center = default) =>
            Frustum(baseRadius, topRadius, height, segments, center);

        public static Mesh Torus(double majorRadius, double minorRadius, int segments = 16, Point center = default)
        {
            int steps = Math.Max(3, segments);
            var mesh = new Mesh();
            var ring = new int[steps][];

            for (int i = 0; i < steps; i++)
            {
                double u = i * 2.0 * Math.PI / steps;
                ring[i] = new int[steps];
                for (int j = 0; j < steps; j++)
                {
                    double v = j * 2.0 * Math.PI / steps;
                    double tubeRadius = majorRadius + minorRadius * Math.Cos(v);
                    var offset = new Vector(tubeRadius * Math.Cos(u), minorRadius * Math.Sin(v), tubeRadius * Math.Sin(u));
                    Vector normal = new Vector(Math.Cos(v) * Math.Cos(u), Math.Sin(v), Math.Cos(v) * Math.Sin(u));
                    ring[i][j] = mesh.AddVertex(new Vertex(center + offset, normal));
                }
            }

            for (int i = 0; i < steps; i++)
            {
                int ni = (i + 1) % steps;
                for (int j = 0; j < steps; j++)
                {
                    int nj = (j + 1) % steps;
                    AddFaceFacingOutward(mesh, ring[i][j], ring[ni][j], ring[ni][nj]);
                    AddFaceFacingOutward(mesh, ring[i][j], ring[ni][nj], ring[i][nj]);
                }
            }

            return mesh;
        }

        // The classic Cornell Box lighting test scene: a room (red left wall, green right wall,
        // white back/floor/ceiling) with no front wall so the interior is visible, plus two
        // blocks. Room: X in [-1,1], Y in [0,2], Z in [-1,1]. Wall normals point inward, toward
        // the room's center - the opposite convention from Box, which is why this doesn't just
        // call Box and drop a face.
        public static Mesh CornellBox()
        {
            var mesh = new Mesh();
            var white = new Color(200, 200, 200);
            var red = new Color(200, 50, 50);
            var green = new Color(50, 200, 50);
            var x = new Vector(1, 0, 0);
            var y = new Vector(0, 1, 0);
            var z = new Vector(0, 0, 1);

            AddQuad(mesh, new Point(0, 1, -1), x, 1, y, 1, z, white);      // back:    Cross(X,Y)=Z
            AddQuad(mesh, new Point(-1, 1, 0), y, 1, z, 1, x, red);        // left:    Cross(Y,Z)=X
            AddQuad(mesh, new Point(1, 1, 0), z, 1, y, 1, -x, green);      // right:   Cross(Z,Y)=-X
            AddQuad(mesh, new Point(0, 0, 0), z, 1, x, 1, y, white);       // floor:   Cross(Z,X)=Y
            AddQuad(mesh, new Point(0, 2, 0), x, 1, z, 1, -y, white);      // ceiling: Cross(X,Z)=-Y

            // Two blocks, a classic Cornell Box detail, each rotated slightly for visual
            // interest rather than axis-aligned.
            AppendMesh(mesh, Box(0.6, 1.2, 0.6), Matrix.RotateY(15), new Vector(-0.35, 0.6, -0.3));
            AppendMesh(mesh, Box(0.6, 0.6, 0.6), Matrix.RotateY(-18), new Vector(0.35, 0.3, 0.3));

            return mesh;
        }

        static Mesh Frustum(double baseRadius, double topRadius, double height, int segments, Point center)
        {
            int steps = Math.Max(3, segments);
            var mesh = new Mesh();
            double halfHeight = height / 2.0;

            // Outward side normal: perpendicular to the generator line (from the base rim to the
            // top rim) in the (radius, height) half-plane, then swept around by angle.
            double dr = topRadius - baseRadius;
            var sideNormal2D = Vector.Normalize(new Vector(height, -dr, 0)); // (radial, vertical)

            var bottomSide = new int[steps];
            var topSide = new int[steps];
            var bottomCap = new int[steps];
            var topCap = new int[steps];

            for (int i = 0; i < steps; i++)
            {
                double theta = i * 2.0 * Math.PI / steps;
                double cos = Math.Cos(theta), sin = Math.Sin(theta);

                var bottomPos = center + new Vector(baseRadius * cos, -halfHeight, baseRadius * sin);
                var topPos = center + new Vector(topRadius * cos, halfHeight, topRadius * sin);
                var sideNormal = new Vector(sideNormal2D.X * cos, sideNormal2D.Y, sideNormal2D.X * sin);

                bottomSide[i] = mesh.AddVertex(new Vertex(bottomPos, sideNormal));
                topSide[i] = mesh.AddVertex(new Vertex(topPos, sideNormal));
                bottomCap[i] = mesh.AddVertex(new Vertex(bottomPos, new Vector(0, -1, 0)));
                topCap[i] = mesh.AddVertex(new Vertex(topPos, new Vector(0, 1, 0)));
            }
            int bottomCenter = mesh.AddVertex(new Vertex(center + new Vector(0, -halfHeight, 0), new Vector(0, -1, 0)));
            int topCenter = mesh.AddVertex(new Vertex(center + new Vector(0, halfHeight, 0), new Vector(0, 1, 0)));

            for (int i = 0; i < steps; i++)
            {
                int next = (i + 1) % steps;
                AddFaceFacingOutward(mesh, bottomSide[i], topSide[next], topSide[i]);
                AddFaceFacingOutward(mesh, bottomSide[i], bottomSide[next], topSide[next]);
                AddFaceFacingOutward(mesh, bottomCenter, bottomCap[next], bottomCap[i]);
                AddFaceFacingOutward(mesh, topCenter, topCap[i], topCap[next]);
            }

            return mesh;
        }

        // Adds a flat quad face as 2 triangles with 4 new (non-shared) vertices, all sharing
        // `normal` (and `color`, if given). faceCenter +/- t1*h1 +/- t2*h2 traces the 4 corners;
        // as long as Cross(t1, t2) == normal, this ordering is always correctly wound (outward)
        // by construction - verified algebraically, not per-call.
        static void AddQuad(Mesh mesh, Point faceCenter, Vector t1, double h1, Vector t2, double h2, Vector normal, Color? color = null)
        {
            Point p0 = faceCenter + t1 * -h1 + t2 * -h2;
            Point p1 = faceCenter + t1 * h1 + t2 * -h2;
            Point p2 = faceCenter + t1 * h1 + t2 * h2;
            Point p3 = faceCenter + t1 * -h1 + t2 * h2;

            Vertex MakeVertex(Point p) => color is Color c ? new Vertex(p, normal, c) : new Vertex(p, normal);

            int i0 = mesh.AddVertex(MakeVertex(p0));
            int i1 = mesh.AddVertex(MakeVertex(p1));
            int i2 = mesh.AddVertex(MakeVertex(p2));
            int i3 = mesh.AddVertex(MakeVertex(p3));

            mesh.AddFace(i0, i1, i2);
            mesh.AddFace(i0, i2, i3);
        }

        // Appends a copy of source's vertices/faces into target, rotating positions and normals
        // and then translating positions - rotation and translation are handled separately
        // (rather than as one combined Matrix) because Matrix's Vector multiplication applies
        // translation too, which would corrupt a rotated normal.
        static void AppendMesh(Mesh target, Mesh source, Matrix rotation, Vector translation)
        {
            int indexOffset = target.Vertices.Count;
            foreach (Vertex vertex in source.Vertices)
            {
                Point position = rotation * vertex.Position + translation;
                bool hasNormal = vertex.Normal.HasValue;
                bool hasColor = vertex.Color.HasValue;
                Vector normal = hasNormal ? rotation * vertex.Normal.GetValueOrDefault() : default;
                Color color = vertex.Color.GetValueOrDefault();

                Vertex copy = (hasNormal, hasColor) switch
                {
                    (true, true) => new Vertex(position, normal, color),
                    (true, false) => new Vertex(position, normal),
                    (false, true) => new Vertex(position, color),
                    _ => new Vertex(position),
                };
                target.AddVertex(copy);
            }
            foreach (Face face in source.Faces)
            {
                target.AddFace(face.A + indexOffset, face.B + indexOffset, face.C + indexOffset);
            }
        }

        // Adds a face from 3 already-created vertices, picking whichever of (b,c)/(c,b) makes
        // the triangle's own normal point roughly the same way as vertex a's stored normal -
        // removes the need to hand-derive winding for every shape's triangulation.
        static void AddFaceFacingOutward(Mesh mesh, int a, int b, int c)
        {
            Point pa = mesh.Vertices[a].Position;
            Point pb = mesh.Vertices[b].Position;
            Point pc = mesh.Vertices[c].Position;
            Vector faceNormal = Vector.Cross(pb - pa, pc - pa);
            Vector reference = mesh.Vertices[a].Normal ?? faceNormal;

            if (Vector.Dot(faceNormal, reference) < 0)
            {
                (b, c) = (c, b);
            }
            mesh.AddFace(a, b, c);
        }
    }
}
