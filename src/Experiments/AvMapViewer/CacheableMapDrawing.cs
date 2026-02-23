using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Avalonia;
using Avalonia.Media;
using SkiaSharp;

using AvUtil;
using PurplePen.Graphics2D;
using PurplePen.MapModel;
using RenderOptions = PurplePen.MapModel.RenderOptions;

namespace AvMapViewer
{
    // Implements IAsyncSkiaDrawing to draw a Map on a Skia canvas. The map is cached, so it can be drawn quickly when needed.
    class CacheableMapDrawing : IAsyncSkiaDrawing
    {
        Map? map;
        RectangleF bounds;

        public Map? Map {
            get { return map; }
            set {
                if (map == value)
                    return;

                this.map = value;

                if (map != null) {
                    using (map.Read()) {
                        bounds = map.Bounds;
                    }
                }

                DrawingChanged?.Invoke(this, EventArgs.Empty);
            }
        }

        public SKRect Bounds => Conv.ToSKRect(bounds);

        public event EventHandler? DrawingChanged;

        public Task DrawAsync(SKCanvas canvas, SKRect rectToDraw, SKSizeI pixelSize, CancellationToken cancelToken)
        {
            return Task.Run(() => Draw(canvas, rectToDraw, cancelToken), cancelToken);
        }


        // This is called in another thread to draw the map.
        private void Draw(SKCanvas canvas, SKRect rectToDraw, CancellationToken cancelToken)
        {
            canvas.Clear(SKColors.White);

            if (map != null) {
                using (IGraphicsTarget grTarget = new Skia_GraphicsTarget(canvas)) {
                    RenderOptions renderOpts = new RenderOptions();
                    renderOpts.usePatternBitmaps = true;
                    renderOpts.renderTemplates = RenderTemplateOption.MapAndTemplates;
                    renderOpts.blendOverprintedColors = false;

                    grTarget.PushAntiAliasing(true);

                    using (map.Read()) {
                        map.Draw(grTarget, Conv.ToRectangleF(rectToDraw), renderOpts, () => cancelToken.ThrowIfCancellationRequested());
                    }

                }
            }
        }
    }

}

