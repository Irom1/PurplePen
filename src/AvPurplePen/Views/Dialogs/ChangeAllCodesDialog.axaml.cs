// ChangeAllCodesDialog.axaml.cs
using Avalonia.Controls;
using Avalonia.Interactivity;
using PurplePen;
using PurplePen.ViewModels;

namespace AvPurplePen.Views
{
    public partial class ChangeAllCodesDialog : Window
    {
        public ChangeAllCodesDialog()
        {
            InitializeComponent();
        }

        private async void Grid_CellEditEnding(object? sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction != DataGridEditAction.Commit)
                return;
            if (DataContext is not ChangeAllCodesDialogViewModel vm)
                return;
            if (e.EditingElement is not TextBox tb)
                return;

            string newCode = tb.Text ?? "";
            string? error = vm.ValidateCode(newCode);
            if (error != null) {
                e.Cancel = true;
                MessageBoxDialogViewModel msgVm = new MessageBoxDialogViewModel {
                    Message = error,
                    Buttons = MessageBoxButtons.Ok,
                    DefaultButton = MessageBoxButton.Ok,
                    Icon = MessageBoxIcon.Error,
                };
                await Services.DialogService.ShowDialogAsync(msgVm);
            }
        }

        private async void OkButton_Click(object? sender, RoutedEventArgs e)
        {
            if (DataContext is not ChangeAllCodesDialogViewModel vm)
                return;

            string? duplicate = vm.FindDuplicateCode();
            if (duplicate != null) {
                MessageBoxDialogViewModel msgVm = new MessageBoxDialogViewModel {
                    Message = string.Format(MiscText.DuplicateCode, duplicate),
                    Buttons = MessageBoxButtons.Ok,
                    DefaultButton = MessageBoxButton.Ok,
                    Icon = MessageBoxIcon.Error,
                };
                await Services.DialogService.ShowDialogAsync(msgVm);
                return;
            }

            Close(true);
        }

        private void CancelButton_Click(object? sender, RoutedEventArgs e) => Close(false);
    }
}
