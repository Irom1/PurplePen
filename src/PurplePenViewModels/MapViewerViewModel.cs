using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Text;

namespace PurplePen.ViewModels
{
    public partial class MapViewerViewModel: ViewModelBase
    {
        [ObservableProperty]
        private IMapDisplay? mapDisplay;

    }
}
