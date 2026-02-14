# Smartitecture - AI-Powered System Intelligence (In Development)

Premium WPF desktop assistant with a glassmorphic UI, startup wizard, and modular AI orchestration across LUMEN (general), FORTIS (security), and NEXA (performance).

Status: In active development. Expect frequent changes and rapid iteration.

## Features

- Modern UI: glassmorphism, gradient backgrounds, smooth animations, premium typography
- Startup Wizard: first-time setup, readiness checks, quick configuration
- AI Orchestrator: multi-model switching; cloud and local placeholders
- Security and Performance: planned connectors for Defender, Firewall, and basic metrics

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

- App entry: App.xaml -> StartupWindow (welcome and wizard) -> MainWindow (dashboard) -> UI/ChatWindow (assistant)
- Modes and services: Services/ (Core, Modes, Connectors, Safety, Hardware)
- UI: MainWindow.xaml, StartupWindow.xaml, UI/ChatWindow.xaml, SettingsWindow.xaml, Controls/AppTopBar.xaml
- Theme: Dynamic Light, Dark, System via Services/ThemeManager.cs and Resources/Themes/*.xaml
- Backend: backend/Smartitecture.Backend (optional local API)
- Website: Website/ (Vite)

Key Files
- Controls/AppTopBar.xaml: shared top bar with back, home, settings
- Services/NavigationService.cs: fade transitions for navigation
- Services/ThemeManager.cs: applies theme dictionaries
- StartupWindow.xaml: welcome and readiness setup
- MainWindow.xaml: dashboard with quick actions
- UI/ChatWindow.xaml: chat experience with model selector and typing indicator

## Repo Layout

- archive/legacy: historical experiments and older code paths
- tools/maintenance: one-off fix scripts

## Team Workflow

Branching
- main: stable integration
- application: app-only work
- website: site-only work
- feature/*: short-lived feature branches
- Create PRs from feature/* to application or website, then merge to main

Commits
- Use Conventional Commits: feat:, fix:, docs:, refactor:, chore:

Pull Requests
- Include description, validation steps, and screenshots for UI changes
- Keep PRs small and focused; link issues when applicable

## Coding Conventions

- C# latest, ImplicitUsings and Nullable enabled (see Directory.Build.props)
- Indentation: 4 spaces; one type per file; filename matches type
- Naming: PascalCase (types and methods), camelCase (locals and params), _camelCase (private fields)
- Async methods end with Async
- XAML: keep UI in XAML; logic in code-behind or ViewModels with thin code-behind

## Configuration and Secrets

- Do not commit secrets. Use environment variables or dotnet user-secrets
- API Keys: OPENAI_API_KEY, ANTHROPIC_API_KEY
- Startup wizard will later assist with saving keys locally

## Testing

- dotnet test --collect:"XPlat Code Coverage"

## Roadmap (Short-term)

- Guided wizard progress indicators and animated transitions
- Live theme switching across windows
- Richer health checks and diagnostics
- Fluent icons and micro-interactions
