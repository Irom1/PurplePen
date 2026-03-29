using CommunityToolkit.Mvvm.ComponentModel;
using PurplePen.MapModel;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace PurplePen.ViewModels
{
    public partial class DescriptionViewerViewModel: ViewModelBase
    {
        [ObservableProperty]
        private DescriptionData? descriptionData;

        public SymbolDB? symbolDB = null;

        public SymbolDB? SymbolDB {
            get {
                return symbolDB!;
            }

            set {
                if (value != null) {
                    Debug.Assert(symbolDB == null, "Symbol database cannot be set more than once");
                    symbolDB = value;
                }
            }
        }

    }

    // This is the basic data that the description viewer needs to display the description.
    public record DescriptionData(
        DescriptionLine[] Description,
        CourseView.CourseViewKind CourseKind,
        int ScoreColumn,
        bool HasCustomLength,
        string LangId);

    // Describes the selected lines in the description viewer. FirstLine and LastLine are inclusive, and are 0-based line numbers.
    public record class SelectedLines(int FirstLine, int LastLine);

}
