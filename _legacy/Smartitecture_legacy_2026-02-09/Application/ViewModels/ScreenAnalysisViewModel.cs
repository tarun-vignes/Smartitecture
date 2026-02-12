using System;
using System.Text.Json;
using System.Threading.Tasks;
using Smartitecture.Services;
using Smartitecture.Services.AgentTools;

namespace Smartitecture.ViewModels
{
    /// <summary>
    /// ViewModel for the screen analysis features.
    /// </summary>
    public class ScreenAnalysisViewModel : ViewModelBase
    {
        private readonly IAgentService _agentService;
        
        // Analysis results
        private bool _isAnalysisResultVisible;
        private string _analysisResultTitle;
        private string _analysisResultContent;
        
        /// <summary>
        /// Initializes a new instance of the ScreenAnalysisViewModel class.
        /// </summary>
        /// <param name="agentService">The agent service</param>
        public ScreenAnalysisViewModel(IAgentService agentService)
        {
            _agentService = agentService ?? throw new ArgumentNullException(nameof(agentService));
        }
        
        #region Properties
        
        /// <summary>
        /// Gets or sets whether the analysis result is visible.
        /// </summary>
        public bool IsAnalysisResultVisible
        {
            get => _isAnalysisResultVisible;
            set => SetProperty(ref _isAnalysisResultVisible, value);
        }
        
        /// <summary>
        /// Gets or sets the analysis result title.
        /// </summary>
        public string AnalysisResultTitle
        {
            get => _analysisResultTitle;
            set => SetProperty(ref _analysisResultTitle, value);
        }
        
        /// <summary>
        /// Gets or sets the analysis result content.
        /// </summary>
        public string AnalysisResultContent
        {
            get => _analysisResultContent;
            set => SetProperty(ref _analysisResultContent, value);
        }
        
        #endregion
        
        #region Methods
        
        /// <summary>
        /// Analyzes the screen with the specified parameters.
        /// </summary>
        /// <param name="captureFullScreen">Whether to capture the full screen or just the active window</param>
        /// <param name="analysisType">The type of analysis to perform</param>
        public async Task AnalyzeScreenAsync(bool captureFullScreen, string analysisType)
        {
            try
            {
                // Show loading state
                AnalysisResultTitle = "Analyzing Screen...";
                AnalysisResultContent = "Please wait while we analyze your screen...";
                IsAnalysisResultVisible = true;
                
                // Create parameters for the screen analysis
                var parameters = new { analysis_type = analysisType };
                var jsonParams = JsonSerializer.Serialize(parameters);
                var jsonElement = JsonDocument.Parse(jsonParams).RootElement;
                
                string result;
                
                if (captureFullScreen)
                {
                    // Capture and analyze the entire screen
                    result = await ScreenAnalysisTools.Tools[0].Handler(jsonElement);
                    AnalysisResultTitle = "Full Screen Analysis Complete";
                }
                else
                {
                    // Capture and analyze only the active window
                    result = await ScreenAnalysisTools.Tools[1].Handler(jsonElement);
                    AnalysisResultTitle = "Active Window Analysis Complete";
                }
                
                // Update UI with results
                AnalysisResultContent = result;
            }
            catch (Exception ex)
            {
                // Handle errors
                AnalysisResultTitle = "Error Analyzing Screen";
                AnalysisResultContent = $"An error occurred: {ex.Message}";
            }
            finally
            {
                // Ensure results are visible
                IsAnalysisResultVisible = true;
            }
        }
        
        #endregion
    }
}
