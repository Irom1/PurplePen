using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Map_Skia.Tests
{
    using HarfBuzzSharp;
    using Map_SkiaStd;
    using PurplePen.Graphics2D;
    using PurplePen.MapModel;
    using SkiaSharp;
    using System.Diagnostics;
    using System.Drawing;

    [TestFixture]
	public class BasicTests
	{
		[Test]
		public void SkiaFontMetrics()
		{
			GDIPlus_TextMetrics gdiMetrics = new GDIPlus_TextMetrics();
			Skia_TextMetrics skiaMetrics = new Skia_TextMetrics();

            ITextFaceMetrics gdiFontMetrics = gdiMetrics.GetTextFaceMetrics("Times New Roman", 24, TextEffects.None);
            ITextFaceMetrics skiaFontMetrics = skiaMetrics.GetTextFaceMetrics("Times New Roman", 24, TextEffects.None);

            Assert.AreEqual(gdiFontMetrics.Ascent, skiaFontMetrics.Ascent);
            Assert.AreEqual(gdiFontMetrics.Descent, skiaFontMetrics.Descent);
            Assert.AreEqual(gdiFontMetrics.CapHeight, skiaFontMetrics.CapHeight, 0.02F);
            Assert.AreEqual(gdiFontMetrics.EmHeight, skiaFontMetrics.EmHeight);
            Assert.AreEqual(gdiFontMetrics.SpaceWidth, skiaFontMetrics.SpaceWidth);
            Assert.AreEqual(gdiFontMetrics.RecommendedLineSpacing, skiaFontMetrics.RecommendedLineSpacing);
            Assert.AreEqual(gdiFontMetrics.GetTextWidth("BananaPhone is great"), skiaFontMetrics.GetTextWidth("BananaPhone is great"), 0.05);

            SizeF gdiSize = gdiFontMetrics.GetTextSize("BananaPhone is great");
            SizeF skiaSize = skiaFontMetrics.GetTextSize("BananaPhone is great");
            Assert.AreEqual(gdiSize.Width, skiaSize.Width, 0.5F);
            Assert.AreEqual(gdiSize.Height, skiaSize.Height, 0.05F);

            ITextFaceMetrics tnrMetrics = skiaMetrics.GetTextFaceMetrics("Trebuchet MS", 50, TextEffects.None);
            Assert.AreEqual(50.0F, tnrMetrics.EmHeight, 0.1F);
            Assert.AreEqual(46.95F, tnrMetrics.Ascent, 0.1F);
            Assert.AreEqual(11.11F, tnrMetrics.Descent, 0.1F);
            Assert.AreEqual(36.25F, tnrMetrics.CapHeight, 0.5F);
            Assert.AreEqual(15.06F, tnrMetrics.SpaceWidth, 0.1F);
            Assert.AreEqual(58.05, tnrMetrics.RecommendedLineSpacing, 0.1F);
            Assert.AreEqual(305.93F, tnrMetrics.GetTextWidth("Hello, world  "), 0.1F);
            Assert.AreEqual(58.06F, tnrMetrics.GetTextSize("Hello, world").Height, 0.1F);

        }

        [Test]
        public void IsTextFaceInstalled()
        {
            Skia_TextMetrics skiaMetrics = new Skia_TextMetrics();

            Assert.IsTrue(skiaMetrics.TextFaceIsInstalled("Arial"));
            Assert.IsTrue(skiaMetrics.TextFaceIsInstalled("Times New Roman"));
            Assert.IsTrue(skiaMetrics.TextFaceIsInstalled("Arial Narrow"));
            Assert.IsTrue(skiaMetrics.TextFaceIsInstalled("Leelawadee UI"));
            Assert.IsTrue(skiaMetrics.TextFaceIsInstalled("Leelawadee UI Semilight"));
            Assert.IsTrue(skiaMetrics.TextFaceIsInstalled("Bahnschrift"));
            Assert.IsTrue(skiaMetrics.TextFaceIsInstalled("Bahnschrift SemiBold"));
            Assert.IsFalse(skiaMetrics.TextFaceIsInstalled("Bahnschrift Bold Banana"));
            Assert.IsFalse(skiaMetrics.TextFaceIsInstalled("Big Chicken"));
            Assert.IsFalse(skiaMetrics.TextFaceIsInstalled("Tekton"));
        }

        // Verifies that two Get() calls with the same family/style return the same instance.
        [Test]
        public void ShapedTypeface_Get_ReturnsSameInstance()
        {
            ShapedTypeface.ClearCache();

            ShapedTypeface a = ShapedTypeface.Get("Arial", SkiaSharp.SKFontStyleWeight.Normal, SkiaSharp.SKFontStyleWidth.Normal, SkiaSharp.SKFontStyleSlant.Upright);
            ShapedTypeface b = ShapedTypeface.Get("Arial", SkiaSharp.SKFontStyleWeight.Normal, SkiaSharp.SKFontStyleWidth.Normal, SkiaSharp.SKFontStyleSlant.Upright);

            Assert.AreSame(a, b);

            b.Dispose();
            a.Dispose();
            ShapedTypeface.ClearCache();
        }

        // Verifies that the cache key comparison is case-insensitive on family name.
        [Test]
        public void ShapedTypeface_Get_CaseInsensitive()
        {
            ShapedTypeface.ClearCache();

            ShapedTypeface lower = ShapedTypeface.Get("arial", SkiaSharp.SKFontStyleWeight.Normal, SkiaSharp.SKFontStyleWidth.Normal, SkiaSharp.SKFontStyleSlant.Upright);
            ShapedTypeface upper = ShapedTypeface.Get("ARIAL", SkiaSharp.SKFontStyleWeight.Normal, SkiaSharp.SKFontStyleWidth.Normal, SkiaSharp.SKFontStyleSlant.Upright);

            Assert.AreSame(lower, upper);

            upper.Dispose();
            lower.Dispose();
            ShapedTypeface.ClearCache();
        }

        // Verifies that different styles produce different cached instances.
        [Test]
        public void ShapedTypeface_Get_DifferentStylesAreDifferentInstances()
        {
            ShapedTypeface.ClearCache();

            ShapedTypeface normal = ShapedTypeface.Get("Arial", SkiaSharp.SKFontStyleWeight.Normal, SkiaSharp.SKFontStyleWidth.Normal, SkiaSharp.SKFontStyleSlant.Upright);
            ShapedTypeface bold = ShapedTypeface.Get("Arial", SkiaSharp.SKFontStyleWeight.Bold, SkiaSharp.SKFontStyleWidth.Normal, SkiaSharp.SKFontStyleSlant.Upright);

            Assert.AreNotSame(normal, bold);

            bold.Dispose();
            normal.Dispose();
            ShapedTypeface.ClearCache();
        }

        // Verifies that a cached entry survives Dispose() and can be reused by a subsequent Get().
        [Test]
        public void ShapedTypeface_CachedEntry_SurvivesDispose()
        {
            ShapedTypeface.ClearCache();

            ShapedTypeface first = ShapedTypeface.Get("Arial", SkiaSharp.SKFontStyleWeight.Normal, SkiaSharp.SKFontStyleWidth.Normal, SkiaSharp.SKFontStyleSlant.Upright);
            first.Dispose();

            // Entry is still in cache with refCount 0; Get() should return the same instance.
            ShapedTypeface second = ShapedTypeface.Get("Arial", SkiaSharp.SKFontStyleWeight.Normal, SkiaSharp.SKFontStyleWidth.Normal, SkiaSharp.SKFontStyleSlant.Upright);
            Assert.AreSame(first, second);

            // The typeface should still be usable (resources not disposed).
            Assert.IsTrue(second.HasGlyph('A'));

            second.Dispose();
            ShapedTypeface.ClearCache();
        }

        // Verifies that ClearCache() removes entries with refCount 0 but not entries still in use.
        [Test]
        public void ShapedTypeface_ClearCache_RemovesUnusedOnly()
        {
            ShapedTypeface.ClearCache();

            ShapedTypeface held = ShapedTypeface.Get("Arial", SkiaSharp.SKFontStyleWeight.Normal, SkiaSharp.SKFontStyleWidth.Normal, SkiaSharp.SKFontStyleSlant.Upright);
            ShapedTypeface released = ShapedTypeface.Get("Times New Roman", SkiaSharp.SKFontStyleWeight.Normal, SkiaSharp.SKFontStyleWidth.Normal, SkiaSharp.SKFontStyleSlant.Upright);
            released.Dispose(); // refCount drops to 0

            ShapedTypeface.ClearCache();

            // "held" should still be the same cached instance.
            ShapedTypeface heldAgain = ShapedTypeface.Get("Arial", SkiaSharp.SKFontStyleWeight.Normal, SkiaSharp.SKFontStyleWidth.Normal, SkiaSharp.SKFontStyleSlant.Upright);
            Assert.AreSame(held, heldAgain);

            // "Times New Roman" was cleared; a new Get() should create a new instance.
            ShapedTypeface newTnr = ShapedTypeface.Get("Times New Roman", SkiaSharp.SKFontStyleWeight.Normal, SkiaSharp.SKFontStyleWidth.Normal, SkiaSharp.SKFontStyleSlant.Upright);
            Assert.AreNotSame(released, newTnr);

            heldAgain.Dispose();
            held.Dispose();
            newTnr.Dispose();
            ShapedTypeface.ClearCache();
        }

        // Verifies that FromTypeface() creates a non-cached instance that disposes normally.
        [Test]
        public void ShapedTypeface_FromTypeface_NotCached()
        {
            ShapedTypeface.ClearCache();

            SkiaSharp.SKTypeface skTypeface = SkiaSharp.SKTypeface.FromFamilyName("Arial");
            ShapedTypeface fromTf = ShapedTypeface.FromTypeface(skTypeface);

            // Should be usable.
            Assert.IsTrue(fromTf.HasGlyph('A'));

            // Should not be the same as a cached instance.
            ShapedTypeface cached = ShapedTypeface.Get("Arial", SkiaSharp.SKFontStyleWeight.Normal, SkiaSharp.SKFontStyleWidth.Normal, SkiaSharp.SKFontStyleSlant.Upright);
            Assert.AreNotSame(fromTf, cached);

            fromTf.Dispose();
            cached.Dispose();
            ShapedTypeface.ClearCache();
        }
    }
}
