using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia.Rendering;
using AvUtil;
using PurplePen;
using PurplePen.MapModel;
using PurplePen.ViewModels;
using SkiaSharp;
using System;

namespace AvPurplePen;

public partial class DescriptionViewer : UserControl
{
    public static readonly StyledProperty<DescriptionData?> DescriptionDataProperty =
        AvaloniaProperty.Register<DescriptionViewer, DescriptionData?>(nameof(DescriptionData));

    private DescriptionRenderer? renderer;

    private const int margin = 3;            // margin size in logical pixels


    public DescriptionData? DescriptionData
    {
        get => GetValue(DescriptionDataProperty);
        set => SetValue(DescriptionDataProperty, value);
    }

    public DescriptionViewer()
    {
        InitializeComponent();
        drawingView.Paint += DrawingView_Paint;
        drawingView.InvalidateSurface();
    }

    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == DescriptionDataProperty && drawingView is not null)
            drawingView.InvalidateSurface();
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        if (this.DataContext is DescriptionViewerViewModel vm) {
            if (vm.SymbolDB != null) {
                // Create the renderer.
                renderer = new DescriptionRenderer(vm.SymbolDB);
                renderer.Margin = margin;
                renderer.DescriptionKind = DescriptionKind.Symbols;     // control always shows symbols.
                renderer.CellSize = 40;     // cell size in logical pixels -- this will actually need to change.
            }
        }
    }

    private void DrawingView_Paint(object? sender, SkiaScrollableDrawingView.PaintEventArgs e)
    {
        if (renderer != null && DescriptionData != null) {
            e.Canvas.Clear(SKColors.White);

            using (Skia_GraphicsTarget grTarget = new Skia_GraphicsTarget(e.Canvas)) {
                renderer.Description = DescriptionData.Description;

                grTarget.PushAntiAliasing(true);
                renderer.RenderToGraphics(grTarget, Conv.ToRectangleF(e.LogicalViewPort));
                grTarget.PopAntiAliasing();
            }
        }
    }
}

