# Smartitecture - AI-Powered System Intelligence (In Development)

Smartitecture is a premium Windows WPF desktop assistant with a glassmorphic UI, a guided startup wizard, a unified AI assistant, and local system tools for diagnostics, security checks, and automation.

Status: In active development. Expect frequent changes and rapid iteration.

## Key Features

- Modern UI: glassmorphism, gradients, smooth transitions, premium typography
- Live System Insights: CPU, memory, disk, and network tiles
- Chat Assistant: one assistant with automatic local/server routing, voice input, chat history, and deleted chat recovery
- Local Tools: system info, process listing, performance snapshots, Defender status, app launching
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

## Optional AI Backend

The app can connect to a hosted or local backend API for broader model answers while keeping cloud secrets out of the desktop app.

Set Environment Variables
```powershell
$env:GEMINI_API_KEY="YOUR_GEMINI_KEY"
$env:SMARTITECTURE_BACKEND_API_KEY="YOUR_BACKEND_KEY"
# Hosted production requires the shared backend key. Local private testing can set this to false.
$env:SMARTITECTURE_REQUIRE_BACKEND_API_KEY="true"
```

Run Backend
```powershell
dotnet run --project .\backend\Smartitecture.Backend\Smartitecture.Backend.csproj --urls http://127.0.0.1:8080
```

Point the App to Backend
```powershell
# Preferred: open Settings > AI Server in the desktop app.
# Enter http://127.0.0.1:8080 and the optional backend API key, then click Test.
#
# Environment variables still work for development:
$env:Backend__BaseUrl="http://127.0.0.1:8080"
$env:Backend__ApiKey="YOUR_BACKEND_KEY"
```

Deployment scripts and release steps are documented in `docs/release/DEPLOYMENT.md`.
The repo also includes `render.yaml` for deploying the backend as a Render Docker web service.

## Architecture Overview

- App entry: App.xaml -> MainWindow -> StartupView/DashboardView/ChatView
- Services: Services/ (Core, Connectors, Providers, Safety)
- UI: MainWindow.xaml, UI/StartupView.xaml, UI/DashboardView.xaml, UI/ChatView.xaml, UI/SettingsView.xaml, Controls/AppTopBar.xaml
- Theme: Resources/Themes/*.xaml via Services/ThemeManager.cs
- Backend: backend/Smartitecture.Backend (optional local API)
- Website: Website/ (Vite)

Key Files
- Controls/AppTopBar.xaml: shared top bar with back, home, settings
- Services/NavigationService.cs: fade transitions for navigation
- Services/ThemeManager.cs: applies theme dictionaries
- UI/StartupView.xaml: welcome and live system overview
- UI/DashboardView.xaml: dashboard with quick actions
- UI/ChatView.xaml: unified assistant experience with voice input, local tools, history, and typing indicator

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
- Backend server keys: `GEMINI_API_KEY` by default, or `OPENAI_API_KEY` when `SMARTITECTURE_AI_PROVIDER=openai`; use `SMARTITECTURE_BACKEND_API_KEY` for hosted backends
- Desktop backend connection: configure in Settings > AI Server or via `Backend__BaseUrl` / `Backend__ApiKey`
- Keep `appsettings.json` safe by default

## Testing

```powershell
dotnet test --collect:"XPlat Code Coverage"
```

## Roadmap (Short-term)

- Guided wizard progress indicators and animated transitions
- Live theme switching across windows
- Richer health checks and diagnostics
- Fluent icons and micro-interactions
