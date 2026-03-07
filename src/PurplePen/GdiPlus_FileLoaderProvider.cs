using PurplePen.MapModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PurplePen
{
#if PORTING
    public class GdiPlus_FileLoaderProvider : IFileLoaderProvider
    {
        public IFileLoader GetFileLoaderForDirectory(string path)
        {
            return new GDIPlus_FileLoader(path);
        }
    }
#endif
}
