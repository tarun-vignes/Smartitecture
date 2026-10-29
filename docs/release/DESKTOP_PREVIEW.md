# Desktop preview handoff

Build a self-contained Windows preview with:

```powershell
.\deployment\build-public-preview.ps1
```

This produces a new folder for each attempt under `artifacts/public-preview`, including a ZIP, plain-language getting-started instructions, and a release manifest with a SHA256 checksum. It includes .NET and WPF, checks required runtime files, rejects configured provider credentials, and does not replace the existing website downloads.

The public-preview build pins .NET 8.0.33. Keep this security patch current before subsequent releases. The Windows SDK reference retains its independent version.

## User handoff

1. Extract the whole ZIP.
2. Open the included Smartitecture folder and run Smartitecture.exe.
3. Try Check my PC, battery information, and Open calculator.
4. Open Settings, change the theme, save, and restart.
5. Check that chat survives navigating away and back, and that Stop interrupts an AI request when online access is available.

The archive is an unsigned preview, not a trusted public installer. Do not ask users to disable Windows protection or trust a development certificate. Online AI is not ready for general public use until the backend and account provider have been deployed and tested.

## Validation in this session

- All 54 desktop service tests passed on October 28, 2026. The earlier Windows Application Control test-load failure did not reproduce on this run.
- A self-contained candidate was launched with the working directory set to Windows, away from the repository. The application log recorded that the main window was shown. This does not prove a full interactive or clean-machine test.
- Slow performance-counter initialization was moved out of the home-view constructor onto a background task. Home navigation and window creation no longer wait for those counters in that constructor.
- Unimplemented startup, automatic-update, notification, and diagnostics-sharing switches are hidden. File attachment is hidden because the old handler only pasted file paths.
- Legacy publish scripting now stops after a failed dotnet publish instead of proceeding to archive potentially stale output.

## Remaining public release gates

- Complete interactive testing on a clean, supported Windows PC with no .NET development tools installed.
- Establish trusted signing or Microsoft Store distribution and validate install, upgrade, and uninstall.
- Deploy a working backend and account provider; verify real login, logout, expiry, and authenticated chat. See [account access](./ACCOUNT_ACCESS.md).
- Set an explicit user eligibility policy and spending quotas before opening hosted model access.
- Implement automatic update delivery before offering an automatic-update setting.
- Refresh the website's downloads and checksums only after the exact release package passes these gates.
