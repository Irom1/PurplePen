/* Copyright (c) 2006-2008, Peter Golde
 * All rights reserved.
 * 
 * Redistribution and use in source and binary forms, with or without 
 * modification, are permitted provided that the following conditions are 
 * met:
 * 
 * 1. Redistributions of source code must retain the above copyright
 * notice, this list of conditions and the following disclaimer.
 * 
 * 2. Redistributions in binary form must reproduce the above copyright
 * notice, this list of conditions and the following disclaimer in the
 * documentation and/or other materials provided with the distribution.
 * 
 * 3. Neither the name of Peter Golde, nor "Purple Pen", nor the names
 * of its contributors may be used to endorse or promote products
 * derived from this software without specific prior written permission.
 * 
 * THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND
 * CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES,
 * INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE
 * DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT OWNER OR
 * CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL,
 * SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING,
 * BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR
 * SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS
 * INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY,
 * WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE
 * USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY
 * OF SUCH DAMAGE.
 */

using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Printing;
using System.Globalization;
using System.Windows.Forms;
using PurplePen.MapModel;
using PurplePen.Graphics2D;
using System.Drawing.Imaging;
using System.Threading;

namespace PurplePen
{

    // Name style for ControlPointName()
    public enum NameStyle { Long, Medium, Short };


    /// <summary>
    /// A whole bunch of static utility functions.
    /// </summary>
    static class WindowsUtil
    {
        // Remove the "&" prefix in menu names
        public static string RemoveHotkeyPrefix(string s)
        {
            return s.Replace("&", "");
        }
        
        // Remove a suffix from a string. If none, return the string itself.
        public static string RemoveSuffix(string s, string suffix)
        {
            if (s == null)
                return s;

            string sTrim = s.Trim();

            if (sTrim.EndsWith(suffix, StringComparison.InvariantCulture))
                return sTrim.Substring(0, sTrim.Length - suffix.Length).Trim();
            else
                return s;
        }


        // Remove a "m" or " m" suffix from a string. If none, return the string itself.
        public static string RemoveMeterSuffix(string s)
        {
            return RemoveSuffix(s, "m");
        }

        // Get a list of print scales from a map scale.
        // Current algorithm: use 4000, 5000, 7500, 10000, 15000, plus the map scale itself.
        public static float[] PrintScaleList(float mapScale)
        {
            List<float> result = new List<float>(new float[] { 4000, 5000, 7500, 10000, 15000 });
            if (!result.Contains(mapScale))
                result.Add(mapScale);
            result.Sort();
            return result.ToArray();
        }


        // Round a rectangle. Returns a sane hittest of rounding each coordinate. Rectangle.Round doesn't do that!
        public static Rectangle Round(RectangleF rect)
        {
            return Rectangle.FromLTRB((int)Math.Round(rect.Left), (int)Math.Round(rect.Top), (int)Math.Round(rect.Right), (int)Math.Round(rect.Bottom));
        }

        private static ThreadLocal<Graphics> hiresGraphics = new ThreadLocal<Graphics>(() => {
            Graphics g = Graphics.FromHwnd(IntPtr.Zero);
            g.ScaleTransform(50F, -50F);
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
            return g;
        });

        // Returns a graphics scaled with negative Y and hi-resolution (50 units/pixel or so).
        // Instances are per-thread, so that tests that use this can run in parallel.
        public static Graphics GetHiresGraphics()
        {
            return hiresGraphics.Value;
        }

        // Go to a given web page.
        public static void GoToWebPage(string url)
        {
            System.Diagnostics.Process.Start(url);
        }

        // Show a given page of help.
        public static void ShowHelpTopic(Form form, string pageName)
        {
            Help.ShowHelp(form, "file:" + Util.GetFileInAppDirectory("Purple Pen Help.chm"), HelpNavigator.Topic, pageName);
        }

        private static Cursor moveHandleCursor;
        private static Cursor deleteHandleCursor;

        /// <summary>
        /// Loads a cursor from a Stream (e.g., embedded resource stream), preserving 32-bit color and alpha transparency.
        /// </summary>
        /// <param name="cursorStream">The stream containing the cursor data.</param>
        /// <returns>A standard Windows Forms Cursor object.</returns>
        public static Cursor LoadCursorFromResource(Stream cursorStream)
        {
            if (cursorStream == null)
                throw new ArgumentNullException(nameof(cursorStream));

            // 1. Create a temporary file path with .cur extension
            string tempPath = Path.GetTempFileName();
            string tempCursorPath = Path.ChangeExtension(tempPath, ".cur");

            // Cleanup the initial temp file created by GetTempFileName if it differs from our new path
            if (File.Exists(tempPath) && tempPath != tempCursorPath)
                File.Delete(tempPath);

            try {
                // 2. Write the stream data to the temporary file
                using (FileStream fs = new FileStream(tempCursorPath, FileMode.Create, FileAccess.Write)) {
                    // Reset stream position if possible, just in case
                    if (cursorStream.CanSeek) {
                        cursorStream.Position = 0;
                    }
                    cursorStream.CopyTo(fs);
                }

                // 3. Load the cursor from the temp file using the P/Invoke method
                IntPtr hCursor = NativeMethods.LoadCursorFromFile(tempCursorPath);

                if (hCursor == IntPtr.Zero) {
                    throw new System.ComponentModel.Win32Exception(Marshal.GetLastWin32Error());
                }

                // 4. Create the managed Cursor object
                return new Cursor(hCursor);
            }
            finally {
                // 5. Clean up: Delete the temp file immediately
                if (File.Exists(tempCursorPath)) {
                    File.Delete(tempCursorPath);
                }
            }
        }

        // Load the MoveHandle cursor.
        public static Cursor MoveHandleCursor
        {
            get
            {
                if (moveHandleCursor == null) {
                    moveHandleCursor = LoadCursorFromResource(typeof(WindowsUtil).Assembly.GetManifestResourceStream("PurplePen.Images.MoveHandle.cur"));
                }
                return moveHandleCursor;
            }
        }

        // Load the DeleteHandle cursor.
        public static Cursor DeleteHandleCursor
        {
            get
            {
                if (deleteHandleCursor == null) {
                    deleteHandleCursor = LoadCursorFromResource(typeof(WindowsUtil).Assembly.GetManifestResourceStream("PurplePen.Images.DeleteHandle.cur"));
                }
                return deleteHandleCursor;
            }
        }

        // Given an array of points that define a path, add a new bend into it at the "right" place where it fits.
        // The oldPoints array may be null or empty.
        public static PointF[] AddPointToArray(PointF[] oldPoints, PointF newPoint)
        {
            if (oldPoints == null || oldPoints.Length == 0) {
                // Simple case -- no old path.
                return new PointF[1] { newPoint };
            }
            else {
                // Complex case. We need to figure out where the newPoint goes by finding the closest point
                // on the path.
                PointF closestPoint;
                SymPath path = new SymPath(oldPoints);
                path.DistanceFromPoint(newPoint, out closestPoint);

                // On which segment does the closest point lie?
                int segmentStart, segmentEnd;
                path.FindSegmentWithPoint(closestPoint, 0.01F, out segmentStart, out segmentEnd);

                // Insert the point in that segment.
                List<PointF> list = new List<PointF>(oldPoints);
                list.Insert(segmentEnd, newPoint);
                return list.ToArray();
            }
        }

        // Given an array of points and a point in it, remove the given point from the array.
        public static PointF[] RemovePointFromArray(PointF[] points, PointF pointToRemove)
        {
            List<PointF> list = new List<PointF>(points);

            return list.FindAll(delegate(PointF pt) { return pt != pointToRemove; }).ToArray();
        }

        public static bool IsPrerelease(string version)
        {
            Version v = new Version(version);
            return (v.Revision < VersionNumber.Stable);
        }

        // Compare version strings. If s1 < s2, return -1; if s1 > s2, return 1, else return 0.
        // Returns 0 if one or both didn't parse.
        public static int CompareVersionStrings(string s1, string s2)
        {
            Version v1, v2;
            if (Version.TryParse(s1, out v1) && Version.TryParse(s2, out v2))
                return v1.CompareTo(v2);
            else
                return 0;
        }

        // Compare version strings. Return true if all exception last component is same.
        // Return false if one or both didn't parse.
        public static bool SameExceptRevision(string s1, string s2)
        {
            Version v1, v2;
            if (Version.TryParse(s1, out v1) && Version.TryParse(s2, out v2))
                return (v1.Major == v2.Major && v1.Minor == v2.Minor && v1.Build == v2.Build);
            else
                return false;
        }

        // Pretty-ize the version string. 
        public static string PrettyVersionString(string verString)
        {
            Version v;

            if (Version.TryParse(verString, out v)) {
                string modifier;

                if (v.Revision >= VersionNumber.Stable)
                    modifier = "";
                else if (v.Revision >= VersionNumber.RC)
                    modifier = " " + string.Format(MiscText.Version_RC, (v.Revision - VersionNumber.RC) / 10.0);
                else if (v.Revision >= VersionNumber.Beta)
                    modifier = " " + string.Format(MiscText.Version_Beta, (v.Revision - VersionNumber.Beta) / 10.0);
                else if (v.Revision >= VersionNumber.Alpha)
                    modifier = " " + string.Format(MiscText.Version_Alpha, (v.Revision - VersionNumber.Alpha) / 10.0);
                else
                    modifier = string.Format(" ({0})", v.Revision);

                return string.Format("{0}.{1}.{2}{3}", v.Major, v.Minor, v.Build, modifier);
            }
            else {
                return verString;
            }
        }


        // Get text describing a paper size.
        public static string GetPaperSizeText(PaperSize paperSize)
        {
            StringBuilder builder = new StringBuilder();

            builder.Append(paperSize.PaperName);
            builder.AppendFormat(" ({0} x {1})", Util.GetDistanceText(paperSize.Width), Util.GetDistanceText(paperSize.Height));
            return builder.ToString();
        }

        // Get text describing a paper size.
        public static string GetPaperSizeText(CoreMapUtil.StandardPaperSize paperSize)
        {
            StringBuilder builder = new StringBuilder();

            builder.Append(paperSize.Name);
            builder.AppendFormat(" ({0} x {1})", Util.GetDistanceText(paperSize.Width), Util.GetDistanceText(paperSize.Height));
            return builder.ToString();
        }

        // Get text describing margins.
        public static string GetMarginsText(Margins margins)
        {
            if (margins.Left == margins.Right && margins.Left == margins.Top && margins.Left == margins.Bottom && margins.Left == margins.Right) {
                // All margins all the same. Simplify the text.
                return string.Format(MiscText.Margins_All, Util.GetDistanceText(margins.Left));
            }
            else {
                return string.Format(MiscText.Margins_LRTB, Util.GetDistanceText(margins.Left), Util.GetDistanceText(margins.Right), Util.GetDistanceText(margins.Top), Util.GetDistanceText(margins.Bottom));
            }
        }

        // Get the text name for a course.
        public static string CourseName(EventDB eventDB, Id<Course> courseId)
        {
            return eventDB.GetCourse(courseId).name;
        }

        // Get the text name for a control. THe Name Style controls how the control points appear:
        // Long:  "Control 32", "Start", "Finish", "Mandatory crossing point".
        // Medium: "32", "Start", "Finish", "Crossing"
        // Short: "32", "S", "F", "C"
        public static string ControlPointName(EventDB eventDB, Id<ControlPoint> controlId, NameStyle style)
        {
            ControlPoint control = eventDB.GetControl(controlId);

            // Control name/code.
            switch (control.kind) {
            case ControlPointKind.Normal:
                if (style == NameStyle.Long)
                    return string.Format(MiscText.Control_Code, control.code);
                else
                    return string.Format("{0}", control.code);

            case ControlPointKind.Start:
                if (style == NameStyle.Short)
                    return MiscText.Start_Short;
                else
                    return MiscText.Start;

            case ControlPointKind.Finish:
                if (style == NameStyle.Short)
                    return MiscText.Finish_Short;
                else
                    return MiscText.Finish;

            case ControlPointKind.CrossingPoint:
                if (style == NameStyle.Long)
                    return MiscText.MandCrossing_Long;
                else if (style == NameStyle.Medium)
                    return MiscText.MandCrossing_Medium;
                else
                    return MiscText.MandCrossing_Short;

            case ControlPointKind.MapExchange:
                if (style == NameStyle.Long)
                    return MiscText.MapExchange_Long;
                else if (style == NameStyle.Medium)
                    return MiscText.MapExchange_Medium;
                else
                    return MiscText.MapExchange_Short;

            case ControlPointKind.MapIssue:
                if (style == NameStyle.Long)
                    return MiscText.MapIssue_Long;
                else if (style == NameStyle.Medium)
                    return MiscText.MapIssue_Medium;
                else
                    return MiscText.MapIssue_Short;

            default:
                Debug.Fail("bad control kind");
                return "";
            }
        }


        public static Point PointFromPointF(PointF pointf)
        {
            return new Point((int)Math.Round(pointf.X), (int)Math.Round(pointf.Y));
        }

        public static string ImageFormatText(ImageFormat imageFormat)
        {
            if (imageFormat.Guid == ImageFormat.Bmp.Guid) return "bmp";
            if (imageFormat.Guid == ImageFormat.Gif.Guid) return "gif";
            if (imageFormat.Guid == ImageFormat.Jpeg.Guid) return "jpeg";
            if (imageFormat.Guid == ImageFormat.Png.Guid) return "png";
            if (imageFormat.Guid == ImageFormat.Tiff.Guid) return "tiff";
            return "unknown";
        }

        public static FontStyle GetFontStyle(bool bold, bool italic)
        {
            FontStyle fontStyle = FontStyle.Regular;
            if (bold)
                fontStyle |= FontStyle.Bold;
            if (italic)
                fontStyle |= FontStyle.Italic;
            return fontStyle;
        }

        public static TextEffects GetTextEffects(bool bold, bool italic)
        {
            TextEffects effects = TextEffects.None;
            if (bold)
                effects |= TextEffects.Bold;
            if (italic)
                effects |= TextEffects.Italic;
            return effects;
        }

        public static TextEffects GetTextEffects(FontStyle fontStyle)
        {
            return GetTextEffects((fontStyle & FontStyle.Bold) != 0, (fontStyle & FontStyle.Italic) != 0);
        }

        static class NativeMethods
        {
            // Windows API for loading a cursor from file. The Cursor constructor does not work
            // correctly with .cur files that have 32-bit color with alpha transparency.
            [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto, BestFitMapping = false, ThrowOnUnmappableChar = true)]
            public static extern IntPtr LoadCursorFromFile(string path);
        }



    }

}
