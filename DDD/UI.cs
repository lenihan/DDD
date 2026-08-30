using System.Collections.Generic;

namespace DDD
{
    internal sealed class RenderOptions
    {
        public bool InitialPerspective { get; init; }
        public bool InitialShowFps { get; init; }
        public bool InitialShowInstructions { get; init; }
        public RenderMode InitialRenderMode { get; init; }
        public bool InitialShowNormals { get; init; }
    }

    interface UI
    {
        void Render(List<object> objects, Point boundingBoxMin, Point boundingBoxMax, string title, RenderOptions options);
    }
}
