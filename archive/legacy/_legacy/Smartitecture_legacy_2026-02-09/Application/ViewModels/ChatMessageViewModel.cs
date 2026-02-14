using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Smartitecture.ViewModels
{
    /// <summary>
    /// Represents a chat message in the conversation.
    /// </summary>
    public class ChatMessageViewModel : INotifyPropertyChanged
    {
        private string _content = string.Empty;
        private bool _isFromUser;
        private bool _isError;
        private DateTime _timestamp = DateTime.Now;
        private ObservableCollection<AgentAction> _actions = new ObservableCollection<AgentAction>();

        /// <summary>
        /// Gets or sets the content of the message.
        /// </summary>
        public string Content
        {
            get => _content;
            set => SetProperty(ref _content, value ?? string.Empty);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the message is from the user (true) or the agent (false).
        /// </summary>
        public bool IsFromUser
        {
            get => _isFromUser;
            set => SetProperty(ref _isFromUser, value);
        }

        /// <summary>
        /// Gets or sets a value indicating whether the message represents an error.
        /// </summary>
        public bool IsError
        {
            get => _isError;
            set => SetProperty(ref _isError, value);
        }

        /// <summary>
        /// Gets or sets the timestamp when the message was created.
        /// </summary>
        public DateTime Timestamp
        {
            get => _timestamp;
            set => SetProperty(ref _timestamp, value);
        }

        /// <summary>
        /// Gets or sets the collection of actions associated with this message.
        /// </summary>
        public ObservableCollection<AgentAction> Actions
        {
            get => _actions;
            set => SetProperty(ref _actions, value);
        }

        #region INotifyPropertyChanged Implementation

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected bool SetProperty<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(field, value)) return false;
            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        #endregion
    }
}
