// CourseLoadDialog.axaml.cs
//
// Code-behind for the Course / Competitor Load dialog.

using Avalonia.Controls;
using Avalonia.Interactivity;
using PurplePen;
using PurplePen.ViewModels;

namespace AvPurplePen.Views
{
    public partial class CourseLoadDialog : Window
    {
        public CourseLoadDialog()
        {
            InitializeComponent();
        }

        private void OkButton_Click(object? sender, RoutedEventArgs e)
        {
            CourseLoadDialogViewModel vm = (CourseLoadDialogViewModel)DataContext!;

            foreach (CourseLoadItem item in vm.CourseLoads) {
                string text = item.LoadText.Trim();
                if (string.IsNullOrEmpty(text)) continue;
                if (!int.TryParse(text, out int v) || v <= 0) {
                    Services.DialogService.ShowDialogAsync(new MessageBoxDialogViewModel {
                        Message = MiscText.BadLoad,
                        Icon = MessageBoxIcon.Error,
                        Buttons = MessageBoxButtons.Ok,
                        DefaultButton = MessageBoxButton.Ok
                    });
                    return;
                }
            }

            Close(true);
        }

        private void CancelButton_Click(object? sender, RoutedEventArgs e)
        {
            Close(false);
        }
    }
}
