using System.Windows.Controls;

namespace Smartitecture.Services
{
    public static class NavigationService
    {
        private static ContentControl? _host;
        private static Smartitecture.UI.ChatView? _chat;

        public static void Initialize(ContentControl host)
        {
            _host = host;
            _chat = null;
        }

        private static void SetContent(UserControl view)
        {
            if (_host == null)
            {
                return;
            }

            _host.Content = view;
        }

        public static void GoHome()
        {
            SetContent(new Smartitecture.UI.StartupView());
        }

        public static void GoDashboard()
        {
            SetContent(new Smartitecture.UI.DashboardView());
        }

        public static void GoChat()
        {
            _chat ??= new Smartitecture.UI.ChatView(new Smartitecture.Services.MultiModelAIService());
            SetContent(_chat);
        }

        public static async System.Threading.Tasks.Task GoChatAsync(string prompt)
        {
            GoChat();
            if (_chat != null) await _chat.SendPromptAsync(prompt);
        }

        public static void GoSettings()
        {
            SetContent(new Smartitecture.UI.SettingsView());
        }

        public static void GoAbout()
        {
            SetContent(new Smartitecture.UI.AboutView());
        }
    }
}
