# Smartitecture — AI-Powered System Intelligence (In Development)

Premium WPF desktop assistant with a glassmorphic UI, startup wizard, and modular AI orchestration across LUMEN (general), FORTIS (security), and NEXA (performance).

Status: In active development. Expect frequent changes and rapid iteration.

## Features

- Modern UI: Glassmorphism, gradient backgrounds, smooth animations, premium typography
- Startup Wizard: First-time setup, readiness checks, quick configuration
- AI Orchestrator: Multi-model switching; Azure OpenAI/local placeholders
- Security + Performance: Planned connectors for Defender/Firewall/basic metrics

## Getting Started

Prerequisites
- Windows 10/11
- .NET SDK 8.0+
- Visual Studio 2022 or VS Code with C# Dev Kit

Build and Run
- git clone https://github.com/tarun-vignes/Smartitecture.git
- cd Smartitecture
- dotnet restore Smartitecture.csproj
- dotnet build -c Debug Smartitecture.csproj
- dotnet run --project Smartitecture.csproj

## Architecture Overview

- App entry: App.xaml ? StartupWindow (welcome/wizard) ? MainWindow (dashboard) ? UI/ChatWindow (assistant)
- Modes and services: see Services/ (Core, Modes, Connectors, Safety, Hardware)
- UI: MainWindow.xaml, StartupWindow.xaml, UI/ChatWindow.xaml, SettingsWindow.xaml, Controls/AppTopBar.xaml
- Theme: Dynamic Light/Dark/System via Services/ThemeManager.cs and Resources/Themes/*.xaml

Key Files
- Controls/AppTopBar.xaml: Shared top bar with back/home/settings
- Services/NavigationService.cs: Fade transitions for navigation
- Services/ThemeManager.cs: Applies theme dictionaries (Light/Dark/System)
- StartupWindow.xaml: Welcome + readiness and quick setup
- MainWindow.xaml: Clean dashboard with quick actions and tips
- UI/ChatWindow.xaml: Chat experience with model selector and typing indicator

## Team Workflow

Branching
- pplication: integration branch for app changes
- eature/*: short-lived feature branches
- Create PRs from eature/* to pplication

Commits
- Use Conventional Commits: eat:, ix:, docs:, 
efactor:, chore:

Pull Requests
- Include description, validation steps, and screenshots for UI changes
- Keep PRs small and focused; link issues when applicable

## Coding Conventions

- C# latest, ImplicitUsings + Nullable enabled (see Directory.Build.props)
- Indentation 4 spaces; one type per file; filename matches type
- Naming: PascalCase (types/methods), camelCase (locals/params), _camelCase (private fields)
- Async methods end with Async
- XAML: keep UI in XAML; logic in code-behind or ViewModels with thin code-behind

## Configuration & Secrets

- Do not commit secrets. Use environment variables or dotnet user-secrets
- API Keys: OPENAI_API_KEY, ANTHROPIC_API_KEY
- Startup wizard will later assist with saving keys locally

## Testing


## Roadmap (Short-term)

- Guided wizard progress indicators and animated transitions
- Live theme switching across windows
- Richer health checks and diagnostics
- Fluent icons and micro-interactions

## License

This project is in development; license to be finalized.

For repo-wide conventions and deeper architecture details, see AGENTS.md.
