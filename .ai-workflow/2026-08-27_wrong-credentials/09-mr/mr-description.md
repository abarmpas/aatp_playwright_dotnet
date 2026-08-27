**Suggested title:** AATP-2: Automate wrong-credentials login negative scenario

## Summary

- Add a negative Login scenario (`Sign in fails with wrong credentials`) that asserts sign-in rejection, visible failure feedback, and that the user stays off the dashboard.
- Introduce `InvalidTestUser` configuration (mirroring `TestUser`) so invalid credentials are read from config/env rather than hard-coded in Gherkin.
- Extend existing Login page objects and step bindings to cover the failure path while keeping the shared Given setup-only (navigation only; assertions remain in Then steps).

## Changes

**Test suite (primary)**

- `Login.feature` — new `@login @negative @authentication` scenario with business-language steps only.
- `InvalidTestUser.cs` (new) + `DependencyInjection.cs` — bind and register `InvalidTestUser:Email` / `InvalidTestUser:Password` from configuration.
- `LoginSteps.cs` — invalid sign-in When/Then bindings; guards that invalid credentials are configured and differ from the valid pair; Given simplified to `NavigateAsync()` only.
- `LoginPage.cs` — `[data-test-id="login-error"]` verification (visibility + exact `"Invalid credentials"` text); removed email logging from `SignInAsync`.
- `DashboardPage.cs` / `DashboardSteps.cs` — `VerifyDashboardIsNotDisplayedAsync` for the negative outcome.
- `appsettings.json` — placeholder `InvalidTestUser` values (`admin@admin.com` / `admin`), overridable via user-secrets or environment.

**Workflow / agent tooling (supporting)**

- Replace sample ticket `AATP-1-successful-login.md` with `AATP-2-wrong-credentials.md`; update orchestrator docs and script README examples.
- `check-artifacts.sh` — portable PASS/FAIL grep for macOS BSD `grep` (weak `\b` support).
- `workflow.sh` — skip `cp` in `approve-plan` when source and destination are the same file.

## Test plan

- [ ] `dotnet restore` / `dotnet build`
- [ ] Playwright browsers installed (`pwsh bin/Debug/net8.0/playwright.ps1 install` if needed)
- [ ] `dotnet test AatpTemplateTestSuite.sln --filter FullyQualifiedName~Login` — expect **2 passed** (existing happy path + new negative scenario)
- [ ] Confirm `InvalidTestUser` overrides work via user-secrets or env without changing the feature file
- [ ] Manual check: wrong credentials show the login error and do not reach the dashboard at https://aatp.vercel.app/

**Workflow run evidence:** `06-run/run-02.log.md` — `FullyQualifiedName~Login` filter, exit 0, 2 passed / 0 failed.

## Risk & impact

- **Layering:** Feature → Steps → Pages pattern preserved; no Playwright in step definitions.
- **Config:** `InvalidTestUser` placeholders are committed demo values (waived in review — not production secrets). If real secrets are used via env/user-secrets, failure screenshots/traces could still capture populated fields; future hardening may mask credential fields in artifacts.
- **Shared Given change:** `Given the user is on the sign in page` no longer asserts form visibility inline — form check moved to Then steps for the negative scenario (`the user remains on the sign in page`). Existing happy-path scenario unchanged in observable outcomes.
- **CI / flakiness:** Error message assertion is exact string match (`"Invalid credentials"`); low risk on the stable playground app. UI timing handled via existing locator wait extensions.
- **Workflow scripts:** Artifact-check and approve-plan fixes are low-risk shell portability improvements; unrelated to test runtime behavior.

## Related

- Ticket: **AATP-2** — User cannot sign in with wrong credentials
- Workflow run: `.ai-workflow/2026-08-27_wrong-credentials/`
- Sample ticket: `.github/agents/samples/AATP-2-wrong-credentials.md`

## Checklist

- [x] Follows `.github/coding.instructions.md`
- [x] Follows `.github/automation.instructions.md` (and Gherkin rules if features changed)
- [x] No secrets or hard-coded credentials in features or source (credentials in `appsettings.json` are approved non-sensitive placeholders; feature text is credential-free)
- [x] Self-review done; focused Login test run passed locally (`06-run/run-02.log.md`). Review blocking item on artifact redaction waived per human feedback — see `08-review/review.md`.
