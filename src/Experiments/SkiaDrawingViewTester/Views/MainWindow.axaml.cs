using System;
using Avalonia.Controls;
using AvUtil;
using SkiaSharp;

namespace SkiaDrawingViewTester.Views
{
    public partial class MainWindow : Window
    {
        private const float STARSIZE = 800f;
        private const float InnerRadiusRatio = 0.38196602f;

        public MainWindow()
        {
            InitializeComponent();
            drawingView.Paint += DrawingView_Paint;
            drawingView.InvalidateSurface();
        }

        private void DrawingView_Paint(object? sender, SkiaScrollableDrawingView.PaintEventArgs e)
        {
            var canvas = e.Canvas;
            canvas.Clear(SKColors.White);

            var center = new SKPoint(
                (float)e.LogicalSize.Width / 2f,
                (float)e.LogicalSize.Height / 2f);
            var largestStarSize = MathF.Min(STARSIZE, MathF.Min((float)e.LogicalSize.Width, (float)e.LogicalSize.Height) * 0.9f);
            var starSize = largestStarSize;
            var colors = new[] { SKColors.RoyalBlue, SKColors.Gold, SKColors.Crimson };

            foreach (var color in colors)
            {
                using var paint = new SKPaint {
                    Color = color,
                    Style = SKPaintStyle.Fill,
                    IsAntialias = true,
                };

                using var path = CreateStarPath(center, starSize / 2f);
                canvas.DrawPath(path, paint);
                starSize *= 0.75f;
            }
        }

        private static SKPath CreateStarPath(SKPoint center, float outerRadius)
        {
            var innerRadius = outerRadius * InnerRadiusRatio;
            var path = new SKPath();

            for (var i = 0; i < 10; i++)
            {
                var angle = (-90f + (i * 36f)) * MathF.PI / 180f;
                var radius = i % 2 == 0 ? outerRadius : innerRadius;
                var point = new SKPoint(
                    center.X + (MathF.Cos(angle) * radius),
                    center.Y + (MathF.Sin(angle) * radius));

                if (i == 0)
                {
                    path.MoveTo(point);
                }
                else
                {
                    path.LineTo(point);
                }
            }

            path.Close();
            return path;
        }
    }
}