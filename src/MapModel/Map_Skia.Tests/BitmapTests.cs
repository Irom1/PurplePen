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


    [TestFixture]
    public class BitmapIOTests
    {
        // Helper: creates a simple test bitmap with known content.
        private SKBitmap CreateTestBitmap(int width, int height)
        {
            SKBitmap bitmap = new SKBitmap(width, height, SKImageInfo.PlatformColorType, SKAlphaType.Premul);
            using (SKCanvas canvas = new SKCanvas(bitmap)) {
                canvas.Clear(SKColors.Blue);
                using (SKPaint paint = new SKPaint()) {
                    paint.Color = SKColors.Red;
                    canvas.DrawRect(10, 10, width / 2, height / 2, paint);
                }
            }
            return bitmap;
        }

        [Test]
        public void ReadBitmapFromJpegStream()
        {
            // Read a JPEG file and verify format, dimensions, and pixel format.
            string jpegPath = TestUtil.GetTestFile("bitmaps\\Water lilies.jpg");

            BitmapWithResolution result;
            using (Stream stream = new FileStream(jpegPath, FileMode.Open, FileAccess.Read)) {
                result = BitmapIO.ReadBitmapFromStream(stream);
            }

            Assert.AreEqual(GraphicsBitmapFormat.JPEG, result.Format);
            Assert.Greater(result.Bitmap.Width, 0);
            Assert.Greater(result.Bitmap.Height, 0);
            Assert.AreEqual(SKImageInfo.PlatformColorType, result.Bitmap.ColorType);
            Assert.AreEqual(SKAlphaType.Premul, result.Bitmap.AlphaType);

            result.Bitmap.Dispose();
        }

        [Test]
        public void ReadBitmapResolution()
        {
            // Read a JPEG and verify that resolution values are reasonable.
            string jpegPath = TestUtil.GetTestFile("bitmaps\\Water lilies.jpg");

            BitmapWithResolution result;
            using (Stream stream = new FileStream(jpegPath, FileMode.Open, FileAccess.Read)) {
                result = BitmapIO.ReadBitmapFromStream(stream);
            }

            // JPEG files typically have 72 or 96 DPI.
            Assert.Greater(result.HorizontalResolution, 0);
            Assert.Greater(result.VerticalResolution, 0);
            Assert.LessOrEqual(result.HorizontalResolution, 1200);
            Assert.LessOrEqual(result.VerticalResolution, 1200);

            result.Bitmap.Dispose();
        }

        [Test]
        public void ReadBitmapFromPngStream()
        {
            // Write a PNG via BitmapIO, then read it back and verify format.
            SKBitmap bitmap = CreateTestBitmap(100, 80);
            BitmapWithResolution bwr = new BitmapWithResolution(bitmap, GraphicsBitmapFormat.PNG, 96, 96);

            MemoryStream ms = new MemoryStream();
            BitmapIO.WriteBitmapToStream(bwr, ms, 100);
            ms.Position = 0;

            BitmapWithResolution result = BitmapIO.ReadBitmapFromStream(ms);

            Assert.AreEqual(GraphicsBitmapFormat.PNG, result.Format);
            Assert.AreEqual(100, result.Bitmap.Width);
            Assert.AreEqual(80, result.Bitmap.Height);
            Assert.AreEqual(SKImageInfo.PlatformColorType, result.Bitmap.ColorType);
            Assert.AreEqual(SKAlphaType.Premul, result.Bitmap.AlphaType);

            result.Bitmap.Dispose();
            bitmap.Dispose();
        }

        [Test]
        public void WriteBitmapToPngStream()
        {
            // Write a bitmap as PNG, read it back with System.Drawing, verify it is valid.
            SKBitmap bitmap = CreateTestBitmap(120, 90);
            BitmapWithResolution bwr = new BitmapWithResolution(bitmap, GraphicsBitmapFormat.PNG, 150, 150);

            MemoryStream ms = new MemoryStream();
            BitmapIO.WriteBitmapToStream(bwr, ms, 100);
            ms.Position = 0;

            SDBitmap loadedBitmap = (SDBitmap)SDImage.FromStream(ms);
            Assert.AreEqual(120, loadedBitmap.Width);
            Assert.AreEqual(90, loadedBitmap.Height);

            loadedBitmap.Dispose();
            bitmap.Dispose();
        }

        [Test]
        public void WriteBitmapToJpegStream()
        {
            // Write a bitmap as JPEG, read it back, verify it is valid.
            SKBitmap bitmap = CreateTestBitmap(120, 90);
            BitmapWithResolution bwr = new BitmapWithResolution(bitmap, GraphicsBitmapFormat.JPEG, 200, 200);

            MemoryStream ms = new MemoryStream();
            BitmapIO.WriteBitmapToStream(bwr, ms, 85);
            ms.Position = 0;

            SDBitmap loadedBitmap = (SDBitmap)SDImage.FromStream(ms);
            Assert.AreEqual(120, loadedBitmap.Width);
            Assert.AreEqual(90, loadedBitmap.Height);

            loadedBitmap.Dispose();
            bitmap.Dispose();
        }

        [Test]
        public void WriteBitmapToGifStream()
        {
            // Writing GIF is a key use case for BitmapIO (Skia can't do this).
            SKBitmap bitmap = CreateTestBitmap(120, 90);
            BitmapWithResolution bwr = new BitmapWithResolution(bitmap, GraphicsBitmapFormat.GIF, 96, 96);

            MemoryStream ms = new MemoryStream();
            BitmapIO.WriteBitmapToStream(bwr, ms, 100);
            ms.Position = 0;

            // Verify the stream contains a valid GIF by reading it back.
            SDBitmap loadedBitmap = (SDBitmap)SDImage.FromStream(ms);
            Assert.AreEqual(120, loadedBitmap.Width);
            Assert.AreEqual(90, loadedBitmap.Height);

            // Also verify BitmapIO reads it back as GIF format.
            ms.Position = 0;
            BitmapWithResolution readBack = BitmapIO.ReadBitmapFromStream(ms);
            Assert.AreEqual(GraphicsBitmapFormat.GIF, readBack.Format);

            readBack.Bitmap.Dispose();
            loadedBitmap.Dispose();
            bitmap.Dispose();
        }

        [Test]
        public void WriteBitmapToTiffStream()
        {
            // TIFF is another format Skia doesn't handle well.
            SKBitmap bitmap = CreateTestBitmap(100, 80);
            BitmapWithResolution bwr = new BitmapWithResolution(bitmap, GraphicsBitmapFormat.TIFF, 300, 300);

            MemoryStream ms = new MemoryStream();
            BitmapIO.WriteBitmapToStream(bwr, ms, 100);
            ms.Position = 0;

            // Read back and verify format.
            BitmapWithResolution readBack = BitmapIO.ReadBitmapFromStream(ms);
            Assert.AreEqual(GraphicsBitmapFormat.TIFF, readBack.Format);
            Assert.AreEqual(100, readBack.Bitmap.Width);
            Assert.AreEqual(80, readBack.Bitmap.Height);

            readBack.Bitmap.Dispose();
            bitmap.Dispose();
        }

        [Test]
        public void ResolutionPreservedPng()
        {
            // Write a PNG with specific DPI, read it back, verify the DPI is preserved.
            SKBitmap bitmap = CreateTestBitmap(100, 80);
            BitmapWithResolution bwr = new BitmapWithResolution(bitmap, GraphicsBitmapFormat.PNG, 300, 250);

            MemoryStream ms = new MemoryStream();
            BitmapIO.WriteBitmapToStream(bwr, ms, 100);
            ms.Position = 0;

            BitmapWithResolution readBack = BitmapIO.ReadBitmapFromStream(ms);
            Assert.AreEqual(300, readBack.HorizontalResolution, 0.5);
            Assert.AreEqual(250, readBack.VerticalResolution, 0.5);

            readBack.Bitmap.Dispose();
            bitmap.Dispose();
        }

        [Test]
        public void ResolutionPreservedJpeg()
        {
            // Write a JPEG with specific DPI, read it back, verify the DPI is preserved.
            SKBitmap bitmap = CreateTestBitmap(100, 80);
            BitmapWithResolution bwr = new BitmapWithResolution(bitmap, GraphicsBitmapFormat.JPEG, 150, 200);

            MemoryStream ms = new MemoryStream();
            BitmapIO.WriteBitmapToStream(bwr, ms, 90);
            ms.Position = 0;

            BitmapWithResolution readBack = BitmapIO.ReadBitmapFromStream(ms);
            Assert.AreEqual(150, readBack.HorizontalResolution, 0.5);
            Assert.AreEqual(200, readBack.VerticalResolution, 0.5);

            readBack.Bitmap.Dispose();
            bitmap.Dispose();
        }

        [Test]
        public void WritePixmapToStream()
        {
            // Write a pixmap as PNG using WritePixmapToStream, read it back, verify.
            SKBitmap bitmap = CreateTestBitmap(100, 80);
            using (SKPixmap pixmap = bitmap.PeekPixels()) {
                PixmapWithResolution pwr = new PixmapWithResolution(pixmap, GraphicsBitmapFormat.PNG, 200, 200);

                MemoryStream ms = new MemoryStream();
                BitmapIO.WritePixmapToStream(pwr, ms, 100);
                ms.Position = 0;

                BitmapWithResolution readBack = BitmapIO.ReadBitmapFromStream(ms);
                Assert.AreEqual(GraphicsBitmapFormat.PNG, readBack.Format);
                Assert.AreEqual(100, readBack.Bitmap.Width);
                Assert.AreEqual(80, readBack.Bitmap.Height);
                Assert.AreEqual(200, readBack.HorizontalResolution, 0.5);
                Assert.AreEqual(200, readBack.VerticalResolution, 0.5);

                readBack.Bitmap.Dispose();
            }
            bitmap.Dispose();
        }

        [Test]
        public void WritePixmapToGifStream()
        {
            // Write a pixmap as GIF, verify it works.
            SKBitmap bitmap = CreateTestBitmap(80, 60);
            using (SKPixmap pixmap = bitmap.PeekPixels()) {
                PixmapWithResolution pwr = new PixmapWithResolution(pixmap, GraphicsBitmapFormat.GIF, 96, 96);

                MemoryStream ms = new MemoryStream();
                BitmapIO.WritePixmapToStream(pwr, ms, 100);
                ms.Position = 0;

                BitmapWithResolution readBack = BitmapIO.ReadBitmapFromStream(ms);
                Assert.AreEqual(GraphicsBitmapFormat.GIF, readBack.Format);
                Assert.AreEqual(80, readBack.Bitmap.Width);
                Assert.AreEqual(60, readBack.Bitmap.Height);

                readBack.Bitmap.Dispose();
            }
            bitmap.Dispose();
        }

        [Test]
        public void ReadBitmapFromNonSeekableStream()
        {
            // BitmapIO.ReadBitmapFromStream should handle non-seekable streams.
            string jpegPath = TestUtil.GetTestFile("bitmaps\\Water lilies.jpg");
            byte[] data = File.ReadAllBytes(jpegPath);

            using (NonSeekableStream ns = new NonSeekableStream(new MemoryStream(data))) {
                BitmapWithResolution result = BitmapIO.ReadBitmapFromStream(ns);

                Assert.AreEqual(GraphicsBitmapFormat.JPEG, result.Format);
                Assert.Greater(result.Bitmap.Width, 0);
                Assert.Greater(result.Bitmap.Height, 0);

                result.Bitmap.Dispose();
            }
        }

        [Test]
        public void HandlesDifferentBitmapColorTypes()
        {
            // WriteBitmapToStream should handle a bitmap with a non-platform color type.
            SKBitmap bitmap = new SKBitmap(50, 40, SKColorType.Rgba8888, SKAlphaType.Unpremul);
            using (SKCanvas canvas = new SKCanvas(bitmap)) {
                canvas.Clear(SKColors.Green);
            }

            BitmapWithResolution bwr = new BitmapWithResolution(bitmap, GraphicsBitmapFormat.PNG, 96, 96);

            MemoryStream ms = new MemoryStream();
            BitmapIO.WriteBitmapToStream(bwr, ms, 100);
            ms.Position = 0;

            BitmapWithResolution readBack = BitmapIO.ReadBitmapFromStream(ms);
            Assert.AreEqual(50, readBack.Bitmap.Width);
            Assert.AreEqual(40, readBack.Bitmap.Height);

            readBack.Bitmap.Dispose();
            bitmap.Dispose();
        }

        /// <summary>
        /// A stream wrapper that does not support seeking, for testing non-seekable stream handling.
        /// </summary>
        private class NonSeekableStream : Stream
        {
            private Stream inner;

            public NonSeekableStream(Stream inner) { this.inner = inner; }

            public override bool CanRead => inner.CanRead;
            public override bool CanSeek => false;
            public override bool CanWrite => false;
            public override long Length => throw new NotSupportedException();
            public override long Position {
                get => throw new NotSupportedException();
                set => throw new NotSupportedException();
            }
            public override void Flush() { inner.Flush(); }
            public override int Read(byte[] buffer, int offset, int count) { return inner.Read(buffer, offset, count); }
            public override long Seek(long offset, SeekOrigin origin) { throw new NotSupportedException(); }
            public override void SetLength(long value) { throw new NotSupportedException(); }
            public override void Write(byte[] buffer, int offset, int count) { throw new NotSupportedException(); }

            protected override void Dispose(bool disposing)
            {
                if (disposing)
                    inner.Dispose();
                base.Dispose(disposing);
            }
        }
    }
}
