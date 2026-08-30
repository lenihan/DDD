using System;

namespace DDD
{
    internal sealed class Framebuffer
    {
        readonly byte[] _pixels;

        public int Width { get; }
        public int Height { get; }

        public Framebuffer(int width, int height)
        {
            Width = width;
            Height = height;
            _pixels = new byte[width * height * 3];
        }

        public void Clear(byte r, byte g, byte b)
        {
            for (int i = 0; i < _pixels.Length; i += 3)
            {
                _pixels[i] = r;
                _pixels[i + 1] = g;
                _pixels[i + 2] = b;
            }
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

        public void FillTriangle(int x0, int y0, int x1, int y1, int x2, int y2, byte r, byte g, byte b)
        {
            int minX = Math.Max(0, Math.Min(x0, Math.Min(x1, x2)));
            int maxX = Math.Min(Width - 1, Math.Max(x0, Math.Max(x1, x2)));
            int minY = Math.Max(0, Math.Min(y0, Math.Min(y1, y2)));
            int maxY = Math.Min(Height - 1, Math.Max(y0, Math.Max(y1, y2)));

            if (EdgeFunction(x0, y0, x1, y1, x2, y2) == 0) return; // degenerate (zero-area)

            for (int y = minY; y <= maxY; y++)
            {
                for (int x = minX; x <= maxX; x++)
                {
                    int w0 = EdgeFunction(x1, y1, x2, y2, x, y);
                    int w1 = EdgeFunction(x2, y2, x0, y0, x, y);
                    int w2 = EdgeFunction(x0, y0, x1, y1, x, y);

                    bool inside = (w0 >= 0 && w1 >= 0 && w2 >= 0) || (w0 <= 0 && w1 <= 0 && w2 <= 0);
                    if (inside)
                    {
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
