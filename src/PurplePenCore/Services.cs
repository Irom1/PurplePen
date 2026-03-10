using PurplePen.Graphics2D;
using PurplePen.MapModel;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace PurplePen
{
    // Extreme rudimentary way of providing services.
    public static class Services
    {
        public static IGraphicsBitmapLoader BitmapLoader;
        public static IFontLoader FontLoader;
        public static ITextMetrics TextMetricsProvider;
        public static IFileLoaderProvider FileLoaderProvider;
        public static IPdfLoadingStatus PdfLoadingUI;
    }

    public interface IFileLoaderProvider
    {
        IFileLoader GetFileLoaderForDirectory(string path);
    }

#if PORTING
    public static class IconBitmaps
    {
        private static SKBitmap LoadIconBitmap(string name)
        {
            using (var stream = typeof(IconBitmaps).Assembly.GetManifestResourceStream("PurplePen.Resources." + name))
            {
                return SKBitmap.Decode(stream);
            }
        }

        private static Lazy<SKBitmap> descLine_OcadToolbox             = new Lazy<SKBitmap>(() => LoadIconBitmap("OcadToolbox.DescLine_OcadToolbox.png"));
        private static Lazy<SKBitmap> whiteOut_OcadToolbox             = new Lazy<SKBitmap>(() => LoadIconBitmap("OcadToolbox.WhiteOut_OcadToolbox.png"));
        private static Lazy<SKBitmap> number_OcadToolbox               = new Lazy<SKBitmap>(() => LoadIconBitmap("OcadToolbox.Number_OcadToolbox.png"));
        private static Lazy<SKBitmap> control_OcadToolbox              = new Lazy<SKBitmap>(() => LoadIconBitmap("OcadToolbox.Control_OcadToolbox.png"));
        private static Lazy<SKBitmap> exchangeStart_OcadToolbox        = new Lazy<SKBitmap>(() => LoadIconBitmap("OcadToolbox.ExchangeStart_OcadToolbox.png"));
        private static Lazy<SKBitmap> start_OcadToolbox                = new Lazy<SKBitmap>(() => LoadIconBitmap("OcadToolbox.Start_OcadToolbox.png"));
        private static Lazy<SKBitmap> mapIssue_OcadToolbox             = new Lazy<SKBitmap>(() => LoadIconBitmap("OcadToolbox.MapIssue_OcadToolbox.png"));
        private static Lazy<SKBitmap> finish_OcadToolbox               = new Lazy<SKBitmap>(() => LoadIconBitmap("OcadToolbox.Finish_OcadToolbox.png"));
        private static Lazy<SKBitmap> firstAid_OcadToolbox             = new Lazy<SKBitmap>(() => LoadIconBitmap("OcadToolbox.FirstAid_OcadToolbox.png"));
        private static Lazy<SKBitmap> water_OcadToolbox                = new Lazy<SKBitmap>(() => LoadIconBitmap("OcadToolbox.Water_OcadToolbox.png"));
        private static Lazy<SKBitmap> crossing_OcadToolbox             = new Lazy<SKBitmap>(() => LoadIconBitmap("OcadToolbox.Crossing_OcadToolbox.png"));
        private static Lazy<SKBitmap> registration_OcadToolbox         = new Lazy<SKBitmap>(() => LoadIconBitmap("OcadToolbox.Registration_OcadToolbox.png"));
        private static Lazy<SKBitmap> forbidden_OcadToolbox            = new Lazy<SKBitmap>(() => LoadIconBitmap("OcadToolbox.Forbidden_OcadToolbox.png"));
        private static Lazy<SKBitmap> line_OcadToolbox                 = new Lazy<SKBitmap>(() => LoadIconBitmap("OcadToolbox.Line_OcadToolbox.png"));
        private static Lazy<SKBitmap> dashedLine_OcadToolbox           = new Lazy<SKBitmap>(() => LoadIconBitmap("OcadToolbox.DashedLine_OcadToolbox.png"));
        private static Lazy<SKBitmap> lineSpecial_OcadToolbox          = new Lazy<SKBitmap>(() => LoadIconBitmap("OcadToolbox.LineSpecial_OcadToolbox.png"));
        private static Lazy<SKBitmap> oOB_OcadToolbox                  = new Lazy<SKBitmap>(() => LoadIconBitmap("OcadToolbox.OOB_OcadToolbox.png"));
        private static Lazy<SKBitmap> dangerous_OcadToolbox            = new Lazy<SKBitmap>(() => LoadIconBitmap("OcadToolbox.Dangerous_OcadToolbox.png"));
        private static Lazy<SKBitmap> constructionBoundary_OcadToolbox = new Lazy<SKBitmap>(() => LoadIconBitmap("OcadToolbox.ConstructionBoundary_OcadToolbox.png"));
        private static Lazy<SKBitmap> construction_OcadToolbox         = new Lazy<SKBitmap>(() => LoadIconBitmap("OcadToolbox.Construction_OcadToolbox.png"));
        private static Lazy<SKBitmap> descText_OcadToolbox             = new Lazy<SKBitmap>(() => LoadIconBitmap("OcadToolbox.DescText_OcadToolbox.png"));


        public static SKBitmap DescLine_OcadToolbox             { get { return descLine_OcadToolbox.Value; } }
        public static SKBitmap WhiteOut_OcadToolbox             { get { return whiteOut_OcadToolbox.Value; } }
        public static SKBitmap Number_OcadToolbox               { get { return number_OcadToolbox.Value; } }
        public static SKBitmap Control_OcadToolbox              { get { return control_OcadToolbox.Value; } }
        public static SKBitmap ExchangeStart_OcadToolbox        { get { return exchangeStart_OcadToolbox.Value; } }
        public static SKBitmap Start_OcadToolbox                { get { return start_OcadToolbox.Value; } }
        public static SKBitmap MapIssue_OcadToolbox             { get { return mapIssue_OcadToolbox.Value; } }
        public static SKBitmap Finish_OcadToolbox               { get { return finish_OcadToolbox.Value; } }
        public static SKBitmap FirstAid_OcadToolbox             { get { return firstAid_OcadToolbox.Value; } }
        public static SKBitmap Water_OcadToolbox                { get { return water_OcadToolbox.Value; } }
        public static SKBitmap Crossing_OcadToolbox             { get { return crossing_OcadToolbox.Value; } } 
        public static SKBitmap Registration_OcadToolbox         { get { return registration_OcadToolbox.Value; } }
        public static SKBitmap Forbidden_OcadToolbox            { get { return forbidden_OcadToolbox.Value; } }
        public static SKBitmap Line_OcadToolbox                 { get { return line_OcadToolbox.Value; } }
        public static SKBitmap DashedLine_OcadToolbox           { get { return dashedLine_OcadToolbox.Value; } }
        public static SKBitmap LineSpecial_OcadToolbox          { get { return lineSpecial_OcadToolbox.Value; } }
        public static SKBitmap OOB_OcadToolbox                  { get { return oOB_OcadToolbox.Value; } }
        public static SKBitmap Dangerous_OcadToolbox            { get { return dangerous_OcadToolbox.Value; } }
        public static SKBitmap ConstructionBoundary_OcadToolbox { get { return constructionBoundary_OcadToolbox.Value; } }
        public static SKBitmap Construction_OcadToolbox         { get { return construction_OcadToolbox.Value; } }
        public static SKBitmap DescText_OcadToolbox             { get { return descText_OcadToolbox.Value; } } 
    }
#endif
}
