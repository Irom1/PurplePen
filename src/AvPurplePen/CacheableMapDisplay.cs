using AvUtil;
using PurplePen;
using PurplePen.Graphics2D;
using PurplePen.MapModel;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AvPurplePen
{
    // Class that implements IAsyncSkiaDrawing to draw an IMapDisplay. 
    // Basically adaptes the IMapDisplay interface into IAsyncSkiaDrawing.
    class CacheableMapDisplay : IAsyncSkiaDrawing
    {
        IMapDisplay mapDisplay;
        RectangleF bounds;

        public CacheableMapDisplay(IMapDisplay mapDisplay)
        {
            this.mapDisplay = mapDisplay;
            mapDisplay.Changed += MapDisplay_Changed;
            bounds = mapDisplay.Bounds;
        }

        public event EventHandler? DrawingChanged;

        public SKRect Bounds => Conv.ToSKRect(bounds);


        public Task DrawAsync(SKCanvas canvas, SKRect rectToDraw, SKSizeI pixelSize, CancellationToken cancelToken)
        {
            float minResolution = Math.Min(rectToDraw.Width / pixelSize.Width, rectToDraw.Height / pixelSize.Height);
            return Task.Run(() => Draw(canvas, rectToDraw, minResolution, cancelToken), cancelToken);
        }

        // The MapDisplay has changed. Raise the DrawingChanged event to indicate that
        // he drawing needs to be redrawn.
        private void MapDisplay_Changed()
        {
            DrawingChanged?.Invoke(this, EventArgs.Empty);
        }

        // This is called in another thread to draw the map.
        private void Draw(SKCanvas canvas, SKRect rectToDraw, float minResolution, CancellationToken cancelToken)
        {
            canvas.Clear(SKColors.White);  // TODO: Is this needed?

            if (mapDisplay != null) {
                using (IGraphicsTarget grTarget = new Skia_GraphicsTarget(canvas)) {
                    mapDisplay.Draw(grTarget, Conv.ToRectangleF(rectToDraw), minResolution, () => cancelToken.ThrowIfCancellationRequested());
                }
            }
        }
    }
}
