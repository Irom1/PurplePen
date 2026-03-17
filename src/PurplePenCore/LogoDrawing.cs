using PurplePen.Graphics2D;
using System;
using System.Drawing;
using System.IO;

namespace PurplePen
{
    public static class LogoDrawing
    {
        public static void DrawPurplePenLogo(IGraphicsTarget g, RectangleF rect)
        {
            IGraphicsBitmap logoImage;

            using (Stream stream = typeof(LogoDrawing).Assembly.GetManifestResourceStream("PurplePen.images.logobkgd.png")) {
                logoImage = Services.BitmapLoader.ReadBitmapFromStream(stream);
            }

            g.DrawBitmap(logoImage, rect, BitmapScaling.HighQuality);

            DrawFancyText(MiscText.AppTitle, Color.Purple, g, new RectangleF(rect.Width * 0.05F, 0, rect.Width * 0.9F, rect.Height * 0.7F));
            DrawFancyText(MiscText.AppSubtitle, Color.Black, g, new RectangleF(0, rect.Height * 0.65F, rect.Width, rect.Height * 0.30F));
        }

        static void DrawFancyText(string text, Color color, IGraphicsTarget g, RectangleF rect)
        {
            const string fontName = "TeX Gyre Pagella";
            const TextEffects textEffects = TextEffects.Bold | TextEffects.Italic;
            const float initialFontSize = 24F;

            ITextMetrics textMetrics = Services.TextMetricsProvider;
            ITextFaceMetrics textFaceMetrics = textMetrics.GetTextFaceMetrics(fontName, initialFontSize, textEffects);
            SizeF textSize = textFaceMetrics.GetTextSize(text); 
            float scale = Math.Max(textSize.Width / (rect.Width * 0.95F), textSize.Height / (rect.Height * 0.95F));

            object fontKey = new object();
            float fontSize = initialFontSize / scale;
            g.CreateFont(fontKey, fontName, fontSize, textEffects);
            g.PushAntiAliasing(true);

            float offset = fontSize / 30F;

            textFaceMetrics = textMetrics.GetTextFaceMetrics(fontName, fontSize, textEffects);
            textSize = textFaceMetrics.GetTextSize(text);

            PointF textLocation = rect.Center();
            textLocation.X -= textSize.Width / 2F;
            textLocation.Y -= textSize.Height / 2F;
            PointF offsetTextLocation = new PointF(textLocation.X + offset, textLocation.Y + offset);

            object brushGray = new object(), brushColor = new object();
            g.CreateSolidBrush(brushGray, CmykColor.FromColor(Color.Gray));
            g.CreateSolidBrush(brushColor, CmykColor.FromColor(color));

            g.DrawText(text, fontKey, brushGray, offsetTextLocation);
            g.DrawText(text, fontKey, brushColor, textLocation);
        }
    }
}
