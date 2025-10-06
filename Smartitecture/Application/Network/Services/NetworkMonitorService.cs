using System;
using System.Net.NetworkInformation;
using System.Threading;
using System.Threading.Tasks;

namespace AIPal.Application.Network.Services
{
    /// <summary>
    /// Service for monitoring network status and providing network information.
    /// </summary>
    public class NetworkMonitorService
    {
        private Timer _monitorTimer;
        private bool _isNetworkAvailable;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="NetworkMonitorService"/> class.
        /// </summary>
        public NetworkMonitorService()
        {
            _isNetworkAvailable = NetworkInterface.GetIsNetworkAvailable();
            
            // Start monitoring network status
            _monitorTimer = new Timer(CheckNetworkStatus, null, TimeSpan.Zero, TimeSpan.FromSeconds(30));
            
            // Subscribe to network availability changes
            NetworkChange.NetworkAvailabilityChanged += OnNetworkAvailabilityChanged;
        }
        
        /// <summary>
        /// Event that is raised when the network status changes.
        /// </summary>
        public event EventHandler<NetworkStatusChangedEventArgs> NetworkStatusChanged;
        
        /// <summary>
        /// Gets a value indicating whether the network is available.
        /// </summary>
        public bool IsNetworkAvailable => _isNetworkAvailable;
        
        /// <summary>
        /// Gets information about the current network connection.
        /// </summary>
        /// <returns>A <see cref="NetworkInfo"/> object containing information about the current network connection.</returns>
        public NetworkInfo GetNetworkInfo()
        {
            var networkInfo = new NetworkInfo
            {
                IsConnected = NetworkInterface.GetIsNetworkAvailable()
            };
            
            try
            {
                // Get network interfaces
                NetworkInterface[] interfaces = NetworkInterface.GetAllNetworkInterfaces();
                
                foreach (NetworkInterface ni in interfaces)
                {
                    // Skip interfaces that are not operational
                    if (ni.OperationalStatus != OperationalStatus.Up)
                    {
                        continue;
                    }
                    
                    // Skip loopback adapters
                    if (ni.NetworkInterfaceType == NetworkInterfaceType.Loopback)
                    {
                        continue;
                    }
                    
                    // Check if this is a wireless connection
                    if (ni.NetworkInterfaceType == NetworkInterfaceType.Wireless80211)
                    {
                        networkInfo.ConnectionType = "Wi-Fi";
                        networkInfo.InterfaceName = ni.Name;
                        networkInfo.Description = ni.Description;
                        networkInfo.Speed = ni.Speed;
                        networkInfo.MacAddress = ni.GetPhysicalAddress().ToString();
                        
                        // Get IP addresses
                        IPInterfaceProperties ipProps = ni.GetIPProperties();
                        foreach (UnicastIPAddressInformation ip in ipProps.UnicastAddresses)
                        {
                            if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                            {
                                networkInfo.IPAddress = ip.Address.ToString();
                                break;
                            }
                        }
                        
                        // Found a wireless connection, no need to check other interfaces
                        break;
                    }
                    else if (ni.NetworkInterfaceType == NetworkInterfaceType.Ethernet)
                    {
                        networkInfo.ConnectionType = "Ethernet";
                        networkInfo.InterfaceName = ni.Name;
                        networkInfo.Description = ni.Description;
                        networkInfo.Speed = ni.Speed;
                        networkInfo.MacAddress = ni.GetPhysicalAddress().ToString();
                        
                        // Get IP addresses
                        IPInterfaceProperties ipProps = ni.GetIPProperties();
                        foreach (UnicastIPAddressInformation ip in ipProps.UnicastAddresses)
                        {
                            if (ip.Address.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork)
                            {
                                networkInfo.IPAddress = ip.Address.ToString();
                                break;
                            }
                        }
                        
                        // Continue checking for wireless connections
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting network info: {ex.Message}");
            }
            
            return networkInfo;
        }
        
        /// <summary>
        /// Tests the internet connection by pinging a reliable host.
        /// </summary>
        /// <returns>A <see cref="ConnectionTestResult"/> containing the test results.</returns>
        public async Task<ConnectionTestResult> TestInternetConnectionAsync()
        {
            var result = new ConnectionTestResult
            {
                IsConnected = NetworkInterface.GetIsNetworkAvailable()
            };
            
            if (!result.IsConnected)
            {
                result.Message = "No network connection available.";
                return result;
            }
            
            try
            {
                // Test connection by pinging reliable hosts
                string[] hosts = { "8.8.8.8", "1.1.1.1", "208.67.222.222" };
                bool pingSuccess = false;
                long totalRoundtripTime = 0;
                int successfulPings = 0;
                
                using (var ping = new Ping())
                {
                    foreach (string host in hosts)
                    {
                        try
                        {
                            PingReply reply = await ping.SendPingAsync(host, 3000);
                            
                            if (reply.Status == IPStatus.Success)
                            {
                                pingSuccess = true;
                                totalRoundtripTime += reply.RoundtripTime;
                                successfulPings++;
                            }
                        }
                        catch
                        {
                            // Ignore ping failures for individual hosts
                        }
                    }
                }
                
                result.HasInternetAccess = pingSuccess;
                
                if (pingSuccess)
                {
                    result.AverageLatency = successfulPings > 0 ? totalRoundtripTime / successfulPings : 0;
                    result.Message = $"Internet connection is working. Average latency: {result.AverageLatency}ms";
                }
                else
                {
                    result.Message = "Network connection available, but no internet access.";
                }
            }
            catch (Exception ex)
            {
                result.HasInternetAccess = false;
                result.Message = $"Error testing internet connection: {ex.Message}";
            }
            
            return result;
        }
        
        /// <summary>
        /// Disposes the network monitor service.
        /// </summary>
        public void Dispose()
        {
            _monitorTimer?.Dispose();
            NetworkChange.NetworkAvailabilityChanged -= OnNetworkAvailabilityChanged;
        }
        
        private void CheckNetworkStatus(object state)
        {
            bool isAvailable = NetworkInterface.GetIsNetworkAvailable();
            
            if (isAvailable != _isNetworkAvailable)
            {
                _isNetworkAvailable = isAvailable;
                OnNetworkStatusChanged(new NetworkStatusChangedEventArgs(isAvailable));
            }
        }
        
        private void OnNetworkAvailabilityChanged(object sender, NetworkAvailabilityEventArgs e)
        {
            _isNetworkAvailable = e.IsAvailable;
            OnNetworkStatusChanged(new NetworkStatusChangedEventArgs(e.IsAvailable));
        }
        
        private void OnNetworkStatusChanged(NetworkStatusChangedEventArgs e)
        {
            NetworkStatusChanged?.Invoke(this, e);
        }
    }
    
    /// <summary>
    /// Event arguments for network status changes.
    /// </summary>
    public class NetworkStatusChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="NetworkStatusChangedEventArgs"/> class.
        /// </summary>
        /// <param name="isAvailable">Whether the network is available.</param>
        public NetworkStatusChangedEventArgs(bool isAvailable)
        {
            IsAvailable = isAvailable;
        }
        
        /// <summary>
        /// Gets a value indicating whether the network is available.
        /// </summary>
        public bool IsAvailable { get; }
    }
    
    /// <summary>
    /// Information about a network connection.
    /// </summary>
    public class NetworkInfo
    {
        /// <summary>
        /// Gets or sets a value indicating whether the device is connected to a network.
        /// </summary>
        public bool IsConnected { get; set; }
        
        /// <summary>
        /// Gets or sets the type of connection (Wi-Fi, Ethernet, etc.).
        /// </summary>
        public string ConnectionType { get; set; }
        
        /// <summary>
        /// Gets or sets the name of the network interface.
        /// </summary>
        public string InterfaceName { get; set; }
        
        /// <summary>
        /// Gets or sets the description of the network interface.
        /// </summary>
        public string Description { get; set; }
        
        /// <summary>
        /// Gets or sets the speed of the connection in bits per second.
        /// </summary>
        public long Speed { get; set; }
        
        /// <summary>
        /// Gets or sets the MAC address of the network interface.
        /// </summary>
        public string MacAddress { get; set; }
        
        /// <summary>
        /// Gets or sets the IP address of the network interface.
        /// </summary>
        public string IPAddress { get; set; }
    }
    
    /// <summary>
    /// Results of a connection test.
    /// </summary>
    public class ConnectionTestResult
    {
        /// <summary>
        /// Gets or sets a value indicating whether the device is connected to a network.
        /// </summary>
        public bool IsConnected { get; set; }
        
        /// <summary>
        /// Gets or sets a value indicating whether the device has internet access.
        /// </summary>
        public bool HasInternetAccess { get; set; }
        
        /// <summary>
        /// Gets or sets the average latency of the connection in milliseconds.
        /// </summary>
        public long AverageLatency { get; set; }
        
        /// <summary>
        /// Gets or sets a message describing the connection status.
        /// </summary>
        public string Message { get; set; }
    }
}
