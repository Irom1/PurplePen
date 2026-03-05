using PurplePen.MapModel;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PurplePen
{
    public class CoreMapUtil
    {
        // Create a ToolboxIcon from a SkiaSharp bitmap.
        public static ToolboxIcon CreateToolboxIcon(SKBitmap bm)
        {
            ToolboxIcon icon = new ToolboxIcon();

            for (int x = 0; x < ToolboxIcon.WIDTH; ++x) {
                for (int y = 0; y < ToolboxIcon.HEIGHT; ++y) {
                    SKColor skColor = bm.GetPixel(x, y);
                    icon.SetPixel(x, y, Color.FromArgb(skColor.Alpha, skColor.Red, skColor.Green, skColor.Blue));
                }
            }

            return icon;
        }


    }
}
