using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Text;

namespace AvUtil
{
    public class GeometryUtil
    {
        // Find the transformation matrix that transform a rectangle (with positive Y upward) to a different
        // rectangle (with positive Y upward)
        public static SKMatrix CreateRectangleTransform(SKRect source, SKRect dest)
        {
            float scaleX = dest.Width / source.Width;
            float scaleY = dest.Height / source.Height;

            float translateX = dest.Left - source.Left * scaleX;
            float translateY = dest.Top - source.Top * scaleY;

            SKMatrix m = SKMatrix.CreateIdentity();
            m.ScaleX = scaleX;
            m.ScaleY = scaleY;
            m.TransX = translateX;
            m.TransY = translateY;
            return m;
        }
    }
}
