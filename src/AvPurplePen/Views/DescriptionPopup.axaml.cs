using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.Templates;
using PurplePen.ViewModels;
using System;

namespace AvPurplePen;

// Code-behind for DescriptionPopup. Builds the Grid dynamically
// from the ViewModel since Grid.RowDefinitions/ColumnDefinitions
// are not bindable in Avalonia.
public partial class DescriptionPopup : UserControl
{
    private const int CELLSIZE = 50; // Fixed cell size in pixels for simplicity

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

        foreach (PopupGridItemViewModel item in vm.MenuItems)
        {
            ContentControl content = new ContentControl
            {
                Content = item,
                ContentTemplate = FindTemplate(item),
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
            };
            Grid.SetRow(content, item.Row);
            Grid.SetColumn(content, item.Column);
            Grid.SetColumnSpan(content, item.ColumnSpan);
            popupGrid.Children.Add(content);
        }
    }

    // Finds the DataTemplate resource for the given item type.
    private DataTemplate? FindTemplate(PopupGridItemViewModel item)
    {
        return item switch
        {
            ButtonGridItemViewModel => this.FindResource("ButtonTemplate") as DataTemplate,
            TextBoxGridItemViewModel => this.FindResource("TextBoxTemplate") as DataTemplate,
            _ => null,
        };
    }
}
