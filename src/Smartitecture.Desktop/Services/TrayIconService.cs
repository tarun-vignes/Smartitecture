using System.Windows.Forms;
using System.Drawing;

namespace Smartitecture.Desktop
{
    public interface ITrayIconService
    {
        void Initialize(Form mainForm);
        void Show();
        void Hide();
    }

    public class TrayIconService : ITrayIconService
    {
        private readonly NotifyIcon _notifyIcon;
        private readonly ContextMenuStrip _contextMenu;
        private readonly ILogger<TrayIconService> _logger;

        public TrayIconService(ILogger<TrayIconService> logger)
        {
            _logger = logger;
            
            // Initialize tray icon
            _notifyIcon = new NotifyIcon
            {
                Icon = new Icon("Assets\tray_icon.ico"),
                Text = "Smartitecture",
                Visible = false
            };

            // Initialize context menu
            _contextMenu = new ContextMenuStrip();
            _contextMenu.Items.Add("Show", null, ShowMainForm);
            _contextMenu.Items.Add("Exit", null, ExitApplication);

            _notifyIcon.ContextMenuStrip = _contextMenu;
            _notifyIcon.DoubleClick += (s, e) => ShowMainForm(s, e);
        }

        public void Initialize(Form mainForm)
        {
            _notifyIcon.DoubleClick += (s, e) => mainForm.Show();
            _notifyIcon.Visible = true;
        }

        public void Show()
        {
            _notifyIcon.Visible = true;
        }

        public void Hide()
        {
            _notifyIcon.Visible = false;
        }

        private void ShowMainForm(object? sender, EventArgs e)
        {
            var mainForm = Application.OpenForms["MainForm"] as Form;
            if (mainForm != null)
            {
                mainForm.Show();
                mainForm.WindowState = FormWindowState.Normal;
                mainForm.BringToFront();
            }
        }

        private void ExitApplication(object? sender, EventArgs e)
        {
            _notifyIcon.Visible = false;
            Application.Exit();
        }
    }
}
