using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Primitives.PopupPositioning;
using Avalonia.Data;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Remote.Protocol.Input;
using Avalonia.Rendering;
using Avalonia.Styling;
using AvUtil;
using PurplePen;
using PurplePen.Graphics2D;
using PurplePen.MapModel;
using PurplePen.ViewModels;
using SkiaSharp;
using System;
using System.Drawing;

namespace AvPurplePen;

public partial class DescriptionViewer : UserControl
{
    public static readonly StyledProperty<DescriptionData?> DescriptionDataProperty =
        AvaloniaProperty.Register<DescriptionViewer, DescriptionData?>(nameof(DescriptionData));

    public static readonly StyledProperty<SelectedLines?> SelectionProperty =
        AvaloniaProperty.Register<DescriptionViewer, SelectedLines?>(nameof(Selection), defaultBindingMode: BindingMode.TwoWay);

    private SymbolDB? symbolDB;
    private DescriptionRenderer? renderer;

    private const int margin = 3;            // margin size in logical pixels
    private const int minCellSize = 20;      // minimum cell size in logical pixels

    public DescriptionViewer()
    {
        InitializeComponent();
        drawingView.Paint += DrawingView_Paint;
        drawingView.PointerPressed += DrawingView_PointerPressed;
        UpdateView();
    }

    public DescriptionData? DescriptionData
    {
        get => GetValue(DescriptionDataProperty);
        set => SetValue(DescriptionDataProperty, value);
    }

    public delegate void DescriptionChangedHandler(object sender, DescriptionChangeKind kind, int line, int box, object newValue);

    // Via a popup-menu, the user requested a change to what is in a box in the description.
    public event DescriptionChangedHandler? Change;

    // Via a mouse, the selected was changed. Does not fire if the selected in changed via
    // the property.
    public event EventHandler? SelectedIndexChange;


    // Indicates which line(s) are selected, or null for
    // nothing selected.
    public SelectedLines? Selection {
        get => GetValue(SelectionProperty);
        set => SetValue(SelectionProperty, value);
    }

#if !PORTING
    // TODO: CustomSymbolText.
#endif


    protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
    {
        base.OnPropertyChanged(change);

        if (change.Property == DescriptionDataProperty && drawingView is not null) {
            DescriptionDataUpdated();
        }
        else if (change.Property == SelectionProperty && drawingView is not null) {
            UpdateView();
        }
        else if (change.Property == BoundsProperty && drawingView is not null) {
            UpdateView();
        }
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        if (this.DataContext is DescriptionViewerViewModel vm) {
            if (vm.SymbolDB != null) {
                // Create the renderer.
                symbolDB = vm.SymbolDB;
                renderer = new DescriptionRenderer(vm.SymbolDB);
                renderer.Margin = margin;
                renderer.DescriptionKind = DescriptionKind.Symbols;     // control always shows symbols.
            }
        }
    }

    // Called when DescriptionData updates.
    private void DescriptionDataUpdated()
    {
        if (Selection != null) {
            if (DescriptionData == null || DescriptionData.Description == null) {
                Selection = null;
            }
            else {
                int maxLine = DescriptionData.Description.Length - 1;
                if (Selection.FirstLine > maxLine || Selection.LastLine > maxLine) {
                    Selection = null;
                }
            }
        }

        UpdateView();
    }

    // The mouse was clicked on the description.
    private void DrawingView_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (renderer == null)
            return;

        PointerPoint pointerPoint = drawingView.GetPointerPoint(e);

   
        HitTestResult hitTest = renderer.HitTest(Conv.ToPointF(pointerPoint.Position));
        if (hitTest.firstLine < 0)
            return;             // clicked outside the description.

        bool alreadySelected = (Selection != null && hitTest.firstLine == Selection.FirstLine);

        if (!alreadySelected) {
            // Move the selected line.
            Selection = new SelectedLines(hitTest.firstLine, hitTest.lastLine);
            SelectedIndexChange?.Invoke(this, EventArgs.Empty);
        }

        PointerUpdateKind whichButton = pointerPoint.Properties.PointerUpdateKind;

        // If the left-click the selected line, or right-click anywhere, then possibly show a popup menu.
        if ((whichButton == PointerUpdateKind.RightButtonPressed || alreadySelected) && hitTest.kind != HitTestKind.None) {
            // Clicked on the selected line, in a potentially interesting place. Show a menu (maybe).
            PopupMenu(hitTest);
        }
    }

    private void PopupMenu(HitTestResult hitTest)
    {
        if (renderer == null || symbolDB == null)
            return;

        Avalonia.Point popupMenuLocation = new Avalonia.Point(hitTest.rect.Left + renderer.CellSize * 0.5F,
                                                              hitTest.rect.Top + renderer.CellSize * 0.75F);
        popupMenuLocation -= drawingView.Offset;

        Flyout flyout = new Flyout() {
            Placement = PlacementMode.AnchorAndGravity,
            PlacementAnchor = PopupAnchor.TopLeft,
            PlacementGravity = PopupGravity.BottomRight,
            HorizontalOffset = popupMenuLocation.X,
            VerticalOffset = popupMenuLocation.Y,
            FlyoutPresenterTheme = new ControlTheme(typeof(FlyoutPresenter)) {
                BasedOn = (ControlTheme)this.FindResource(typeof(FlyoutPresenter))!,
                Setters = { new Setter(FlyoutPresenter.PaddingProperty, new Thickness(3)) }
            },
            Content = new DescriptionPopup() {
                DataContext = new DescriptionPopupViewModel(symbolDB, "en", 8, 'D')
            }
        };

        flyout.ShowAt(drawingView);
    }



    // Called when something requires the view to be redrawn.
    private void UpdateView()
    {
        if (renderer != null && drawingView  != null && DescriptionData != null) { 
            Avalonia.Size size = Bounds.Size;
            float boxSize = Math.Max(minCellSize, (float) (size.Width - (margin * 2)) / 8F);

            if (DescriptionData.Description != null) {
                renderer.Description = DescriptionData.Description;
            }
            else {
                renderer.Description = new DescriptionLine[0];
            }

            renderer.CellSize = boxSize;
            SizeF descriptionSize = renderer.Measure();
            drawingView.LogicalExtent = Conv.ToAvSize(descriptionSize);

            drawingView.InvalidateSurface();
        }
    }

    private void DrawingView_Paint(object? sender, SkiaScrollableDrawingView.PaintEventArgs e)
    {
        RectangleF clipRectangle = Conv.ToRectangleF(e.LogicalViewPort);
        e.Canvas.Clear(SKColors.White);

        if (renderer != null && DescriptionData != null) {
            using (Skia_GraphicsTarget grTarget = new Skia_GraphicsTarget(e.Canvas)) {
                renderer.Description = DescriptionData.Description;

                grTarget.PushAntiAliasing(true);
                DrawSelection(grTarget, clipRectangle);
                renderer.RenderToGraphics(grTarget, clipRectangle);
                grTarget.PopAntiAliasing();
            }
        }
    }

    private void DrawSelection(IGraphicsTarget grTarget, RectangleF clipRectangle)
    {
        if (renderer != null && Selection != null && DescriptionData != null && DescriptionData.Description != null &&
            Selection.FirstLine >= 0 && Selection.LastLine >= 0 &&
            Selection.FirstLine < DescriptionData.Description.Length && Selection.LastLine < DescriptionData.Description.Length) 
        {
            RectangleF selectedRect = (renderer.LineBounds(Selection.FirstLine, Selection.LastLine));
            if (selectedRect.IntersectsWith(clipRectangle)) {
                object selectionBrush = new object();
                grTarget.CreateSolidBrush(selectionBrush, CmykColor.FromColor(Color.Yellow));
                grTarget.FillRectangle(selectionBrush, selectedRect);
            }
        }
    }
}


