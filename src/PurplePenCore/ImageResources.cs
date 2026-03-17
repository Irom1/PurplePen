using PurplePen.Graphics2D;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PurplePen
{
    public static class ImageResources
    {
        private static IGraphicsBitmap LoadBitmapFromResource(string name)
        {
            using (var stream = typeof(ImageResources).Assembly.GetManifestResourceStream("PurplePen.Resources." + name)) {
                return Services.BitmapLoader.ReadBitmapFromStream(stream);
            }
        }

        private static Lazy<IGraphicsBitmap> logoImage = new Lazy<IGraphicsBitmap>(() => LoadBitmapFromResource("logobkgd.png"));
        private static Lazy<IGraphicsBitmap> descLine_OcadToolbox = new Lazy<IGraphicsBitmap>(() => LoadBitmapFromResource("OcadToolbox.DescLine_OcadToolbox.png"));
        private static Lazy<IGraphicsBitmap> whiteOut_OcadToolbox = new Lazy<IGraphicsBitmap>(() => LoadBitmapFromResource("OcadToolbox.WhiteOut_OcadToolbox.png"));
        private static Lazy<IGraphicsBitmap> number_OcadToolbox = new Lazy<IGraphicsBitmap>(() => LoadBitmapFromResource("OcadToolbox.Number_OcadToolbox.png"));
        private static Lazy<IGraphicsBitmap> control_OcadToolbox = new Lazy<IGraphicsBitmap>(() => LoadBitmapFromResource("OcadToolbox.Control_OcadToolbox.png"));
        private static Lazy<IGraphicsBitmap> exchangeStart_OcadToolbox = new Lazy<IGraphicsBitmap>(() => LoadBitmapFromResource("OcadToolbox.ExchangeStart_OcadToolbox.png"));
        private static Lazy<IGraphicsBitmap> start_OcadToolbox = new Lazy<IGraphicsBitmap>(() => LoadBitmapFromResource("OcadToolbox.Start_OcadToolbox.png"));
        private static Lazy<IGraphicsBitmap> mapIssue_OcadToolbox = new Lazy<IGraphicsBitmap>(() => LoadBitmapFromResource("OcadToolbox.MapIssue_OcadToolbox.png"));
        private static Lazy<IGraphicsBitmap> finish_OcadToolbox = new Lazy<IGraphicsBitmap>(() => LoadBitmapFromResource("OcadToolbox.Finish_OcadToolbox.png"));
        private static Lazy<IGraphicsBitmap> firstAid_OcadToolbox = new Lazy<IGraphicsBitmap>(() => LoadBitmapFromResource("OcadToolbox.FirstAid_OcadToolbox.png"));
        private static Lazy<IGraphicsBitmap> water_OcadToolbox = new Lazy<IGraphicsBitmap>(() => LoadBitmapFromResource("OcadToolbox.Water_OcadToolbox.png"));
        private static Lazy<IGraphicsBitmap> crossing_OcadToolbox = new Lazy<IGraphicsBitmap>(() => LoadBitmapFromResource("OcadToolbox.Crossing_OcadToolbox.png"));
        private static Lazy<IGraphicsBitmap> registration_OcadToolbox = new Lazy<IGraphicsBitmap>(() => LoadBitmapFromResource("OcadToolbox.Registration_OcadToolbox.png"));
        private static Lazy<IGraphicsBitmap> forbidden_OcadToolbox = new Lazy<IGraphicsBitmap>(() => LoadBitmapFromResource("OcadToolbox.Forbidden_OcadToolbox.png"));
        private static Lazy<IGraphicsBitmap> line_OcadToolbox = new Lazy<IGraphicsBitmap>(() => LoadBitmapFromResource("OcadToolbox.Line_OcadToolbox.png"));
        private static Lazy<IGraphicsBitmap> dashedLine_OcadToolbox = new Lazy<IGraphicsBitmap>(() => LoadBitmapFromResource("OcadToolbox.DashedLine_OcadToolbox.png"));
        private static Lazy<IGraphicsBitmap> lineSpecial_OcadToolbox = new Lazy<IGraphicsBitmap>(() => LoadBitmapFromResource("OcadToolbox.LineSpecial_OcadToolbox.png"));
        private static Lazy<IGraphicsBitmap> oOB_OcadToolbox = new Lazy<IGraphicsBitmap>(() => LoadBitmapFromResource("OcadToolbox.OOB_OcadToolbox.png"));
        private static Lazy<IGraphicsBitmap> dangerous_OcadToolbox = new Lazy<IGraphicsBitmap>(() => LoadBitmapFromResource("OcadToolbox.Dangerous_OcadToolbox.png"));
        private static Lazy<IGraphicsBitmap> constructionBoundary_OcadToolbox = new Lazy<IGraphicsBitmap>(() => LoadBitmapFromResource("OcadToolbox.ConstructionBoundary_OcadToolbox.png"));
        private static Lazy<IGraphicsBitmap> construction_OcadToolbox = new Lazy<IGraphicsBitmap>(() => LoadBitmapFromResource("OcadToolbox.Construction_OcadToolbox.png"));
        private static Lazy<IGraphicsBitmap> descText_OcadToolbox = new Lazy<IGraphicsBitmap>(() => LoadBitmapFromResource("OcadToolbox.DescText_OcadToolbox.png"));


        public static IGraphicsBitmap LogoImage => logoImage.Value;
        public static IGraphicsBitmap DescLine_OcadToolbox => descLine_OcadToolbox.Value; 
        public static IGraphicsBitmap WhiteOut_OcadToolbox => whiteOut_OcadToolbox.Value; 
        public static IGraphicsBitmap Number_OcadToolbox => number_OcadToolbox.Value; 
        public static IGraphicsBitmap Control_OcadToolbox => control_OcadToolbox.Value; 
        public static IGraphicsBitmap ExchangeStart_OcadToolbox => exchangeStart_OcadToolbox.Value; 
        public static IGraphicsBitmap Start_OcadToolbox => start_OcadToolbox.Value; 
        public static IGraphicsBitmap MapIssue_OcadToolbox => mapIssue_OcadToolbox.Value; 
        public static IGraphicsBitmap Finish_OcadToolbox => finish_OcadToolbox.Value; 
        public static IGraphicsBitmap FirstAid_OcadToolbox => firstAid_OcadToolbox.Value; 
        public static IGraphicsBitmap Water_OcadToolbox => water_OcadToolbox.Value; 
        public static IGraphicsBitmap Crossing_OcadToolbox => crossing_OcadToolbox.Value; 
        public static IGraphicsBitmap Registration_OcadToolbox => registration_OcadToolbox.Value; 
        public static IGraphicsBitmap Forbidden_OcadToolbox => forbidden_OcadToolbox.Value; 
        public static IGraphicsBitmap Line_OcadToolbox => line_OcadToolbox.Value; 
        public static IGraphicsBitmap DashedLine_OcadToolbox => dashedLine_OcadToolbox.Value; 
        public static IGraphicsBitmap LineSpecial_OcadToolbox => lineSpecial_OcadToolbox.Value; 
        public static IGraphicsBitmap OOB_OcadToolbox => oOB_OcadToolbox.Value; 
        public static IGraphicsBitmap Dangerous_OcadToolbox => dangerous_OcadToolbox.Value; 
        public static IGraphicsBitmap ConstructionBoundary_OcadToolbox => constructionBoundary_OcadToolbox.Value; 
        public static IGraphicsBitmap Construction_OcadToolbox => construction_OcadToolbox.Value; 
        public static IGraphicsBitmap DescText_OcadToolbox => descText_OcadToolbox.Value; 
    }
}

