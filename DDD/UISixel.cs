using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace DDD
{
    class UISixel : UI
    {
        const double DegreesPerSecond = 60.0;
        const int TargetFramesPerSecond = 12;
        const int FramebufferWidth = 480;
        const int FramebufferHeight = 480;

        public void Render(List<object> objects, Point boundingBoxMin, Point boundingBoxMax, string title)
        {
            EnsureSixelSupported();

            using var stopRequested = new ManualResetEventSlim(false);
            ConsoleCancelEventHandler onCancel = (_, e) =>
            {
                e.Cancel = true;
                stopRequested.Set();
            };
            Console.CancelKeyPress += onCancel;

            try
            {
                if (!string.IsNullOrEmpty(title))
                {
                    Console.WriteLine(title);
                }
                Console.Write("[?25l");

                var stopwatch = Stopwatch.StartNew();
                bool firstFrame = true;
                int frameLines = (FramebufferHeight + 5) / 6;
                double frameIntervalMs = 1000.0 / TargetFramesPerSecond;

                while (!stopRequested.IsSet)
                {
                    double elapsedSeconds = stopwatch.Elapsed.TotalSeconds;
                    (double angleX, double angleY) = TurntableAngles(elapsedSeconds);

                    Framebuffer framebuffer = Rasterizer.Render(objects, boundingBoxMin, boundingBoxMax,
                        angleX, angleY, FramebufferWidth, FramebufferHeight);
                    string frame = SixelEncoder.Encode(framebuffer, Rasterizer.Palette);

                    if (!firstFrame)
                    {
                        Console.Write($"[{frameLines}A");
                    }
                    firstFrame = false;
                    Console.Write(frame);

                    double sleepMs = frameIntervalMs - (stopwatch.Elapsed.TotalMilliseconds % frameIntervalMs);
                    if (sleepMs > 0)
                    {
                        stopRequested.Wait(TimeSpan.FromMilliseconds(sleepMs));
                    }
                }
            }
            finally
            {
                Console.Write("[?25h");
                Console.WriteLine();
                Console.CancelKeyPress -= onCancel;
            }
        }

        static (double AngleX, double AngleY) TurntableAngles(double elapsedSeconds)
        {
            double phaseSeconds = 360.0 / DegreesPerSecond;
            double t = elapsedSeconds % (phaseSeconds * 2);
            return t < phaseSeconds
                ? (t * DegreesPerSecond, 0.0)
                : (0.0, (t - phaseSeconds) * DegreesPerSecond);
        }

        static void EnsureSixelSupported()
        {
            // There's no reliable, portable way to positively confirm sixel support up front -
            // e.g. Windows Terminal only sets WT_SESSION for shells it spawns itself via a
            // profile, not for a process that gets rehosted into it after the fact. So this only
            // blocks terminals we're confident won't understand the escape sequences at all;
            // everything else is allowed through and left to render (or not).
            string term = Environment.GetEnvironmentVariable("TERM") ?? string.Empty;
            bool knownUnsupported = term.Equals("dumb", StringComparison.OrdinalIgnoreCase);

            if (knownUnsupported)
            {
                throw new InvalidOperationException(
                    "Out-3d needs a terminal that supports sixel graphics, and this one " +
                    $"(TERM={term}) does not. Known-good terminals: Windows Terminal, WezTerm, iTerm2, mlterm, xterm -ti vt340.");
            }
        }
    }
}
