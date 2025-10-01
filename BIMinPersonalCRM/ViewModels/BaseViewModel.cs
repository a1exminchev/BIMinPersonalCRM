using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace BIMinPersonalCRM.ViewModels
{
    /// <summary>
    ///     Provides a base implementation of <see cref="INotifyPropertyChanged"/>.
    ///     View models should derive from this class and call
    ///     <see cref="OnPropertyChanged"/> whenever a bound property changes.
    /// </summary>
    public abstract class BaseViewModel : INotifyPropertyChanged
    {
        /// <inheritdoc />
        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        ///     Notifies listeners that a property value has changed.
        /// </summary>
        /// <param name="propertyName">The name of the property that changed.</param>
        protected void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
