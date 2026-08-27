# Code Review

## 1. Verdict

**Needs changes** — reviewed the AATP-2 wrong-credentials implementation against `main`, the files in `05-implement/change-log.md`, the approved plan, and the repository coding, automation, and Gherkin instructions.

## 2. Findings

### Blocking

#### Invalid credentials can still be written to failure artifacts

- **Where:** `AatpTemplateTestSuite/Pages/LoginPage.cs`, `SignInAsync`; nearby artifact handling in `AatpTemplateTestSuite/Hooks/BeforeScenarioHooks.cs` and `AfterScenarioHooks.cs`
- **Issue:** Removing `Log.Information("Signing in as {Email}", email)` prevents direct log disclosure, but it does not satisfy the approved artifact-redaction requirement. Tracing is enabled with screenshots and DOM snapshots while `FillAsync` receives the configured email and password. On a later scenario failure, the trace is persisted, and the full-page failure screenshot can also show the populated email field. Environment or user-secret overrides can therefore appear in saved artifacts.
- **Rule:** `02-plan/plan.md` requires that traces, screenshots, and logs not expose invalid credential values (Config / secrets, line 44), and `00-intake/brief.md` marks the invalid email/password as redacted in artifacts (Data needs, line 33).
- **Fix:** Define an artifact-safe authentication policy before approval: mask the credential fields in failure screenshots and exclude credential entry/credential-bearing DOM snapshots from persisted traces, or restrict this scenario to explicitly non-sensitive fixed dummy values and revise the redaction requirement accordingly. Removing only the Serilog statement is insufficient.

## 3. What looks good

- The feature adds exactly one independent negative scenario, uses the approved tags, and keeps selectors, URLs, and credential values out of Gherkin.
- The shared Given now performs setup only through `LoginPage.NavigateAsync()`; observable checks remain in Then steps.
- Step definitions preserve the required layer boundary: no Playwright dependency, business-level orchestration only, and clear `Assert.That(...)` configuration guards.
- `InvalidTestUser` follows the existing configuration pattern, is registered at the composition root, remains environment-overridable, and is checked to differ from the valid credential pair.
- Page selectors and locators are private, use `data-test-id`, and the new verification methods reuse project wait/visibility extensions.
- Failure feedback is synchronized before the remaining negative outcomes and checks the approved exact text.
- The focused workflow run passed both Login scenarios (`06-run/run-02.log.md`: 2 passed, 0 failed), `git diff --check` passed, and the reviewed files have no IDE linter diagnostics.

## 4. Next step

**Orchestrator resolution:** Blocking waived for AATP-2 — InvalidTestUser values are intentionally committed non-sensitive placeholders per human feedback. Proceed to MR description. Future hardening: mask credential fields in failure screenshots/traces when secrets may be used.
