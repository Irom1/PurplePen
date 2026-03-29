using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvUtil
{
    /// <summary>
    /// Provides conversion methods between SkiaSharp, Avalonia, and System.Drawing types.
    /// </summary>
    public static class Conv
    {

#if false // We can't reference Matrix here. Must move this code elsewhere.

        /// <summary>
        /// Converts a System.Drawing.Drawing2D.Matrix to a SkiaSharp.SKMatrix.
        /// </summary>
        /// <param name="mat">The matrix to convert.</param>
        /// <returns>A SkiaSharp.SKMatrix representation of the input matrix.</returns>
        public static SKMatrix ToSKMatrix(Matrix mat)
        {
            float[] elements = mat.Elements;
            return new SKMatrix(elements[0], elements[1], elements[4], elements[2], elements[3], elements[5], 0, 0, 1);
        }

        /// <summary>
        /// Converts a System.Drawing.Drawing2D.Matrix to an Avalonia.Matrix.
        /// </summary>
        /// <param name="mat">The matrix to convert.</param>
        /// <returns>An Avalonia.Matrix representation of the input matrix.</returns>
        public static Avalonia.Matrix ToAvMatrix(Matrix mat)
        {
            float[] elements = mat.Elements;
            return new Avalonia.Matrix(elements[0], elements[1], elements[2], elements[3], elements[4], elements[5]);
        }

#endif

        /// <summary>
        /// Converts a System.Drawing.PointF to a SkiaSharp.SKPoint.
        /// </summary>
        /// <param name="pt">The point to convert.</param>
        /// <returns>A SkiaSharp.SKPoint representation of the input point.</returns>
        public static SKPoint ToSKPoint(PointF pt)
        {
            return new SKPoint(pt.X, pt.Y);
        }

        /// <summary>
        /// Converts a System.Drawing.PointF to an Avalonia.Point.
        /// </summary>
        /// <param name="pt">The point to convert.</param>
        /// <returns>An Avalonia.Point representation of the input point.</returns>
        public static Avalonia.Point ToAvPoint(PointF pt)
        {
            return new Avalonia.Point(pt.X, pt.Y);
        }

        /// <summary>
        /// Converts a SkiaSharp.SKPoint to a System.Drawing.PointF.
        /// </summary>
        /// <param name="pt">The point to convert.</param>
        /// <returns>A System.Drawing.PointF representation of the input point.</returns>
        public static PointF ToPointF(SKPoint pt)
        {
            return new PointF(pt.X, pt.Y);
        }

        /// <summary>
        /// Converts an Avalonia.Point to a System.Drawing.PointF.
        /// </summary>
        /// <param name="pt">The point to convert.</param>
        /// <returns>A System.Drawing.PointF representation of the input point.</returns>
        public static PointF ToPointF(Avalonia.Point pt)
        {
            return new PointF((float)pt.X, (float)pt.Y);
        }

        /// <summary>
        /// Converts a System.Drawing.RectangleF to a SkiaSharp.SKRect.
        /// </summary>
        /// <param name="rect">The rectangle to convert.</param>
        /// <returns>A SkiaSharp.SKRect representation of the input rectangle.</returns>
        public static SKRect ToSKRect(RectangleF rect)
        {
            return new SKRect(rect.Left, rect.Top, rect.Right, rect.Bottom);
        }

        /// <summary>
        /// Converts a Avalonia.Rect to a SkiaSharp.SKRect.
        /// </summary>
        /// <param name="rect">The rectangle to convert.</param>
        /// <returns>A SkiaSharp.SKRect representation of the input rectangle.</returns>
        public static SKRect ToSKRect(Avalonia.Rect rect)
        {
            return new SKRect((float)rect.Left, (float)rect.Top, (float)rect.Right, (float)rect.Bottom);
        }

        /// <summary>
        /// Converts a System.Drawing.RectangleF to an Avalonia.Rect.
        /// </summary>
        /// <param name="rect">The rectangle to convert.</param>
        /// <returns>An Avalonia.Rect representation of the input rectangle.</returns>
        public static Avalonia.Rect ToAvRect(RectangleF rect)
        {
            return new Avalonia.Rect(rect.Left, rect.Top, rect.Width, rect.Height);
        }

        /// <summary>
        /// Converts a System.Drawing.SizeF to an Avalonia.Size.
        /// </summary>
        /// <param name="size">The size to convert.</param>
        /// <returns>An Avalonia.Size representation of the input size.</returns>
        public static Avalonia.Size ToAvSize(SizeF size)
        {
            return new Avalonia.Size(size.Width, size.Height);
        }

        /// <summary>
        /// Converts a SkiaSharp.SKRect to an Avalonia.Rect.
        /// </summary>
        /// <param name="rect">The rectangle to convert.</param>
        /// <returns>An Avalonia.Rect representation of the input rectangle.</returns>
        public static Avalonia.Rect ToAvRect(SKRect rect)
        {
            return new Avalonia.Rect(rect.Left, rect.Top, rect.Width, rect.Height);
        }

        /// <summary>
        /// Converts a SkiaSharp.SKRect to a System.Drawing.RectangleF.
        /// </summary>
        /// <param name="rect">The rectangle to convert.</param>
        /// <returns>A System.Drawing.RectangleF representation of the input rectangle.</returns>
        public static RectangleF ToRectangleF(SKRect rect)
        {
            return new RectangleF(rect.Left, rect.Top, rect.Width, rect.Height);
        }

        /// <summary>
        /// Converts an Avalonia.Rect to a System.Drawing.RectangleF.
        /// </summary>
        /// <param name="rect">The rectangle to convert.</param>
        /// <returns>A System.Drawing.RectangleF representation of the input rectangle.</returns>
        public static RectangleF ToRectangleF(Avalonia.Rect rect)
        {
            return new RectangleF((float)rect.X, (float)rect.Y, (float)rect.Width, (float)rect.Height);
        }
    }
}
