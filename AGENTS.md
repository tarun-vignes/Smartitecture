# Repository Guidelines

## Project Structure & Module Organization
- `Smartitecture.sln`, `Smartitecture.csproj`: WPF app entry (net8.0-windows).
- `UI/`: WPF pages and views (XAML + code-behind).
- `Services/`: AI services and helpers; `Services/Core` (router, enums), `Services/Interfaces` (contracts), `Services/Modes` (LUMEN/FORTIS/NEXA).
- `Assets/`, `deployment/`: branding and packaging scripts.
- `Website/`: Vite site; `backend/`: local API; `archive/legacy`: historical experiments; `tools/maintenance`: one-off fix scripts.

## Build, Test, and Development Commands
- Restore/build: `dotnet restore` and `dotnet build -c Debug`
- Run app: `dotnet run --project Smartitecture.csproj`
- Format code: `dotnet format`
- Website (optional): `cd Website && npm ci && npm run dev` â€¢ build: `npm run build`

## Coding Style & Naming Conventions
- C# latest with `ImplicitUsings` and `Nullable` enabled (see `Directory.Build.props`).
- Indentation: 4 spaces. One type per file; filename matches type.
- Naming: PascalCase (types/methods), camelCase (locals/params), `_camelCase` (private fields), `I*` (interfaces), `*Async` (async methods).
- XAML: keep UI in `UI/`; logic in ViewModels. Keep code-behind thin.

## Testing Guidelines
- Test names: `Class_Method_ShouldBehavior`. Prefer service/command tests over UI. Mock external calls.

## Commit & Pull Request Guidelines
- Use concise, action-oriented commits. Prefer Conventional Commits (e.g., `feat:`, `fix:`, `test:`). Example: `fix(commands): confirm safe shutdown flow`.
- PRs must include: clear description, linked issues, validation steps, and screenshots for UI changes. Keep scope small and focused.

## Architecture Overview
- Orchestrator: `Services/SmartitectureAIService` with `AIModeRouter` (`Services/Core`) selecting between modes.
- Modes: `LumenService` (general + research), `FortisService` (security), `NexaService` (performance) implementing `IAIMode`.
- LLM backends: `MultiModelAIService` (switching, streaming) and `OpenAIService`; `KnowledgeBaseService` and `WebResearchEngine` provide grounding.

## Security & Configuration
- Do not commit secrets. Use environment variables or `dotnet user-secrets` for local dev. `appsettings.json` should contain safe defaults only.

## AI Mode Specifications

### LUMEN (General Assistant & Automation)
**Core Capabilities:**
- File System Automation: Create, move, organize, search files across drives
- Web Research Integration: Real-time Wikipedia, web scraping, data aggregation
- Application Control: Launch, manage, and automate any installed software
- Workflow Automation: Email integration, calendar management, task scheduling
- System Commands: PowerShell/CMD execution with safety guardrails
- Clipboard & Screenshot Management: Advanced copy/paste operations
- Network Operations: Download files, API integrations, web interactions

**Technical Implementation:**
- FileSystemWatcher for real-time file monitoring
- Process management via System.Diagnostics
- Registry manipulation for system configuration
- COM automation for Office applications
- HTTP clients for web service integration

### FORTIS (Security & Defense Expert)
**Core Capabilities:**
- Real-Time Threat Detection: File scanning, network monitoring, process analysis
- Windows Defender Integration: Full system scans, quarantine management
- Firewall Management: Port monitoring, rule configuration, traffic analysis
- Registry Security: Autorun analysis, malicious entry detection
- Network Security: Port scanning, WiFi analysis, intrusion detection
- Hardware Security: TPM integration, secure boot verification
- Behavioral Analysis: Process monitoring, anomaly detection

**Defense Connector Framework:**
- Windows Security Center API integration
- PowerShell security cmdlets execution
- WMI queries for system security status
- Event log monitoring and analysis
- Certificate store management
- Antivirus API integration (Windows Defender, third-party)

### NEXA (Performance & System Optimization)
**Core Capabilities:**
- Real-Time System Monitoring: CPU, RAM, disk, network, temperature
- Process Management: Start, stop, analyze, optimize running processes
- Hardware Diagnostics: Component health, driver status, performance metrics
- Startup Optimization: Manage autostart programs, services
- System Cleanup: Temp files, registry cleanup, disk optimization
- Performance Tuning: Power settings, visual effects, system configuration
- Automation Scripts: Scheduled maintenance, system optimization routines

**Low-Level Integration:**
- Performance counters via System.Diagnostics.PerformanceCounter
- WMI queries for hardware information
- Registry manipulation for system optimization
- Service control manager integration
- Hardware sensor monitoring (OpenHardwareMonitor integration)
- Windows Task Scheduler automation

## Security & Safety Framework
- All system operations require user confirmation for destructive actions
- Sandbox mode for testing system changes
- Rollback capabilities for system modifications
- Audit logging for all system interactions
- Permission-based access control for sensitive operations

## Planned System Components

1. SystemConnector Framework (`Services/Connectors/`)
   - `WindowsDefenderConnector.cs` - Real antivirus integration
   - `FirewallConnector.cs` - Network security management
   - `PerformanceConnector.cs` - Hardware monitoring
   - `ProcessConnector.cs` - System process control
   - `RegistryConnector.cs` - Safe registry operations

2. Security Defense Layer (`Services/Security/`)
   - `ThreatDetectionEngine.cs` - Real-time threat analysis
   - `SecurityAuditService.cs` - System security assessment
   - `QuarantineManager.cs` - Malware isolation
   - `NetworkMonitor.cs` - Traffic analysis and alerts

3. System Automation Engine (`Services/Automation/`)
   - `TaskScheduler.cs` - Automated system maintenance
   - `FileSystemAutomation.cs` - Intelligent file management
   - `ApplicationLauncher.cs` - Software control and integration
   - `WorkflowEngine.cs` - Complex multi-step automation

4. Hardware Integration Layer (`Services/Hardware/`)
   - `SensorMonitor.cs` - Temperature, fan speed, voltages
   - `PerformanceMetrics.cs` - Real-time system statistics
   - `HardwareDiagnostics.cs` - Component health checking
   - `PowerManagement.cs` - Energy optimization

5. Safety & Permissions System (`Services/Safety/`)
   - `OperationValidator.cs` - Pre-execution safety checks
   - `UserConfirmationService.cs` - Approval workflows
   - `AuditLogger.cs` - Complete operation logging
   - `RollbackManager.cs` - Undo system changes

