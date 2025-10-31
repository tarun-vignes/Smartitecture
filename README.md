# Smartitecture - AI-Powered System Intelligence (In Development)

Premium WPF desktop assistant with a glassmorphic UI, startup wizard, and modular AI orchestration across three modes: LUMEN (general), FORTIS (security), and NEXA (performance).

Status: In active development. Expect frequent changes and rapid iteration.

## Features

- Modern UI: glassmorphism, gradient backgrounds, smooth animations, clear typography
- Startup wizard: first-time setup, readiness checks, quick configuration
- AI orchestrator: multi-model switching, Azure OpenAI and Claude support
- Security and performance: basic connectors for Defender, Firewall, and system metrics

## Getting Started

Prerequisites
- Windows 10/11
- .NET SDK 8.0+
- Visual Studio 2022 or VS Code with C# Dev Kit

Build and run
```powershell
git clone https://github.com/tarun-vignes/Smartitecture.git
cd Smartitecture
dotnet restore Smartitecture.csproj
dotnet build -c Debug Smartitecture.csproj
dotnet run --project Smartitecture.csproj
```

## Architecture Overview

- App entry: `Smartitecture.App` -> `StartupWindow` (wizard) -> `MainWindow` (dashboard)
- Modes and services: see `Services/` (Core, Modes, Connectors, Safety, Hardware)
- UI windows: `MainWindow.xaml`, `UI/ChatWindow.xaml`, `SettingsWindow.xaml`, `StartupWindow.xaml`
- Theme system: `App.xaml` resource dictionary (colors, typography, cards, buttons)

## Team Workflow

Branching
- `application`: integration branch for app changes
- `feature/*`: short-lived feature branches
- Open PRs from `feature/*` to `application`

Commits
- Use Conventional Commits: `feat:`, `fix:`, `docs:`, `refactor:`, `chore:`
- Examples: `feat(ui): add startup wizard`, `fix(services): harden permission checks`

Pull Requests
- Include description, validation steps, and screenshots for UI changes
- Keep PRs small and focused; link issues when applicable

## Coding Conventions

- C# latest, `ImplicitUsings` and `Nullable` enabled (see `Directory.Build.props`)
- Indentation: 4 spaces; one type per file; filename matches type
- Naming: PascalCase (types/methods), camelCase (locals/params), `_camelCase` (private fields)
- Async method names end with `Async`
- XAML: keep UI in XAML; keep code-behind thin and use ViewModels/services

## Configuration and Secrets

- Do not commit secrets; use environment variables or `dotnet user-secrets`
- API keys
  - `OPENAI_API_KEY` (OpenAI)
  - `ANTHROPIC_API_KEY` (Claude)

## Testing

- xUnit test project: `Smartitecture.Tests/`
- Run: `dotnet test --collect:"XPlat Code Coverage"`

## Roadmap (Short Term)

- Wizard progress indicators and animated transitions
- Live theme switching across windows
- Richer health checks and diagnostics
- Fluent icons and more micro-interactions

## More Documentation

- Codebase overview: `docs/ARCHITECTURE.md`
- Contributing: `CONTRIBUTING.md`

## License

This project is in development; license to be finalized.
