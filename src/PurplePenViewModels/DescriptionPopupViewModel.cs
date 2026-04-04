using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;

namespace PurplePen.ViewModels
{
    // ViewModel for the popup grid used in the DescriptionViewer
    // to show contextual actions like buttons and text inputs.
    public partial class DescriptionPopupViewModel: ViewModelBase
    {
        // Number of rows in the grid.
        [ObservableProperty] private int rows;

        // Number of columns in the grid.
        [ObservableProperty] private int columns;

        // Items to display in the grid, each with row/column placement.
        public ObservableCollection<PopupGridItemViewModel> MenuItems { get; } = new();

        // Parameterless constructor for the Avalonia designer.
        public DescriptionPopupViewModel()
        {
            Rows = 3;
            Columns = 2;

            // Sample data for designer preview.
            MenuItems.Add(new ButtonGridItemViewModel("Cut") { Row = 0, Column = 0 });
            MenuItems.Add(new ButtonGridItemViewModel("Copy") { Row = 0, Column = 1 });
            MenuItems.Add(new TextBoxGridItemViewModel("Enter custom value...") {
                Row = 1,
                Column = 0,
                ColumnSpan = 2
            });
            MenuItems.Add(new ButtonGridItemViewModel("Save") { Row = 2, Column = 0 });
            MenuItems.Add(new ButtonGridItemViewModel("Cancel") { Row = 2, Column = 1 });
        }
    }

    // Represents one item in the popup grid with its position.
    public abstract partial class PopupGridItemViewModel : ObservableObject
    {
        // Grid row for this item.
        [ObservableProperty] private int row;

        // Grid column for this item.
        [ObservableProperty] private int column;

        // Number of columns this item spans (default 1).
        [ObservableProperty] private int columnSpan = 1;
    }

    // A button item in the popup grid.
    public partial class ButtonGridItemViewModel : PopupGridItemViewModel
    {
        // Text displayed on the button.
        [ObservableProperty] private string buttonText;

        public ButtonGridItemViewModel() => ButtonText = "";
        public ButtonGridItemViewModel(string text) => ButtonText = text;
    }

    // A text input item in the popup grid.
    public partial class TextBoxGridItemViewModel : PopupGridItemViewModel
    {
        // Current text value.
        [ObservableProperty] private string inputText = "";

        // Placeholder/watermark text.
        [ObservableProperty] private string placeholderText;

        public TextBoxGridItemViewModel() => PlaceholderText = "";
        public TextBoxGridItemViewModel(string placeholder) => PlaceholderText = placeholder;
    }
}
