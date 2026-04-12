// CoursePartBanner.axaml.cs
//
// Code-behind for the CoursePartBanner user control. This banner sits at the top
// of the map area and lets the user select course parts, variations, and access
// course properties. DataContext is set by the parent (MainWindow).

using Avalonia.Controls;

namespace AvPurplePen
{
    /// <summary>
    /// Banner control for selecting course parts, variations, and accessing properties.
    /// Migrated from WinForms PurplePen/CoursePartBanner.cs.
    /// </summary>
    public partial class CoursePartBanner : UserControl
    {
        public CoursePartBanner()
        {
            InitializeComponent();
        }
    }
}
