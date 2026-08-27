# Reuse map

## Search notes
- Paths inspected:
  - `AatpTemplateTestSuite/Features/Authentication/Login.feature`
  - `AatpTemplateTestSuite/Steps/LoginSteps.cs`, `Steps/DashboardSteps.cs`
  - `AatpTemplateTestSuite/Pages/LoginPage.cs`, `Pages/DashboardPage.cs`, `Pages/BasePage.cs`
  - `AatpTemplateTestSuite/Infra/Configuration/TestUser.cs`, `TestSettings.cs`
  - `AatpTemplateTestSuite/Infra/DependencyInjection.cs`
  - `AatpTemplateTestSuite/Infra/Extensions/LocatorExtensions.cs`, `PageExtensions.cs`
  - `AatpTemplateTestSuite/Hooks/BeforeTestRunHooks.cs`, `BeforeScenarioHooks.cs`, `AfterScenarioHooks.cs`
  - `AatpTemplateTestSuite/appsettings.json`, `appsettings.Development.json`
  - Live target HTML/JS at `https://aatp.vercel.app/` (no `00-intake/page.html`)
- Keywords: login, sign in, credentials, invalid, wrong, error, negative, TestUser, data-test-id, dashboard

## Existing matches

| Layer | Path | Symbol / scenario | Verdict | Reason |
|-------|------|-------------------|---------|--------|
| Feature | `AatpTemplateTestSuite/Features/Authentication/Login.feature` | `Scenario: Successful sign in lands the user on the dashboard` | **extend** | Same feature and actor; add one negative scenario with tags `@login` `@negative` `@authentication` per brief. Do not duplicate happy path. |
| Steps | `AatpTemplateTestSuite/Steps/LoginSteps.cs` | `GivenTheUserIsOnTheSignInPageAsync()`, `WhenTheUserSignsInWithValidCredentialsAsync()` | **extend** | Reuse `Given` as-is (`NavigateAsync` + `VerifySignInFormIsDisplayedAsync`). Add `When` for invalid credentials mirroring valid step (config guard + `SignInAsync`). Add `Then` steps for failure feedback and staying on sign-in. |
| Steps | `AatpTemplateTestSuite/Steps/DashboardSteps.cs` | `ThenTheDashboardIsDisplayedAsync()` | **extend** | Positive assertion exists only. Add complementary `Then` for dashboard not shown (brief AC #2). |
| Page | `AatpTemplateTestSuite/Pages/LoginPage.cs` | `NavigateAsync()`, `SignInAsync(email, password)`, `EnterEmailAsync`, `EnterPasswordAsync`, `ClickSignInAsync`, `VerifySignInFormIsDisplayedAsync()` | **extend** | Form submit path fully reusable for wrong credentials. Add scoped `login-error` locator + `VerifySignInErrorIsDisplayedAsync()` (and optionally `VerifyUserRemainsOnSignInPageAsync()` via form visibility + path check). |
| Page | `AatpTemplateTestSuite/Pages/DashboardPage.cs` | `VerifyDashboardIsDisplayedAsync()` | **extend** | Uses `Page.WaitForPathAsync("/dashboard")` — suitable for happy path only. Add `VerifyDashboardIsNotDisplayedAsync()` using `PageExtensions.IsAtPathAsync` and/or hidden dashboard locators. |
| Page | `AatpTemplateTestSuite/Pages/BasePage.cs` | `NavigateToAsync(path)` | **reuse** | No change; login page navigation already uses `/`. |
| Extension | `AatpTemplateTestSuite/Infra/Extensions/LocatorExtensions.cs` | `IsLocatorVisibleAsync`, `WaitForLocatorToBeVisibleAsync`, `WaitForLocatorInnerTextAsync` | **reuse** | Sufficient for asserting visible error message on `login-error`. |
| Extension | `AatpTemplateTestSuite/Infra/Extensions/PageExtensions.cs` | `IsAtPathAsync(expectedPath)`, `WaitForPathAsync(expectedPath)` | **reuse** | `IsAtPathAsync("/dashboard")` returns false when sign-in fails — use in negative dashboard assertion. |
| Hooks/Config | `AatpTemplateTestSuite/Hooks/BeforeTestRunHooks.cs`, `BeforeScenarioHooks.cs`, `AfterScenarioHooks.cs` | Browser launch, per-scenario context, failure screenshot/trace | **reuse** | No login-specific hooks; failure diagnostics already cover negative scenario failures. |
| Hooks/Config | `AatpTemplateTestSuite/Infra/Configuration/TestSettings.cs` + `appsettings.json` | `BaseUrl` = `https://aatp.vercel.app` | **reuse** | Brief precondition (reachable BaseUrl) already satisfied; overridable via env (`TestSettings__BaseUrl`). |
| Hooks/Config | `AatpTemplateTestSuite/Infra/Configuration/TestUser.cs` + `appsettings.json` | `TestUser.Email`, `TestUser.Password` (`test@test.com` / `test`) | **reuse** | Keep for existing happy-path scenario only. **Do not** reuse for negative test — values are the known-valid playground credentials. |
| Hooks/Config | `AatpTemplateTestSuite/Infra/DependencyInjection.cs` | `RegisterTestRunDependencies()`, `Bind<TestUser>()` | **extend** | Pattern for binding config sections exists; register a dedicated invalid-user section (see Gaps). |

## Gaps (create only if needed)

| Layer | Proposed name | Why not covered by reuse |
|-------|---------------|--------------------------|
| Config | `InvalidTestUser` (or `InvalidUser`) in `Infra/Configuration/` + `InvalidUser` section in `appsettings.json` | Brief blocker: no dedicated invalid-user settings exist. `TestUser` holds valid playground creds; negative scenario must read invalid email/password from config / user-secrets / env without hard-coding in Gherkin. |
| Config/DI | Register invalid-user binding in `DependencyInjection.cs` | `TestUser` is the only user model registered today; negative step needs its own injected options instance. |
| Page | `LoginPage.VerifySignInErrorIsDisplayedAsync()` | No error locator or verification method exists; `login-error` test id confirmed on live app but absent from repo. |
| Page | `DashboardPage.VerifyDashboardIsNotDisplayedAsync()` (or equivalent in `LoginPage`) | Only positive dashboard verification exists; brief requires explicit “dashboard is not shown” assertion. |
| Steps | `WhenTheUserSignsInWithInvalidCredentialsAsync()` | Valid-credentials `When` binds to `TestUser`; invalid path needs separate step + config source. |
| Steps | `ThenSignInFailureIsShownAsync()` / `ThenTheDashboardIsNotDisplayedAsync()` | No failure-feedback or negative-dashboard step definitions exist. |
| Feature | New scenario in `Login.feature` (e.g. “Sign in fails with wrong credentials”) | Only happy-path scenario exists. |

## Locator hints (from HTML or known data-test-ids)
- Sign-in form (already in `LoginPage.cs`): `login-form`, `email-input`, `password-input`, `login-button`
- Sign-in container/title (present on live app, not yet in page object): `login-container`, `login-title`, `login-hint`
- **Failure feedback (discovered from live app JS/HTML):** `login-error` — rendered when submit fails; messages include `"Invalid credentials"` (wrong email/password) and `"Please fill in all fields"` (empty fields — out of scope unless same path)
- Dashboard (already in `DashboardPage.cs`): `dashboard-container`, `dashboard-title`, `products-grid`
- Login path `/`; successful auth navigates to `/dashboard` (`DashboardPage.DashboardPath`)

## Risks
- **Blocking open question:** Dedicated `InvalidUser` config section is not defined anywhere in repo; Plan/Implement must add it (with safe placeholder values in JSON and secrets/env for real runs) before the negative scenario can run in CI.
- **Credential separation:** `appsettings.json` commits valid `TestUser` creds for the playground demo; invalid creds must differ (e.g. `wrong@example.com` / `wrong-password`) and must not accidentally match valid pair.
- **Error message text:** App shows `"Invalid credentials"` for wrong pair; assert via visible `login-error` element rather than hard-coding exact wording in Gherkin (keep business language in feature; verification in page object).
- **Empty-field vs wrong-credentials:** Live app uses the same `login-error` element for both cases; keep scenario focused on wrong credentials (non-empty invalid values) to avoid overlapping with out-of-scope empty-field validation.
