using System.Windows.Forms;
using Smartitecture.Core.ViewModels;
using Smartitecture.Core.Services;

namespace Smartitecture.Desktop
{
    public partial class MainForm : Form
    {
        private readonly ILogger<MainForm> _logger;
        private readonly IAuthenticationService _authService;
        private readonly IWindowManager _windowManager;
        private readonly ITrayIconService _trayIconService;

        public MainForm(
            ILogger<MainForm> logger,
            IAuthenticationService authService,
            IWindowManager windowManager,
            ITrayIconService trayIconService)
        {
            InitializeComponent();
            _logger = logger;
            _authService = authService;
            _windowManager = windowManager;
            _trayIconService = trayIconService;

            // Initialize tray icon
            _trayIconService.Initialize(this);
        }

        private async void MainForm_Load(object sender, EventArgs e)
        {
            try
            {
                // Check if user is authenticated
                if (!_authService.IsAuthenticated)
                {
                    await _windowManager.ShowLoginDialog();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error initializing main form");
                MessageBox.Show("Failed to initialize application. Please try again.", 
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing)
            {
                // Minimize to tray instead of closing
                e.Cancel = true;
                this.WindowState = FormWindowState.Minimized;
                this.Hide();
            }
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
            {
                this.Hide();
            }
        }
    }
}
