# Smartitecture - AI-Powered System Intelligence (In Development)

Smartitecture is a premium Windows WPF desktop assistant with a glassmorphic UI, a guided startup wizard, and modular AI orchestration across LUMEN (general), FORTIS (security), and NEXA (performance).

Status: In active development. Expect frequent changes and rapid iteration.

## Key Features

- Modern UI: glassmorphism, gradients, smooth transitions, premium typography
- Live System Insights: CPU, memory, disk, and network tiles
- Chat Assistant: multi-model selector, chat history, deleted chat recovery
- AI Modes: LUMEN (general), FORTIS (security), NEXA (performance)
- Themes: Light, Dark, System
- Localization: app strings via resource dictionaries
- Safety: confirmations for destructive system actions

## Getting Started

Prerequisites
- Windows 10/11
- .NET SDK 8.0+
- Visual Studio 2022 or VS Code with C# Dev Kit

Build and Run (App)
```powershell
git clone https://github.com/tarun-vignes/Smartitecture.git
cd Smartitecture
dotnet restore Smartitecture.csproj
dotnet build -c Debug Smartitecture.csproj
dotnet run --project Smartitecture.csproj
```

## Optional Local Backend

The app can connect to a local backend API (for cloud model access). This is optional and can be replaced with other providers later.

Set Environment Variables
```powershell
$env:OPENAI_API_KEY="YOUR_OPENAI_KEY"
# Optional: protect backend with a shared key
$env:SMARTITECTURE_BACKEND_KEY="YOUR_BACKEND_KEY"
```

Run Backend
```powershell
dotnet run --project .\backend\Smartitecture.Backend\Smartitecture.Backend.csproj -- --urls http://127.0.0.1:8089
```

Point the App to Backend
```powershell
$env:Backend__BaseUrl="http://127.0.0.1:8089"
# Only if you set a backend key:
$env:Backend__ApiKey="YOUR_BACKEND_KEY"
```

## Architecture Overview

- App entry: App.xaml -> StartupWindow (welcome and wizard) -> MainWindow (dashboard) -> UI/ChatWindow (assistant)
- Modes and services: Services/ (Core, Modes, Connectors, Safety, Hardware)
- UI: MainWindow.xaml, StartupWindow.xaml, UI/ChatWindow.xaml, SettingsWindow.xaml, Controls/AppTopBar.xaml
- Theme: Resources/Themes/*.xaml via Services/ThemeManager.cs
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

```powershell
dotnet test --collect:"XPlat Code Coverage"
```

## Roadmap (Short-term)

- Guided wizard progress indicators and animated transitions
- Live theme switching across windows
- Richer health checks and diagnostics
- Fluent icons and micro-interactions
