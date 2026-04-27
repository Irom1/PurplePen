// CourseAppearanceDialog.axaml.cs
using Avalonia.Controls;
using Avalonia.Interactivity;
using PurplePen.ViewModels;

namespace AvPurplePen.Views
{
    public partial class CourseAppearanceDialog : Window
    {
        public CourseAppearanceDialog()
        {
            InitializeComponent();
        }

        private CourseAppearanceDialogViewModel? ViewModel => DataContext as CourseAppearanceDialogViewModel;

        private void OkButton_Click(object? sender, RoutedEventArgs e) => Close(true);
        private void CancelButton_Click(object? sender, RoutedEventArgs e) => Close(false);

        // When "Use IOF standard sizes" is checked, reset size inputs to standard values.
        private void CheckBoxStandardSizes_Click(object? sender, RoutedEventArgs e)
        {
            if (ViewModel?.IsStandardSizes == true)
                ViewModel.ResetToStandardSizes();
        }

        // When "Use purple color from map" is checked, reset CMYK inputs to map default.
        private void CheckBoxDefaultPurple_Click(object? sender, RoutedEventArgs e)
        {
            if (ViewModel?.IsDefaultPurple == true)
                ViewModel.ResetToDefaultPurple();
        }
    }
}
