using System;
using System.Collections.Generic;

namespace DDD
{
    // 2D charts (PLAN.md 1i), built on the existing point/mesh model rather than a new rendering
    // concept. Data always lands on the Z=0 plane; X defaults to the data's index (0, 1, 2, ...)
    // when not given explicitly - the common case of graphing a single data series.
    //
    // No axis/tick/gridline geometry is added here - Out-3d already draws X/Y axis lines through
    // the scene's bounding-box center for every scene, which doubles as a chart's axes for free.
    // Numeric tick labels are still future work: they need real screen-space text tied to
    // projected data coordinates, which nothing in Rasterizer supports yet (BitmapFont today only
    // draws Out-3d's own fixed-position FPS/instructions overlays, not arbitrary 3D-anchored text).
    //
    // New-LineGraph isn't built yet - it needs either a new line-segment primitive or ribbon-mesh
    // geometry, more design work than a first pass needed.
    public static class Graphing
    {
        public static List<Point> ScatterPlot(double[] y, double[]? x = null)
        {
            ValidateLengths(y, x);
            var points = new List<Point>(y.Length);
            for (int i = 0; i < y.Length; i++)
            {
                points.Add(new Point(x != null ? x[i] : i, y[i], 0));
            }
            return points;
        }

        public static List<Mesh> BarChart(double[] y, double[]? x = null, double barWidth = 0.6)
        {
            ValidateLengths(y, x);
            var bars = new List<Mesh>(y.Length);
            for (int i = 0; i < y.Length; i++)
            {
                double value = y[i];
                double barX = x != null ? x[i] : i;
                // Box() centers on its own midpoint - a bar spans [0, value] (or [value, 0] for a
                // negative value), so its center sits at value/2, not at value itself.
                Point center = new Point(barX, value / 2.0, 0);
                bars.Add(Primitives.Box(barWidth, Math.Abs(value), barWidth, center));
            }
            return bars;
        }

        static void ValidateLengths(double[] y, double[]? x)
        {
            if (x != null && x.Length != y.Length)
            {
                throw new ArgumentException($"X and Y must have the same length (X has {x.Length}, Y has {y.Length}).");
            }
        }

        // heights[z][x] - a jagged array (not a rectangular double[,]) since that's what
        // PowerShell actually builds naturally (@(@(0,0),@(0,1))), unlike a true 2D array which
        // has no convenient PowerShell literal syntax at all. Every row must be the same length.
        // Grid spacing in world units; Y is the height/value axis, matching the convention
        // ScatterPlot/BarChart already use (X = position, Y = value).
        public static Mesh Surface(double[][] heights, double xSpacing = 1.0, double zSpacing = 1.0)
        {
            int rows = heights.Length;
            if (rows < 2)
            {
                throw new ArgumentException($"Surface needs at least 2 rows of height values (got {rows}).");
            }
            int cols = heights[0].Length;
            if (cols < 2)
            {
                throw new ArgumentException($"Surface needs at least 2 columns of height values (got {cols}).");
            }
            for (int z = 0; z < rows; z++)
            {
                if (heights[z].Length != cols)
                {
                    throw new ArgumentException($"Every row must have the same number of height values (row 0 has {cols}, row {z} has {heights[z].Length}).");
                }
            }

            var mesh = new Mesh();
            var indices = new int[rows, cols];

            for (int z = 0; z < rows; z++)
            {
                for (int x = 0; x < cols; x++)
                {
                    // A per-vertex normal from the local slope (central difference in the
                    // interior, one-sided at an edge - the clamped neighbor indices naturally
                    // produce both) - the same technique a heightmap normal map uses, not just
                    // an average of adjacent face normals.
                    int xPrev = Math.Max(x - 1, 0);
                    int xNext = Math.Min(x + 1, cols - 1);
                    int zPrev = Math.Max(z - 1, 0);
                    int zNext = Math.Min(z + 1, rows - 1);
                    double dhdx = (heights[z][xNext] - heights[z][xPrev]) / (Math.Max(xNext - xPrev, 1) * xSpacing);
                    double dhdz = (heights[zNext][x] - heights[zPrev][x]) / (Math.Max(zNext - zPrev, 1) * zSpacing);
                    Vector tangentU = new Vector(1, dhdx, 0);
                    Vector tangentV = new Vector(0, dhdz, 1);
                    Vector normal = Vector.Normalize(Vector.Cross(tangentV, tangentU));

                    Point position = new Point(x * xSpacing, heights[z][x], z * zSpacing);
                    indices[z, x] = mesh.AddVertex(new Vertex(position, normal));
                }
            }

            for (int z = 0; z < rows - 1; z++)
            {
                for (int x = 0; x < cols - 1; x++)
                {
                    int v00 = indices[z, x];
                    int v10 = indices[z, x + 1];
                    int v01 = indices[z + 1, x];
                    int v11 = indices[z + 1, x + 1];
                    // This winding gives a +Y-facing normal on a flat grid, matching the
                    // per-vertex normals above.
                    mesh.AddFace(v00, v01, v10);
                    mesh.AddFace(v10, v01, v11);
                }
            }

            return mesh;
        }
    }
}
