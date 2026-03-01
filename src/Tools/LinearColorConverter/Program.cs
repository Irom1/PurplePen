using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace LinearColorConverter
{
    class Program
    {
        static ColorModel colorModel = new ColorModel();

        static void Main(string[] args)
        {
            Console.WriteLine("Populating...");

            colorModel.Populate();

            Test(0.0F, 0.135F, 0.395F, 0.0F);


            Test(0, 0, 0, 0);
            Test(1, 1, 1, 1);
            Test(1, 0, 0, 0);
            Test(0, 1, 0, 0);
            Test(0, 0, 1, 0);
            Test(0, 0, 0, 1);
            Console.WriteLine();
            Test(0.0F, 0.4F, 0.6F, 0.8F);
            Test(0.05F, 0.4F, 0.6F, 0.8F);
            Test(0.1F, 0.4F, 0.6F, 0.8F);
            Test(0.15F, 0.4F, 0.6F, 0.8F);
            Test(0.2F, 0.4F, 0.6F, 0.8F);
            Console.WriteLine();
            Test(0.2F, 0.4F, 0.6F, 0.8F);
            Test(0.2F, 0.45F, 0.6F, 0.8F);
            Test(0.2F, 0.5F, 0.6F, 0.8F);
            Test(0.2F, 0.55F, 0.6F, 0.8F);
            Test(0.2F, 0.6F, 0.6F, 0.8F);
            Console.WriteLine();
            Test(0.2F, 0.4F, 0.6F, 0.8F);
            Test(0.2F, 0.4F, 0.65F, 0.8F);
            Test(0.2F, 0.4F, 0.7F, 0.8F);
            Test(0.2F, 0.4F, 0.75F, 0.8F);
            Test(0.2F, 0.4F, 0.8F, 0.8F);
            Console.WriteLine();
            Test(0.2F, 0.4F, 0.6F, 0.8F);
            Test(0.2F, 0.4F, 0.6F, 0.85F);
            Test(0.2F, 0.4F, 0.6F, 0.9F);
            Test(0.2F, 0.4F, 0.6F, 0.95F);
            Test(0.2F, 0.4F, 0.6F, 1.0F);
            
            
            double maxDiff = 0;
            float maxC = 0, maxM = 0, maxY = 0, maxK = 0;

            Console.WriteLine("Testing...");

            for (int i = 0; i < 100000; ++i) {
                if (i % 1000 == 0) {
                    Console.Write($"{i}... ");
                }
                Random rand = new Random();
                float c = (float)rand.NextDouble();
                float m = (float)rand.NextDouble();
                float y = (float)rand.NextDouble();
                float k = (float)rand.NextDouble();
                RGB correct = colorModel.ConvertUsingICC(c, y, m, k);
                RGB interp = colorModel.ConvertUsingInterpolation(c, y, m, k);
                double diff = Diff(correct, interp);

                if (diff > maxDiff) {
                    maxC = c; maxM = m; maxY = y; maxK = k;
                }
            }

            Console.WriteLine();
            Test(maxC, maxM, maxY, maxK);
            

            colorModel.OutputSamplesCode("CmykConverterSamples.cs");
            colorModel.OutputSamplesData("swopsamples.dat");
        }

        static void Test(float c, float y, float m, float k)
        {
            RGB correct = colorModel.ConvertUsingICC(c, y, m, k);
            RGB interp = colorModel.ConvertUsingInterpolation(c, y, m, k);

            Console.WriteLine($"Converting [C={c:F3},Y={y:F3},M={m:F3},K={k:F3}]: Correct is (R:{correct.R},G:{correct.G},B:{correct.B})  Interpolated is (R:{interp.R},G:{interp.G},B:{interp.B})");
        }

        static double Diff(RGB rgb1, RGB rgb2)
        {
            return Math.Sqrt(((rgb1.B - rgb2.B) * (rgb1.B - rgb2.B)) + ((rgb1.G - rgb2.G) * (rgb1.G - rgb2.G)) + ((rgb1.R - rgb2.R) * (rgb1.R - rgb2.R)));
        }
    }

    struct RGB
    {
        public byte R, G, B;
        public RGB(byte r, byte g, byte b)
        {
            this.R = r;
            this.G = g;
            this.B = b;
        }

        public RGB(RGBDbl rgb)
        {
            this.R = (byte)Math.Round(rgb.R * 255);
            this.G = (byte)Math.Round(rgb.G * 255);
            this.B = (byte)Math.Round(rgb.B * 255);
        }
    }

    struct RGBDbl
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

    class ColorModel
    {
        public const int SAMPLESIZE = 12;
        public RGB[,,,] samples = new RGB[SAMPLESIZE, SAMPLESIZE, SAMPLESIZE, SAMPLESIZE];

        Uri SwopUri;

        public void Populate()
        {
            string swopFileName = GetFileInAppDirectory("USWebCoatedSWOP.icc");
            SwopUri = new Uri(swopFileName);
            float delta = 1.0F / (SAMPLESIZE - 1);

            for (int i = 0; i < SAMPLESIZE; ++i) {
                float cyan = i * delta;
                for (int j = 0; j < SAMPLESIZE; ++j) {
                    Console.WriteLine($"{i},{j}");
                    float mag = j * delta;
                    for (int k = 0; k < SAMPLESIZE; ++k) {
                        float yel = k * delta;
                        for (int l = 0; l < SAMPLESIZE; ++l) {
                            float blk = l * delta;

                            samples[i, j, k, l] = ConvertUsingICC(cyan, mag, yel, blk);
                        }
                    }
                }
            }
        }

        public RGB ConvertUsingICC(float c, float m, float y, float k)
        {
            float[] colorValues = { c, m, y, k };
            Color color;
            if (c < 0.001F && m < 0.001F && y < 0.001F && k < 0.001F)
                return new RGB(255, 255, 255);
            else if (k > 0.999F)
                return new RGB(0, 0, 0);
            else {
                color = Color.FromValues(colorValues, SwopUri);

                return new RGB(color.R, color.G, color.B);
            }
        }

        private static RGBDbl Interp(RGBDbl rgbLow, RGBDbl rgbHigh, float frac)
        {
            return new RGBDbl(rgbLow.R * (1 - frac) + rgbHigh.R * frac,
                rgbLow.G * (1 - frac) + rgbHigh.G * frac,
                rgbLow.B * (1 - frac) + rgbHigh.B * frac);
        }


        public RGB ConvertUsingInterpolation(float c, float m, float y, float k)
        {
            // https://en.wikipedia.org/wiki/Trilinear_interpolation

            int iLow = (int) Math.Floor(c * (SAMPLESIZE - 1));
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

        public void OutputSamplesCode(string filename)
        {
            using (TextWriter writer = new StreamWriter(filename)) {
                writer.WriteLine("// Lookup values for linear interpolation of CMYK -> RGB conversion.");
                writer.WriteLine("// Automatically created by the LinearColorConverter tool at {0}", DateTime.Now);
                writer.WriteLine();
                writer.WriteLine("namespace PurplePen {");
                writer.WriteLine("    partial class CmykConverter {");
                writer.WriteLine("        private byte[,,,,] samples = {");

                for (int i = 0; i < SAMPLESIZE; ++i) {
                    writer.WriteLine("            {");
                    for (int j = 0; j < SAMPLESIZE; ++j) {
                        writer.WriteLine("                {");
                        for (int k = 0; k < SAMPLESIZE; ++k) {
                            writer.Write("                    {");
                            for (int l = 0; l < SAMPLESIZE; ++l) {
                                writer.Write("{{{0},{1},{2}}}, ", samples[i, j, k, l].R, samples[i, j, k, l].G, samples[i, j, k, l].B);
                            }
                            writer.WriteLine("},");
                        }
                        writer.WriteLine("                },");
                    }
                    writer.WriteLine("            },");
                }

                writer.WriteLine("        };");
                writer.WriteLine("    }");
                writer.WriteLine("}");
            }
        }

        public void OutputSamplesData(string filename)
        {
            using (FileStream fs = new FileStream(filename, FileMode.Create, FileAccess.Write)) {
                for (int i = 0; i < SAMPLESIZE; ++i) {
                    for (int j = 0; j < SAMPLESIZE; ++j) {
                        for (int k = 0; k < SAMPLESIZE; ++k) {
                            for (int l = 0; l < SAMPLESIZE; ++l) {
                                fs.WriteByte(samples[i, j, k, l].R);
                                fs.WriteByte(samples[i, j, k, l].G);
                                fs.WriteByte(samples[i, j, k, l].B);
                            }
                        }
                    }
                }
            }
        }

        private string GetFileInAppDirectory(string filename)
        {
            // Using Application.StartupPath would be
            // simpler and probably faster, but doesn't work with NUnit.
            string codebase = this.GetType().Assembly.CodeBase;
            Uri uri = new Uri(codebase);
            string appPath = Path.GetDirectoryName(uri.LocalPath);

            // Create the core objects needed for the application to run.
            return Path.Combine(appPath, filename);
        }
    }
}
