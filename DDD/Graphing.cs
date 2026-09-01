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
    // New-LineGraph and New-Surface aren't built yet either - a line graph needs either a new
    // line-segment primitive or ribbon-mesh geometry, and a height-field surface needs its own
    // grid-mesh construction (similar to how Primitives.Plane builds one), both more design work
    // than ScatterPlot/BarChart needed.
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
    }
}
