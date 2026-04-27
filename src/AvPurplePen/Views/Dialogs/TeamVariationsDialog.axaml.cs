// TeamVariationsDialog.axaml.cs
//
// Code-behind for the Course / Relay Team Variations dialog.
// Handles Calculate (opens HTML report in browser), Assign Legs (sub-dialog),
// and Export (CSV/XML via Avalonia file picker).

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using PurplePen;
using PurplePen.ViewModels;

namespace AvPurplePen.Views
{
    public partial class TeamVariationsDialog : Window
    {
        public TeamVariationsDialog()
        {
            InitializeComponent();
        }

        private TeamVariationsDialogViewModel Vm => (TeamVariationsDialogViewModel)DataContext!;
        private Controller Controller => Vm.Controller!;

        private void CloseButton_Click(object? sender, RoutedEventArgs e) => Close(true);

        private void CalculateButton_Click(object? sender, RoutedEventArgs e)
        {
            TeamVariationsDialogViewModel vm = Vm;
            if (vm.Controller == null) return;

            string? error = Controller.ValidateFixedBranchAssignments(
                vm.NumberOfLegs, vm.FixedBranchAssignments);
            if (error != null) {
                Services.DialogService.ShowDialogAsync(new MessageBoxDialogViewModel {
                    Message = error,
                    Icon = MessageBoxIcon.Error,
                    Buttons = MessageBoxButtons.Ok,
                    DefaultButton = MessageBoxButton.Ok,
                });
                return;
            }

            string html = vm.NumberOfTeams == 0
                ? new Reports().CreateRelayVariationNotCreated()
                : new Reports().CreateRelayVariationReport(
                    Controller.GetVariationReportData(vm.RelaySettings));

            string tempFile = Path.Combine(Path.GetTempPath(), "relay_variations.html");
            File.WriteAllText(tempFile, html);
            Process.Start(new ProcessStartInfo(tempFile) { UseShellExecute = true });

            vm.StatusText = vm.NumberOfTeams == 0
                ? "Report opened in browser (no teams configured)."
                : $"Report opened in browser ({vm.NumberOfTeams} teams × {vm.NumberOfLegs} legs).";
        }

        private async void AssignLegsButton_Click(object? sender, RoutedEventArgs e)
        {
            TeamVariationsDialogViewModel vm = Vm;
            if (vm.Controller == null) return;

            List<char[]> codes = Controller.GetLegAssignmentCodes();
            LegAssignmentsDialogViewModel legVm =
                new LegAssignmentsDialogViewModel(codes, vm.FixedBranchAssignments);

            if (!await Services.DialogService.ShowDialogAsync(legVm)) return;

            FixedBranchAssignments newAssignments = legVm.GetFixedBranchAssignments();
            string? error = Controller.ValidateFixedBranchAssignments(vm.NumberOfLegs, newAssignments);
            if (error != null) {
                await Services.DialogService.ShowDialogAsync(new MessageBoxDialogViewModel {
                    Message = error,
                    Icon = MessageBoxIcon.Error,
                    Buttons = MessageBoxButtons.Ok,
                    DefaultButton = MessageBoxButton.Ok,
                });
                return;
            }

            vm.FixedBranchAssignments = newAssignments;
        }

        private async void ExportButton_Click(object? sender, RoutedEventArgs e)
        {
            TeamVariationsDialogViewModel vm = Vm;
            if (vm.Controller == null) return;

            FilePickerSaveOptions options = new FilePickerSaveOptions {
                Title = "Export Relay Variations",
                SuggestedFileName = Path.GetFileName(vm.DefaultExportFileName),
                FileTypeChoices = new[] {
                    new FilePickerFileType("CSV files") { Patterns = new[] { "*.csv" } },
                    new FilePickerFileType("XML files") { Patterns = new[] { "*.xml" } },
                },
            };

            if (!string.IsNullOrEmpty(vm.DefaultExportFileName)) {
                options.SuggestedStartLocation = await StorageProvider.TryGetFolderFromPathAsync(
                    Path.GetDirectoryName(vm.DefaultExportFileName) ?? "");
            }

            IStorageFile? file = await StorageProvider.SaveFilePickerAsync(options);
            if (file == null) return;

            string path = file.Path.LocalPath;
            VariationExportFileType fileType = path.EndsWith(".xml", StringComparison.OrdinalIgnoreCase)
                ? VariationExportFileType.Xml
                : VariationExportFileType.Csv;

            Controller.ExportRelayVariationsReport(vm.RelaySettings, fileType, path);
        }
    }
}
