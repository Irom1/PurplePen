using NUnit.Framework;
using PurplePen.Graphics2D;
using PurplePen.MapModel;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.IO;
using TestingUtils;
using SDBitmap = System.Drawing.Bitmap;
using SDImage = System.Drawing.Image;
using SDSize = System.Drawing.Size;

namespace Map_Skia.Tests
{

    [TestFixture]
    public class BitmapTests
    {

        [Test]
        public void DrawSkiaBitmap()
        {
            string expectedResult = TestUtil.GetTestFile("bitmaps\\DrawSkiaBitmap_baseline.png");

            const string baseBitmapName = "Water lilies.jpg";
            string baseBitmapPath = TestUtil.GetTestFile("bitmaps\\" + baseBitmapName);
            SDBitmap baseBitmap = (SDBitmap) SDImage.FromFile(baseBitmapPath);
            SDSize size = baseBitmap.Size;

            SKBitmap skBitmap = SKBitmap.Decode(baseBitmapPath);
            IGraphicsBitmap skiaBitmap = new Skia_Bitmap(skBitmap);

            Assert.AreEqual(size.Width, skiaBitmap.PixelWidth);
            Assert.AreEqual(size.Height, skiaBitmap.PixelHeight);


            RenderingUtil.RenderingTest(size.Width, new System.Drawing.RectangleF(0, 0, size.Width, size.Height), false, expectedResult,
                grTarget => {
                    grTarget.DrawBitmap(skiaBitmap, new System.Drawing.RectangleF(0, 0, size.Width, size.Height), BitmapScaling.HighQuality, 0.0001F);
                }
            );
        }

        [Test]
        public void DrawSkiaBitmapPart()
        {
            string expectedResult = TestUtil.GetTestFile("bitmaps\\DrawSkiaBitmapPart_baseline.png");

            const string baseBitmapName = "Water lilies.jpg";
            string baseBitmapPath = TestUtil.GetTestFile("bitmaps\\" + baseBitmapName);
            SDBitmap baseBitmap = (SDBitmap)SDImage.FromFile(baseBitmapPath);
            SDSize size = baseBitmap.Size;

            SKBitmap skBitmap = SKBitmap.Decode(baseBitmapPath);
            IGraphicsBitmap skiaBitmap = new Skia_Bitmap(skBitmap);

            RenderingUtil.RenderingTest(size.Width, new System.Drawing.RectangleF(0, 0, size.Width, size.Height), false, expectedResult,
                grTarget => {
                    grTarget.DrawBitmapPart(skiaBitmap, 
                                            100, 130, 400, 200,
                                            new System.Drawing.RectangleF(20, 10, 500, 400), 
                                            BitmapScaling.HighQuality, 0.0001F);
                }
            );
        }

        [Test]
        public void CropSkiaBitmap()
        {
            string expectedResult = TestUtil.GetTestFile("bitmaps\\CropSkiaBitmap_baseline.png");

            const string baseBitmapName = "Water lilies.jpg";
            string baseBitmapPath = TestUtil.GetTestFile("bitmaps\\" + baseBitmapName);
            SDBitmap baseBitmap = (SDBitmap)SDImage.FromFile(baseBitmapPath);
            SDSize size = baseBitmap.Size;

            SKBitmap skBitmap = SKBitmap.Decode(baseBitmapPath);
            IGraphicsBitmap skiaBitmap = new Skia_Bitmap(skBitmap);

            IGraphicsBitmap croppedBitmap = skiaBitmap.Crop(100, 130, 450, 250);
            Assert.AreEqual(450, croppedBitmap.PixelWidth);
            Assert.AreEqual(250, croppedBitmap.PixelHeight);

            RenderingUtil.RenderingTest(croppedBitmap.PixelWidth, new System.Drawing.RectangleF(0, 0, croppedBitmap.PixelWidth, croppedBitmap.PixelHeight), false, expectedResult,
                grTarget => {
                    grTarget.DrawBitmap(croppedBitmap,
                                        new System.Drawing.RectangleF(0, 0, croppedBitmap.PixelWidth, croppedBitmap.PixelHeight),
                                        BitmapScaling.HighQuality, 0.0001F);
                }
            );
        }

        [Test]
        public void WriteSkiaBitmapToStream()
        {
            string expectedResult = TestUtil.GetTestFile("bitmaps\\WriteSkiaBitmapToStream_baseline.png");

            const string baseBitmapName = "Water lilies.jpg";
            string baseBitmapPath = TestUtil.GetTestFile("bitmaps\\" + baseBitmapName);
            SDBitmap baseBitmap = (SDBitmap)SDImage.FromFile(baseBitmapPath);
            SDSize size = baseBitmap.Size;

            SKBitmap skBitmap = SKBitmap.Decode(baseBitmapPath);
            IGraphicsBitmap skiaBitmap = new Skia_Bitmap(skBitmap);

            MemoryStream memStream = new MemoryStream();
            skiaBitmap.WriteToStream(GraphicsBitmapFormat.PNG, memStream);
            memStream.Seek(0, SeekOrigin.Begin);

            SDBitmap loadedBitmap = (SDBitmap) SDImage.FromStream(memStream);

            TestUtil.CompareBitmapBaseline(loadedBitmap, expectedResult);
        }


        [Test]
        public void DrawSkiaImage()
        {
            string expectedResult = TestUtil.GetTestFile("bitmaps\\DrawSkiaBitmap_baseline.png");

            const string baseBitmapName = "Water lilies.jpg";
            string baseBitmapPath = TestUtil.GetTestFile("bitmaps\\" + baseBitmapName);
            SDBitmap baseBitmap = (SDBitmap)SDImage.FromFile(baseBitmapPath);
            SDSize size = baseBitmap.Size;

            SKBitmap skBitmap = SKBitmap.Decode(baseBitmapPath);
            SKImage skImage = SKImage.FromBitmap(skBitmap);
            IGraphicsBitmap skiaImage = new Skia_Image(skImage);

            Assert.AreEqual(size.Width, skiaImage.PixelWidth);
            Assert.AreEqual(size.Height, skiaImage.PixelHeight);


            RenderingUtil.RenderingTest(size.Width, new System.Drawing.RectangleF(0, 0, size.Width, size.Height), false, expectedResult,
                grTarget => {
                    grTarget.DrawBitmap(skiaImage, new System.Drawing.RectangleF(0, 0, size.Width, size.Height), BitmapScaling.HighQuality, 0.0001F);
                }
            );
        }

        [Test]
        public void DrawSkiaImagePart()
        {
            string expectedResult = TestUtil.GetTestFile("bitmaps\\DrawSkiaBitmapPart_baseline.png");

            const string baseBitmapName = "Water lilies.jpg";
            string baseBitmapPath = TestUtil.GetTestFile("bitmaps\\" + baseBitmapName);
            SDBitmap baseBitmap = (SDBitmap)SDImage.FromFile(baseBitmapPath);
            SDSize size = baseBitmap.Size;

            SKBitmap skBitmap = SKBitmap.Decode(baseBitmapPath);
            SKImage skImage = SKImage.FromBitmap(skBitmap);
            IGraphicsBitmap skiaImage = new Skia_Image(skImage);


            RenderingUtil.RenderingTest(size.Width, new System.Drawing.RectangleF(0, 0, size.Width, size.Height), false, expectedResult,
                grTarget => {
                    grTarget.DrawBitmapPart(skiaImage,
                                            100, 130, 400, 200,
                                            new System.Drawing.RectangleF(20, 10, 500, 400),
                                            BitmapScaling.HighQuality, 0.0001F);
                }
            );
        }

        [Test]
        public void CropSkiaImage()
        {
            string expectedResult = TestUtil.GetTestFile("bitmaps\\CropSkiaBitmap_baseline.png");

            const string baseBitmapName = "Water lilies.jpg";
            string baseBitmapPath = TestUtil.GetTestFile("bitmaps\\" + baseBitmapName);
            SDBitmap baseBitmap = (SDBitmap)SDImage.FromFile(baseBitmapPath);
            SDSize size = baseBitmap.Size;

            SKBitmap skBitmap = SKBitmap.Decode(baseBitmapPath);
            SKImage skImage = SKImage.FromBitmap(skBitmap);
            IGraphicsBitmap skiaImage = new Skia_Image(skImage);

            IGraphicsBitmap croppedImage = skiaImage.Crop(100, 130, 450, 250);
            Assert.AreEqual(450, croppedImage.PixelWidth);
            Assert.AreEqual(250, croppedImage.PixelHeight);

            RenderingUtil.RenderingTest(croppedImage.PixelWidth, new System.Drawing.RectangleF(0, 0, croppedImage.PixelWidth, croppedImage.PixelHeight), false, expectedResult,
                grTarget => {
                    grTarget.DrawBitmap(croppedImage,
                                        new System.Drawing.RectangleF(0, 0, croppedImage.PixelWidth, croppedImage.PixelHeight),
                                        BitmapScaling.HighQuality, 0.0001F);
                }
            );
        }

        [Test]
        public void WriteSkiaImageToStream()
        {
            string expectedResult = TestUtil.GetTestFile("bitmaps\\WriteSkiaBitmapToStream_baseline.png");

            const string baseBitmapName = "Water lilies.jpg";
            string baseBitmapPath = TestUtil.GetTestFile("bitmaps\\" + baseBitmapName);
            SDBitmap baseBitmap = (SDBitmap)SDImage.FromFile(baseBitmapPath);
            SDSize size = baseBitmap.Size;

            SKBitmap skBitmap = SKBitmap.Decode(baseBitmapPath);
            SKImage skImage = SKImage.FromBitmap(skBitmap);
            IGraphicsBitmap skiaImage = new Skia_Image(skImage);

            MemoryStream memStream = new MemoryStream();
            skiaImage.WriteToStream(GraphicsBitmapFormat.PNG, memStream);
            memStream.Seek(0, SeekOrigin.Begin);

            SDBitmap loadedBitmap = (SDBitmap)SDImage.FromStream(memStream);

            TestUtil.CompareBitmapBaseline(loadedBitmap, expectedResult);
        }


        [Test]
        public void DrawSkiaPixmap()
        {
            string expectedResult = TestUtil.GetTestFile("bitmaps\\DrawSkiaBitmap_baseline.png");

            const string baseBitmapName = "Water lilies.jpg";
            string baseBitmapPath = TestUtil.GetTestFile("bitmaps\\" + baseBitmapName);
            SDBitmap baseBitmap = (SDBitmap)SDImage.FromFile(baseBitmapPath);
            SDSize size = baseBitmap.Size;

            SKBitmap skBitmap = SKBitmap.Decode(baseBitmapPath);
            SKPixmap skPixmap = skBitmap.PeekPixels();
            IGraphicsBitmap skiaPixmap = new Skia_Pixmap(skPixmap);

            Assert.AreEqual(size.Width, skiaPixmap.PixelWidth);
            Assert.AreEqual(size.Height, skiaPixmap.PixelHeight);

            RenderingUtil.RenderingTest(size.Width, new System.Drawing.RectangleF(0, 0, size.Width, size.Height), false, expectedResult,
                grTarget => {
                    grTarget.DrawBitmap(skiaPixmap, new System.Drawing.RectangleF(0, 0, size.Width, size.Height), BitmapScaling.HighQuality, 0.0001F);
                }
            );
        }

        [Test]
        public void DrawSkiaPixmapPart()
        {
            string expectedResult = TestUtil.GetTestFile("bitmaps\\DrawSkiaBitmapPart_baseline.png");

            const string baseBitmapName = "Water lilies.jpg";
            string baseBitmapPath = TestUtil.GetTestFile("bitmaps\\" + baseBitmapName);
            SDBitmap baseBitmap = (SDBitmap)SDImage.FromFile(baseBitmapPath);
            SDSize size = baseBitmap.Size;

            SKBitmap skBitmap = SKBitmap.Decode(baseBitmapPath);
            SKPixmap skPixmap = skBitmap.PeekPixels();
            IGraphicsBitmap skiaPixmap = new Skia_Pixmap(skPixmap);


            RenderingUtil.RenderingTest(size.Width, new System.Drawing.RectangleF(0, 0, size.Width, size.Height), false, expectedResult,
                grTarget => {
                    grTarget.DrawBitmapPart(skiaPixmap,
                                            100, 130, 400, 200,
                                            new System.Drawing.RectangleF(20, 10, 500, 400),
                                            BitmapScaling.HighQuality, 0.0001F);
                }
            );
        }

        [Test]
        public void CropSkiaPixmap()
        {
            string expectedResult = TestUtil.GetTestFile("bitmaps\\CropSkiaBitmap_baseline.png");

            const string baseBitmapName = "Water lilies.jpg";
            string baseBitmapPath = TestUtil.GetTestFile("bitmaps\\" + baseBitmapName);
            SDBitmap baseBitmap = (SDBitmap)SDImage.FromFile(baseBitmapPath);
            SDSize size = baseBitmap.Size;

            SKBitmap skBitmap = SKBitmap.Decode(baseBitmapPath);
            SKPixmap skPixmap = skBitmap.PeekPixels();
            IGraphicsBitmap skiaPixmap = new Skia_Pixmap(skPixmap);

            IGraphicsBitmap croppedPixmap = skiaPixmap.Crop(100, 130, 450, 250);
            Assert.AreEqual(450, croppedPixmap.PixelWidth);
            Assert.AreEqual(250, croppedPixmap.PixelHeight);

            RenderingUtil.RenderingTest(croppedPixmap.PixelWidth, new System.Drawing.RectangleF(0, 0, croppedPixmap.PixelWidth, croppedPixmap.PixelHeight), false, expectedResult,
                grTarget => {
                    grTarget.DrawBitmap(croppedPixmap,
                                        new System.Drawing.RectangleF(0, 0, croppedPixmap.PixelWidth, croppedPixmap.PixelHeight),
                                        BitmapScaling.HighQuality, 0.0001F);
                }
            );
        }

        [Test]
        public void WriteSkiaPixmapToStream()
        {
            string expectedResult = TestUtil.GetTestFile("bitmaps\\WriteSkiaBitmapToStream_baseline.png");

            const string baseBitmapName = "Water lilies.jpg";
            string baseBitmapPath = TestUtil.GetTestFile("bitmaps\\" + baseBitmapName);
            SDBitmap baseBitmap = (SDBitmap)SDImage.FromFile(baseBitmapPath);
            SDSize size = baseBitmap.Size;

            SKBitmap skBitmap = SKBitmap.Decode(baseBitmapPath);
            SKPixmap skPixmap = skBitmap.PeekPixels();
            IGraphicsBitmap skiaPixmap = new Skia_Pixmap(skPixmap);

            MemoryStream memStream = new MemoryStream();
            skiaPixmap.WriteToStream(GraphicsBitmapFormat.PNG, memStream);
            memStream.Seek(0, SeekOrigin.Begin);

            SDBitmap loadedBitmap = (SDBitmap)SDImage.FromStream(memStream);

            TestUtil.CompareBitmapBaseline(loadedBitmap, expectedResult);
        }

        [Test]
        public void SkiaBitmapGraphicsLoader()
        {
            string expectedResult = TestUtil.GetTestFile("bitmaps\\SkiaBitmapLoader_baseline.png");

            const string baseBitmapName = "Water lilies.jpg";
            string baseBitmapPath = TestUtil.GetTestFile("bitmaps\\" + baseBitmapName);
            SDBitmap baseBitmap = (SDBitmap)SDImage.FromFile(baseBitmapPath);
            SDSize size = baseBitmap.Size;

            IGraphicsBitmap skiaBitmap;
            using (Stream stream = new FileStream(baseBitmapPath, FileMode.Open, FileAccess.Read)) {
                skiaBitmap = new SkiaBitmapGraphicsLoader().ReadBitmapFromStream(stream);
            }

            Assert.AreEqual(GraphicsBitmapFormat.JPEG, skiaBitmap.GetOriginalFormat());

            RenderingUtil.RenderingTest(size.Width, new System.Drawing.RectangleF(0, 0, size.Width, size.Height), false, expectedResult,
                grTarget => {
                    grTarget.DrawBitmap(skiaBitmap, new System.Drawing.RectangleF(0, 0, size.Width, size.Height), BitmapScaling.HighQuality, 0.0001F);
                }
            );
        }


    }
}
