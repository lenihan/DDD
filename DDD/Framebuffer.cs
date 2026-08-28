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
