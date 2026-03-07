using PurplePen.Graphics2D;
using PurplePen.MapModel;
using System;
using System.Collections.Generic;
using System.Linq;
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
    }

    public interface IFileLoaderProvider
    {
        IFileLoader GetFileLoaderForDirectory(string path);
    }
}
