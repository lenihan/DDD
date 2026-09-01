using System;
using System.Collections.Generic;

namespace DDD
{
    [Serializable()]
    public sealed class Mesh
    {
        public List<Vertex> Vertices { get; } = new List<Vertex>();
        public List<Face> Faces { get; } = new List<Face>();
        public Material? Material { get; set; }

        public int AddVertex(Vertex vertex)
        {
            Vertices.Add(vertex);
            return Vertices.Count - 1;
        }
        public int AddVertex(Point position) => AddVertex(new Vertex(position));

        public void AddFace(Face face) => Faces.Add(face);
        public void AddFace(int a, int b, int c) => AddFace(new Face(a, b, c));

        // The mesh's own "origin" - the same bounding-box-center convention Out-3d/Rasterizer
        // already use as the pivot point for framing and rotating a scene.
        public Point BoundingBoxCenter()
        {
            if (Vertices.Count == 0) return new Point(0, 0, 0);

            double minX = double.MaxValue, minY = double.MaxValue, minZ = double.MaxValue;
            double maxX = double.MinValue, maxY = double.MinValue, maxZ = double.MinValue;
            foreach (Vertex vertex in Vertices)
            {
                Point p = vertex.Position;
                if (p.X < minX) minX = p.X;
                if (p.X > maxX) maxX = p.X;
                if (p.Y < minY) minY = p.Y;
                if (p.Y > maxY) maxY = p.Y;
                if (p.Z < minZ) minZ = p.Z;
                if (p.Z > maxZ) maxZ = p.Z;
            }
            return new Point((minX + maxX) / 2.0, (minY + maxY) / 2.0, (minZ + maxZ) / 2.0);
        }

        public override string ToString()
        {
            return String.Format(System.Globalization.CultureInfo.InvariantCulture,
                "Mesh: {0} vertices, {1} faces\n", Vertices.Count, Faces.Count);
        }
    }
}
