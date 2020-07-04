using System;
using System.Collections.Generic;

namespace DDD
{
    interface UI
    {
        void ShowWindow(List<object> Objects, Point BoundingBoxMin, Point BoundingBoxMax, string Title);
    }
}