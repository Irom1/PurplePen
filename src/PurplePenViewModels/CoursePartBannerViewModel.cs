using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.Text;

namespace PurplePen.ViewModels
{
    public partial class CoursePartBannerViewModel: ViewModelBase
    {
        // Should the variations dropdown be shown?
        [ObservableProperty]
        int enabledVariations;

        // Should the parts dropdown be shown?
        [ObservableProperty]
        bool enableParts;

        // Should the properties button be shown?
        [ObservableProperty]
        bool enableProperties;

        // The list of variations to show in the dropdown. 
        public ObservableCollection<object> AvailableVariations { get; } = new ObservableCollection<object>();

        // Currently selected variation in the dropdown.
        [ObservableProperty]
        object? currentVariation;

        // Text strings in the parts dropdown.
        public ObservableCollection<string> AvailableParts { get; } = new ObservableCollection<string>();

        // Selected index in the parts drop-down.
        [ObservableProperty, NotifyPropertyChangedFor(nameof(SelectedPart))]
        int selectedPartIndex;

        // Return selected part, or -1 for all parts. Note that the dropdown is 0-based,
        // with All Parts being index 0, so we subtract 1 to get the part number.
        public int SelectedPart {
            get { return SelectedPartIndex - 1; }
            set { SelectedPartIndex = value + 1; }
        }

        [ObservableProperty]
        private int numberOfParts = 1;

        public CoursePartBannerViewModel()
        {
            AvailableParts.Add(MiscText.AllParts);
        }

        partial void OnNumberOfPartsChanged(int oldValue, int newValue)
        {
            AvailableParts.Clear();
            AvailableParts.Add(MiscText.AllParts);

            for (int i = 1; i <= NumberOfParts; ++i)
                AvailableParts.Add(string.Format(MiscText.PartXOfY, i, NumberOfParts));
        }

        [RelayCommand]
        public void PropertiesButtonClicked()
        {
            // TODO. The properties button was clicked.
        }
    }
}
