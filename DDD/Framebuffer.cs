using System;

namespace DDD
{
    internal sealed class Framebuffer
    {
        readonly byte[] _pixels;

        // One value per pixel, smaller = closer to the camera (see Rasterizer's DrawMesh, which
        // feeds in -rotatedZ - the same camera-relative convention ViewDirection/backface
        // culling already use). Reset to +Infinity by Clear so the very first fill at any pixel
        // always wins, regardless of face/mesh draw order - this is what actually fixes the
        // depth-ordering gap described on Framebuffer.FillTriangle below.
        readonly float[] _depth;

        public int Width { get; }
        public int Height { get; }

        public Framebuffer(int width, int height)
        {
            Width = width;
            Height = height;
            _pixels = new byte[width * height * 3];
            // A fresh array defaults every element to 0.0, not +Infinity - without this, an
            // un-Cleared Framebuffer's very first fill at any pixel would compare its depth
            // against 0.0 instead of "nothing here yet" and could lose to it.
            _depth = new float[width * height];
            Array.Fill(_depth, float.PositiveInfinity);
        }

        public void Clear(byte r, byte g, byte b)
        {
            for (int i = 0; i < _pixels.Length; i += 3)
            {
                _pixels[i] = r;
                _pixels[i + 1] = g;
                _pixels[i + 2] = b;
            }
            Array.Fill(_depth, float.PositiveInfinity);
        }

        public void SetPixel(int x, int y, byte r, byte g, byte b)
        {
            if (x < 0 || x >= Width || y < 0 || y >= Height) return;
            int i = (y * Width + x) * 3;
            _pixels[i] = r;
            _pixels[i + 1] = g;
            _pixels[i + 2] = b;
        }

        public (byte R, byte G, byte B) GetPixel(int x, int y)
        {
            int i = (y * Width + x) * 3;
            return (_pixels[i], _pixels[i + 1], _pixels[i + 2]);
        }

        // z0/z1/z2 are per-vertex depth (see the field comment on _depth above) - each pixel's
        // depth is the barycentric interpolation of the three, compared against what's already
        // in the depth buffer before the pixel is written. A farther face drawn after a nearer
        // one at the same pixel no longer simply overwrites it (the bug PLAN.md 1g describes) -
        // whichever is actually closest to the camera wins, regardless of draw order, both
        // within one mesh's own faces and across separate meshes sharing this Framebuffer.
        public void FillTriangle(int x0, int y0, double z0, int x1, int y1, double z1, int x2, int y2, double z2, byte r, byte g, byte b)
        {
            int minX = Math.Max(0, Math.Min(x0, Math.Min(x1, x2)));
            int maxX = Math.Min(Width - 1, Math.Max(x0, Math.Max(x1, x2)));
            int minY = Math.Max(0, Math.Min(y0, Math.Min(y1, y2)));
            int maxY = Math.Min(Height - 1, Math.Max(y0, Math.Max(y1, y2)));

            int area = EdgeFunction(x0, y0, x1, y1, x2, y2);
            if (area == 0) return; // degenerate (zero-area)

            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    int w0 = EdgeFunction(x1, y1, x2, y2, x, y);
                    int w1 = EdgeFunction(x2, y2, x0, y0, x, y);
                    int w2 = EdgeFunction(x0, y0, x1, y1, x, y);

                    bool inside = (w0 >= 0 && w1 >= 0 && w2 >= 0) || (w0 <= 0 && w1 <= 0 && w2 <= 0);
                    if (!inside) continue;

                    double depth = (w0 * z0 + w1 * z1 + w2 * z2) / area;
                    int depthIndex = y * Width + x;
                    if (depth < _depth[depthIndex])
                    {
                        _depth[depthIndex] = (float)depth;
                        SetPixel(x, y, r, g, b);
                    }
                }
            }
        }

        static int EdgeFunction(int ax, int ay, int bx, int by, int px, int py) =>
            (px - ax) * (by - ay) - (py - ay) * (bx - ax);

        public void DrawLine(int x0, int y0, int x1, int y1, byte r, byte g, byte b)
        {
            int dx = Math.Abs(x1 - x0), sx = x0 < x1 ? 1 : -1;
            int dy = -Math.Abs(y1 - y0), sy = y0 < y1 ? 1 : -1;
            int err = dx + dy;
            int x = x0, y = y0;

            while (true)
            {
                SetPixel(x, y, r, g, b);
                if (x == x1 && y == y1) break;
                int e2 = 2 * err;
                if (e2 >= dy) { err += dy; x += sx; }
                if (e2 <= dx) { err += dx; y += sy; }
            }
        }
    }
}
