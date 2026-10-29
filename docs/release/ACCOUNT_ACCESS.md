# Account access deployment

The desktop supports OAuth device authorization: it displays a verification code, opens the identity provider in the browser, polls for completion, verifies API access, and protects the access token with Windows user-scoped encryption. Passwords and provider client secrets are never collected by the desktop.

## Deployment prerequisites

Register a **public/native application** with an identity provider that supports OpenID Connect discovery, the device authorization grant, and JWT access tokens for a custom API. Enable the permitted sign-in methods there. No identity-provider account has been provisioned by this change.

Configure the backend environment:

- `SMARTITECTURE_AUTH_AUTHORITY`: HTTPS issuer/discovery authority.
- `SMARTITECTURE_AUTH_AUDIENCE`: the identifier for this API.
- `SMARTITECTURE_AUTH_CLIENT_ID`: public/native application identifier; not a secret.
- `SMARTITECTURE_AUTH_SCOPE`: provider-specific delegated API scopes, typically including `openid profile`.

The provider must publish `device_authorization_endpoint` and `token_endpoint` in its discovery document. Its device flow must accept the configured audience and scopes. Provider compatibility must be verified before distribution.

When an authority is configured, account authentication replaces shared-key access for `/v1/*`. Tokens are validated using Microsoft's JWT middleware, including issuer, audience, signature, and expiry. A subject claim is required. Rate limiting partitions authenticated requests by issuer and subject; anonymous requests use the remote IP. This is an in-memory request-rate limit, not a persistent spending quota or entitlement system.

Without an authority, existing private/shared-key deployments retain their existing authentication behavior. `/auth/config` returns 503 so the desktop does not imply sign-in is ready.

Set the public service URL in the release package's `Backend.BaseUrl` before distribution. This is operator configuration performed once per package, not something public users should enter. No production URL or identity tenant is activated by this change. The advanced URL override remains available for private installations.

## Session behavior

- Settings offers Sign in and Sign out / cancel.
- A successful token exchange must also pass `GET /v1/account` before the app saves the session.
- Access tokens are encrypted in the Windows user's local application-data directory and bound to the exact service address.
- Expired sessions are ignored; users sign in again. Refresh-token renewal is not implemented.
- Sign out removes this PC's saved token. It does not revoke the issued token at the identity provider or end the browser session.
- Local tools remain available when sign-in is unavailable or cancelled.

## Required release verification

Verify real sign-in, denied consent, cancellation, expiry, sign-out, and an authenticated chat against the configured identity tenant. Confirm missing tokens, invalid signatures, wrong issuer, wrong audience, and expired tokens are rejected by both chat endpoints. Verify an ordinary shared key cannot bypass account authentication. Confirm quotas and account eligibility policy before opening paid model access to the public.

This implementation is not a production deployment or an end-to-end identity-provider certification.

References: [OAuth device authorization specification](https://datatracker.ietf.org/doc/html/rfc8628), [Microsoft JWT bearer authentication](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/configure-jwt-bearer-authentication?view=aspnetcore-8.0).
