# Smartitecture Deployment

This is the current production path:

1. Deploy `backend/Smartitecture.Backend` as the hosted AI relay.
2. Publish the WPF desktop app as a Windows folder or zip package.
3. In the desktop app, open Settings > AI Server and enter the backend URL plus optional API key.

## Backend

Required environment variables:

- `GEMINI_API_KEY`: Gemini key used only by the backend. `GOOGLE_API_KEY` is also accepted.
- `SMARTITECTURE_AI_PROVIDER`: Optional provider override. Defaults to `gemini`; set to `openai` to use OpenAI.
- `SMARTITECTURE_BACKEND_API_KEY`: Shared key required from desktop clients in hosted production.
- `SMARTITECTURE_REQUIRE_BACKEND_API_KEY`: Defaults to `true` in Production. Set `false` only for local/private testing.
- `GEMINI_MODEL`: Optional Gemini model override. Defaults to `gemini-3.1-flash-lite`.
- `OPENAI_API_KEY` / `OPENAI_MODEL`: Used only when `SMARTITECTURE_AI_PROVIDER=openai`.

## Render Hosting

The repo includes `render.yaml` for a Docker web service with automatic HTTPS.

1. Push the repo to GitHub.
2. In Render, choose **New > Blueprint**.
3. Connect the GitHub repo and select the branch.
4. Render will detect `render.yaml`.
5. Add the required secret environment variables when prompted:
   - `GEMINI_API_KEY`
   - `SMARTITECTURE_BACKEND_API_KEY`
6. Deploy the service.
7. Open the Render service URL and verify `/health`, for example:

```powershell
Invoke-RestMethod https://your-service.onrender.com/health
```

8. Run the backend smoke test:

```powershell
.\deployment\smoke-test.ps1 -BackendUrl https://your-service.onrender.com -BackendApiKey "your-backend-key"
```

Use the same Render URL and backend API key in the desktop app under Settings > AI Server.

Local Docker run:

```powershell
$env:GEMINI_API_KEY="your-gemini-key"
$env:SMARTITECTURE_BACKEND_API_KEY="change-me"
$env:SMARTITECTURE_REQUIRE_BACKEND_API_KEY="false"
docker compose -f docker-compose.backend.yml up --build
```

The backend will listen on `http://127.0.0.1:8080` locally.

Health check:

```powershell
Invoke-RestMethod http://127.0.0.1:8080/health
```

Manual publish:

```powershell
.\deployment\publish-backend.ps1
```

Run the published backend:

```powershell
cd .\artifacts\backend
$env:GEMINI_API_KEY="your-gemini-key"
$env:SMARTITECTURE_BACKEND_API_KEY="change-me"
$env:SMARTITECTURE_REQUIRE_BACKEND_API_KEY="true"
dotnet Smartitecture.Backend.dll --urls http://127.0.0.1:8080
```

## Desktop

Framework-dependent publish:

```powershell
.\deployment\publish-desktop.ps1 -Zip
```

Self-contained publish:

```powershell
.\deployment\publish-desktop.ps1 -SelfContained -Zip
```

The entry point is:

```text
artifacts\desktop\Smartitecture.exe
```

## MSIX Installer

Create an unsigned MSIX:

```powershell
.\deployment\package-msix.ps1
```

Create a development signing certificate:

```powershell
.\deployment\create-dev-cert.ps1
```

Trust the development certificate for local installation:

```powershell
Import-Certificate -FilePath .\artifacts\certs\Smartitecture-dev-signing.cer -CertStoreLocation Cert:\CurrentUser\TrustedPeople
```

If `Add-AppxPackage` still reports `0x800B0109`, open PowerShell as Administrator and trust the certificate for the machine:

```powershell
Import-Certificate -FilePath .\artifacts\certs\Smartitecture-dev-signing.cer -CertStoreLocation Cert:\LocalMachine\Root
```

Create a signed development MSIX:

```powershell
.\deployment\package-msix.ps1 -CertificatePath .\artifacts\certs\Smartitecture-dev-signing.pfx -CertificatePassword "SmartitectureDev123!"
```

Install locally:

```powershell
Add-AppxPackage .\artifacts\msix\Smartitecture-1.0.0.1-win-x64.msix
```

For public release, replace the development certificate with a trusted code-signing certificate and update the manifest publisher to match the certificate subject.

## Desktop AI Server Setup

After installing or launching the app:

1. Open Settings.
2. Find AI Server.
3. Enter the backend URL, for example `https://api.example.com` or `http://127.0.0.1:8080`.
4. Enter the API key if the backend requires one.
5. Click Test.
6. Save settings.

The desktop app stores this in local user preferences, not in the repository.

## Smoke Test

Run:

```powershell
.\deployment\smoke-test.ps1 -BackendUrl http://127.0.0.1:8080 -BackendApiKey change-me
```

Manual checks before shipping:

- App launches.
- Settings save persists.
- Settings > AI Server test succeeds.
- Chat fallback works when no server is configured.
- Ask `what can you do`.
- Ask `why is my PC slow`.
- Ask `why is my PC hot`.
- Ask `show my IP`.
- Ask `Defender status`.
- Ask `Scan results`.
- Ask `How much battery do I have?`.
- Ask `open calculator`.
- Voice button shows a useful Windows permission message if microphone/speech privacy is blocked.
- Language switching still renders Settings and Chat without blank labels.

Use `docs/release/RELEASE_CHECKLIST.md` before publishing a release build.
