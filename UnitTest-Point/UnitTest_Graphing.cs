using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DDD_UnitTest
{
    [TestClass]
    public class Graphing
    {
        [TestMethod]
        public void ScatterPlotUsesTheDataIndexAsXWhenXIsOmitted()
        {
            double[] y = { 10.0, 20.0, 30.0 };
            List<DDD.Point> points = DDD.Graphing.ScatterPlot(y);

            Assert.AreEqual(3, points.Count);
            Assert.AreEqual(new DDD.Point(0, 10, 0), points[0]);
            Assert.AreEqual(new DDD.Point(1, 20, 0), points[1]);
            Assert.AreEqual(new DDD.Point(2, 30, 0), points[2]);
        }

        [TestMethod]
        public void ScatterPlotUsesExplicitXWhenGiven()
        {
            double[] y = { 5.0, 6.0 };
            double[] x = { 100.0, 200.0 };
            List<DDD.Point> points = DDD.Graphing.ScatterPlot(y, x);

            Assert.AreEqual(new DDD.Point(100, 5, 0), points[0]);
            Assert.AreEqual(new DDD.Point(200, 6, 0), points[1]);
        }

        [TestMethod]
        public void ScatterPlotThrowsWhenXAndYLengthsDiffer()
        {
            double[] y = { 1.0, 2.0 };
            double[] x = { 1.0 };
            Assert.ThrowsExactly<ArgumentException>(() => DDD.Graphing.ScatterPlot(y, x));
        }

        [TestMethod]
        public void BarChartBuildsABoxSpanningZeroToEachValue()
        {
            double[] y = { 4.0 };
            List<DDD.Mesh> bars = DDD.Graphing.BarChart(y, barWidth: 1.0);

            Assert.AreEqual(1, bars.Count);
            DDD.Mesh bar = bars[0];
            Assert.AreEqual(24, bar.Vertices.Count); // same shape Primitives.Box always produces

            // width=1, height=4, depth=1, centered at (0, 2, 0) - the box spans Y in [0, 4].
            (double MinY, double MaxY) = YExtent(bar);
            Assert.AreEqual(0.0, MinY, 1e-9);
            Assert.AreEqual(4.0, MaxY, 1e-9);
        }

        [TestMethod]
        public void BarChartHandlesANegativeValueByCenteringBelowZero()
        {
            double[] y = { -3.0 };
            List<DDD.Mesh> bars = DDD.Graphing.BarChart(y, barWidth: 1.0);

            (double MinY, double MaxY) = YExtent(bars[0]);
            Assert.AreEqual(-3.0, MinY, 1e-9);
            Assert.AreEqual(0.0, MaxY, 1e-9);
        }

        [TestMethod]
        public void BarChartPositionsBarsAtTheGivenXValues()
        {
            double[] y = { 2.0, 2.0 };
            double[] x = { 0.0, 10.0 };
            List<DDD.Mesh> bars = DDD.Graphing.BarChart(y, x, barWidth: 1.0);

            Assert.AreEqual(0.0, bars[0].BoundingBoxCenter().X, 1e-9);
            Assert.AreEqual(10.0, bars[1].BoundingBoxCenter().X, 1e-9);
        }

        [TestMethod]
        public void BarChartThrowsWhenXAndYLengthsDiffer()
        {
            double[] y = { 1.0, 2.0 };
            double[] x = { 1.0 };
            Assert.ThrowsExactly<ArgumentException>(() => DDD.Graphing.BarChart(y, x));
        }

        static (double MinY, double MaxY) YExtent(DDD.Mesh mesh)
        {
            double minY = double.MaxValue, maxY = double.MinValue;
            foreach (DDD.Vertex v in mesh.Vertices)
            {
                minY = Math.Min(minY, v.Position.Y);
                maxY = Math.Max(maxY, v.Position.Y);
            }
            return (minY, maxY);
        }

        [TestMethod]
        public void SurfacePositionsVerticesAtXHeightZ()
        {
            double[][] heights = { new[] { 0.0, 1.0 }, new[] { 2.0, 3.0 } }; // heights[z][x]
            DDD.Mesh surface = DDD.Graphing.Surface(heights, xSpacing: 2.0, zSpacing: 3.0);

            Assert.AreEqual(4, surface.Vertices.Count);
            // Insertion order is row-major (z outer, x inner): (z=0,x=0), (z=0,x=1), (z=1,x=0), (z=1,x=1).
            Assert.AreEqual(new DDD.Point(0, 0, 0), surface.Vertices[0].Position);
            Assert.AreEqual(new DDD.Point(2, 1, 0), surface.Vertices[1].Position);
            Assert.AreEqual(new DDD.Point(0, 2, 3), surface.Vertices[2].Position);
            Assert.AreEqual(new DDD.Point(2, 3, 3), surface.Vertices[3].Position);
        }

        [TestMethod]
        public void SurfaceProducesTheExpectedVertexAndFaceCounts()
        {
            double[][] heights =
            {
                new[] { 0.0, 0.0, 0.0, 0.0 },
                new[] { 0.0, 0.0, 0.0, 0.0 },
                new[] { 0.0, 0.0, 0.0, 0.0 },
            };
            DDD.Mesh surface = DDD.Graphing.Surface(heights);

            Assert.AreEqual(12, surface.Vertices.Count); // 3 rows x 4 cols
            Assert.AreEqual(12, surface.Faces.Count);     // 2 rows x 3 cols of cells, 2 triangles each
        }

        [TestMethod]
        public void FlatSurfaceHasUpwardNormalsEverywhere()
        {
            double[][] heights =
            {
                new[] { 0.0, 0.0, 0.0 },
                new[] { 0.0, 0.0, 0.0 },
                new[] { 0.0, 0.0, 0.0 },
            };
            DDD.Mesh surface = DDD.Graphing.Surface(heights);

            foreach (DDD.Vertex v in surface.Vertices)
            {
                Assert.AreEqual(new DDD.Vector(0, 1, 0), v.Normal);
            }
        }

        [TestMethod]
        public void SurfaceOnALinearRampTiltsTheNormalByTheExpectedAmount()
        {
            // height = x (independent of z) - a uniform 45-degree ramp. At an interior point the
            // central-difference slope dh/dx = 1, dh/dz = 0, giving tangentU=(1,1,0),
            // tangentV=(0,0,1); Cross(tangentV,tangentU) = (-1,1,0), normalized -> (-1,1,0)/sqrt(2).
            double[][] heights =
            {
                new[] { 0.0, 1.0, 2.0 },
                new[] { 0.0, 1.0, 2.0 },
                new[] { 0.0, 1.0, 2.0 },
            };
            DDD.Mesh surface = DDD.Graphing.Surface(heights);

            DDD.Vector expected = DDD.Vector.Normalize(new DDD.Vector(-1, 1, 0));
            // Row-major insertion, 3 columns per row: (z=1,x=1) is index 1*3+1=4.
            DDD.Vector actual = surface.Vertices[4].Normal!.Value;
            Assert.AreEqual(expected.X, actual.X, 1e-9);
            Assert.AreEqual(expected.Y, actual.Y, 1e-9);
            Assert.AreEqual(expected.Z, actual.Z, 1e-9);
        }

        [TestMethod]
        public void SurfaceThrowsWithFewerThanTwoRows()
        {
            double[][] heights = { new[] { 0.0, 0.0 } };
            Assert.ThrowsExactly<ArgumentException>(() => DDD.Graphing.Surface(heights));
        }

        [TestMethod]
        public void SurfaceThrowsWithFewerThanTwoColumns()
        {
            double[][] heights = { new[] { 0.0 }, new[] { 0.0 } };
            Assert.ThrowsExactly<ArgumentException>(() => DDD.Graphing.Surface(heights));
        }

        [TestMethod]
        public void SurfaceThrowsWhenRowsHaveDifferentLengths()
        {
            double[][] heights = { new[] { 0.0, 0.0 }, new[] { 0.0, 0.0, 0.0 } };
            Assert.ThrowsExactly<ArgumentException>(() => DDD.Graphing.Surface(heights));
        }

        [TestMethod]
        public void SurfaceFaceWindingMatchesStoredVertexNormalsOnABumpyGrid()
        {
            // Same style of check UnitTest_Primitives uses for every other primitive: a face's
            // cross-product winding must point the same general direction as the (summed)
            // normals of the vertices it references - here on a genuinely uneven grid, not just
            // the flat/linear-ramp cases above.
            var heights = new double[4][];
            for (int z = 0; z < 4; z++)
            {
                heights[z] = new double[4];
                for (int x = 0; x < 4; x++)
                {
                    heights[z][x] = Math.Sin(x) * Math.Cos(z);
                }
            }
            DDD.Mesh surface = DDD.Graphing.Surface(heights);

            foreach (DDD.Face face in surface.Faces)
            {
                DDD.Vertex va = surface.Vertices[face.A];
                DDD.Vertex vb = surface.Vertices[face.B];
                DDD.Vertex vc = surface.Vertices[face.C];

                DDD.Vector faceNormal = DDD.Vector.Cross(vb.Position - va.Position, vc.Position - va.Position);
                DDD.Vector referenceNormal = (va.Normal ?? default) + (vb.Normal ?? default) + (vc.Normal ?? default);
                Assert.IsTrue(DDD.Vector.Dot(faceNormal, referenceNormal) > 0,
                    $"Face ({face.A},{face.B},{face.C}) winding doesn't match its vertices' stored normals");
            }
        }
    }
}
