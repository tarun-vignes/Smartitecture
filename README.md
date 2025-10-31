# Smartitecture — AI-Powered System Intelligence (In Development)

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
  - Scope examples: `feat(ui): …`, `fix(services): …`

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