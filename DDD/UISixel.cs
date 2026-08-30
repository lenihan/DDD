using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace DDD
{
    class UISixel : UI
    {
        const double DegreesPerSecond = 60.0;
        const int TargetFramesPerSecond = 12;

        // Fallback framebuffer size used when the console's pixel dimensions can't be
        // determined (e.g. redirected output) - matches the fixed size this used before
        // console-filling was added.
        const int DefaultFramebufferWidth = 480;
        const int DefaultFramebufferHeight = 480;

        // Rough terminal cell size in pixels, used to translate Console.WindowWidth/Height
        // (character cells - .NET has no cross-platform API for actual cell pixel size) into a
        // sixel image size that approximately fills the visible window.
        const int AssumedCellPixelWidth = 10;
        const int AssumedCellPixelHeight = 20;
        const int WindowRowMargin = 2; // leave a couple rows unfilled so the image doesn't force a scroll
        const int MinFramebufferDimension = 200;
        const int MaxFramebufferDimension = 2400;

        const double RotationStepDegrees = 3.0;
        const double ZoomStepFactor = 1.1;
        const double ZoomMin = 0.1;
        const double ZoomMax = 10.0;
        const double EmaAlpha = 0.15;
        const int TextScale = 2;
        const int OverlayMargin = 4;
        static readonly (byte R, byte G, byte B) OverlayColor = (200, 200, 200);
        const string InstructionsText = "ARROWS:ROTATE  []:ROLL  +/-:ZOOM  T:TURNTABLE  P:PERSP  F:FPS  H:HELP  ESC:QUIT";

        public void Render(List<object> objects, Point boundingBoxMin, Point boundingBoxMax, string title, RenderOptions options)
        {
            EnsureSixelSupported();

            (int framebufferWidth, int framebufferHeight) = DetermineFramebufferSize();

            using var stopRequested = new ManualResetEventSlim(false);
            ConsoleCancelEventHandler onCancel = (_, e) =>
            {
                e.Cancel = true;
                stopRequested.Set();
            };
            Console.CancelKeyPress += onCancel;

            bool titlePrinted = !string.IsNullOrEmpty(title);

            // Row each frame is redrawn at. Deliberately an absolute cursor position rather than
            // a relative "move up N lines" computed from the image's pixel height - terminals
            // vary in how many text rows they advance the cursor after a sixel image (DEC's
            // 6-pixels-per-sixel-row unit doesn't necessarily match the terminal's actual font
            // cell height), so a relative move drifts frame to frame. An absolute position can't
            // drift.
            int frameStartRow = titlePrinted ? 2 : 1;

            // Draw in the terminal's alternate screen buffer, same as vim/less/htop - the
            // terminal snaps back to exactly what it showed before on exit (scroll position,
            // cursor position, and all prior content), so there's nothing to manually erase.
            bool useAlternateScreen = !Console.IsOutputRedirected;

            try
            {
                if (useAlternateScreen)
                {
                    Console.Write("\x1b[?1049h");
                }
                if (titlePrinted)
                {
                    Console.WriteLine(title);
                }
                Console.Write("\x1b[?25l");

                var stopwatch = Stopwatch.StartNew();
                double frameIntervalMs = 1000.0 / TargetFramesPerSecond;
                bool canPollKeys = !Console.IsInputRedirected;

                var palette = new List<(byte R, byte G, byte B)>(Rasterizer.Palette) { OverlayColor };

                double angleX = 0.0, angleY = 0.0, angleZ = 0.0;
                bool manualRotation = false;
                double zoom = 1.0;
                bool perspective = options.InitialPerspective;
                bool showFps = options.InitialShowFps;
                bool showInstructions = options.InitialShowInstructions;

                double lastFrameStartMs = stopwatch.Elapsed.TotalMilliseconds;
                double emaFrameMs = 0.0;

                while (!stopRequested.IsSet)
                {
                    double elapsedSeconds = stopwatch.Elapsed.TotalSeconds;

                    if (!manualRotation)
                    {
                        (angleX, angleY) = TurntableAngles(elapsedSeconds);
                    }

                    void EnsureManualMode()
                    {
                        if (manualRotation) return;
                        (angleX, angleY) = TurntableAngles(elapsedSeconds);
                        manualRotation = true;
                    }

                    if (canPollKeys)
                    {
                        while (Console.KeyAvailable)
                        {
                            ConsoleKeyInfo key = Console.ReadKey(intercept: true);

                            if (key.Key == ConsoleKey.Escape)
                            {
                                stopRequested.Set();
                                break;
                            }
                            else if (key.Key == ConsoleKey.UpArrow)
                            {
                                EnsureManualMode();
                                angleX -= RotationStepDegrees;
                            }
                            else if (key.Key == ConsoleKey.DownArrow)
                            {
                                EnsureManualMode();
                                angleX += RotationStepDegrees;
                            }
                            else if (key.Key == ConsoleKey.LeftArrow)
                            {
                                EnsureManualMode();
                                angleY -= RotationStepDegrees;
                            }
                            else if (key.Key == ConsoleKey.RightArrow)
                            {
                                EnsureManualMode();
                                angleY += RotationStepDegrees;
                            }
                            else
                            {
                                switch (key.KeyChar)
                                {
                                    case '[':
                                        EnsureManualMode();
                                        angleZ -= RotationStepDegrees;
                                        break;
                                    case ']':
                                        EnsureManualMode();
                                        angleZ += RotationStepDegrees;
                                        break;
                                    case '+':
                                    case '=':
                                        zoom = Math.Min(ZoomMax, zoom * ZoomStepFactor);
                                        break;
                                    case '-':
                                        zoom = Math.Max(ZoomMin, zoom / ZoomStepFactor);
                                        break;
                                    case 't':
                                    case 'T':
                                        manualRotation = false;
                                        break;
                                    case 'p':
                                    case 'P':
                                        perspective = !perspective;
                                        break;
                                    case 'f':
                                    case 'F':
                                        showFps = !showFps;
                                        break;
                                    case 'h':
                                    case 'H':
                                        showInstructions = !showInstructions;
                                        break;
                                }
                            }
                        }
                    }

                    Framebuffer framebuffer = Rasterizer.Render(objects, boundingBoxMin, boundingBoxMax,
                        angleX, angleY, framebufferWidth, framebufferHeight, angleZ, perspective, zoom);

                    double nowMs = stopwatch.Elapsed.TotalMilliseconds;
                    double instantFrameMs = nowMs - lastFrameStartMs;
                    lastFrameStartMs = nowMs;
                    emaFrameMs = emaFrameMs == 0.0 ? instantFrameMs : (EmaAlpha * instantFrameMs) + ((1 - EmaAlpha) * emaFrameMs);

                    if (showInstructions)
                    {
                        BitmapFont.DrawText(framebuffer, InstructionsText, OverlayMargin, OverlayMargin,
                            OverlayColor.R, OverlayColor.G, OverlayColor.B, TextScale);
                    }
                    if (showFps)
                    {
                        double fps = emaFrameMs > 0.0 ? 1000.0 / emaFrameMs : 0.0;
                        string fpsText = $"FPS: {fps:0.0}";
                        int textWidth = BitmapFont.MeasureWidth(fpsText, TextScale);
                        BitmapFont.DrawText(framebuffer, fpsText, framebufferWidth - textWidth - OverlayMargin, OverlayMargin,
                            OverlayColor.R, OverlayColor.G, OverlayColor.B, TextScale);
                    }

                    string frame = SixelEncoder.Encode(framebuffer, palette);

                    Console.Write($"\x1b[{frameStartRow};1H");
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
                Console.Write("\x1b[?25h");
                if (useAlternateScreen)
                {
                    Console.Write("\x1b[?1049l");
                }
                Console.CancelKeyPress -= onCancel;
            }
        }

        static (int Width, int Height) DetermineFramebufferSize()
        {
            if (Console.IsOutputRedirected)
            {
                return (DefaultFramebufferWidth, DefaultFramebufferHeight);
            }

            try
            {
                int columns = Math.Max(Console.WindowWidth, 20);
                int rows = Math.Max(Console.WindowHeight - WindowRowMargin, 10);
                int width = Math.Clamp(columns * AssumedCellPixelWidth, MinFramebufferDimension, MaxFramebufferDimension);
                int height = Math.Clamp(rows * AssumedCellPixelHeight, MinFramebufferDimension, MaxFramebufferDimension);
                return (width, height);
            }
            catch (IOException)
            {
                return (DefaultFramebufferWidth, DefaultFramebufferHeight);
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
