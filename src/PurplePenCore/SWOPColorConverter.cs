using PurplePen.Graphics2D;
using PurplePen.MapModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using SD = System.Drawing;

namespace PurplePen
{
    public class SwopColorConverter: IColorConverter
    {
        const int SAMPLESIZE = 12;
        private static Dictionary<CmykColor, SD.Color> cmykToColor = new Dictionary<CmykColor,SD.Color>();
        private static RGB[,,,] samples = new RGB[SAMPLESIZE, SAMPLESIZE, SAMPLESIZE, SAMPLESIZE];

        static SwopColorConverter()
        {
            byte[] data = new byte[SAMPLESIZE * SAMPLESIZE * SAMPLESIZE * SAMPLESIZE * 3];

            Assembly assembly = Assembly.GetExecutingAssembly();
            using (Stream stream = assembly.GetManifestResourceStream("PurplePen.swopsamples.dat")) {
                Debug.Assert(stream != null, "Could not find the embedded resource.");

                int read = stream.Read(data, 0, data.Length);
                Debug.Assert(read == data.Length, "Could not read all of the data from the embedded resources");

                int pos = 0;

                for (int i = 0; i < SAMPLESIZE; ++i) {
                    for (int j = 0; j < SAMPLESIZE; ++j) {
                        for (int k = 0; k < SAMPLESIZE; ++k) {
                            for (int l = 0; l < SAMPLESIZE; ++l) {
                                samples[i, j, k, l].R = data[pos++];
                                samples[i, j, k, l].G = data[pos++];
                                samples[i, j, k, l].B = data[pos++];
                            }
                        }
                    }
                }
            }
        }

        private static RGBDbl Interp(RGBDbl rgbLow, RGBDbl rgbHigh, float frac)
        {
            return new RGBDbl(rgbLow.R * (1 - frac) + rgbHigh.R * frac,
                rgbLow.G * (1 - frac) + rgbHigh.G * frac,
                rgbLow.B * (1 - frac) + rgbHigh.B * frac);
        }


        private static RGB ConvertUsingInterpolation(float c, float m, float y, float k)
        {
            // https://en.wikipedia.org/wiki/Trilinear_interpolation

            int iLow = (int)Math.Floor(c * (SAMPLESIZE - 1));
            int iHigh = iLow < SAMPLESIZE - 1 ? iLow + 1 : iLow;
            float iFrac = c * (SAMPLESIZE - 1) - iLow;
            int jLow = (int)Math.Floor(m * (SAMPLESIZE - 1));
            int jHigh = jLow < SAMPLESIZE - 1 ? jLow + 1 : jLow;
            float jFrac = m * (SAMPLESIZE - 1) - jLow;
            int kLow = (int)Math.Floor(y * (SAMPLESIZE - 1));
            int kHigh = kLow < SAMPLESIZE - 1 ? kLow + 1 : kLow;
            float kFrac = y * (SAMPLESIZE - 1) - kLow;
            int lLow = (int)Math.Floor(k * (SAMPLESIZE - 1));
            int lHigh = lLow < SAMPLESIZE - 1 ? lLow + 1 : lLow;
            float lFrac = k * (SAMPLESIZE - 1) - lLow;

            RGBDbl rgb000 = Interp(new RGBDbl(samples[iLow, jLow, kLow, lLow]), new RGBDbl(samples[iHigh, jLow, kLow, lLow]), iFrac);
            RGBDbl rgb001 = Interp(new RGBDbl(samples[iLow, jLow, kLow, lHigh]), new RGBDbl(samples[iHigh, jLow, kLow, lHigh]), iFrac);
            RGBDbl rgb010 = Interp(new RGBDbl(samples[iLow, jLow, kHigh, lLow]), new RGBDbl(samples[iHigh, jLow, kHigh, lLow]), iFrac);
            RGBDbl rgb011 = Interp(new RGBDbl(samples[iLow, jLow, kHigh, lHigh]), new RGBDbl(samples[iHigh, jLow, kHigh, lHigh]), iFrac);
            RGBDbl rgb100 = Interp(new RGBDbl(samples[iLow, jHigh, kLow, lLow]), new RGBDbl(samples[iHigh, jHigh, kLow, lLow]), iFrac);
            RGBDbl rgb101 = Interp(new RGBDbl(samples[iLow, jHigh, kLow, lHigh]), new RGBDbl(samples[iHigh, jHigh, kLow, lHigh]), iFrac);
            RGBDbl rgb110 = Interp(new RGBDbl(samples[iLow, jHigh, kHigh, lLow]), new RGBDbl(samples[iHigh, jHigh, kHigh, lLow]), iFrac);
            RGBDbl rgb111 = Interp(new RGBDbl(samples[iLow, jHigh, kHigh, lHigh]), new RGBDbl(samples[iHigh, jHigh, kHigh, lHigh]), iFrac);

            RGBDbl rgb00 = Interp(rgb000, rgb100, jFrac);
            RGBDbl rgb01 = Interp(rgb001, rgb101, jFrac);
            RGBDbl rgb10 = Interp(rgb010, rgb110, jFrac);
            RGBDbl rgb11 = Interp(rgb011, rgb111, jFrac);

            RGBDbl rgb0 = Interp(rgb00, rgb10, kFrac);
            RGBDbl rgb1 = Interp(rgb01, rgb11, kFrac);

            RGBDbl rgb = Interp(rgb0, rgb1, lFrac);

            return new RGB(rgb);
        }



        public static SD.Color CmykToRgbColor(CmykColor cmykColor)
        {
            SD.Color result;

            if (cmykColor.Cyan == 0 && cmykColor.Magenta == 0 && cmykColor.Yellow == 0 && cmykColor.Black == 0) {
                // The default mapping doesn't quite map white to pure white.
                if (cmykColor.Alpha == 1)
                    return SD.Color.White;
                else
                    return SD.Color.FromArgb((byte) Math.Round(cmykColor.Alpha * 255), SD.Color.White);
            }

            if (cmykColor.Black == 1) {
                // The default mapping doesn't quite map black to pure black.
                if (cmykColor.Alpha == 1)
                    return SD.Color.Black;
                else
                    return SD.Color.FromArgb((byte)Math.Round(cmykColor.Alpha * 255), SD.Color.Black);
            }

            if (!cmykToColor.TryGetValue(cmykColor, out result)) {
                RGB rgb = ConvertUsingInterpolation(cmykColor.Cyan, cmykColor.Magenta, cmykColor.Yellow, cmykColor.Black);
                result = SD.Color.FromArgb((byte) Math.Round(cmykColor.Alpha * 255), rgb.R, rgb.G, rgb.B);

                lock (cmykToColor) {
                    cmykToColor[cmykColor] = result;
                }
            }

            return result;
        }

        public SD.Color ToColor(CmykColor cmykColor)
        {
            return CmykToRgbColor(cmykColor);
        }

        private struct RGB
        {
            public byte R, G, B;
            public RGB(float r, float g, float b)
            {
                this.R = (byte)Math.Round(r * 255);
                this.G = (byte)Math.Round(g * 255);
                this.B = (byte)Math.Round(b * 255);
            }

            public RGB(RGBDbl rgb)
            {
                this.R = (byte)Math.Round(rgb.R * 255);
                this.G = (byte)Math.Round(rgb.G * 255);
                this.B = (byte)Math.Round(rgb.B * 255);
            }
        }

        private struct RGBDbl
        {
            public double R, G, B;
            public RGBDbl(double r, double g, double b)
            {
                this.R = r;
                this.G = g;
                this.B = b;
            }

            public RGBDbl(RGB rgb)
            {
                this.R = rgb.R / 255.0;
                this.G = rgb.G / 255.0;
                this.B = rgb.B / 255.0;
            }
        }

    }
}
