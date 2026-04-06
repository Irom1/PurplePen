using Avalonia.Controls;
using Avalonia.Controls.Templates;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Markup.Xaml.Templates;
using Avalonia.Media;
using Avalonia.Threading;
using Avalonia.VisualTree;
using PurplePen;
using PurplePen.ViewModels;
using System;
using System.Collections.Generic;

namespace AvPurplePen;

// Code-behind for DescriptionPopup. Builds the Grid dynamically
// from the ViewModel since Grid.RowDefinitions/ColumnDefinitions
// are not bindable in Avalonia.
public partial class DescriptionPopup : UserControl
{
    // Routed event raised when a button in the popup grid is clicked.
    public static readonly RoutedEvent<PopupItemSelectedEventArgs> ItemSelectedEvent =
        RoutedEvent.Register<DescriptionPopup, PopupItemSelectedEventArgs>(nameof(PopupItemSelected), RoutingStrategies.Bubble);

    // CLR event wrapper for ItemSelectedEvent.
    public event EventHandler<PopupItemSelectedEventArgs>? PopupItemSelected
    {
        add => AddHandler(ItemSelectedEvent, value);
        remove => RemoveHandler(ItemSelectedEvent, value);
    }
    // Grid cell size in DIPs. Used by DescriptionViewer to calculate bitmap sizes.
    public const int CELLSIZE = 36;

    // Per-side overhead (padding + border) of the popupBtn button style, in DIPs.
    // Must match the Padding and BorderThickness in the popupBtn styles in
    // DescriptionPopup.axaml (currently Padding="3" + BorderThickness="1" = 4).
    public const double BUTTON_CHROME_PER_SIDE = 4;

    private TextBlock? infoTextControl;

    public DescriptionPopup()
    {
        InitializeComponent();

        // Listen for Button.Click events bubbling up from buttons inside the grid.
        AddHandler(Button.ClickEvent, OnGridButtonClick, RoutingStrategies.Bubble);

        // Listen for KeyDown events bubbling up from text boxes inside the grid.
        AddHandler(KeyDownEvent, OnGridKeyDown, RoutingStrategies.Bubble);
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

        for (int i = 0; i < vm.Columns; i++)
            popupGrid.ColumnDefinitions.Add(new ColumnDefinition(CELLSIZE, GridUnitType.Pixel));

        // Track which rows contain separators so they get Auto height.
        HashSet<int> separatorRows = new HashSet<int>();
        foreach (PopupGridItemViewModel item in vm.MenuItems)
        {
            if (item is SeparatorGridItemViewModel)
                separatorRows.Add(item.Row);
        }

        // Add row definitions as needed: Auto for separator rows, fixed CELLSIZE for others.
        for (int i = 0; i < vm.Rows; i++)
        {
            if (separatorRows.Contains(i))
                popupGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto));
            else
                popupGrid.RowDefinitions.Add(new RowDefinition(CELLSIZE, GridUnitType.Pixel));
        }

        popupGrid.RowDefinitions.Add(new RowDefinition(GridLength.Auto)); // Extra row for the information text.

        ContentControl? focusControl = null;

        foreach (PopupGridItemViewModel item in vm.MenuItems)
        {
            ContentControl content = new ContentControl
            {
                Content = item,
                ContentTemplate = FindTemplate(item),
                HorizontalAlignment = Avalonia.Layout.HorizontalAlignment.Stretch,
                VerticalAlignment = Avalonia.Layout.VerticalAlignment.Stretch,
            };
            content.PointerEntered += GridItem_PointerEntered;
            content.PointerExited += GridItem_PointerExited;
            if (item is TextBoxGridItemViewModel) {
                content.GotFocus += GridItem_GotFocus;
                if (focusControl == null)
                    focusControl = content; // Need to focus the first text box.
            }
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

        // Give the first text box control the focus.
        if (focusControl != null) {
            Dispatcher.UIThread.Post(() => {
                // Focus the TextBox inside the ContentControl's template,
                // not the ContentControl itself.
                TextBox? textBox = focusControl.FindDescendantOfType<TextBox>();
                if (textBox != null) {
                    textBox.Focus();
                    textBox.SelectAll();
                }
            }, DispatcherPriority.Input);
        }
    }

    // Updates the info text when the pointer enters a grid item.
    private void GridItem_PointerEntered(object? sender, PointerEventArgs e)
    {
        if (sender is ContentControl contentControl &&
            contentControl.Content is PopupGridItemViewModel item &&
            infoTextControl != null)
        {
            infoTextControl.Text = item.InfoText;
        }
    }

    // Removes the info text when the pointer exits a grid item.
    private void GridItem_PointerExited(object? sender, PointerEventArgs e)
    {
        if (sender is ContentControl contentControl &&
            contentControl.Content is PopupGridItemViewModel item &&
            infoTextControl != null) {
            infoTextControl.Text = " ";
        }
    }

    // Updates the info text when the text box gets focus.
    private void GridItem_GotFocus(object? sender, RoutedEventArgs e)
    {
        if (sender is ContentControl contentControl &&
            contentControl.Content is PopupGridItemViewModel item &&
            infoTextControl != null) 
        {
            infoTextControl.Text = item.InfoText;
        }
    }

    // Handles Click events from buttons inside the popup grid.
    // Walks up from the clicked button to find the ContentControl whose
    // DataContext is a ButtonGridItemViewModel, extracts its symbol, and raises
    // the ItemSelected routed event.
    private void OnGridButtonClick(object? sender, RoutedEventArgs e)
    {
        if (e.Source is Button button &&
            button.FindAncestorOfType<ContentControl>() is ContentControl contentControl &&
            contentControl.Content is ButtonGridItemViewModel item &&
            this.DataContext is DescriptionPopupViewModel vm)
        {
            RaiseEvent(new PopupItemSelectedEventArgs(ItemSelectedEvent, vm.DescriptionChangeData, item.Symbol));
        }
    }

    // Handles KeyDown events from text boxes inside the popup grid.
    // When Enter is pressed, extracts the text from the TextBoxGridItemViewModel
    // and raises the PopupItemSelected routed event.
    private void OnGridKeyDown(object? sender, KeyEventArgs e)
    {
        if (e.Key == Key.Enter &&
            e.Source is TextBox textBox &&
            textBox.FindAncestorOfType<ContentControl>() is ContentControl contentControl &&
            contentControl.Content is TextBoxGridItemViewModel item &&
            this.DataContext is DescriptionPopupViewModel vm)
        {
            RaiseEvent(new PopupItemSelectedEventArgs(ItemSelectedEvent, vm.DescriptionChangeData, item.InputText));
            e.Handled = true;
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

// Event args for the ItemSelected routed event, carrying the symbol of text
// of the changed item, as well as which box was changed and what type of change it was.
public class PopupItemSelectedEventArgs : RoutedEventArgs
{
    // The type of change.
    public DescriptionChangeKind DescriptionChangeKind { get; }

    // Which line in the description was changed.
    public int ChangedLine { get; }

    // Which box in that lines was changed.
    public int ChangedBox { get; }

    // If a text item, the new text. Can be null for for no text or if a symbol was selected.
    public string? NewText { get; }

    // If a symbol item, the new symbol. Can be null for "no symbol" or if a text item was changed.
    public Symbol? NewSymbol { get; }

    public PopupItemSelectedEventArgs(RoutedEvent routedEvent, DescriptionChangeData changeData, Symbol? symbolSelected) : base(routedEvent)
    {
        DescriptionChangeKind = changeData.DescriptionChangeKind;
        ChangedLine = changeData.ChangedLine;
        ChangedBox = changeData.ChangedBox;
        NewSymbol = symbolSelected;
    }

    public PopupItemSelectedEventArgs(RoutedEvent routedEvent, DescriptionChangeData changeData, string? textSelected) : base(routedEvent)
    {
        DescriptionChangeKind = changeData.DescriptionChangeKind;
        ChangedLine = changeData.ChangedLine;
        ChangedBox = changeData.ChangedBox;
        NewText = textSelected;
    }
}


