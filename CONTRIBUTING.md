# Contributing to Smartitecture

Thanks for your interest in contributing! This project is currently in active development. Please follow the workflow below to ensure smooth collaboration.

## Branching

- `application`: integration branch for application changes
- `feature/<short-name>`: create small, focused branches off `application`

## Commits

- Use Conventional Commits:
  - `feat(ui): ...` new features
  - `fix(services): ...` bug fixes
  - `docs(readme): ...` docs updates
  - `refactor(core): ...` refactors, no behavior change
  - `chore(build): ...` tooling, build, deps

## Pull Requests

- PR from `feature/*` → `application`
- Include:
  - Clear description and linked issues
  - Validation steps to reproduce and verify
  - Screenshots/GIFs for UI changes
- Keep PRs small and focused

## Code Style

- C# latest, `ImplicitUsings` + `Nullable` (see `Directory.Build.props`)
- 4-space indentation, one type per file, file name matches type
- Async methods end with `Async`
- Prefer services and ViewModels over heavy code-behind

## Testing

- Add xUnit tests in `Smartitecture.Tests/` mirroring namespaces
- Run `dotnet test` locally before opening PRs

## Security & Secrets

- Do not commit secrets
- Use environment variables or `dotnet user-secrets`

## UI/UX

- Follow the glassmorphism design system in `App.xaml`
- Use `Card.Glass`, `Button.Primary`, `Button.Glass`, and text styles
- Keep layouts spacious and readable

