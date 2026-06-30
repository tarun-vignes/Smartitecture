# Release Checklist

Use this before every desktop release.

## Build

- Run `dotnet restore Smartitecture.sln`.
- Run `dotnet build Smartitecture.sln -c Release`.
- Run `dotnet test Smartitecture.sln -c Release --no-restore`. If no tests run, record that as a release risk.
- Run `.\deployment\publish-backend.ps1`.
- Run `.\deployment\publish-desktop.ps1 -Zip`.
- Run `.\deployment\package-msix.ps1 -CertificatePath .\artifacts\certs\Smartitecture-dev-signing.pfx -CertificatePassword "SmartitectureDev123!"` for a signed development beta package.
- Confirm `Package.appxmanifest` uses a valid resource language such as `en-US`, not `x-generate`.

## Backend

- Set `GEMINI_API_KEY` only on the backend host. If using OpenAI instead, set `SMARTITECTURE_AI_PROVIDER=openai` and `OPENAI_API_KEY`.
- Set `SMARTITECTURE_BACKEND_API_KEY` for hosted production. Production deployments require it unless `SMARTITECTURE_REQUIRE_BACKEND_API_KEY=false`.
- Confirm `GET /health` returns `status = ok`.
- Run `.\deployment\smoke-test.ps1 -BackendUrl <url> -BackendApiKey <key>`.

## Desktop Install

- Install the MSIX or run the published `Smartitecture.exe`.
- Confirm beta handoff steps in `docs/release/BETA_TESTING.md` match the package version and backend URL.
- For dev-signed MSIX builds, trust the `.cer` first. If Windows reports `0x800B0109`, import it into `Cert:\LocalMachine\Root` from an elevated PowerShell window.
- On a clean machine or fresh Windows user profile, run `.\deployment\qa-clean-machine.ps1 -BackendUrl <url> -BackendApiKey <key> -LaunchApp`.
- Open Settings > AI Server.
- Enter the backend URL and optional API key.
- Click Test and confirm the status changes to Connected.
- Save settings.

## Chat Smoke Test

- Ask `what can you do`.
- Ask `what is AI`.
- Ask `why is my PC slow`.
- Ask `why is my PC hot`.
- Ask `show my IP`.
- Ask `Defender status`.
- Ask `Scan results`.
- Ask `How much battery do I have?`.
- Ask `What should I close to free RAM?`.
- Ask `open calculator`.
- Ask a broad server-backed question while backend is configured.
- Disable or clear backend URL, then confirm local fallback is understandable.

## UI Smoke Test

- Navigate Home, Dashboard, Chat, Settings, and About.
- Switch language and confirm no labels are blank.
- Switch light/dark/system theme.
- Open History and Clear Chat.
- Press Voice and confirm either transcription works or the Windows permission guidance is clear.
- Confirm `%LOCALAPPDATA%\Smartitecture\Logs\app.log` is created after launch.
- Trigger at least one local diagnostic and confirm `%LOCALAPPDATA%\Smartitecture\Logs\audit.log` records the tool action.

## Release Notes

- Record version.
- Record backend URL environment.
- Record validation date.
- Record known limitations.
- Record the app log path and instructions for collecting tester reports.
