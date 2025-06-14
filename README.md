# Smartitecture - Your Windows AI Companion

[![Smartitecture CI](https://github.com/tarun-vignes/Smartitecture/actions/workflows/dotnet-desktop.yml/badge.svg)](https://github.com/tarun-vignes/Smartitecture/actions/workflows/dotnet-desktop.yml)

Smartitecture is a modern Windows desktop application designed to help everyone better understand their computers, avoid scams, and optimize performance. With its AI-powered assistant, Smartitecture provides natural conversation and system task automation in a simple, accessible interface.

## Technical Overview

Smartitecture is built on .NET 8.0 using a hybrid architecture that combines WinUI 3, WPF, and ASP.NET Core technologies. This document provides a comprehensive technical overview of the codebase structure, architecture, and implementation details for developers joining the project.

## Architecture

Smartitecture follows a clean architecture pattern with clear separation of concerns:

### Core Layers

1. **UI Layer**: WinUI 3 application with XAML-based UI components
2. **Application Layer**: Business logic, services, and application workflows
3. **Domain Layer**: Core business entities and interfaces
4. **Infrastructure Layer**: External service implementations, data access, and platform-specific code

### Key Architectural Patterns

- **MVVM (Model-View-ViewModel)**: UI architecture pattern using CommunityToolkit.Mvvm
- **Mediator Pattern**: Using MediatR for request/response workflows and decoupled communication
- **Dependency Injection**: Microsoft.Extensions.DependencyInjection for service resolution
- **Command Pattern**: For encapsulating system operations and user actions

## Project Structure

```
Smartitecture/
├── Application/                # Core application logic
│   ├── API/                    # ASP.NET Core API components
│   ├── Commands/               # Command handlers and definitions
│   ├── Network/                # Network monitoring and analysis
│   ├── Security/               # Security monitoring and protection
│   ├── Services/               # Core application services
│   ├── UI/                     # UI-related logic
│   └── ViewModels/             # MVVM view models
├── Assets/                     # Application assets and resources
├── Smartitecture/              # Main WinUI application project
│   ├── Application/            # Application-specific implementations
│   ├── Services/               # Platform-specific service implementations
│   ├── Themes/                 # UI themes and styles
│   └── UI/                     # UI components and pages
├── Smartitecture.Tests/        # Unit and integration tests
└── Website/                    # Companion website (if applicable)
```

## Key Technologies

### UI Framework
- **WinUI 3**: Modern native UI framework for Windows applications
- **Windows App SDK**: Provides access to Windows platform features
- **WPF Integration**: For compatibility with existing WPF components
- **Windows Forms Integration**: For specific system integration scenarios

### Backend Services
- **ASP.NET Core**: For hosting internal API endpoints
- **Azure OpenAI SDK**: For AI-powered features and natural language processing
- **System.Management**: For WMI access to system information

### Architecture & Patterns
- **CommunityToolkit.Mvvm**: MVVM implementation with source generators
- **MediatR**: For implementing the mediator pattern
- **Microsoft.Extensions.DependencyInjection**: IoC container
- **Microsoft.Extensions.Hosting**: For background service hosting

## Core Components

### API Host Service

The `ApiHostService` class hosts an ASP.NET Core web server within the desktop application, enabling:
- Local API endpoints for web-based components
- Integration with web technologies
- Communication with external services

```csharp
public class ApiHostService : IDisposable
{
    // Hosts an ASP.NET Core web server within the desktop application
    // Manages API endpoints for internal and external communication
}
```

### Security Monitoring

The `SecurityMonitorService` provides comprehensive security monitoring:
- Windows Defender status monitoring
- Suspicious process detection
- Security recommendations
- Malware scanning integration

```csharp
public class SecurityMonitorService
{
    // Monitors system security status
    // Provides security recommendations
    // Detects suspicious activities
}
```

### LLM Integration

The `ILLMService` interface and implementations provide AI capabilities:
- Natural language processing
- Command parsing and execution
- Context-aware responses
- Integration with Azure OpenAI

```csharp
public interface ILLMService
{
    // Processes natural language input
    // Generates responses based on context
    // Extracts commands from user input
}
```

### Command System

The command system uses a combination of the Command and Mediator patterns:
- `CommandMapper`: Maps natural language to executable commands
- `ISystemCommand`: Interface for all system commands
- Command handlers: Implement specific system operations

## Namespace Structure

The project recently transitioned from the `AIPal` namespace to `Smartitecture`. To maintain compatibility, namespace aliases are used:

```csharp
// In NamespaceFixup.cs and GlobalUsings.cs
using AIPal = Smartitecture;
using AIPal.API = Smartitecture.Application.API;
using AIPal.Application = Smartitecture.Application;
// Additional namespace aliases...
```

## Testing

The `Smartitecture.Tests` project contains unit and integration tests:
- Command tests: Verify command execution logic
- Security tests: Validate security monitoring features
- Service tests: Test core application services

Tests use:
- **xUnit**: Testing framework
- **Moq**: Mocking library for isolating components

## Build Configuration

The solution uses MSBuild with custom configuration in `Directory.Build.props`:
- Target framework: .NET 8.0 with Windows 10.0.19041.0 minimum version
- Runtime identifiers: win-x86, win-x64, win-arm64
- Platform targets: x86, x64, ARM64
- Language version: Latest C# features

## Development Environment

### Requirements
- Visual Studio 2022 (17.8 or later)
- .NET SDK 8.0 or later
- Windows App SDK 1.4 or later
- Windows 10 version 1809 (build 17763) or later

### Required Workloads
- .NET Desktop Development
- Universal Windows Platform development
- Windows App SDK C# Templates
- ASP.NET and web development

## Key Features

### 🤖 Intelligent Assistance
- Conversational AI interface powered by advanced LLM technology
- Natural language processing optimized for elderly users
- Simple, jargon-free explanations of technical concepts
- Voice interaction support for accessibility

### 🔒 Security & Privacy Protection
- Malware detection with simple, clear explanations
- Scam detection focused on common threats targeting seniors
- Security alert analyzer that explains popups in plain language
- Wi-Fi security assessment and recommendations
- Educational content about online safety

### 🖥️ System Optimization
- Hardware-aware diagnostics that adapt to different computer capabilities
- Automatic closing of unnecessary background applications
- Performance analysis with hardware-specific insights
- Startup program analysis for boot time optimization
- Disk health checking with storage recommendations

### 🔍 Screen Analysis
- Screen capture and analysis without requiring screenshots
- Context-aware help based on what's currently on screen
- Text extraction from screen content
- UI navigation assistance for confusing interfaces

### 🌐 Network & Connectivity
- Wi-Fi troubleshooting with step-by-step guidance
- Network status monitoring with clear visual indicators
- Security term explanations in plain language
- Public Wi-Fi safety education

### 🎨 Accessibility Features
- Adjustable text size for better readability
- High contrast mode for visual impairments
- Read-aloud functionality for those with vision difficulties
- Reduced motion options for less distracting interfaces
- Dark/light mode with simple toggle

### 🌍 Multi-Language Support
- Support for 30 languages with native script display
- Easy language selection interface
- Persistent language settings across application restarts
- Both native language names and English names for easy identification

## Getting Started

### System Requirements

- Windows 10 version 1809 or higher (Windows 11 recommended for best experience)
- 4GB RAM minimum (8GB recommended)
- 500MB free disk space
- Internet connection for AI features
- .NET 7.0 or higher

### Installation

1. Download the latest installer from the [Releases](https://github.com/tarun-vignes/Smartitecture/releases) page
2. Run the installer and follow the on-screen instructions
3. Launch Smartitecture from the Start menu or desktop shortcut
4. Complete the initial setup wizard designed for elderly users

### For Developers

1. Clone the repository: `git clone https://github.com/tarun-vignes/Smartitecture.git`
2. Open `Smartitecture.sln` in Visual Studio 2022
3. Install required workloads if prompted:
   - Windows App SDK
   - .NET Desktop Development
   - Universal Windows Platform development
4. Restore NuGet packages
5. Build and run the solution

## Usage Guide

### First-Time Setup

When you first launch Smartitecture, you'll be guided through a simple setup process:

1. **Language Selection**: Choose from 30 supported languages
2. **Accessibility Preferences**: Set text size, contrast, and other accessibility options
3. **Theme Selection**: Choose between light, dark, or system theme
4. **Getting Started Tour**: A brief introduction to Smartitecture's features

### Common Tasks

#### Security Checks

1. Navigate to the "Security" tab
2. Select "Check for Malware" or "Optimize System"
3. Follow the simple, jargon-free instructions

#### Screen Help

1. When you encounter a confusing screen or message
2. Navigate to the "Screen Help" tab
3. Click "Analyze Current Screen"
4. Smartitecture will explain what you're seeing and suggest actions

#### Wi-Fi Troubleshooting

1. If you're having internet problems
2. Ask Smartitecture about your Wi-Fi in the chat interface
3. Follow the step-by-step troubleshooting guidance

## Technology

Smartitecture is built using modern technologies to provide a seamless, accessible experience:

- **Frontend**: WinUI 3 with XAML for a modern, accessible interface
- **Backend**: .NET 7 for robust performance and security
- **AI Engine**: Azure OpenAI Service with function calling capabilities
- **Security**: Windows Security APIs for malware detection and system protection
- **Accessibility**: Built on Microsoft's Accessibility Standards

## Security &## Privacy & Security

Smartitecture is designed with privacy and security as core principles:

- **Local Processing**: Screen analysis and system diagnostics happen locally on your device
- **Minimal Data Collection**: Only essential data is sent to AI services
- **Transparent Permissions**: Clear explanations of why permissions are needed
- **Security Best Practices**:
  - Digitally signed binaries

## Getting Started

- **Documentation**: [Full User Guide](https://github.com/tarun-vignes/Smartitecture/wiki)
- **Issue Reporting**: [GitHub Issues](https://github.com/tarun-vignes/Smartitecture/issues)
- **Discussion**: [Community Forum](https://github.com/tarun-vignes/Smartitecture/discussions)

## Contributing

We welcome contributions to make Smartitecture better for elderly users:

1. Fork the repository
2. Create a feature branch: `git checkout -b feature/amazing-feature`
3. Commit your changes: `git commit -m 'Add some amazing feature'`
4. Push to the branch: `git push origin feature/amazing-feature`
5. Open a Pull Request

Please read our [Contributing Guidelines](CONTRIBUTING.md) for details.

## Deployment

### Website Deployment with Netlify

The Smartitecture companion website is deployed to Netlify, showcasing all the elderly-friendly features implemented in the desktop application:

1. The website is built using Vite and React
2. Deployment is configured via the `netlify.toml` file in the Website directory
3. The website is accessible at: https://aipal-companion.windsurf.build

[![Netlify Status](https://api.netlify.com/api/v1/badges/0c565a6b-0b0d-45d7-b8e2-142d0a020b64/deploy-status)](https://app.netlify.com/sites/smartitecture-companion/deploys)

#### Features Showcased on the Website

- Friendly blue robot logo and accessible branding
- Multi-language support with 30 languages
- Dark/light mode options for better visibility
- Information about security tools and system diagnostics
- Comprehensive documentation for elderly users

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Acknowledgments

- Microsoft for Windows App SDK and Azure OpenAI Service
- The open source community for various libraries and tools
- All contributors who have helped make Smartitecture more accessible for elderly users

## Developer Documentation

### Getting Started for Developers

#### Setting Up Your Development Environment

1. **Clone the Repository**
   ```bash
   git clone https://github.com/tarun-vignes/Smartitecture.git
   cd Smartitecture
   ```

2. **Install Prerequisites**
   - Install Visual Studio 2022 with required workloads
   - Install .NET SDK 8.0 or later
   - Install Windows App SDK 1.4 or later

3. **Configure Azure OpenAI (if using AI features)**
   - Create an Azure OpenAI resource
   - Configure the connection in `appsettings.json` or user secrets

4. **Build the Solution**
   ```bash
   dotnet restore
   dotnet build
   ```

5. **Run the Application**
   - Open the solution in Visual Studio
   - Set Smartitecture as the startup project
   - Press F5 to run in debug mode

### Project Configuration

#### appsettings.json

The application uses `appsettings.json` for configuration:

```json
{
  "AzureOpenAI": {
    "Endpoint": "https://your-resource.openai.azure.com/",
    "DeploymentName": "your-deployment",
    "ApiKey": "your-api-key"
  },
  "ApiHost": {
    "Port": 5000,
    "EnableCors": true
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning"
    }
  }
}
```

#### User Secrets (Development)

For local development, use user secrets to store sensitive information:

```bash
dotnet user-secrets init --project Smartitecture/Smartitecture.csproj
dotnet user-secrets set "AzureOpenAI:ApiKey" "your-api-key" --project Smartitecture/Smartitecture.csproj
```

### Common Issues and Solutions

#### Build Errors

##### Namespace Conflicts

The project recently transitioned from `AIPal` to `Smartitecture` namespace. If you encounter namespace conflicts:

1. Check `NamespaceFixup.cs` and `GlobalUsings.cs` for proper namespace aliases
2. Ensure using directives are correctly formatted (use `using AIPal = Smartitecture;` not `global using alias`)
3. Verify that all project references are correctly set up

##### Runtime Identifier Issues

If you encounter runtime identifier errors:

1. Ensure `Directory.Build.props` has the correct runtime identifiers:
   ```xml
   <RuntimeIdentifiers>win-x86;win-x64;win-arm64</RuntimeIdentifiers>
   ```
2. Verify that project files use the same runtime identifiers
3. Clean and rebuild the solution

##### WinUI and WPF Integration Issues

When working with both WinUI and WPF:

1. Use the correct Application class with full namespace qualification
2. Use namespace aliases to avoid conflicts (e.g., `WinUIApplication` vs `WPFApplication`)
3. Ensure all required framework references are included

#### Runtime Issues

##### API Host Service Not Starting

If the internal API server doesn't start:

1. Check if the port is already in use
2. Verify that ASP.NET Core dependencies are correctly referenced
3. Check for firewall restrictions

##### Azure OpenAI Connection Issues

If AI features aren't working:

1. Verify Azure OpenAI configuration in appsettings.json
2. Check API key and endpoint URL
3. Ensure the deployment name is correct
4. Verify network connectivity to Azure services

### Code Architecture Details

#### Core Components

1. **UI Layer**
   - `Smartitecture/UI/`: WinUI 3 pages and controls
   - `Application/UI/`: Shared UI components and helpers
   - `Smartitecture/Themes/`: Application themes and styles

2. **Application Layer**
   - `Application/Services/`: Core application services
   - `Application/ViewModels/`: MVVM view models
   - `Application/Commands/`: Command handlers and definitions

3. **API Layer**
   - `Application/API/`: ASP.NET Core API components
   - `Application/API/Services/`: API service implementations
   - `Application/API/Models/`: API data transfer objects

4. **Security Layer**
   - `Application/Security/`: Security monitoring and protection
   - `Application/Security/Services/`: Security service implementations
   - `Application/Security/Tools/`: Security utilities and helpers

5. **Network Layer**
   - `Application/Network/`: Network monitoring and analysis
   - `Application/Network/Services/`: Network service implementations
   - `Application/Network/Tools/`: Network utilities and helpers

#### Key Interfaces and Classes

1. **Services**
   - `ILLMService`: Interface for language model services
   - `IAgentService`: Interface for agent-based operations
   - `SecurityMonitorService`: Monitors system security
   - `NetworkMonitorService`: Monitors network connectivity
   - `ApiHostService`: Hosts internal API endpoints

2. **ViewModels**
   - `MainViewModel`: Main application view model
   - `SecurityViewModel`: Security features view model
   - `ScreenAnalysisViewModel`: Screen analysis view model

3. **Commands**
   - `ISystemCommand`: Interface for all system commands
   - `CommandMapper`: Maps natural language to commands

### Testing Strategy

1. **Unit Tests**
   - Located in `Smartitecture.Tests/`
   - Uses xUnit as the testing framework
   - Uses Moq for mocking dependencies

2. **Test Categories**
   - `Commands/`: Tests for command handlers
   - `Security/`: Tests for security features
   - `Services/`: Tests for core services

### Contributing Guidelines

#### Coding Standards

1. **C# Coding Style**
   - Follow Microsoft's C# coding conventions
   - Use meaningful names for classes, methods, and variables
   - Add XML documentation comments for public APIs
   - Use nullable reference types appropriately

2. **Architecture Guidelines**
   - Maintain separation of concerns between layers
   - Use dependency injection for service resolution
   - Follow MVVM pattern for UI components
   - Use MediatR for cross-component communication

3. **Testing Requirements**
   - Write unit tests for new functionality
   - Ensure tests are isolated and don't depend on external services
   - Mock external dependencies using Moq

#### Pull Request Process

1. Create a feature branch from `main`
2. Implement your changes with appropriate tests
3. Ensure all tests pass locally
4. Update documentation as needed
5. Submit a pull request with a clear description of changes
6. Address any code review feedback
