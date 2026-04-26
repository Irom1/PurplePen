using PurplePen.MapModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PurplePen
{
#if PORTING
    // Has all the settings for creating Route Gadget files.
    public class RouteGadgetCreationSettings
    {
        public bool mapDirectory, fileDirectory;   // directory to place output files in
        public string outputDirectory;              // the output directory if mapDirectory and fileDirectoy are false.
        public string fileBaseName;                      // base name for file names which are .xml,.gif
        public int xmlVersion = 3;                      // version of IOF XML to use (2 or 3).

        public RouteGadgetCreationSettings Clone()
        {
            return (RouteGadgetCreationSettings)base.MemberwiseClone();
        }
    }

    // All the information needed to print courses.
    public class CoursePrintSettings
    {
        public Id<Course>[] CourseIds;          // Courses to print, None is all controls.
        public bool AllCourses = true;          // If true, overrides the course ids in CourseIds except for "all controls".

        // variation choices for courses with variations.
        public Dictionary<Id<Course>, VariationChoices> VariationChoicesPerCourse = new Dictionary<Id<Course>, VariationChoices>();

        public int Count = 1;                         // count of copies to print
        public bool CropLargePrintArea = true;       // If true, crop a large print area instead of printing multiple pages
        public bool PrintMapExchangesOnOneMap = false;
        public bool PauseAfterCourseOrPart = false;  // If true, printing pauses after each course or part of course printed.
        public ColorModel PrintingColorModel = ColorModel.CMYK;
    }
#endif
    // CoursePdfSettings, OcadCreationSettings, and BitmapCreationSettings are defined in
    // CoursePdf.cs, OcadCreation.cs, and BitmapCreation.cs respectively.
}
