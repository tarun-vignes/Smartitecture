# Smartitecture � AI-Powered System Intelligence (In Development)

Premium WPF desktop assistant with a glassmorphic UI, startup wizard, and modular AI orchestration across LUMEN (general), FORTIS (security), and NEXA (performance).

Status: In active development. Expect frequent changes and rapid iteration.

## Features

- Modern UI: Glassmorphism, gradient backgrounds, smooth animations, premium typography
- Startup Wizard: First-time setup, readiness checks, quick configuration
- AI Orchestrator: Multi-model switching, Azure OpenAI/Claude support
- Security + Performance: Connectors for Defender, Firewall, and basic system metrics

## Getting Started

Prerequisites
- Windows 10/11
- .NET SDK 8.0+
- Visual Studio 2022 or VS Code with C# Dev Kit

Build and Run
```powershell
git clone https://github.com/tarun-vignes/Smartitecture.git
cd Smartitecture
dotnet restore Smartitecture.csproj
dotnet build -c Debug Smartitecture.csproj
dotnet run --project Smartitecture.csproj
```

## Architecture Overview

- App entry: `Smartitecture.App` ? `StartupWindow` (wizard) ? `MainWindow` (dashboard)
- Modes and services: see `Services/` (Core, Modes, Connectors, Safety, Hardware)
- UI: `MainWindow.xaml`, `UI/ChatWindow.xaml`, `SettingsWindow.xaml`, `StartupWindow.xaml`
- Theme: `App.xaml` resource dictionary (colors, typography, cards, buttons)

## Team Workflow

- Branching
  - `application`: integration branch for app changes
  - `feature/*`: short-lived feature branches
  - Create PRs from `feature/*` to `application`

- Commits
  - Use Conventional Commits: `feat:`, `fix:`, `docs:`, `refactor:`, `chore:`
  - Scope examples: `feat(ui): �`, `fix(services): �`

- Pull Requests
  - Include description, validation steps, and screenshots for UI changes
  - Keep PRs small and focused; link issues when applicable

## Coding Conventions

- C# latest, `ImplicitUsings` + `Nullable` enabled (see `Directory.Build.props`)
- Indentation 4 spaces, one type per file, file name matches type
- Naming: PascalCase (types/methods), camelCase (locals/params), `_camelCase` (private fields)
- Async method names end with `Async`
- XAML: keep UI in XAML; logic in code-behind or ViewModels with thin code-behind

## Configuration & Secrets

- Do not commit secrets. Use environment variables or `dotnet user-secrets`
- API Keys:
  - `OPENAI_API_KEY` (OpenAI)
  - `ANTHROPIC_API_KEY` (Claude)
- Startup wizard can save keys to your user environment

## Testing

- xUnit test project under `Smartitecture.Tests/`
- Run: `dotnet test --collect:"XPlat Code Coverage"`

## Roadmap (Short-term)

- Guided wizard progress indicators and animated transitions
- Live theme switching across windows
- Richer health checks and diagnostics
- Fluent icons and more micro-interactions

## License

This project is in development; license to be finalized.

See full codebase overview in [docs/ARCHITECTURE.md](docs/ARCHITECTURE.md).
# 🏗️ Smartitecture - Advanced AI Desktop Assistant

**An intelligent desktop automation platform powered by advanced AI and multi-model language processing.**

![Smartitecture AI Assistant](https://img.shields.io/badge/AI-Powered-blue?style=for-the-badge)
![.NET 8](https://img.shields.io/badge/.NET-8.0-purple?style=for-the-badge)
![WPF](https://img.shields.io/badge/WPF-Desktop-green?style=for-the-badge)
![Status](https://img.shields.io/badge/Status-Production%20Ready-brightgreen?style=for-the-badge)

## 🚀 **What Makes Smartitecture Special?**

Smartitecture isn't just another chatbot - it's an **intelligent AI assistant** that actually **knows things** and can **do things**. Ask it "What color is grass?" and it will tell you it's green. Ask it to open Calculator, and it will. Ask it who the president is, and it knows the answer.

### ✨ **Key Features**

#### 🧠 **Advanced AI Intelligence**
- **Knowledge Base**: Real factual answers (not generic responses)
- **Multi-Model Support**: 6 different AI models including Azure OpenAI GPT-4
- **Context Awareness**: Remembers conversation history and learns
- **Intelligent Training**: Continuously improves responses

#### 🤖 **System Automation**
- **Command Execution**: Calculator, File Explorer, Task Manager, System Shutdown
- **System Diagnostics**: Performance monitoring and analysis
- **File Operations**: Automated file management and organization
- **Process Management**: Monitor and control running applications

#### 💬 **Natural Conversation**
- **Real-Time Streaming**: Responses appear as they're generated
- **Personality**: Engaging, helpful, and informative communication
- **Smart Parsing**: Understands natural language commands
- **Learning**: Adapts to user preferences and patterns

## 🎯 **Try These Examples**

**Knowledge Questions:**
- "What color is grass?" → "Grass is **green** due to chlorophyll..."
- "Who is the president?" → "**Joe Biden** is the current President..."
- "What is AI?" → "**Artificial Intelligence** is computer technology..."

**System Commands:**
- "Open calculator" → Launches Windows Calculator
- "Show me task manager" → Opens Task Manager
- "What's the current time?" → Shows exact time and date

**Math & Calculations:**
- "Calculate 15 + 27" → "**15 + 27 = 42**"
- "What's the square root of 64?" → "**8**"

## 🛠️ **Technical Architecture**

### **Core Components**
- **MultiModelAIService**: Advanced AI processing with multiple model support
- **KnowledgeBaseService**: Factual information database with smart retrieval
- **IntelligentTrainingService**: Machine learning for response improvement
- **Command System**: Extensible automation framework

### **AI Models Supported**
1. **Advanced AI Assistant** (Default) - Enhanced knowledge-based responses
2. **Azure OpenAI GPT-4** - State-of-the-art language model
3. **Local Ollama Model** - Privacy-focused local processing
4. **Anthropic Claude** - Advanced reasoning capabilities
5. **Google Gemini** - Multi-modal AI processing
6. **System Expert Mode** - Specialized for system administration

## 🚀 **Quick Start**

### **Option 1: Run from Source**
```bash
git clone [https://github.com/tarun-vignes/Smartitecture.git](https://github.com/tarun-vignes/Smartitecture.git)
cd Smartitecture
dotnet run --project SmartitectureSimple.csproj
