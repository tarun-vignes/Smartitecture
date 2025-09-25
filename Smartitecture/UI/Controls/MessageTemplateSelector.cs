using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Smartitecture.ViewModels;

namespace Smartitecture.UI.Controls
{
    /// <summary>
    /// Selects the appropriate template for chat messages based on the message type.
    /// </summary>
    public class MessageTemplateSelector : DataTemplateSelector
    {
        /// <summary>
        /// Gets or sets the template for user messages.
        /// </summary>
        public DataTemplate UserTemplate { get; set; }

        /// <summary>
        /// Gets or sets the template for agent messages.
        /// </summary>
        public DataTemplate AgentTemplate { get; set; }

        /// <summary>
        /// Selects the appropriate template based on the message type.
        /// </summary>
        protected override DataTemplate SelectTemplateCore(object item, DependencyObject container)
        {
            if (item is ChatMessageViewModel message)
            {
                return message.IsFromUser ? UserTemplate : AgentTemplate;
            }

            return base.SelectTemplateCore(item, container);
        }
    }
}
