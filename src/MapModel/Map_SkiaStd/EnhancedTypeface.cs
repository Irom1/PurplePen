using SkiaSharp;
using SkiaSharp.HarfBuzz;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;

namespace Map_SkiaStd
{
    // Wraps an SKTypeface together with its associated HarfBuzz font objects,
    // providing glyph existence checks and text shaping capabilities.
    //
    // The HarfBuzz pipeline is: SKTypeface -> SKStreamAsset -> Blob -> Face -> Font.
    // These objects are created together and must be disposed together.
    //
    // Instances obtained via Get() are cached by font family/style and reference-counted.
    // Dispose() decrements the reference count but does not release resources; cached
    // entries remain available for reuse. Call ClearCache() to dispose entries whose
    // reference count has reached zero.
    //
    // Instances obtained via FromTypeface() are not cached and dispose normally when
    // their reference count reaches zero.
    public class ShapedTypeface : IDisposable
    {
        public readonly SKTypeface Typeface;
        public readonly SKFont CheckFont;           // Used only for glyph existence checks
        public readonly HarfBuzzSharp.Blob HBBlob;
        public readonly HarfBuzzSharp.Face HBFace;
        public readonly HarfBuzzSharp.Font HBFont;
        private readonly SKStreamAsset fontStream;  // Must stay alive; HBBlob references its memory

        // Cache key: family name (upper-cased for case-insensitive comparison), weight, width, slant.
        private static readonly ConcurrentDictionary<(string, SKFontStyleWeight, SKFontStyleWidth, SKFontStyleSlant), ShapedTypeface> cache
            = new ConcurrentDictionary<(string, SKFontStyleWeight, SKFontStyleWidth, SKFontStyleSlant), ShapedTypeface>();

        private int refCount;
        private readonly bool isCached;

        // Private constructor that builds the HarfBuzz pipeline from family name and style.
        //
        // Parameters:
        //   familyName - the font family name (e.g., "Segoe UI", "Arial")
        //   weight - font weight (e.g., SKFontStyleWeight.Normal, SKFontStyleWeight.Bold)
        //   width - font width/stretch (e.g., SKFontStyleWidth.Normal)
        //   slant - font slant (e.g., SKFontStyleSlant.Upright, SKFontStyleSlant.Italic)
        //   cached - true if this instance is managed by the static cache
        private ShapedTypeface(string familyName,
                              SKFontStyleWeight weight,
                              SKFontStyleWidth width,
                              SKFontStyleSlant slant,
                              bool cached)
        {
            Typeface = SKTypeface.FromFamilyName(familyName, weight, width, slant);

            // SKFont is used solely for checking glyph availability via GetGlyph().
            // The size doesn't matter for glyph existence checks.
            CheckFont = new SKFont(Typeface);

            // Build the HarfBuzz font from the typeface's raw font data.
            // OpenStream() gives us the raw TrueType/OpenType data as an SKStreamAsset.
            // ToHarfBuzzBlob() wraps the stream's memory (does not copy), so the stream
            // must remain alive for the lifetime of this instance.
            fontStream = Typeface.OpenStream();
            HBBlob = fontStream.ToHarfBuzzBlob();
            HBFace = new HarfBuzzSharp.Face(HBBlob, 0);
            HBFace.UnitsPerEm = Typeface.UnitsPerEm;
            HBFont = new HarfBuzzSharp.Font(HBFace);

            // Set the font scale to design units. HarfBuzz will return glyph positions
            // in these units; we scale to display coordinates later using
            // (fontSize / unitsPerEm).
            HBFont.SetScale(HBFace.UnitsPerEm, HBFace.UnitsPerEm);

            // Cached entries start at 0; each Get() call increments.
            // Non-cached entries are never created through this path.
            refCount = 0;
            isCached = cached;
        }

        // Private constructor that builds the HarfBuzz pipeline from an existing SKTypeface.
        // This constructor is used for the non-cached path. Takes ownership of the typeface.
        private ShapedTypeface(SKTypeface typeface)
        {
            Typeface = typeface;

            CheckFont = new SKFont(Typeface);

            fontStream = Typeface.OpenStream();
            HBBlob = fontStream.ToHarfBuzzBlob();
            HBFace = new HarfBuzzSharp.Face(HBBlob, 0);
            HBFace.UnitsPerEm = Typeface.UnitsPerEm;
            HBFont = new HarfBuzzSharp.Font(HBFace);

            HBFont.SetScale(HBFace.UnitsPerEm, HBFace.UnitsPerEm);

            refCount = 1;
            isCached = false;
        }

        // Returns a cached ShapedTypeface for the given family name and style. If one already
        // exists in the cache, its reference count is incremented and the same instance is
        // returned. Otherwise a new instance is created with reference count 1.
        //
        // Thread-safe: uses ConcurrentDictionary.GetOrAdd and Interlocked.Increment.
        public static ShapedTypeface Get(string familyName,
                                         SKFontStyleWeight weight,
                                         SKFontStyleWidth width,
                                         SKFontStyleSlant slant)
        {
            (string, SKFontStyleWeight, SKFontStyleWidth, SKFontStyleSlant) key =
                (familyName.ToUpperInvariant(), weight, width, slant);

            ShapedTypeface entry = cache.GetOrAdd(key,
                k => new ShapedTypeface(familyName, weight, width, slant, cached: true));

            // Cached entries are created with refCount=0. Each Get() caller increments,
            // so refCount always equals the number of active callers holding a reference.
            Interlocked.Increment(ref entry.refCount);
            return entry;
        }

        // Creates a non-cached ShapedTypeface from an existing SKTypeface. Takes ownership
        // of the typeface. The instance is not stored in the cache and disposes its resources
        // normally when the reference count reaches zero.
        public static ShapedTypeface FromTypeface(SKTypeface typeface)
        {
            return new ShapedTypeface(typeface);
        }

        // Returns true if this typeface contains a glyph for the given Unicode codepoint.
        // A return value of false means the font would render the .notdef glyph (tofu).
        public bool HasGlyph(int codepoint)
        {
            return CheckFont.GetGlyph(codepoint) != 0;
        }

        // Decrements the reference count. For cached entries, resources are not released
        // (the entry stays in the cache for reuse). For non-cached entries, resources are
        // disposed when the count reaches zero.
        public void Dispose()
        {
            int newCount = Interlocked.Decrement(ref refCount);

            if (!isCached && newCount <= 0)
            {
                DisposeResources();
            }
        }

        // Releases all native resources held by this instance.
        private void DisposeResources()
        {
            CheckFont?.Dispose();
            HBFont?.Dispose();
            HBFace?.Dispose();
            HBBlob?.Dispose();
            fontStream?.Dispose();
            Typeface?.Dispose();
        }

        // Disposes and removes all cached entries whose reference count is zero or less
        // (no active callers). Entries still held by callers are left untouched.
        //
        // Thread-safe: iterates a snapshot of cache keys and uses TryRemove. A small race
        // window exists where an entry could be re-requested between the count check and
        // removal, but GetOrAdd handles this by creating a new entry if needed.
        public static void ClearCache()
        {
            foreach ((string, SKFontStyleWeight, SKFontStyleWidth, SKFontStyleSlant) key in cache.Keys)
            {
                if (cache.TryGetValue(key, out ShapedTypeface entry))
                {
                    if (entry.refCount <= 0)
                    {
                        if (cache.TryRemove(key, out ShapedTypeface removed))
                        {
                            removed.DisposeResources();
                        }
                    }
                }
            }
        }
    }

    // This class handles Skia rendering of text, using HarfBuzz for shaping,
    // and handling font fallbacks for missing glyphs.
    //
    // Text rendering with proper international support requires two key capabilities:
    //
    // 1. Text shaping (via HarfBuzz): Converts Unicode text into positioned glyphs,
    //    handling ligatures, kerning, and complex script rules (e.g., Arabic joining,
    //    Devanagari conjuncts). Without shaping, text in complex scripts renders
    //    incorrectly or illegibly.
    //
    // 2. Font fallback: When the primary font doesn't contain a glyph for a character
    //    (e.g., emoji, CJK characters), the text is split into runs and each run is
    //    rendered with the first available font that supports those characters.
    //
    // The overall pipeline is:
    //   Input text
    //     -> Segment by typeface coverage (which font has glyphs for which characters)
    //     -> Shape each segment independently with HarfBuzz
    //     -> Accumulate glyph positions across segments
    //     -> Draw all segments as a single SKTextBlob with multiple font runs
    //
    // This class does not own the ShapedTypeface instances passed to it; the caller
    // retains ownership and is responsible for disposing them.
    public class EnhancedTypeface
    {
        // A contiguous range of text that should be shaped with a specific typeface.
        // Used internally to represent the result of font fallback segmentation.
        private struct TextRun
        {
            public ShapedTypeface Entry;
            public int Start;   // Start index in the original text (UTF-16 char index)
            public int Length;  // Length in UTF-16 chars
        }

        // The result of shaping a single text run with HarfBuzz.
        private struct ShapedRunResult
        {
            public ShapedTypeface Entry;
            public ushort[] GlyphIds;    // Glyph indices within the typeface
            public SKPoint[] Positions;  // Glyph positions, scaled to fontSize, with X accumulated from prior runs
            public uint[] Clusters;      // Cluster values adjusted to indices in the full original text
            public float Width;          // Total advance width of this run
            public int TextStart;        // Start index of this run's text in the original string
            public int TextLength;       // Length of this run's text in UTF-16 chars
        }

        private ShapedTypeface mainEntry;
        private ShapedTypeface[] fallbackEntries;
        private HarfBuzzSharp.Feature[] features;

        // Create an EnhancedTypeface with the main ShapedTypeface, fallback ShapedTypefaces
        // for missing glyphs, and HarfBuzz properties for shaping.
        //
        // The harfBuzzProperties dictionary maps OpenType feature tags (4-character strings
        // like "kern", "liga", "calt") to integer values (typically 1 to enable, 0 to disable).
        // These are passed to HarfBuzz during shaping to control typographic features.
        //
        // This class does not take ownership of the ShapedTypeface instances; the caller
        // must keep them alive for the lifetime of this EnhancedTypeface and dispose them
        // separately.
        public EnhancedTypeface(ShapedTypeface mainTypeface,
                              ShapedTypeface[] fallbackTypefaces,
                              IDictionary<string, int> harfBuzzProperties)
        {
            mainEntry = mainTypeface;
            fallbackEntries = fallbackTypefaces ?? new ShapedTypeface[0];

            // Convert the properties dictionary to HarfBuzz Feature objects.
            List<HarfBuzzSharp.Feature> featureList = new List<HarfBuzzSharp.Feature>();
            if (harfBuzzProperties != null)
            {
                foreach (KeyValuePair<string, int> kvp in harfBuzzProperties)
                {
                    string tag = kvp.Key;
                    if (tag.Length >= 4)
                    {
                        featureList.Add(new HarfBuzzSharp.Feature(
                            new HarfBuzzSharp.Tag(tag[0], tag[1], tag[2], tag[3]),
                            (uint)kvp.Value));
                    }
                }
            }
            features = featureList.ToArray();
        }

        // Finds the first typeface (main or fallback) that contains a glyph for the
        // given Unicode codepoint. Returns the main entry if no typeface has the glyph,
        // which will result in a .notdef (tofu) glyph being rendered.
        private ShapedTypeface FindTypefaceForCodepoint(int codepoint)
        {
            if (mainEntry.HasGlyph(codepoint))
                return mainEntry;

            for (int i = 0; i < fallbackEntries.Length; i++)
            {
                if (fallbackEntries[i].HasGlyph(codepoint))
                    return fallbackEntries[i];
            }

            // No typeface has this glyph; fall back to main (will render .notdef).
            return mainEntry;
        }

        // Segments the input text into contiguous runs, where each run uses a single typeface.
        // Adjacent characters that map to the same typeface are merged into one run.
        //
        // Iterates by Unicode codepoint (not UTF-16 char) to correctly handle surrogate
        // pairs for emoji, supplementary CJK characters, etc.
        private List<TextRun> SegmentByTypeface(string text)
        {
            List<TextRun> runs = new List<TextRun>();

            if (string.IsNullOrEmpty(text))
                return runs;

            ShapedTypeface currentEntry = null;
            int runStart = 0;
            int i = 0;

            while (i < text.Length)
            {
                // Decode the Unicode codepoint at position i.
                // Surrogate pairs (used for codepoints above U+FFFF) occupy two UTF-16 chars.
                int codepoint;
                int charCount;
                if (char.IsHighSurrogate(text[i]) && i + 1 < text.Length && char.IsLowSurrogate(text[i + 1]))
                {
                    codepoint = char.ConvertToUtf32(text[i], text[i + 1]);
                    charCount = 2;
                }
                else
                {
                    codepoint = text[i];
                    charCount = 1;
                }

                ShapedTypeface entry = FindTypefaceForCodepoint(codepoint);

                if (entry != currentEntry && currentEntry != null)
                {
                    // The typeface changed; emit the accumulated run.
                    runs.Add(new TextRun
                    {
                        Entry = currentEntry,
                        Start = runStart,
                        Length = i - runStart
                    });
                    runStart = i;
                }

                currentEntry = entry;
                i += charCount;
            }

            // Emit the final run.
            if (currentEntry != null)
            {
                runs.Add(new TextRun
                {
                    Entry = currentEntry,
                    Start = runStart,
                    Length = text.Length - runStart
                });
            }

            return runs;
        }

        // Shapes a single text run using HarfBuzz and returns the positioned glyphs.
        //
        // HarfBuzz operates in font design units (typically 1000 or 2048 units per em).
        // The positions it returns must be scaled by (fontSize / unitsPerEm) to convert
        // to display coordinates.
        //
        // The xOffset parameter is the accumulated advance width from prior runs, so that
        // this run's glyphs are positioned after the preceding text.
        private ShapedRunResult ShapeRun(TextRun run, string fullText, float fontSize, float xOffset)
        {
            string runText = fullText.Substring(run.Start, run.Length);
            ShapedTypeface entry = run.Entry;

            // Scale factor: converts HarfBuzz design units to display coordinates.
            float scale = fontSize / entry.HBFace.UnitsPerEm;

            using (HarfBuzzSharp.Buffer buffer = new HarfBuzzSharp.Buffer())
            {
                // Add the run's text to the buffer as UTF-16.
                buffer.AddUtf16(runText);

                // Let HarfBuzz auto-detect text direction (LTR/RTL), script (Latin, Arabic, etc.),
                // and language from the text content.
                buffer.GuessSegmentProperties();

                // Perform shaping: applies the font's OpenType layout rules (GSUB/GPOS tables)
                // to convert Unicode codepoints into positioned glyphs. This handles ligatures,
                // kerning, mark positioning, and complex script reordering.
                entry.HBFont.Shape(buffer, features);

                // Extract the shaping results.
                HarfBuzzSharp.GlyphInfo[] infos = buffer.GlyphInfos;
                HarfBuzzSharp.GlyphPosition[] hbPositions = buffer.GlyphPositions;
                int glyphCount = infos.Length;

                ushort[] glyphIds = new ushort[glyphCount];
                SKPoint[] positions = new SKPoint[glyphCount];
                uint[] clusters = new uint[glyphCount];
                float cursorX = xOffset;

                for (int i = 0; i < glyphCount; i++)
                {
                    // After shaping, GlyphInfo.Codepoint contains the glyph ID (not the
                    // original Unicode codepoint). This is the index into the font's glyph table.
                    glyphIds[i] = (ushort)infos[i].Codepoint;

                    // Compute the display position for this glyph:
                    //   X = cursor position + per-glyph X offset (scaled from design units)
                    //   Y = per-glyph Y offset (scaled, negated because HarfBuzz uses Y-up
                    //       while Skia uses Y-down)
                    //
                    // These positions are relative to the baseline at Y=0; the caller adds
                    // the ascent offset when drawing.
                    positions[i] = new SKPoint(
                        cursorX + hbPositions[i].XOffset * scale,
                        -hbPositions[i].YOffset * scale
                    );

                    // Adjust cluster values to be indices into the full original text string,
                    // not just this run's substring. HarfBuzz cluster values are UTF-16 char
                    // indices into the text that was added to the buffer.
                    clusters[i] = (uint)(infos[i].Cluster + run.Start);

                    // Advance the cursor by this glyph's advance width.
                    cursorX += hbPositions[i].XAdvance * scale;
                }

                return new ShapedRunResult
                {
                    Entry = entry,
                    GlyphIds = glyphIds,
                    Positions = positions,
                    Clusters = clusters,
                    Width = cursorX - xOffset,
                    TextStart = run.Start,
                    TextLength = run.Length
                };
            }
        }

        // Shapes the complete text string, handling font fallback.
        // Returns a list of shaped runs (one per typeface segment) and the total advance width.
        private (List<ShapedRunResult> runs, float totalWidth) ShapeText(string text, float fontSize)
        {
            List<TextRun> textRuns = SegmentByTypeface(text);
            List<ShapedRunResult> shapedRuns = new List<ShapedRunResult>();
            float xOffset = 0;

            foreach (TextRun textRun in textRuns)
            {
                ShapedRunResult result = ShapeRun(textRun, text, fontSize, xOffset);
                shapedRuns.Add(result);
                xOffset += result.Width;
            }

            return (shapedRuns, xOffset);
        }

        // Builds an SKTextBlob from the shaped runs. Each run becomes a separate positioned
        // run in the blob, allowing different typefaces within a single blob.
        //
        // The yBaseline parameter is added to all glyph Y positions to shift from baseline-
        // relative coordinates to the desired drawing position.
        //
        // Returns null if there are no glyphs to draw.
        private SKTextBlob BuildTextBlob(List<ShapedRunResult> runs, float fontSize, float yBaseline, SKPaint paint)
        {
            using (SKTextBlobBuilder builder = new SKTextBlobBuilder())
            {
                bool hasGlyphs = false;

                foreach (ShapedRunResult run in runs)
                {
                    if (run.GlyphIds.Length == 0)
                        continue;

                    hasGlyphs = true;

                    // Apply the baseline offset to Y positions.
                    SKPoint[] adjustedPositions = new SKPoint[run.Positions.Length];
                    for (int i = 0; i < run.Positions.Length; i++)
                    {
                        adjustedPositions[i] = new SKPoint(
                            run.Positions[i].X,
                            run.Positions[i].Y + yBaseline
                        );
                    }

                    // Each run gets its own font in the text blob, enabling mixed-typeface rendering.
                    // All properties are explicitly set to avoid platform-dependent defaults
                    // that can cause non-deterministic glyph rasterization.
                    using (SKFont skFont = new SKFont(run.Entry.Typeface, fontSize))
                    {
                        skFont.Edging = paint.IsAntialias ? SKFontEdging.Antialias : SKFontEdging.Alias;
                        skFont.Hinting = SKFontHinting.None;
                        skFont.Subpixel = false;
                        skFont.EmbeddedBitmaps = false;
                        skFont.LinearMetrics = false;
                        skFont.BaselineSnap = false;
                        skFont.ForceAutoHinting = false;
                        SKPositionedRunBuffer runBuffer = builder.AllocatePositionedRun(skFont, run.GlyphIds.Length);
                        runBuffer.SetGlyphs(run.GlyphIds);
                        runBuffer.SetPositions(adjustedPositions);
                    }
                }

                return hasGlyphs ? builder.Build() : null;
            }
        }

        // Returns the ascent of the main typeface at the given font size.
        // The ascent is the distance from the top of the tallest glyph to the baseline,
        // returned as a positive value.
        private float GetMainAscent(float fontSize)
        {
            using (SKFont skFont = new SKFont(mainEntry.Typeface, fontSize))
            {
                skFont.GetFontMetrics(out SKFontMetrics metrics);
                return -metrics.Ascent; // Skia reports ascent as negative; we return positive.
            }
        }

        // Shape the text using HarfBuzz, and then draw it on the canvas
        // using SkiaSharp, using the main typeface and fallback typefaces as needed.
        //
        // The origin is the top-left corner of the text block. Internally, the text is
        // drawn at origin.Y + ascent because Skia's text drawing functions position text
        // at the baseline, not the top.
        //
        // When DRAW_TEXT_AS_PATHS is defined, text is rendered by converting glyph outlines
        // to paths and filling/stroking them. This bypasses the platform glyph rasterizer
        // (DirectWrite on Windows) entirely, producing deterministic pixel-identical output
        // across runs, at the cost of losing hinting optimizations.
        public void DrawText(SKCanvas canvas, string text, SKPoint origin, float fontSize, SKPaint paint)
        {
            if (string.IsNullOrEmpty(text))
                return;

#if DRAW_TEXT_AS_PATHS
            using (SKPath textPath = GetTextPath(text, origin, fontSize))
            {
                if (!textPath.IsEmpty)
                {
                    canvas.DrawPath(textPath, paint);
                }
            }
#else
            (List<ShapedRunResult> runs, float totalWidth) = ShapeText(text, fontSize);
            float ascent = GetMainAscent(fontSize);

            using (SKTextBlob blob = BuildTextBlob(runs, fontSize, ascent, paint))
            {
                if (blob != null)
                {
                    canvas.DrawText(blob, origin.X, origin.Y, paint);
                }
            }
#endif
        }

        // Shape the text using HarfBuzz and return an SKPath that outlines the text glyphs,
        // using the main typeface and fallback typefaces as needed.
        //
        // The origin is the top-left corner of the text block. Internally, the ascent is
        // added to the Y coordinate because glyph paths are relative to the baseline.
        //
        // Returns an empty path if the text is null or empty.
        public SKPath GetTextPath(string text, SKPoint origin, float fontSize)
        {
            SKPath resultPath = new SKPath();

            if (string.IsNullOrEmpty(text))
                return resultPath;

            (List<ShapedRunResult> runs, float totalWidth) = ShapeText(text, fontSize);
            float ascent = GetMainAscent(fontSize);

            foreach (ShapedRunResult run in runs)
            {
                if (run.GlyphIds.Length == 0)
                    continue;

                using (SKFont skFont = new SKFont(run.Entry.Typeface, fontSize))
                {
                    for (int i = 0; i < run.GlyphIds.Length; i++)
                    {
                        using (SKPath glyphPath = skFont.GetGlyphPath(run.GlyphIds[i]))
                        {
                            if (glyphPath != null && !glyphPath.IsEmpty)
                            {
                                // Translate glyph path to the correct position, including
                                // origin offset and baseline adjustment.
                                float x = origin.X + run.Positions[i].X;
                                float y = origin.Y + run.Positions[i].Y + ascent;
                                resultPath.AddPath(glyphPath, x, y);
                            }
                        }
                    }
                }
            }

            return resultPath;
        }

        // Returns the total advance width of the shaped text. The advance width is the
        // horizontal distance the cursor moves after drawing the full text string.
        public float MeasureTextAdvanceWidth(string text, float fontSize)
        {
            if (string.IsNullOrEmpty(text))
                return 0;

            (List<ShapedRunResult> runs, float totalWidth) = ShapeText(text, fontSize);
            return totalWidth;
        }

        // Returns the tight bounding box of the shaped text. The bounds are computed from
        // per-glyph bounding boxes (via SKFont.GetGlyphWidths) and are relative to the
        // baseline at Y=0 (negative Top means above baseline, positive Bottom means below).
        // This matches the coordinate convention used by SKPaint.MeasureText and
        // SKFont.MeasureText.
        public SKRect MeasureTextBounds(string text, float fontSize)
        {
            if (string.IsNullOrEmpty(text))
                return SKRect.Empty;

            (List<ShapedRunResult> runs, float totalWidth) = ShapeText(text, fontSize);

            // Compute tight bounds by unioning each glyph's bounding box offset by its position.
            SKRect totalBounds = SKRect.Empty;
            bool hasBounds = false;

            foreach (ShapedRunResult run in runs)
            {
                if (run.GlyphIds.Length == 0)
                    continue;

                using (SKFont skFont = new SKFont(run.Entry.Typeface, fontSize))
                {
                    // GetGlyphWidths returns per-glyph advance widths and tight bounding boxes.
                    // The bounds are relative to each glyph's origin (0, 0).
                    SKRect[] glyphBounds = new SKRect[run.GlyphIds.Length];
                    skFont.GetGlyphWidths(run.GlyphIds, null, glyphBounds);

                    for (int i = 0; i < run.GlyphIds.Length; i++)
                    {
                        // Offset the glyph's tight bounds by its shaped position.
                        SKRect gb = glyphBounds[i];
                        SKRect positioned = new SKRect(
                            run.Positions[i].X + gb.Left,
                            run.Positions[i].Y + gb.Top,
                            run.Positions[i].X + gb.Right,
                            run.Positions[i].Y + gb.Bottom
                        );

                        if (!hasBounds)
                        {
                            totalBounds = positioned;
                            hasBounds = true;
                        }
                        else
                        {
                            totalBounds = SKRect.Union(totalBounds, positioned);
                        }
                    }
                }
            }

            return totalBounds;
        }

        // Measure the size of the shaped text, and return the position of each glyph.
        //
        // Each GlyphPosition includes the glyph ID, the source text it represents
        // (derived from HarfBuzz cluster mapping), its absolute position on the canvas,
        // and the typeface that should be used to render it.
        public GlyphPosition[] GetGlyphPositions(string text, SKPoint origin, float fontSize, SKPaint paint)
        {
            if (string.IsNullOrEmpty(text))
                return new GlyphPosition[0];

            (List<ShapedRunResult> runs, float totalWidth) = ShapeText(text, fontSize);
            float ascent = GetMainAscent(fontSize);

            List<GlyphPosition> result = new List<GlyphPosition>();

            foreach (ShapedRunResult run in runs)
            {
                for (int i = 0; i < run.GlyphIds.Length; i++)
                {
                    // Determine the source text that this glyph represents using HarfBuzz
                    // cluster mapping. A "cluster" groups one or more glyphs that correspond
                    // to one or more source characters (e.g., a ligature glyph maps to multiple
                    // source characters, or a combining sequence produces one glyph from
                    // multiple codepoints).
                    //
                    // The cluster value is the UTF-16 char index in the original text where
                    // this glyph's source characters start. We find the end of the cluster
                    // by looking for the next different cluster value.
                    int clusterStart = (int)run.Clusters[i];
                    int clusterEnd = FindClusterEnd(run, i, text.Length);

                    string glyphText = (clusterStart < clusterEnd)
                        ? text.Substring(clusterStart, clusterEnd - clusterStart)
                        : "";

                    result.Add(new GlyphPosition
                    {
                        GlyphId = run.GlyphIds[i],
                        GlyphText = glyphText,
                        Position = new SKPoint(
                            origin.X + run.Positions[i].X,
                            origin.Y + ascent + run.Positions[i].Y
                        ),
                        Typeface = run.Entry.Typeface
                    });
                }
            }

            return result.ToArray();
        }

        // Finds the end of the cluster that glyph at index glyphIndex belongs to.
        // Scans forward through the cluster array to find the next different cluster value,
        // which marks the start of the next cluster (and thus the end of the current one).
        // If this is the last unique cluster in the run, the end is the run's text end.
        //
        // For LTR text, clusters increase monotonically. For RTL text, they decrease.
        // This method handles both cases by taking the absolute range.
        private int FindClusterEnd(ShapedRunResult run, int glyphIndex, int textLength)
        {
            uint currentCluster = run.Clusters[glyphIndex];

            // Look for the next glyph with a different cluster value.
            for (int j = glyphIndex + 1; j < run.Clusters.Length; j++)
            {
                if (run.Clusters[j] != currentCluster)
                {
                    int nextCluster = (int)run.Clusters[j];
                    // For LTR, nextCluster > currentCluster; for RTL, it may be less.
                    // Return whichever is further from clusterStart.
                    return Math.Max((int)currentCluster + 1, nextCluster);
                }
            }

            // This is the last unique cluster in the run; it extends to the end of
            // the run's portion of the original text.
            return Math.Min(run.TextStart + run.TextLength, textLength);
        }
    }

    // Indicates the position of a glyph, including its ID, the text it represents,
    // its position on the canvas, and the typeface used to render it.
    public class GlyphPosition
    {
        public int GlyphId;
        public string GlyphText;
        public SKPoint Position;
        public SKTypeface Typeface;
    }
}
