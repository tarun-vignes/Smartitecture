# Smartitecture public release plan

## Product outcome

A Windows 10/11 user can install Smartitecture, complete setup, get a useful first result, and recover from common failures without a terminal, development tools, or help from the developer.

Initial scope: the existing Windows desktop assistant, local diagnostics, and optional connected AI. Cross-platform clients and new automation capabilities are separate work.

## Baseline assessed on 2026-10-27

- Desktop Debug build passed with zero warnings and errors (`dotnet build Smartitecture.csproj -c Debug --no-restore`). This does not establish installer or runtime readiness.
- Desktop publishing and MSIX packaging scripts already exist. Public release still requires trusted signing; development certificate installation is unsuitable for ordinary users.
- AI server setup currently asks users for a backend URL and shared API key. Decide the public AI access model before redesigning that flow.
- `Services/PreferencesService.cs` serializes the backend key into local JSON and suppresses load/save failures. Protect credentials and make persistence failures visible.
- The backend already has request rate limiting. Shared-key authentication still needs a different access model if AI will be included for individual public users.
- Installation, chat, UI, and audit-log smoke checks already exist in the release checklist. Execute them against the actual release package on a clean machine.

## Delivery order and acceptance criteria

### 1. Reliable local application

- Store credentials using Windows user-scoped protection and migrate existing preferences safely.
- Save settings atomically; report failures and preserve recoverable settings.
- Verify launch from an installed location and an unrelated working directory, with no administrator privileges.
- Make unavailable AI, network failures, and permission failures understandable and recoverable.
- Acceptance: a fresh Windows user can launch, use a local diagnostic, save preferences, restart, and retain settings without developer assistance.

### 2. Guided setup and first result

- Explain local capabilities and what data connected AI receives.
- Let users complete setup without cloud AI and configure AI later.
- Offer a small set of useful first actions, such as checking system performance or Defender status.
- Request sensitive permissions when needed and explain the intended action before confirmation.
- Acceptance: a first-time user reaches a useful result without editing files or entering shell commands.

### 3. Public AI access

Product decision pending: included hosted AI with accounts, user-supplied provider access, or local tools with optional AI.

- Included hosted AI requires individual identity, revocable sessions, usage limits, cost controls, and an account recovery/deletion flow. Do not ship a privileged shared backend secret inside the desktop package.
- User-supplied access requires protected credential storage, connection validation, and clear provider cost guidance.
- Keep local tools useful when the AI service is unavailable.
- Acceptance: an authorized user can connect, receive an answer, understand quota or authentication failures, and disconnect safely.

### 4. Install, update, and support

- Produce a versioned self-contained Windows package and distribute through trusted signing or a suitable store channel.
- Verify install, launch, upgrade with preferences preserved, and uninstall on clean supported Windows machines.
- Implement and verify the update behavior exposed in Settings; avoid presenting an unimplemented preference as working functionality.
- Publish installation help, privacy information, known limitations, release notes, and a support contact.
- Provide user-initiated diagnostics export with credential redaction.
- Acceptance: users can install and upgrade without an SDK, manual certificate trust, or developer assistance.

### 5. Release gate

- Run Release builds and the existing automated tests; record results rather than assuming Debug success covers release readiness.
- Complete the existing release checklist against the exact package to be distributed.
- Verify keyboard navigation, display scaling, supported language labels, offline behavior, and destructive-operation confirmations.
- Run a small external beta, resolve blocking usability/reliability issues, and then broaden distribution.

## Immediate implementation slice

Start with protected, reliable preferences and a guided setup flow. These improve every AI access model. Then complete the selected AI access model and validate the installer before labeling the software ready for general public use.
