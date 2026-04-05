using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.Templates;
using Avalonia.Media;
using PurplePen.ViewModels;
using System;

namespace AvPurplePen;

// Code-behind for DescriptionPopup. Builds the Grid dynamically
// from the ViewModel since Grid.RowDefinitions/ColumnDefinitions
// are not bindable in Avalonia.
public partial class DescriptionPopup : UserControl
{
    private const int CELLSIZE = 40; // Fixed cell size in pixels for simplicity

    private TextBlock? infoTextControl;

    public DescriptionPopup()
    {
        InitializeComponent();
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        if (DataContext is DescriptionPopupViewModel vm)
            BuildGrid(vm);
    }

    // Builds the grid layout and populates it with templated items
    // from the ViewModel.
    private void BuildGrid(DescriptionPopupViewModel vm)
    {
        popupGrid.RowDefinitions.Clear();
        popupGrid.ColumnDefinitions.Clear();
        popupGrid.Children.Clear();

        for (int i = 0; i < vm.Rows; i++)
            popupGrid.RowDefinitions.Add(new RowDefinition(CELLSIZE, GridUnitType.Pixel));
        for (int i = 0; i < vm.Columns; i++)
            popupGrid.ColumnDefinitions.Add(new ColumnDefinition(CELLSIZE, GridUnitType.Pixel));
        popupGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto)); // Extra row for the information text.

        foreach (PopupGridItemViewModel item in vm.MenuItems)
        {
            ContentControl content = new ContentControl
            {
                Content = item,
                ContentTemplate = FindTemplate(item),
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
            };
            content.PointerEntered += ContentControl_PointerEntered;
            content.PointerExited += ContentControl_PointerExited;
            Grid.SetRow(content, item.Row);
            Grid.SetColumn(content, item.Column);
            Grid.SetColumnSpan(content, item.ColumnSpan);
            popupGrid.Children.Add(content);
        }

        // Add the information text in the last row, spanning all columns.
        infoTextControl = new TextBlock { Text = " ", TextWrapping = TextWrapping.Wrap, FontSize = 18, FontWeight = FontWeight.Bold };
        ContentControl infoTextContent = new ContentControl {
            Content = infoTextControl,
            HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
            VerticalAlignment = Avalonia.Layout.VerticalAlignment.Top,
        };

        Grid.SetRow(infoTextContent, vm.Rows);
        Grid.SetColumn(infoTextContent, 0);
        Grid.SetColumnSpan(infoTextContent, vm.Columns);
        popupGrid.Children.Add(infoTextContent);

    }

    // Updates the info text when the pointer enters a grid item.
    private void ContentControl_PointerEntered(object? sender, PointerEventArgs e)
    {
        if (sender is ContentControl contentControl &&
            contentControl.Content is PopupGridItemViewModel item &&
            infoTextControl != null)
        {
            infoTextControl.Text = item.InfoText;
        }
    }

    // Removes the info text when the pointer exits a grid item.
    private void ContentControl_PointerExited(object? sender, PointerEventArgs e)
    {
        if (sender is ContentControl contentControl &&
            contentControl.Content is PopupGridItemViewModel item &&
            infoTextControl != null) {
            infoTextControl.Text = " ";
        }
    }

    // Finds the DataTemplate resource for the given item type.
    private DataTemplate? FindTemplate(PopupGridItemViewModel item)
    {
        return item switch
        {
            ButtonGridItemViewModel => this.FindResource("ButtonTemplate") as DataTemplate,
            SeparatorGridItemViewModel => this.FindResource("SeparatorTemplate") as DataTemplate,
            TextBoxGridItemViewModel => this.FindResource("TextBoxTemplate") as DataTemplate,
            _ => null,
        };
    }
}
