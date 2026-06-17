#if !SMARTITECTURE_REAL_BUILD
using System.Windows.Controls;
using System.Windows.Shapes;

namespace Smartitecture.UI
{
    public partial class ChatView
    {
        private void InitializeComponent()
        {
        }

        private Grid ChatContent = null!;
        private Border ProviderStatusPill = null!;
        private Ellipse ProviderStatusDot = null!;
        private TextBlock ProviderStatusText = null!;
        private Button ProviderRefreshButton = null!;
        private Button HistoryButton = null!;
        private Button ClearChatButton = null!;
        private ScrollViewer ChatScrollViewer = null!;
        private StackPanel ChatMessagesPanel = null!;
        private Border TypingIndicator = null!;
        private TextBlock TypingDots = null!;
        private TextBox MessageInput = null!;
        private Button AttachButton = null!;
        private Button VoiceButton = null!;
        private Button SendButton = null!;
        private Grid HistoryPanel = null!;
        private TabControl HistoryTabs = null!;
        private StackPanel HistoryListPanel = null!;
        private TextBlock HistoryEmptyText = null!;
        private StackPanel DeletedListPanel = null!;
        private TextBlock DeletedEmptyText = null!;
        private Grid DeleteConfirmOverlay = null!;
        private Border DeleteConfirmCard = null!;
        private TextBlock DeleteConfirmTitleText = null!;
        private TextBlock DeleteConfirmWarningText = null!;
        private TextBlock DeleteConfirmRecoveryText = null!;
        private Button DeleteConfirmCancelButton = null!;
        private Button DeleteConfirmConfirmButton = null!;
        private Grid ToolConfirmOverlay = null!;
        private Border ToolConfirmCard = null!;
        private TextBlock ToolConfirmMessage = null!;
    }
}
#endif
