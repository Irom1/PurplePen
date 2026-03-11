using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PurplePen
{
    // A struct used to return information about the current selection.
    public struct SelectionInfo
    {
        public CourseDesignator ActiveCourseDesignator;
        public SelectionKind SelectionKind;
        public Id<ControlPoint> SelectedControl;
        public Id<CourseControl> SelectedCourseControl;
        public Id<CourseControl> SelectedCourseControl2;
        public LegInsertionLoc LegInsertionLoc;
        public Id<Special> SelectedSpecial;
        public Symbol SelectedKeySymbol;
        public DescriptionLine.TextLineKind SelectedTextLineKind;
    }

    public enum SelectionKind { None, Control, Special, Leg, Title, SecondaryTitle, Header, TextLine, Key, MapExchangeOrFlipAtControl };


}
