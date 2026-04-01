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

        [ObservableProperty]
        private SelectedLines? selection;

        public SymbolDB? symbolDB = null;

        public SymbolDB? SymbolDB {
            get {
                return symbolDB;
            }

            set {
                if (value != null) {
                    Debug.Assert(symbolDB == null, "Symbol database cannot be set more than once");
                    symbolDB = value;
                }
            }
        }

        public Controller? controller = null;

        public Controller? Controller {
            get {
                return controller;
            }

            set {
                if (value != null) {
                    Debug.Assert(controller == null, "Controller cannot be set more than once");
                    controller = value;
                }
            }
        }

        // The user has clicked on a new line in the description viewer. 
        partial void OnSelectionChanged(SelectedLines? value)
        {
            if (controller == null)
                return;

            if (value == null)
                controller.SelectDescriptionLine(-1);
            else
                controller.SelectDescriptionLine(value.FirstLine);
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
