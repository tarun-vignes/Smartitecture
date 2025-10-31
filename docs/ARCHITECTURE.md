# Smartitecture Architecture and Codebase Overview

This document explains how the application is structured so new contributors can navigate the codebase confidently.

## Runtime Flow

1. App start
   - Entry point: `App.xaml:1` and `App.xaml.cs:39`
   - Shows `StartupWindow.xaml:1` (welcome + wizard)
   - From startup, users can launch the main dashboard (`MainWindow.xaml:1`)

2. Startup window
   - `StartupWindow.xaml:1` — Hero, readiness ribbon, quick-setup cards, footer navigation
   - `StartupWindow.xaml.cs:1` — Readiness checks (env var keys, Defender availability, Firewall status), preferences persistence, wizard control

3. Main dashboard and chat
   - `MainWindow.xaml:1` — Primary dashboard UI (cards for modes, activity, network, chat shell)
   - `MainWindow.xaml.cs:1` — AI mode router usage, chat streaming, dashboard updates
   - `UI/ChatWindow.xaml:1`, `UI/ChatWindow.xaml.cs:1` — Dedicated chat window (optional)
   - `SettingsWindow.xaml:1` — Preferences UI

## Theming and UI System

- `App.xaml:1` — Global resource dictionary
  - Color system: Primary/Secondary + Success/Warning/Error
  - Premium styles: `Card.Glass`, `Button.Primary`, `Button.Glass`
  - Typography: `Text.H1`, `Text.H2`, `Text.Body`, `Text.Caption`
  - Animation resources and skeleton base

## Services and Modes

- Orchestrator and services live under `Services/`
  - Core router and abstractions (see `Services/Core/`)
  - AI modes (see `Services/Modes/`)
    - `LumenService` — General assistant and research
    - `FortisService` — Security and defense
    - `NexaService` — Performance and optimization
  - Model backends
    - `Services/MultiModelAIService.cs:1` — Switching/streaming
    - `Services/OpenAIService.cs:1` — OpenAI
    - `Services/ClaudeService.cs:1` — Anthropic (Claude)
    - `Services/WebResearchEngine.cs:1`, `Services/KnowledgeBaseService.cs` — Grounding
  - Connectors (Windows integration) — `Services/Connectors/`
    - `WindowsDefenderConnector.cs:1` — Defender availability + scans (placeholder)
    - `FirewallConnector.cs:1` — Firewall info (netsh probe, placeholder for rules)
    - `PerformanceConnector.cs:1` — CPU/disk/memory heuristics
  - Safety and Security — `Services/Safety/`, `Services/Security/`
  - Hardware — `Services/Hardware/`
  - Preferences — `Services/PreferencesService.cs:1` (JSON persistence in `%LocalAppData%`)

## Configuration

- `appsettings.json:1` — Safe defaults only (do not commit secrets)
- Environment variables for secrets
  - `OPENAI_API_KEY` — OpenAI
  - `ANTHROPIC_API_KEY` — Claude
- `Directory.Build.props:1` — SDK-wide settings (LangVersion, Nullable, RIDs)
- `Directory.Build.targets:1` — Optional build customizations

## Project Files

- `Smartitecture.sln:1` — Solution
- `Smartitecture.csproj:1` — WPF entry project (net8.0-windows)
  - Includes `App.xaml`, `MainWindow.xaml`, `StartupWindow.xaml`, `UI/ChatWindow.xaml`, `SettingsWindow.xaml`
  - Compiles `Services/**` and `UI/**` trees; excludes legacy `Application/**` by default

## Tests

- `Smartitecture.Tests/Smartitecture.Tests.csproj:1` — xUnit test project (modern)
- `tests/Smartitecture.Tests/` — Legacy mirror folder (do not duplicate tests)

## Assets and Deployment

- `Assets/` — Images and branding (e.g., `Assets/smartitecture-logo.png`)
- Packaging directories (optional): `deploy/`, `deployment/`, `temp-deploy/`

## Legacy/Experimental Code

- `Application/**` — Legacy commands and services; not compiled by default
  - Keep around for reference; migrate code into `Services/**` & `UI/**` when modernizing

## Build & Run

- Restore/build: `dotnet restore` • `dotnet build -c Debug`
- Run app: `dotnet run --project Smartitecture.csproj`
- Tests: `dotnet test --collect:"XPlat Code Coverage"`
- Format: `dotnet format`

## Branching Model

- `application` — Integration branch for the app
- `feature/*` — Short-lived feature branches; open PRs into `application`

## Contribution Guidelines

- See `CONTRIBUTING.md:1`
- Use Conventional Commits; keep PRs focused; add screenshots for UI changes

## Roadmap (Short-term)

- Wizard progress indicator with animations
- Live theme switching across windows
- Richer health checks (Defender API, firewall rules)
- Fluent icons and micro-interactions throughout

