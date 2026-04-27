// AllControlsPropertiesDialog.axaml.cs
//
// Code-behind for the All Controls Properties dialog.

using Avalonia.Controls;
using Avalonia.Interactivity;
using PurplePen;
using PurplePen.ViewModels;

namespace AvPurplePen.Views
{
    public partial class AllControlsPropertiesDialog : Window
    {
        public AllControlsPropertiesDialog()
        {
            InitializeComponent();
        }

        private void OkButton_Click(object? sender, RoutedEventArgs e)
        {
            AllControlsPropertiesDialogViewModel vm = (AllControlsPropertiesDialogViewModel)DataContext!;

            if (!float.TryParse(vm.PrintScaleText, out float scale) || scale < 100 || scale > 100000) {
                Services.DialogService.ShowDialogAsync(new MessageBoxDialogViewModel {
                    Message = MiscText.BadScale,
                    Icon = MessageBoxIcon.Error,
                    Buttons = MessageBoxButtons.Ok,
                    DefaultButton = MessageBoxButton.Ok
                });
                scaleCombo.Focus();
                return;
            }

            Close(true);
        }

        private void CancelButton_Click(object? sender, RoutedEventArgs e)
        {
            Close(false);
        }
    }
}
