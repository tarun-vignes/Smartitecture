// This file provides namespace aliases to fix the AIPal to Smartitecture namespace transition
// It will be included in all projects through Directory.Build.targets

// Main namespace aliases
using AIPal = Smartitecture;
using AIPal.API = Smartitecture.Application.API;
using AIPal.API.Services = Smartitecture.Application.API.Services;
using AIPal.Application = Smartitecture.Application;
using AIPal.Application.Network = Smartitecture.Application.Network;
using AIPal.Application.Network.Tools = Smartitecture.Application.Network.Tools;
using AIPal.Application.Security = Smartitecture.Application.Security;
using AIPal.Application.Security.Tools = Smartitecture.Application.Security.Tools;
using AIPal.ViewModels = Smartitecture.ViewModels;

// Common WPF/WinUI namespaces
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Controls;

// Windows UI Xaml namespaces
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

// Common interfaces
using System.ComponentModel;
using System.Windows.Input;

// MediatR interfaces for request/response pattern
using MediatR;
using MediatR.Pipeline;

// Common types
using ICommand = System.Windows.Input.ICommand;
using IValueConverter = System.Windows.Data.IValueConverter;
using IRequestHandler = MediatR.IRequestHandler;
using Tool = Smartitecture.Application.Models.Tool;
using AgentTool = Smartitecture.Application.Models.AgentTool;
using AgentAction = Smartitecture.Application.Models.AgentAction;
using QueryDto = Smartitecture.Application.API.Models.QueryDto;
using QueryResponseDto = Smartitecture.Application.API.Models.QueryResponseDto;
using AgentRequestDto = Smartitecture.Application.Models.AgentRequestDto;
using ToolParameter = Smartitecture.Application.Models.ToolParameter;
using IAgentService = Smartitecture.Application.Services.IAgentService;
using ILLMService = Smartitecture.Application.Services.ILLMService;
using CommandMapper = Smartitecture.Application.Services.CommandMapper;
using IQueryService = Smartitecture.Application.API.Services.IQueryService;
using ISystemCommand = Smartitecture.Application.Commands.ISystemCommand;
using SecurityMonitorService = Smartitecture.Application.Security.Services.SecurityMonitorService;
using NetworkMonitorService = Smartitecture.Application.Network.Services.NetworkMonitorService;
using HandlerRegistry = Smartitecture.Application.Services.HandlerRegistry;
using NotificationEventArgs = Smartitecture.Application.Services.NotificationEventArgs;
using AzureOpenAIOptions = Smartitecture.Application.Services.AzureOpenAIOptions;
using MainViewModel = Smartitecture.ViewModels.MainViewModel;
using ScreenAnalysisViewModel = Smartitecture.ViewModels.ScreenAnalysisViewModel;
using SecurityViewModel = Smartitecture.ViewModels.SecurityViewModel;
using AppTheme = Smartitecture.Services.AppTheme;
using ApiHost = Smartitecture.Services.ApiHost;

// Fix for Application namespace conflict
using WinUIApplication = Microsoft.UI.Xaml.Application;
using WPFApplication = System.Windows.Application;

// Testing frameworks
#if DEBUG
using Xunit;
using Moq;
using Xunit.Abstractions;
#endif

// Empty namespace declaration to group the aliases
namespace Smartitecture.NamespaceAliases
{
    // This namespace serves as a container for the aliases
    // All using directives are moved outside the namespace
}
