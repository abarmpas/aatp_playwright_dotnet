# Plan

## Scenario
- Name: Sign in fails with wrong credentials
- Tags: `@login` `@negative` `@authentication`
- Files to **modify**:
  - `AatpTemplateTestSuite/Features/Authentication/Login.feature` — add the single negative scenario alongside the existing happy path.
  - `AatpTemplateTestSuite/Steps/LoginSteps.cs` — change the shared `GivenTheUserIsOnTheSignInPageAsync()` to call only `LoginPage.NavigateAsync()`, inject invalid-user configuration, and add invalid sign-in, failure-feedback, and sign-in-page outcome bindings.
  - `AatpTemplateTestSuite/Steps/DashboardSteps.cs` — add the complementary dashboard-not-displayed binding.
  - `AatpTemplateTestSuite/Pages/LoginPage.cs` — add failure-feedback verification that asserts visibility and exact text, while retaining the existing form verification for Then outcomes; stop logging the supplied email so invalid credentials remain redacted in artifacts.
  - `AatpTemplateTestSuite/Pages/DashboardPage.cs` — add negative dashboard verification using the existing dashboard locator and project extensions.
  - `AatpTemplateTestSuite/Infra/DependencyInjection.cs` — bind and register the dedicated invalid-user configuration type.
  - `AatpTemplateTestSuite/appsettings.json` — add the dedicated `InvalidTestUser` section with the approved non-sensitive placeholders `admin@admin.com` / `admin`.
- Files to **create** (only if reuse-map gap + brief require):
  - `AatpTemplateTestSuite/Infra/Configuration/InvalidTestUser.cs` — required by the reuse-map Config gap because the existing `TestUser` type represents known-valid credentials and a separately injectable invalid-user contract does not exist.

## Gherkin draft
```gherkin
Feature: Login

  @login @negative @authentication
  Scenario: Sign in fails with wrong credentials
    Given the user is on the sign in page
    When the user signs in with invalid credentials
    Then sign in failure is shown
    And the user remains on the sign in page
    And the dashboard is not displayed
```

## Step ↔ page matrix
| Step text | Step class | Page methods |
|-----------|------------|--------------|
| `Given the user is on the sign in page` | `LoginSteps` | Change shared `GivenTheUserIsOnTheSignInPageAsync()` to call only `LoginPage.NavigateAsync()`; its existing form wait provides setup readiness without an assertion |
| `When the user signs in with invalid credentials` | `LoginSteps` | Reuse `LoginPage.SignInAsync(email, password)` with injected `InvalidTestUser`; assert configuration is non-empty and does not equal the valid pair |
| `Then sign in failure is shown` | `LoginSteps` | Extend `LoginPage` with `VerifySignInErrorIsDisplayedAsync()`; assert the error is visible and its text is exactly `Invalid credentials` |
| `And the user remains on the sign in page` | `LoginSteps` | Reuse `LoginPage.VerifySignInFormIsDisplayedAsync()` |
| `And the dashboard is not displayed` | `DashboardSteps` | Extend `DashboardPage` with `VerifyDashboardIsNotDisplayedAsync()` |

## Config / secrets
- Add `InvalidTestUser` with `SectionName`, `Email`, and `Password`, following the existing `TestUser` configuration pattern.
- Commit an `InvalidTestUser` section to `appsettings.json` with approved non-sensitive placeholders: email `admin@admin.com` and password `admin`.
- Keep the section overridable through user-secrets or environment variables (`InvalidTestUser__Email` and `InvalidTestUser__Password`).
- Keep invalid values non-empty and ensure the pair differs from `TestUser`; enforce both conditions with clear `Assert.That(...)` configuration guards in `LoginSteps`.
- Do not log the email or password submitted through `LoginPage.SignInAsync`; traces, screenshots, and logs must not expose invalid credential values.

## Layering check
- Steps: no Playwright usings; use `Assert.That(...)` with clear messages for configuration guards and scenario outcomes.
- Pages: keep selectors and locators private; expose public actions and `Verify*` methods only.
- Reuse `SignInAsync()`, `VerifySignInFormIsDisplayedAsync()`, `IsLocatorVisibleAsync()`, and `IsLocatorHiddenAsync()` instead of duplicating interactions or using raw waits.
- Keep the Given setup-only: `GivenTheUserIsOnTheSignInPageAsync()` calls only `NavigateAsync()`, which already waits for the form; do not call assertion-bearing `VerifySignInFormIsDisplayedAsync()` from Given.
- Keep form, error, path, and dashboard assertions in Then steps. `VerifySignInErrorIsDisplayedAsync()` verifies both visibility and exact text `Invalid credentials`.
- Keep the feature in business language and independent of credential values, paths, selectors, and implementation details.

## Risks
- The committed invalid placeholders must never accidentally match the existing valid pair; guard the complete pair before submission.
- The exact error assertion intentionally couples automation to the approved `Invalid credentials` product copy; a copy change will require an automation update.
- Synchronize on visible failure feedback before checking that the sign-in form remains and the dashboard stays hidden, avoiding a race immediately after submission.
- The existing `SignInAsync()` logs the email; retaining that log would violate the brief's artifact-redaction requirement.

## Won't do (out of scope)
- Do not change or duplicate the successful sign-in scenario.
- Do not cover account lockout, password reset, SSO, remember-me, or empty-field validation.
- Do not add new hooks, extensions, page classes, or feature files.
- Do not hard-code credential values in Gherkin or step definitions.
