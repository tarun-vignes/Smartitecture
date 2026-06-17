# Smartitecture Beta Testing

Beta version: `1.0.0.1`

## What This Beta Tests

- Smartitecture desktop install and launch.
- Hosted AI backend through Render.
- Local PC diagnostics and app launching.
- Settings persistence.
- Theme, language, and voice-permission behavior.

## Backend

- URL: `https://smartitecture-backend.onrender.com`
- API key: use the current beta `SMARTITECTURE_BACKEND_API_KEY`.

The Gemini API key stays on Render. Do not put it on tester machines.

Render free instances can spin down with inactivity. The first request after idle time can take around a minute.

## Install

For the dev-signed beta package, open PowerShell as Administrator and trust the certificate:

```powershell
Import-Certificate -FilePath .\artifacts\certs\Smartitecture-dev-signing.cer -CertStoreLocation Cert:\LocalMachine\Root
```

Install:

```powershell
Add-AppxPackage -Path .\artifacts\msix-beta\Smartitecture-1.0.0.1-win-x64.msix
```

Launch from Start Menu:

```text
Smartitecture
```

## Configure AI Server

Open Smartitecture > Settings > AI Server:

```text
Server URL: https://smartitecture-backend.onrender.com
API Key: current beta backend API key
```

Click **Test**, then **Save**.

## Test Prompts

Ask these in Chat:

```text
What is Google?
Who is the president of the USA?
Why is my PC slow?
What should I close to free RAM?
Show my IP
Defender status
Scan results
How much battery do I have?
Open calculator
Open settings
Open camera
Open notepad
```

## Pass Criteria

- AI Server shows connected.
- General questions answer through the backend.
- PC questions use local diagnostics.
- App launch commands open installed Windows apps or give a clear failure message.
- No UI text overlaps or blocks controls.
- Language/theme switching does not blank out labels.
- Voice either transcribes or explains Windows microphone/speech permission setup.

## Known Beta Limits

- Windows only.
- Dev-signed MSIX requires trusting the development certificate.
- Render free tier can cold-start slowly.
- Some app launch names may need the exact Start Menu display name.
- There are limited automated tests; clean-machine QA is still required before sharing wider.
