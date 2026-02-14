using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Smartitecture.ViewModels
{
    /// <summary>
    /// Base class for view models that implements INotifyPropertyChanged.
    /// </summary>
    public abstract class ViewModelBase : INotifyPropertyChanged
    {
        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Sets the property to the specified value and raises the PropertyChanged event if the value changes.
        /// </summary>
        /// <typeparam name="T">The type of the property</typeparam>
        /// <param name="storage">Reference to the backing field of the property</param>
        /// <param name="value">The new value for the property</param>
        /// <param name="propertyName">The name of the property (automatically determined when not provided)</param>
        /// <returns>True if the property was changed, false otherwise</returns>
        protected bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(storage, value))
            {
                return false;
            }

            storage = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        /// <summary>
        /// Raises the PropertyChanged event for the specified property.
        /// </summary>
        /// <param name="propertyName">The name of the property that changed</param>
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
