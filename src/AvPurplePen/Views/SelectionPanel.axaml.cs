using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using PurplePen;
using SkiaSharp;
using System;
using System.Collections.Generic;
using System.Reflection.Metadata.Ecma335;

namespace AvPurplePen;

public partial class SelectionPanel : UserControl
{
    public static readonly StyledProperty<TextPart[]> SelectionDescriptionProperty =
        AvaloniaProperty.Register<SelectionPanel, TextPart[]>(nameof(SelectionDescription), defaultValue: []);

    static SelectionPanel()
    {
        //SelectionDescriptionProperty.Changed.AddClassHandler<SelectionPanel>((panel, args) =>
        //    panel.SelectionDescriptionChanged(args.GetOldValue<TextPart[]?>() ?? [], args.GetNewValue<TextPart[]?>() ?? []));
    }

    public SelectionPanel()
    {
        InitializeComponent();
    }

    public TextPart[] SelectionDescription {
        get => GetValue(SelectionDescriptionProperty);
        set => SetValue(SelectionDescriptionProperty, value);
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == SelectionDescriptionProperty) {
            SelectionDescriptionChanged(change.GetOldValue<TextPart[]?>() ?? [], change.GetNewValue<TextPart[]?>() ?? []);
        }
    }

    private void SelectionDescriptionChanged(TextPart[] oldDescription, TextPart[] newDescription)
    {
        if (HasSelectionDescChanged(oldDescription, newDescription)) {
            // Recreate all the TextBlocks in "textBlockStack".
            List<TextBlock> textBlocks = CreateTextBlocks(newDescription);
            textBlockStack.Children.Clear();
            textBlockStack.Children.AddRange(textBlocks);
        }
    }

    private bool HasSelectionDescChanged(TextPart[] oldDescription, TextPart[] newDescription)
    {
        if (oldDescription == null || newDescription == null)
            return (oldDescription != newDescription);

        if (oldDescription.Length != newDescription.Length)
            return true;

        for (int i = 0; i < oldDescription.Length; ++i) {
            if (oldDescription[i].format != newDescription[i].format ||
                oldDescription[i].text != newDescription[i].text)
                return true;
        }

        return false;
    }

    private List<TextBlock> CreateTextBlocks(TextPart[] description)
    {
        List<TextBlock> textBlocks = new List<TextBlock>();
        TextBlock? currentTextBlock = null;

        double baseFontSize = 15;
        const int HEADERGAP = 4;    // number of pixels extra space before a header
        const int LEFTRIGHTPADDING = 3; // pixels of left/right padding.
        const int INDENT = 12;    // number of pixels to index non-header lines


        foreach (TextPart part in description) {
            // Add a line break after previous control if requested.
            if ((part.format == TextFormat.Header || part.format == TextFormat.NewLine)) {
                if (currentTextBlock != null) {
                    textBlocks.Add(currentTextBlock);
                    currentTextBlock = null;
                }
            }

            if (currentTextBlock == null) {
                // Create a new TextBlock.
                currentTextBlock = new TextBlock();
                currentTextBlock.TextWrapping = TextWrapping.Wrap;
                currentTextBlock.Inlines = new InlineCollection();
                currentTextBlock.Padding = new Thickness(LEFTRIGHTPADDING, 0, LEFTRIGHTPADDING, 0);
                currentTextBlock.LineSpacing = -2;  // Move lines a little closer together to make it look better.
            }

            string text = part.text;
            Run run = new Run();
            run.FontSize = baseFontSize;
            if (currentTextBlock.Inlines!.Count > 0) {
                // Add a space between consecutive runs in the same text block.
                text = " " + text;
            }
            run.Text = text;

            if (part.format == TextFormat.Title) {
                // A bit bigger font.
                run.FontSize = baseFontSize * 1.15F;
                run.FontWeight = FontWeight.Bold;
            }
            else if (part.format == TextFormat.Header) {
                // Add a gap before headers.
                currentTextBlock.Padding = new Thickness(currentTextBlock.Padding.Left, HEADERGAP, currentTextBlock.Padding.Right, currentTextBlock.Padding.Bottom);
                run.FontWeight = FontWeight.Bold;
            }
            else if (part.format == TextFormat.NewLine) {
                // Add an indent before non-headers.
                currentTextBlock.Padding = new Thickness(INDENT + LEFTRIGHTPADDING, currentTextBlock.Padding.Top, currentTextBlock.Padding.Right, currentTextBlock.Padding.Bottom);
            }

            currentTextBlock.Inlines!.Add(run);
        }

        if (currentTextBlock != null) {
            textBlocks.Add(currentTextBlock);
        }

        return textBlocks;
    }
}