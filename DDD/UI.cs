using System.Collections.Generic;

namespace DDD
{
    interface UI
    {
        void Render(List<object> objects, Point boundingBoxMin, Point boundingBoxMax, string title);
    }
}
