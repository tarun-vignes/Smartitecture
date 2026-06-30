# Clean Machine QA

Use this on a Windows 10/11 machine or fresh Windows user profile before sending a beta build to testers.

## Inputs

- MSIX package: `artifacts/msix-beta/Smartitecture-1.0.0.5-win-x64.msix`
- Dev certificate: `artifacts/certs/Smartitecture-dev-signing.cer`
- Backend URL: `https://smartitecture-backend.onrender.com`
- Backend API key: the value configured in Render as `SMARTITECTURE_BACKEND_API_KEY`

Do not copy the Gemini API key to the desktop machine.

## Install

Open PowerShell as Administrator and trust the development certificate:

```powershell
Import-Certificate -FilePath .\artifacts\certs\Smartitecture-dev-signing.cer -CertStoreLocation Cert:\LocalMachine\Root
```

Install the package:

```powershell
Add-AppxPackage -Path .\artifacts\msix-beta\Smartitecture-1.0.0.5-win-x64.msix
```

Confirm it is registered:

```powershell
Get-AppxPackage -Name Smartitecture
Get-StartApps | Where-Object Name -eq "Smartitecture"
```

## Automated QA

Run:

```powershell
.\deployment\qa-clean-machine.ps1 `
  -BackendUrl "https://smartitecture-backend.onrender.com" `
  -BackendApiKey "your-backend-api-key" `
  -LaunchApp
```

Expected results:

- Backend health passes.
- Backend chat returns a non-empty answer.
- Smartitecture package is installed.
- Start Menu identity exists.
- App launches.

## Manual QA

In Smartitecture, open Settings > AI Server:

- Server URL: `https://smartitecture-backend.onrender.com`
- API Key: same backend API key used in the QA script
- Click Test.
- Click Save.

In Chat, ask:

- `What is Google?`
- `Who is the president of the USA?`
- `Why is my PC slow?`
- `What should I close to free RAM?`
- `Show my IP`
- `Defender status`
- `Scan results`
- `How much battery do I have?`
- `Open calculator`

Pass criteria:

- General questions answer through the AI server.
- PC questions use local diagnostics.
- Sensitive or system-impacting actions ask for confirmation.
- The app stays responsive while diagnostics run.
- No text is clipped or overlapping in Chat, Settings, Dashboard, or About.
- Voice either transcribes or gives clear Windows permission guidance.
- Language and theme changes keep all labels readable.
- `%LOCALAPPDATA%\Smartitecture\Logs\app.log` is created after launch.
- `%LOCALAPPDATA%\Smartitecture\Logs\audit.log` records local tool actions after diagnostics/app-launch prompts.

## Known Beta Limits

- The app is Windows-only.
- The Render free instance may spin down and take around a minute to wake.
- The current MSIX uses a development certificate. Public release needs a trusted code-signing certificate.
- Windows icon cache may require unpinning and repinning Smartitecture after app icon changes.
- Automated unit/integration tests are still limited; this checklist is the current beta release gate.
