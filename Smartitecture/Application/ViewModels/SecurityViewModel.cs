using System;
using System.Text.Json;
using System.Threading.Tasks;
using Smartitecture.Services;
using Smartitecture.Services.AgentTools;

namespace Smartitecture.ViewModels
{
    /// <summary>
    /// ViewModel for the security and optimization features.
    /// </summary>
    public class SecurityViewModel : ViewModelBase
    {
        private readonly IAgentService _agentService;
        
        // Security check results
        private bool _isSecurityResultVisible;
        private string _securityResultTitle;
        private string _securityResultContent;
        
        // Optimization results
        private bool _isOptimizationResultVisible;
        private string _optimizationResultTitle;
        private string _optimizationResultContent;
        
        // Alert explanation
        private bool _isAlertExplanationVisible;
        private string _alertExplanationTitle;
        private string _alertExplanationContent;
        
        /// <summary>
        /// Initializes a new instance of the SecurityViewModel class.
        /// </summary>
        /// <param name="agentService">The agent service</param>
        public SecurityViewModel(IAgentService agentService)
        {
            _agentService = agentService ?? throw new ArgumentNullException(nameof(agentService));
        }
        
        #region Security Check Properties
        
        /// <summary>
        /// Gets or sets whether the security check result is visible.
        /// </summary>
        public bool IsSecurityResultVisible
        {
            get => _isSecurityResultVisible;
            set => SetProperty(ref _isSecurityResultVisible, value);
        }
        
        /// <summary>
        /// Gets or sets the security check result title.
        /// </summary>
        public string SecurityResultTitle
        {
            get => _securityResultTitle;
            set => SetProperty(ref _securityResultTitle, value);
        }
        
        /// <summary>
        /// Gets or sets the security check result content.
        /// </summary>
        public string SecurityResultContent
        {
            get => _securityResultContent;
            set => SetProperty(ref _securityResultContent, value);
        }
        
        #endregion
        
        #region Optimization Properties
        
        /// <summary>
        /// Gets or sets whether the optimization result is visible.
        /// </summary>
        public bool IsOptimizationResultVisible
        {
            get => _isOptimizationResultVisible;
            set => SetProperty(ref _isOptimizationResultVisible, value);
        }
        
        /// <summary>
        /// Gets or sets the optimization result title.
        /// </summary>
        public string OptimizationResultTitle
        {
            get => _optimizationResultTitle;
            set => SetProperty(ref _optimizationResultTitle, value);
        }
        
        /// <summary>
        /// Gets or sets the optimization result content.
        /// </summary>
        public string OptimizationResultContent
        {
            get => _optimizationResultContent;
            set => SetProperty(ref _optimizationResultContent, value);
        }
        
        #endregion
        
        #region Alert Explanation Properties
        
        /// <summary>
        /// Gets or sets whether the alert explanation is visible.
        /// </summary>
        public bool IsAlertExplanationVisible
        {
            get => _isAlertExplanationVisible;
            set => SetProperty(ref _isAlertExplanationVisible, value);
        }
        
        /// <summary>
        /// Gets or sets the alert explanation title.
        /// </summary>
        public string AlertExplanationTitle
        {
            get => _alertExplanationTitle;
            set => SetProperty(ref _alertExplanationTitle, value);
        }
        
        /// <summary>
        /// Gets or sets the alert explanation content.
        /// </summary>
        public string AlertExplanationContent
        {
            get => _alertExplanationContent;
            set => SetProperty(ref _alertExplanationContent, value);
        }
        
        #endregion
        
        #region Methods
        
        /// <summary>
        /// Runs a security check with the specified detail level.
        /// </summary>
        /// <param name="detailed">Whether to perform a detailed check</param>
        public async Task RunSecurityCheckAsync(bool detailed)
        {
            try
            {
                // Show loading state
                SecurityResultTitle = "Running Security Check...";
                SecurityResultContent = "Please wait while we check your system for security issues...";
                IsSecurityResultVisible = true;
                
                // Create parameters for the security check
                var parameters = new { detailed = detailed };
                var jsonParams = JsonSerializer.Serialize(parameters);
                var jsonElement = JsonDocument.Parse(jsonParams).RootElement;
                
                // Run the security check
                string result = await SecurityTools.Tools[0].Handler(jsonElement);
                
                // Update UI with results
                SecurityResultTitle = "Security Check Complete";
                SecurityResultContent = result;
            }
            catch (Exception ex)
            {
                // Handle errors
                SecurityResultTitle = "Error Running Security Check";
                SecurityResultContent = $"An error occurred: {ex.Message}";
            }
            finally
            {
                // Ensure results are visible
                IsSecurityResultVisible = true;
            }
        }
        
        /// <summary>
        /// Runs system optimization with the specified aggressiveness.
        /// </summary>
        /// <param name="aggressive">Whether to be aggressive in closing applications</param>
        public async Task RunOptimizationAsync(bool aggressive)
        {
            try
            {
                // Show loading state
                OptimizationResultTitle = "Optimizing System...";
                OptimizationResultContent = "Please wait while we optimize your system by closing unnecessary applications...";
                IsOptimizationResultVisible = true;
                
                // Create parameters for the optimization
                var parameters = new { aggressive = aggressive };
                var jsonParams = JsonSerializer.Serialize(parameters);
                var jsonElement = JsonDocument.Parse(jsonParams).RootElement;
                
                // Run the optimization
                string result = await SecurityTools.Tools[1].Handler(jsonElement);
                
                // Update UI with results
                OptimizationResultTitle = "System Optimization Complete";
                OptimizationResultContent = result;
            }
            catch (Exception ex)
            {
                // Handle errors
                OptimizationResultTitle = "Error Optimizing System";
                OptimizationResultContent = $"An error occurred: {ex.Message}";
            }
            finally
            {
                // Ensure results are visible
                IsOptimizationResultVisible = true;
            }
        }
        
        /// <summary>
        /// Explains a security alert based on the provided text.
        /// </summary>
        /// <param name="alertText">The text of the alert to explain</param>
        public async Task ExplainAlertAsync(string alertText)
        {
            try
            {
                // Show loading state
                AlertExplanationTitle = "Analyzing Alert...";
                AlertExplanationContent = "Please wait while we analyze the security alert...";
                IsAlertExplanationVisible = true;
                
                // Create parameters for the alert explanation
                var parameters = new { alert_text = alertText };
                var jsonParams = JsonSerializer.Serialize(parameters);
                var jsonElement = JsonDocument.Parse(jsonParams).RootElement;
                
                // Get the explanation
                string result = await SecurityTools.Tools[2].Handler(jsonElement);
                
                // Update UI with results
                AlertExplanationTitle = "Alert Analysis Complete";
                AlertExplanationContent = result;
            }
            catch (Exception ex)
            {
                // Handle errors
                AlertExplanationTitle = "Error Analyzing Alert";
                AlertExplanationContent = $"An error occurred: {ex.Message}";
            }
            finally
            {
                // Ensure results are visible
                IsAlertExplanationVisible = true;
            }
        }
        
        #endregion
    }
}
