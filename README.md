# AIPal - Your Windows AI Companion for Elderly Users

AIPal is a modern Windows desktop application designed specifically to help elderly and less tech-savvy users understand their computers, avoid scams, and optimize performance. With its AI-powered assistant, AIPal provides natural conversation and system task automation in a simple, accessible interface.

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

1. Download the latest installer from the [Releases](https://github.com/tarun-vignes/AIPal/releases) page
2. Run the installer and follow the on-screen instructions
3. Launch AIPal from the Start menu or desktop shortcut
4. Complete the initial setup wizard designed for elderly users

### For Developers

1. Clone the repository: `git clone https://github.com/tarun-vignes/AIPal.git`
2. Open `AIPal.sln` in Visual Studio 2022
3. Install required workloads if prompted:
   - Windows App SDK
   - .NET Desktop Development
   - Universal Windows Platform development
4. Restore NuGet packages
5. Build and run the solution

## Usage Guide

### First-Time Setup

When you first launch AIPal, you'll be guided through a simple setup process:

1. **Language Selection**: Choose from 30 supported languages
2. **Accessibility Preferences**: Set text size, contrast, and other accessibility options
3. **Theme Selection**: Choose between light, dark, or system theme
4. **Getting Started Tour**: A brief introduction to AIPal's features

### Common Tasks

#### Security Checks

1. Navigate to the "Security" tab
2. Select "Check for Malware" or "Optimize System"
3. Follow the simple, jargon-free instructions

#### Screen Help

1. When you encounter a confusing screen or message
2. Navigate to the "Screen Help" tab
3. Click "Analyze Current Screen"
4. AIPal will explain what you're seeing and suggest actions

#### Wi-Fi Troubleshooting

1. If you're having internet problems
2. Ask AIPal about your Wi-Fi in the chat interface
3. Follow the step-by-step troubleshooting guidance

## Technology

AIPal is built using modern technologies to provide a seamless, accessible experience:

- **Frontend**: WinUI 3 with XAML for a modern, accessible interface
- **Backend**: .NET 7 for robust performance and security
- **AI Engine**: Azure OpenAI Service with function calling capabilities
- **Security**: Windows Security APIs for malware detection and system protection
- **Accessibility**: Built on Microsoft's Accessibility Standards

## Security & Privacy

AIPal prioritizes the security and privacy of elderly users:

- **Local Processing**: Screen analysis and system diagnostics happen locally on your device
- **Minimal Data Collection**: Only essential data is sent to AI services
- **Transparent Permissions**: Clear explanations of why permissions are needed
- **Security Best Practices**:
  - Digitally signed binaries
  - UAC elevation for admin tasks
  - Input validation and command sanitization
  - Clear permission dialogs
  - Secure API access

## Support & Community

- **Documentation**: [Full User Guide](https://github.com/tarun-vignes/AIPal/wiki)
- **Issue Reporting**: [GitHub Issues](https://github.com/tarun-vignes/AIPal/issues)
- **Discussion**: [Community Forum](https://github.com/tarun-vignes/AIPal/discussions)

## Contributing

We welcome contributions to make AIPal better for elderly users:

1. Fork the repository
2. Create a feature branch: `git checkout -b feature/amazing-feature`
3. Commit your changes: `git commit -m 'Add some amazing feature'`
4. Push to the branch: `git push origin feature/amazing-feature`
5. Open a Pull Request

Please read our [Contributing Guidelines](CONTRIBUTING.md) for details.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Acknowledgments

- Microsoft for Windows App SDK and Azure OpenAI Service
- The open source community for various libraries and tools
- All contributors who have helped make AIPal more accessible for elderly users
