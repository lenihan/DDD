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
    }
}
