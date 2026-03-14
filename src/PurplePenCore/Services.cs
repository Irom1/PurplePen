using Microsoft.Extensions.DependencyInjection;
using PurplePen.Graphics2D;
using PurplePen.MapModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace PurplePen
{
    // Extreme rudimentary way of providing services.
    public static class Services
    {
        private static IServiceProvider serviceProvider;

        public static void RegisterServiceProvider(IServiceProvider serviceProvider)
        {
            Services.serviceProvider = serviceProvider;
        }

        public static IGraphicsBitmapLoader BitmapLoader => serviceProvider.GetRequiredService<IGraphicsBitmapLoader>();
        public static IBitmapGraphicsTargetProvider BitmapGraphicsTargetProvider => serviceProvider.GetRequiredService<IBitmapGraphicsTargetProvider>();
        public static IFontLoader FontLoader => serviceProvider.GetRequiredService<IFontLoader>();
        public static ITextMetrics TextMetricsProvider => serviceProvider.GetRequiredService<ITextMetrics>();
        public static IFileLoaderProvider FileLoaderProvider => serviceProvider.GetRequiredService<IFileLoaderProvider>();
        public static IPdfLoadingStatus PdfLoadingUI => serviceProvider.GetRequiredService<IPdfLoadingStatus>();
        public static IPdfWriter PdfWriter => serviceProvider.GetRequiredService<IPdfWriter>();
    }

    public interface IFileLoaderProvider
    {
        IFileLoader GetFileLoaderForDirectory(string path);
    }

#if PORTING
    public static class IconBitmaps
    {
        private static IGraphicsBitmap LoadIconBitmap(string name)
        {
            using (var stream = typeof(IconBitmaps).Assembly.GetManifestResourceStream("PurplePen.Resources." + name))
            {
                return Services.BitmapLoader.ReadBitmapFromStream(stream);
            }
        }

        private static Lazy<IGraphicsBitmap> descLine_OcadToolbox             = new Lazy<IGraphicsBitmap>(() => LoadIconBitmap("OcadToolbox.DescLine_OcadToolbox.png"));
        private static Lazy<IGraphicsBitmap> whiteOut_OcadToolbox             = new Lazy<IGraphicsBitmap>(() => LoadIconBitmap("OcadToolbox.WhiteOut_OcadToolbox.png"));
        private static Lazy<IGraphicsBitmap> number_OcadToolbox               = new Lazy<IGraphicsBitmap>(() => LoadIconBitmap("OcadToolbox.Number_OcadToolbox.png"));
        private static Lazy<IGraphicsBitmap> control_OcadToolbox              = new Lazy<IGraphicsBitmap>(() => LoadIconBitmap("OcadToolbox.Control_OcadToolbox.png"));
        private static Lazy<IGraphicsBitmap> exchangeStart_OcadToolbox        = new Lazy<IGraphicsBitmap>(() => LoadIconBitmap("OcadToolbox.ExchangeStart_OcadToolbox.png"));
        private static Lazy<IGraphicsBitmap> start_OcadToolbox                = new Lazy<IGraphicsBitmap>(() => LoadIconBitmap("OcadToolbox.Start_OcadToolbox.png"));
        private static Lazy<IGraphicsBitmap> mapIssue_OcadToolbox             = new Lazy<IGraphicsBitmap>(() => LoadIconBitmap("OcadToolbox.MapIssue_OcadToolbox.png"));
        private static Lazy<IGraphicsBitmap> finish_OcadToolbox               = new Lazy<IGraphicsBitmap>(() => LoadIconBitmap("OcadToolbox.Finish_OcadToolbox.png"));
        private static Lazy<IGraphicsBitmap> firstAid_OcadToolbox             = new Lazy<IGraphicsBitmap>(() => LoadIconBitmap("OcadToolbox.FirstAid_OcadToolbox.png"));
        private static Lazy<IGraphicsBitmap> water_OcadToolbox                = new Lazy<IGraphicsBitmap>(() => LoadIconBitmap("OcadToolbox.Water_OcadToolbox.png"));
        private static Lazy<IGraphicsBitmap> crossing_OcadToolbox             = new Lazy<IGraphicsBitmap>(() => LoadIconBitmap("OcadToolbox.Crossing_OcadToolbox.png"));
        private static Lazy<IGraphicsBitmap> registration_OcadToolbox         = new Lazy<IGraphicsBitmap>(() => LoadIconBitmap("OcadToolbox.Registration_OcadToolbox.png"));
        private static Lazy<IGraphicsBitmap> forbidden_OcadToolbox            = new Lazy<IGraphicsBitmap>(() => LoadIconBitmap("OcadToolbox.Forbidden_OcadToolbox.png"));
        private static Lazy<IGraphicsBitmap> line_OcadToolbox                 = new Lazy<IGraphicsBitmap>(() => LoadIconBitmap("OcadToolbox.Line_OcadToolbox.png"));
        private static Lazy<IGraphicsBitmap> dashedLine_OcadToolbox           = new Lazy<IGraphicsBitmap>(() => LoadIconBitmap("OcadToolbox.DashedLine_OcadToolbox.png"));
        private static Lazy<IGraphicsBitmap> lineSpecial_OcadToolbox          = new Lazy<IGraphicsBitmap>(() => LoadIconBitmap("OcadToolbox.LineSpecial_OcadToolbox.png"));
        private static Lazy<IGraphicsBitmap> oOB_OcadToolbox                  = new Lazy<IGraphicsBitmap>(() => LoadIconBitmap("OcadToolbox.OOB_OcadToolbox.png"));
        private static Lazy<IGraphicsBitmap> dangerous_OcadToolbox            = new Lazy<IGraphicsBitmap>(() => LoadIconBitmap("OcadToolbox.Dangerous_OcadToolbox.png"));
        private static Lazy<IGraphicsBitmap> constructionBoundary_OcadToolbox = new Lazy<IGraphicsBitmap>(() => LoadIconBitmap("OcadToolbox.ConstructionBoundary_OcadToolbox.png"));
        private static Lazy<IGraphicsBitmap> construction_OcadToolbox         = new Lazy<IGraphicsBitmap>(() => LoadIconBitmap("OcadToolbox.Construction_OcadToolbox.png"));
        private static Lazy<IGraphicsBitmap> descText_OcadToolbox             = new Lazy<IGraphicsBitmap>(() => LoadIconBitmap("OcadToolbox.DescText_OcadToolbox.png"));


        public static IGraphicsBitmap DescLine_OcadToolbox             { get { return descLine_OcadToolbox.Value; } }
        public static IGraphicsBitmap WhiteOut_OcadToolbox             { get { return whiteOut_OcadToolbox.Value; } }
        public static IGraphicsBitmap Number_OcadToolbox               { get { return number_OcadToolbox.Value; } }
        public static IGraphicsBitmap Control_OcadToolbox              { get { return control_OcadToolbox.Value; } }
        public static IGraphicsBitmap ExchangeStart_OcadToolbox        { get { return exchangeStart_OcadToolbox.Value; } }
        public static IGraphicsBitmap Start_OcadToolbox                { get { return start_OcadToolbox.Value; } }
        public static IGraphicsBitmap MapIssue_OcadToolbox             { get { return mapIssue_OcadToolbox.Value; } }
        public static IGraphicsBitmap Finish_OcadToolbox               { get { return finish_OcadToolbox.Value; } }
        public static IGraphicsBitmap FirstAid_OcadToolbox             { get { return firstAid_OcadToolbox.Value; } }
        public static IGraphicsBitmap Water_OcadToolbox                { get { return water_OcadToolbox.Value; } }
        public static IGraphicsBitmap Crossing_OcadToolbox             { get { return crossing_OcadToolbox.Value; } } 
        public static IGraphicsBitmap Registration_OcadToolbox         { get { return registration_OcadToolbox.Value; } }
        public static IGraphicsBitmap Forbidden_OcadToolbox            { get { return forbidden_OcadToolbox.Value; } }
        public static IGraphicsBitmap Line_OcadToolbox                 { get { return line_OcadToolbox.Value; } }
        public static IGraphicsBitmap DashedLine_OcadToolbox           { get { return dashedLine_OcadToolbox.Value; } }
        public static IGraphicsBitmap LineSpecial_OcadToolbox          { get { return lineSpecial_OcadToolbox.Value; } }
        public static IGraphicsBitmap OOB_OcadToolbox                  { get { return oOB_OcadToolbox.Value; } }
        public static IGraphicsBitmap Dangerous_OcadToolbox            { get { return dangerous_OcadToolbox.Value; } }
        public static IGraphicsBitmap ConstructionBoundary_OcadToolbox { get { return constructionBoundary_OcadToolbox.Value; } }
        public static IGraphicsBitmap Construction_OcadToolbox         { get { return construction_OcadToolbox.Value; } }
        public static IGraphicsBitmap DescText_OcadToolbox             { get { return descText_OcadToolbox.Value; } } 
    }
#endif
}
