using System.Windows.Controls;

namespace Smartitecture.Services
{
    public static class NavigationService
    {
        private static ContentControl? _host;

        public static void Initialize(ContentControl host)
        {
            _host = host;
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
            SetContent(new Smartitecture.UI.ChatView(new Smartitecture.Services.MultiModelAIService()));
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
