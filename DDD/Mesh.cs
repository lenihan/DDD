using System;
using System.Collections.Generic;

namespace DDD
{
    [Serializable()]
    public sealed class Mesh
    {
        public List<Vertex> Vertices { get; } = new List<Vertex>();
        public List<Face> Faces { get; } = new List<Face>();

        public int AddVertex(Vertex vertex)
        {
            Vertices.Add(vertex);
            return Vertices.Count - 1;
        }
        public int AddVertex(Point position) => AddVertex(new Vertex(position));

        public void AddFace(Face face) => Faces.Add(face);
        public void AddFace(int a, int b, int c) => AddFace(new Face(a, b, c));

        public override string ToString()
        {
            return String.Format(System.Globalization.CultureInfo.InvariantCulture,
                "Mesh: {0} vertices, {1} faces\n", Vertices.Count, Faces.Count);
        }
    }
}
