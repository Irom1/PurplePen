using CommunityToolkit.Mvvm.ComponentModel;
using PurplePen.Graphics2D;
using PurplePen.MapModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Text;
using System.Threading.Channels;

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

        SymbolDB symbolDB;
        string langId;
        int currentRow, currentCol;

        // Items to display in the grid, each with row/column placement.
        public ObservableCollection<PopupGridItemViewModel> MenuItems { get; } = new();

        // Parameterless constructor for the Avalonia designer.
        public DescriptionPopupViewModel(SymbolDB symbolDB, string langId, int columns, char kind)
        {
            this.symbolDB = symbolDB;
            this.langId = langId;
            this.Columns = columns;
            this.MenuItems.Clear();
            SymbolImageCache.Instance.Configure(symbolDB, 100); // Cache symbol images at 100 pixel box size.

            // Sample data for designer preview.
            AddSymbolsOfKind(kind);

            Rows = currentRow + (currentCol > 0 ? 1 : 0);
        }

        private void AddSymbolsOfKind(char kind)
        {
            foreach (Symbol symbol in symbolDB.AllSymbols) {
                if (symbol.Kind == kind && symbol.HasVisualImage) {
                    IGraphicsBitmap image = SymbolImageCache.Instance.GetSymbolImage(symbol.Id);

                    ButtonGridItemViewModel button = new ButtonGridItemViewModel(image);

                    AddItem(button, 1);
                }
            }
        }

        private void AddItem(PopupGridItemViewModel item, int columnSpan)
        {
            // Check if this item is too wide to fit on the current row
            if (currentCol + columnSpan > Columns) {
                // Drop to the next line
                currentRow++;
                currentCol = 0;
            }

            // Assign the calculated coordinates to the item
            item.Row = currentRow;
            item.Column = currentCol;
            item.ColumnSpan = columnSpan;

            MenuItems.Add(item);

            // Move the "cursor" forward by however many columns this item takes up
            currentCol += item.ColumnSpan;

            // If the cursor hits the exact edge, wrap to the next line
            if (currentCol >= Columns) {
                currentRow++;
                currentCol = 0;
            }
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
        [ObservableProperty] private IGraphicsBitmap? buttonBitmap;

        public ButtonGridItemViewModel(IGraphicsBitmap bitmap)
        {
            ButtonBitmap = bitmap;
        }
    }

    // A text input item in the popup grid.
    public partial class TextBoxGridItemViewModel : PopupGridItemViewModel
    {
        // Current text value.
        [ObservableProperty] private string inputText = "";

        // Placeholder/watermark text.
        [ObservableProperty] private string placeholderText;

        public TextBoxGridItemViewModel()
        {
            PlaceholderText = "";
        }

        public TextBoxGridItemViewModel(string placeholder)
        {
            PlaceholderText = placeholder;
        }
    }

    // Maintains a cache of symbol images for the description popup, keyed by symbol ID.
    class SymbolImageCache
    {
        public static SymbolImageCache Instance { get; } = new SymbolImageCache();

        private SymbolDB? symbolDB;
        private string standard = "";
        private int boxSize;

        private Dictionary<string, IGraphicsBitmap> cache = new();

        private SymbolImageCache() { }

        public void Configure(SymbolDB symbolDB, int boxSize)
        {
            if (this.symbolDB != symbolDB || this.boxSize != boxSize) {
                this.symbolDB = symbolDB;
                this.boxSize = boxSize;
                this.standard = symbolDB.Standard;
                ClearCache();
            }
        }

        public IGraphicsBitmap GetSymbolImage(string symbolID)
        {
            if (symbolDB == null)
                throw new InvalidOperationException("SymbolImageCache not configured with SymbolDB");

            if (symbolDB.Standard != standard) {
                // The symbolDB.Standard has changed. 
                ClearCache();
                standard = symbolDB.Standard;
            }

            if (! cache.ContainsKey(symbolID)) {
                Symbol symbol = symbolDB[symbolID];

                int pixelWidth = symbol.IsWide ? boxSize * 8 : boxSize;
                int pixelHeight = boxSize;

                // Use transparent background.
                using (IBitmapGraphicsTarget grTarget = Services.BitmapGraphicsTargetProvider.CreateBitmapGraphicsTarget(pixelWidth, pixelHeight, CmykColor.FromCmyka(0, 0, 0, 0, 0), DefaultColorConverter.Instance)) {
                    grTarget.PushAntiAliasing(false);
                    symbol.Draw(grTarget, CmykColor.FromColor(Color.Black), new RectangleF(0, 0, pixelWidth, pixelHeight));
                    cache[symbolID] = grTarget.FinishBitmap();
                }
            }

            if (symbolID == "3.4")
                using (Stream stm = new FileStream(@"D:\Temp\symbol3.4.png", FileMode.Create))
                    cache[symbolID].WriteToStream(GraphicsBitmapFormat.PNG, stm);
            return cache[symbolID];
        }

        private void ClearCache()
        {
            cache.Clear();
        }
    }
}
