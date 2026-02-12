using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.NetworkInformation;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Runtime.InteropServices;
using System.Windows.Threading;
using Smartitecture.Services;

namespace Smartitecture.UI
{
    public partial class StartupView : UserControl
    {
        private readonly DispatcherTimer _refreshTimer;
        private PerformanceCounter? _cpuCounter;
        private PerformanceCounter? _memoryAvailableCounter;
        private PerformanceCounter? _diskCounter;
        private PerformanceCounter? _netRecvCounter;
        private PerformanceCounter? _netSentCounter;
        private long _totalMemoryMb;
        private long _netSpeedBits;
        private string _netInterfaceLabel = "Network: --";
        private DriveInfo? _systemDrive;

        public StartupView()
        {
            InitializeComponent();
            Loaded += StartupView_Loaded;
            Unloaded += StartupView_Unloaded;

            InitializeCounters();

            _refreshTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(1)
            };
            _refreshTimer.Tick += (_, __) => UpdateMetrics();
        }

        private void StartupView_Loaded(object sender, RoutedEventArgs e)
        {
            Focus();
            UpdateStaticInfo();
            UpdateMetrics();
            _refreshTimer.Start();
        }

        private void StartupView_Unloaded(object sender, RoutedEventArgs e)
        {
            _refreshTimer.Stop();
            DisposeCounters();
        }

        private void InitializeCounters()
        {
            try
            {
                _cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
                _cpuCounter.NextValue();
            }
            catch { }

            try
            {
                _memoryAvailableCounter = new PerformanceCounter("Memory", "Available MBytes");
                _memoryAvailableCounter.NextValue();
            }
            catch { }

            try
            {
                _diskCounter = new PerformanceCounter("PhysicalDisk", "% Disk Time", "_Total");
                _diskCounter.NextValue();
            }
            catch { }

            try
            {
                var nic = SelectPrimaryNetworkInterface();
                if (nic != null)
                {
                    _netInterfaceLabel = nic.Description;
                    _netSpeedBits = nic.Speed;
                    var instanceName = FindNetworkInstanceName(nic);
                    if (!string.IsNullOrWhiteSpace(instanceName))
                    {
                        _netRecvCounter = new PerformanceCounter("Network Interface", "Bytes Received/sec", instanceName);
                        _netSentCounter = new PerformanceCounter("Network Interface", "Bytes Sent/sec", instanceName);
                        _netRecvCounter.NextValue();
                        _netSentCounter.NextValue();
                    }
                }
            }
            catch { }

            try
            {
                var root = Path.GetPathRoot(Environment.SystemDirectory);
                if (!string.IsNullOrWhiteSpace(root))
                {
                    _systemDrive = DriveInfo.GetDrives().FirstOrDefault(d => d.IsReady && d.RootDirectory.FullName.Equals(root, StringComparison.OrdinalIgnoreCase));
                }
            }
            catch { }

            try
            {
                var status = new MEMORYSTATUSEX();
                status.dwLength = (uint)Marshal.SizeOf<MEMORYSTATUSEX>();
                if (GlobalMemoryStatusEx(ref status))
                {
                    _totalMemoryMb = (long)(status.ullTotalPhys / (1024 * 1024));
                }
            }
            catch { }
        }

        private void UpdateStaticInfo()
        {
            CpuSubText.Text = Format("Startup.CoresFormat", "Cores: {0}", Environment.ProcessorCount);

            if (_systemDrive != null)
            {
                var totalGb = _systemDrive.TotalSize / 1024d / 1024d / 1024d;
                DiskSubText.Text = Format(
                    "Startup.SystemDriveTotalFormat",
                    "System drive {0}: {1} GB total",
                    _systemDrive.Name.TrimEnd('\\'),
                    totalGb.ToString("0.0", CultureInfo.CurrentCulture));
            }
            else
            {
                DiskSubText.Text = GetString("Startup.SystemDriveUnknown", "System drive: --");
            }

            NetworkSubText.Text = string.IsNullOrWhiteSpace(_netInterfaceLabel)
                ? GetString("Startup.NetworkUnknown", "Network: --")
                : _netInterfaceLabel;
        }

        private void UpdateMetrics()
        {
            var now = DateTime.Now;
            LastUpdatedText.Text = Format("Startup.UpdatedFormat", "Updated {0}", now.ToString("T", CultureInfo.CurrentCulture));

            var cpu = ReadCounter(_cpuCounter);
            CpuValueText.Text = $"{cpu:0}%";
            CpuBar.Value = Clamp(cpu);

            var availableMb = ReadCounter(_memoryAvailableCounter);
            var usedMb = _totalMemoryMb > 0 ? Math.Max(0, _totalMemoryMb - availableMb) : 0;
            var memPct = _totalMemoryMb > 0 ? usedMb * 100.0 / _totalMemoryMb : 0;
            MemoryValueText.Text = _totalMemoryMb > 0
                ? $"{usedMb / 1024.0:0.0} GB / {_totalMemoryMb / 1024.0:0.0} GB ({memPct:0}%)"
                : GetString("Startup.MemoryUnknown", "Memory: --");
            MemoryBar.Value = Clamp(memPct);
            MemorySubText.Text = availableMb > 0
                ? Format("Startup.AvailableFormat", "Available: {0} GB", (availableMb / 1024.0).ToString("0.0", CultureInfo.CurrentCulture))
                : Format("Startup.AvailableFormat", "Available: {0} GB", "--");

            var disk = ReadCounter(_diskCounter);
            DiskValueText.Text = Format("Startup.DiskActiveFormat", "{0}% active", disk.ToString("0", CultureInfo.CurrentCulture));
            DiskBar.Value = Clamp(disk);
            if (_systemDrive != null)
            {
                var freeGb = _systemDrive.AvailableFreeSpace / 1024d / 1024d / 1024d;
                DiskSubText.Text = Format("Startup.DiskFreeFormat", "Free: {0} GB", freeGb.ToString("0.0", CultureInfo.CurrentCulture));
            }

            var recv = ReadCounter(_netRecvCounter);
            var sent = ReadCounter(_netSentCounter);
            var downMbps = recv * 8 / 1_000_000.0;
            var upMbps = sent * 8 / 1_000_000.0;
            NetworkValueText.Text = Format(
                "Startup.NetworkRatesFormat",
                "Down {0} Mbps | Up {1} Mbps",
                downMbps.ToString("0.0", CultureInfo.CurrentCulture),
                upMbps.ToString("0.0", CultureInfo.CurrentCulture));
            var totalBits = (recv + sent) * 8;
            var netPct = _netSpeedBits > 0 ? totalBits * 100.0 / _netSpeedBits : 0;
            NetworkBar.Value = Clamp(netPct);
        }

        private static float ReadCounter(PerformanceCounter? counter)
        {
            try { return counter?.NextValue() ?? 0f; }
            catch { return 0f; }
        }

        private static double Clamp(double value)
        {
            if (value < 0) return 0;
            if (value > 100) return 100;
            return value;
        }

        private static NetworkInterface? SelectPrimaryNetworkInterface()
        {
            try
            {
                return NetworkInterface.GetAllNetworkInterfaces()
                    .Where(n => n.OperationalStatus == OperationalStatus.Up
                                && n.NetworkInterfaceType != NetworkInterfaceType.Loopback
                                && n.NetworkInterfaceType != NetworkInterfaceType.Tunnel)
                    .OrderByDescending(n => n.Speed)
                    .FirstOrDefault();
            }
            catch
            {
                return null;
            }
        }

        private static string? FindNetworkInstanceName(NetworkInterface nic)
        {
            try
            {
                var category = new PerformanceCounterCategory("Network Interface");
                var instances = category.GetInstanceNames();

                var match = instances.FirstOrDefault(n => n.Equals(nic.Description, StringComparison.OrdinalIgnoreCase));
                if (!string.IsNullOrWhiteSpace(match)) return match;

                match = instances.FirstOrDefault(n => n.IndexOf(nic.Description, StringComparison.OrdinalIgnoreCase) >= 0);
                if (!string.IsNullOrWhiteSpace(match)) return match;

                match = instances.FirstOrDefault(n => n.IndexOf(nic.Name, StringComparison.OrdinalIgnoreCase) >= 0);
                return match;
            }
            catch
            {
                return null;
            }
        }

        private void DisposeCounters()
        {
            _cpuCounter?.Dispose();
            _memoryAvailableCounter?.Dispose();
            _diskCounter?.Dispose();
            _netRecvCounter?.Dispose();
            _netSentCounter?.Dispose();
        }

        private void LaunchDashboard_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoDashboard();
        }

        private void OpenSettings_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoSettings();
        }

        private void HealthCheck_Click(object sender, RoutedEventArgs e)
        {
            UpdateMetrics();
        }

        private void OpenAbout_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.GoAbout();
        }

        // Top bar events
        private void TopBar_BackClicked(object sender, RoutedEventArgs e)
        {
            // No back on startup
        }

        private void TopBar_HomeClicked(object sender, RoutedEventArgs e)
        {
            // Already home
        }

        private void TopBar_SettingsClicked(object sender, RoutedEventArgs e)
        {
            OpenSettings_Click(sender, e);
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers == ModifierKeys.Alt)
            {
                if (e.Key == Key.S)
                {
                    OpenSettings_Click(sender, new RoutedEventArgs());
                    e.Handled = true;
                }
                else if (e.Key == Key.D)
                {
                    LaunchDashboard_Click(sender, new RoutedEventArgs());
                    e.Handled = true;
                }
            }
        }

        private static string GetString(string key, string fallback)
        {
            return Application.Current?.TryFindResource(key) as string ?? fallback;
        }

        private static string Format(string key, string fallback, params object[] args)
        {
            var template = GetString(key, fallback);
            try
            {
                return string.Format(CultureInfo.CurrentCulture, template, args);
            }
            catch
            {
                return string.Format(CultureInfo.CurrentCulture, fallback, args);
            }
        }

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern bool GlobalMemoryStatusEx(ref MEMORYSTATUSEX lpBuffer);

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
        private struct MEMORYSTATUSEX
        {
            public uint dwLength;
            public uint dwMemoryLoad;
            public ulong ullTotalPhys;
            public ulong ullAvailPhys;
            public ulong ullTotalPageFile;
            public ulong ullAvailPageFile;
            public ulong ullTotalVirtual;
            public ulong ullAvailVirtual;
            public ulong ullAvailExtendedVirtual;
        }
    }
}
