// ViewModelBase.cs
//
// Abstract base class for all ViewModels. Inherits from ObservableValidator
// which provides both INotifyPropertyChanged (via ObservableObject) and
// INotifyDataErrorInfo for validation support. ViewModels that don't use
// validation simply won't have any validation attributes, and the validator
// machinery sits dormant with no overhead.

using CommunityToolkit.Mvvm.ComponentModel;

namespace PurplePen.ViewModels
{
    /// <summary>
    /// Base class for all ViewModels. Provides property change notification
    /// and optional data validation via <see cref="ObservableValidator"/>.
    /// </summary>
    public abstract class ViewModelBase : ObservableValidator
    {
    }
}
