using System;
using System.Threading.Tasks;
using AIPal.Application.Network.Services;
using AIPal.Application.Network.Tools;

namespace AIPal.Application.Commands
{
    /// <summary>
    /// Command to check WiFi security.
    /// </summary>
    public class CheckWiFiSecurityCommand : ICommand<string>
    {
        private readonly NetworkMonitorService _networkMonitorService;
        
        /// <summary>
        /// Initializes a new instance of the <see cref="CheckWiFiSecurityCommand"/> class.
        /// </summary>
        /// <param name="networkMonitorService">The network monitor service.</param>
        public CheckWiFiSecurityCommand(NetworkMonitorService networkMonitorService)
        {
            _networkMonitorService = networkMonitorService ?? throw new ArgumentNullException(nameof(networkMonitorService));
        }
        
        /// <summary>
        /// Executes the command to check WiFi security.
        /// </summary>
        /// <returns>A string containing the WiFi security assessment.</returns>
        public async Task<string> ExecuteAsync()
        {
            // Get network information
            var networkInfo = _networkMonitorService.GetNetworkInfo();
            
            // Check if connected to WiFi
            if (networkInfo.ConnectionType != "Wi-Fi" || !networkInfo.IsConnected)
            {
                return "You are not currently connected to a WiFi network. Please connect to a WiFi network and try again.";
            }
            
            // Test internet connection
            var connectionTest = await _networkMonitorService.TestInternetConnectionAsync();
            if (!connectionTest.HasInternetAccess)
            {
                return "You are connected to a WiFi network, but there appears to be no internet access. " +
                       "This could be due to an issue with your WiFi connection or your internet service provider.";
            }
            
            // Use the NetworkSecurityTools to check WiFi security
            string securityAssessment = await NetworkSecurityTools.CheckWiFiSecurity(networkInfo);
            
            return securityAssessment;
        }
    }
}
