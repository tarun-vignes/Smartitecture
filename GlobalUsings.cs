// Global using directives to fix namespace issues
namespace Smartitecture.GlobalUsings
{
    using AIPal = Smartitecture;
    using AIPal.API = Smartitecture.Application.API;
    using AIPal.Application = Smartitecture.Application;
    using AIPal.Application.Network = Smartitecture.Application.Network;
    using AIPal.Application.Security = Smartitecture.Application.Security;
    using AIPal.ViewModels = Smartitecture.ViewModels;

    // System.Windows namespaces
    using System.Windows;
    using System.Windows.Data;
    using System.Windows.Media;
    using System.Windows.Controls;

    // Common interfaces
    using System.ComponentModel;
    using System.Windows.Input;

    // Testing frameworks
    using Xunit;
    using Moq;
}
