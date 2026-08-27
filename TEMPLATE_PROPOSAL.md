# AATP Playwright .NET Template — Proposal

> **Purpose:** Review this design before any scaffolding. No project files are created yet.  
> **Stack:** .NET 8 · Microsoft.Playwright · Reqnroll (SpecFlow successor) · NUnit · Serilog  
> **Source of truth:** `.github/coding.instructions.md`, `.github/automation.instructions.md`, `.github/gherkin.instructions.md`

---

## 1. Goal

Ship a reusable **template test suite** that teams can clone and extend for UI automation. The template should:

- Enforce Page Object Model (POM) + SpecFlow/Reqnroll layering from day one
- Encode the repo’s coding, automation, and Gherkin rules as structure (not just docs)
- Stay deliberately small: one sample journey, shared extensions, DI wiring, and config — enough to copy, not a full product suite

---

## 2. Principles (from existing instructions)

| Area | Intent |
|------|--------|
| **Coding** | Microsoft .NET conventions, SOLID where useful, composition over inheritance, DI, async/await only, Serilog, no comment noise |
| **Automation** | POM = locators + UI actions + reusable UI verifies; Steps = business orchestration + scenario assertions; prefer project extensions over raw Playwright |
| **Gherkin** | Business language, independent scenarios, Given/When/Then semantics, no locators in feature text |

---

## 3. Proposed solution layout

```text
aatp_playwright_dotnet/
├── .github/
│   ├── coding.instructions.md          # already present
│   ├── automation.instructions.md      # already present
│   ├── gherkin.instructions.md         # already present
│   └── workflows/
│       └── tests.yml                   # build, install browsers, run suite
├── AatpTemplateTestSuite/              # single test project (NUnit + Reqnroll)
│   ├── AatpTemplateTestSuite.csproj
│   ├── appsettings.json                # base URLs, timeouts, browser options
│   ├── appsettings.Development.json
│   ├── Features/                       # *.feature only
│   │   └── Authentication/
│   │       └── Login.feature           # one illustrative journey
│   ├── Steps/                          # step defs — no Microsoft.Playwright usings
│   │   ├── LoginSteps.cs               # steps tied to one page
│   │   └── DashboardSteps.cs           # steps tied to one page
│   ├── Pages/                          # Page Objects
│   │   ├── BasePage.cs
│   │   ├── LoginPage.cs
│   │   └── DashboardPage.cs
│   ├── Hooks/                          # one file per lifecycle phase
│   │   ├── BeforeTestRunHooks.cs       # Serilog config, Playwright install/init
│   │   ├── BeforeScenarioHooks.cs      # browser context + page per scenario
│   │   ├── AfterScenarioHooks.cs       # trace/screenshot on failure, dispose
│   │   └── AfterTestRunHooks.cs        # Log.CloseAndFlush, Playwright dispose
│   └── Infra/                          # everything the tests run on, not the tests
│       ├── DependencyInjection.cs      # container registrations
│       ├── Extensions/                 # LocatorExtensions, PageExtensions, etc.
│       │   ├── LocatorExtensions.cs
│       │   └── PageExtensions.cs
│       ├── Configuration/
│       │   ├── TestSettings.cs
│       │   ├── TestUser.cs
│       │   └── ViewportSettings.cs
│       └── Drivers/
│           └── PlaywrightDriver.cs
├── .editorconfig
├── .gitignore
├── Directory.Build.props               # shared analyzers / nullable / LangVersion
├── global.json                         # pin SDK 8.x
├── AatpTemplateTestSuite.sln
└── README.md                           # how to restore, install browsers, run
```

**One project, no `src/`.** `AatpTemplateTestSuite` sits directly at the repo root: a template should be cloneable and runnable in minutes, and a `src/` wrapper only pays off once there are sibling projects to group. Splitting into `Pages` / `Core` / `Tests` libraries can wait until a second consumer appears; the folders below already separate responsibilities. Root namespace and assembly name are both `AatpTemplateTestSuite`.

**Top-level split.** `Features`, `Steps`, `Pages` and `Hooks` are the test-facing surface a new engineer touches daily; `Infra` holds the plumbing they consume but rarely edit. `Hooks` sits beside `Steps` rather than inside it because hooks are lifecycle wiring, not step definitions — Reqnroll discovers `[Binding]` classes regardless of folder, so nothing breaks.

**Inside `Infra`.** Three purpose-named folders — `Extensions`, `Configuration`, `Drivers` — with `DependencyInjection.cs` sitting directly under `Infra` as the single composition root. No `Support` catch-all: anything new gets its own named folder rather than a shared junk drawer. Namespaces follow the folders (`AatpTemplateTestSuite.Infra.Extensions`, `…Infra.Drivers`, …).

---

## 4. Layering (non-negotiable)

```text
Feature (Gherkin)
    → Step Definitions   (business logic, orchestration, Assert.That)
        → Page Objects   (private locators, public Click/Enter/Verify*Async)
            → Extensions (wait/retry helpers preferred over raw Playwright)
                → Playwright API
```

### Page Objects
- Private selectors (`*Selector`) and locators (`*Locator`)
- Public actions: `Click*`, `Enter*`, `Select*`, `Navigate*`, …
- Public verifies: `Verify*Async`
- Member order: fields → ctor → selectors/locators → public methods → private methods
- No scenario orchestration or business rules

### Step Definitions
- No `using Microsoft.Playwright;` / no FluentAssertions
- Call page methods or shared helpers only
- Scenario assertions with `Assert.That(...)` + clear failure messages
- Soft asserts only when collecting multiple failures in one scenario
- One-page steps → `PageNameSteps.cs`; multi-page → business-segment `*Steps.cs`

### Extensions (seed set)
Mirror the automation instructions so new tests default to helpers:

| Prefer | Instead of |
|--------|------------|
| `IsLocatorVisibleAsync` / `IsLocatorHiddenAsync` | `IsVisibleAsync` |
| `WaitForLocatorToBeVisibleAsync` | ad-hoc waits |
| `WaitForLocatorInnerTextAsync` | `InnerTextAsync` |
| `WaitForAttributeToBePopulatedAsync` | `GetAttributeAsync` |
| `CountElementsAsync` / `AllElementsAsync` | `CountAsync` / `AllAsync` |

Also: cookie helper on context, optional `AddCookiesAsync` pattern as in instructions.

### Hooks
One file per lifecycle phase instead of a single `PlaywrightHooks`, so each class has one reason to change and merge conflicts stay local:

| File | Scope | Responsibility |
|------|-------|----------------|
| `BeforeTestRunHooks.cs` | `[BeforeTestRun]` | Serilog configuration, config binding, Playwright instance + browser launch |
| `BeforeScenarioHooks.cs` | `[BeforeScenario]` | Fresh browser context and page, cookies, tracing start, scenario log scope |
| `AfterScenarioHooks.cs` | `[AfterScenario]` | Screenshot/trace on failure, `Log.Error` for failed scenarios, dispose context |
| `AfterTestRunHooks.cs` | `[AfterTestRun]` | Close browser and Playwright, `Log.CloseAndFlush()` |

Shared state (browser, context, page, settings) stays in the DI container / scenario context rather than static fields on the hook classes, so the four files stay independent.

### Logging
No wrapper or bootstrap class. Any file that needs diagnostics uses Serilog's static API directly, with the level chosen per situation:

```csharp
using Serilog;

Log.Debug("Resolved base url {BaseUrl}", settings.BaseUrl);
Log.Information("Signing in as {User}", user);
Log.Warning("Retrying {Action} after transient failure", nameof(SubmitAsync));
Log.Error(ex, "Login failed for {User}", user);
```

Rough convention for the template:

| Level | Use for |
|-------|---------|
| `Debug` | Config values, resolved selectors, verbose flow while diagnosing locally |
| `Information` | Business milestones in steps (scenario start, key action completed) |
| `Warning` | Recoverable oddities — retries, fallbacks, unexpected-but-handled state |
| `Error` | Failed scenarios and caught exceptions, always with the exception object |

`Log.Logger` still needs one assignment before the first call, otherwise every `Log.*` silently writes nowhere. That single `LoggerConfiguration` lives in `BeforeTestRunHooks.cs` (console sink, level from `appsettings.json`), with `Log.CloseAndFlush()` in `AfterTestRunHooks.cs` — no dedicated `Logging/` folder.

---

## 5. Tech choices

| Concern | Choice | Rationale |
|---------|--------|-----------|
| Runtime | **.NET 8** | Stated requirement; LTS |
| Browser automation | **Microsoft.Playwright** | Official .NET bindings |
| BDD | **Reqnroll** | SpecFlow successor, actively maintained, NUnit plugin |
| Test runner | **NUnit** | Aligns with `Assert.That` / `Assert.Multiple` / `Assert.MultipleAsync` rules |
| DI | **Reqnroll's built-in container (BoDi)** | Steps and hooks get constructor injection with no extra plugin; see section 14 |
| Config | **IConfiguration** + `appsettings*.json` | Base URL, headless, slowMo, viewport, timeouts |
| Logging | **Serilog** static `Log` | Coding instructions; configured once in hooks, console sink (+ optional file) |
| Selectors | `data-test-id` / `data-qa` + scoped CSS | Per automation rules; avoid XPath / text-only |

Optional later (not in v1 template): Allure/ReportPortal, parallel workers profile, Docker run image, API helpers for Given preconditions.

---

## 6. Sample content (minimal, real patterns)

**Target application:** [Agile Actors' Testing Playground](https://aatp.vercel.app/) — a public Next.js e-commerce playground. Sign-in credentials are published on the page itself (`test@test.com` / `test`), so a fresh clone runs green with no environment setup.

Why it fits: every meaningful element already carries a `data-test-id`, matching the selector rule in `automation.instructions.md` with no compromise. The login page exposes `login-form`, `email-input`, `password-input`, `login-button`; `/dashboard` exposes `dashboard-container`, `dashboard-title`, `products-grid`, `product-card-N`, `add-to-cart-button`, `view-details-button` — enough room to grow the suite later without changing target.

One feature for v1 — enough to demonstrate the stack, not domain depth:

```gherkin
Feature: Login
  As a registered user
  I want to sign in to the testing playground
  So that I can browse the product catalogue

  Scenario: Successful sign in lands the user on the dashboard
    Given the user is on the sign in page
    When the user signs in with valid credentials
    Then the dashboard is displayed
```

Layering for this scenario:

| Layer | Responsibility |
|-------|----------------|
| `Login.feature` | Business language only — no URLs, no credentials, no selectors |
| `LoginSteps.cs` | Reads credentials from config and calls `LoginPage` |
| `DashboardSteps.cs` | Calls the dashboard verification |
| `LoginPage.cs` | Private scoped selectors (`[data-test-id="login-form"] [data-test-id="email-input"]`), public `EnterEmailAsync` / `EnterPasswordAsync` / `ClickSignInAsync`, plus one `SignInAsync` composing them |
| `DashboardPage.cs` | `VerifyDashboardIsDisplayedAsync()` — waits for the `/dashboard` URL, then groups the container and products-grid checks |

Notes:
- The "signs in with valid credentials" step is deliberately one business-level step covering fill email, fill password, submit — per the rule against fill/click/verify step fragmentation.
- Credentials come from configuration (user-secrets locally, env vars in CI) even though this app publishes them, so the template demonstrates the right habit rather than hard-coded literals.
- URL assertion uses Playwright's `Expect(Page).ToHaveURLAsync` semantics through a project wait helper, not a hard delay.
- Hook files create and dispose the context and page per scenario and attach Serilog context.

---

## 7. Configuration sketch

```json
{
  "TestSettings": {
    "BaseUrl": "https://aatp.vercel.app",
    "Browser": "Chromium",
    "Headless": true,
    "DefaultTimeoutMs": 15000,
    "NavigationTimeoutMs": 30000,
    "TracingEnabled": true,
    "Viewport": { "Width": 1920, "Height": 1080 }
  },
  "TestUser": {
    "Email": "test@test.com",
    "Password": "test"
  },
  "Serilog": {
    "MinimumLevel": "Information"
  }
}
```

Environment overrides via `TestSettings__BaseUrl`, etc., for CI.

**Secrets.** Two sources only, no `.env` handling and no extra dependency:

- **Local:** `dotnet user-secrets` (the csproj carries a `UserSecretsId`), so credentials live outside the repo tree and can't be committed by accident.

  The `TestUser` values above are the playground's own published demo credentials, committed on purpose so a fresh clone runs green; the README states that any real target must move `TestUser:Password` into user-secrets or CI secrets instead.
- **CI:** environment variables from GitHub secrets, using the same `TestSettings__*` double-underscore convention.

`appsettings.json` holds non-sensitive defaults only. The configuration builder chains JSON → user-secrets → environment variables, so the same `TestSettings` binding works in both places, and the README documents the `dotnet user-secrets set` commands a newcomer needs.

---

## 8. Developer experience (README outline)

1. `dotnet restore` / `dotnet build`
2. `pwsh bin/.../playwright.ps1 install` (or `dotnet tool` equivalent documented)
3. `dotnet test` / filter by feature tag
4. `dotnet test` again after repointing `BaseUrl` at your own environment
5. `dotnet user-secrets set` for real credentials — the committed playground ones are a demo exception
6. Link to `.github/*.instructions.md` as the coding contract for AI and humans

---

## 9. CI (included in the scaffold)

`.github/workflows/tests.yml`, triggered on push, pull request, and `workflow_dispatch`:

| Step | Detail |
|------|--------|
| Checkout + setup SDK | `actions/setup-dotnet` pinned to the `global.json` 8.x version |
| NuGet cache | `actions/cache` keyed on `packages.lock.json` / csproj hash |
| Restore + build | `--configuration Release`, warnings-as-errors kept on |
| Install browsers | `playwright.ps1 install --with-deps chromium` from the build output |
| Test | `dotnet test --no-build` headless; defaults hit the public playground so the run is green with no repo configuration |
| Artifacts | Upload traces, screenshots and TRX on failure via `if: always()` |

Notes: Ubuntu runner with `--with-deps` so no manual apt step. `TestSettings__Headless=true` is forced in CI, and `TestSettings__BaseUrl` can be overridden by a repo variable when a team repoints the template at their own environment — with credentials then coming from GitHub secrets rather than `appsettings.json`.

---

## 10. What v1 will include vs defer

| Include in first scaffold | Defer |
|---------------------------|-------|
| Solution + one test project | Multi-project Core/Pages split |
| Reqnroll + NUnit + Playwright + Serilog + DI | Reporting portals |
| Extensions seed + BasePage + Login/Dashboard sample | Catalogue, cart and checkout coverage |
| Hooks + settings + README | Docker / k8s runners |
| Existing `.github` instruction files | Extra instruction files |
| GitHub Actions workflow (`tests.yml`) | Matrix across browsers/OS, scheduled runs |

---

## 11. Decisions taken

- **No `Logging/` folder** — Serilog used directly via static `Log` (`Debug`/`Information`/`Warning`/`Error`), configured once in `BeforeTestRunHooks`.
- **Hooks split by phase** — four files instead of one `PlaywrightHooks`.
- **`Hooks` outside `Steps`**, and `Extensions` / `Configuration` / `Drivers` grouped under `Infra`.
- **Single project, no `src/`** — `AatpTemplateTestSuite` at the repo root; solution `AatpTemplateTestSuite.sln`.
- **CI ships with the scaffold** — a single-job GitHub Actions workflow (section 9), not a follow-up PR.
- **Secrets via user-secrets + env vars only** — no `.env.example`, no dotenv package.
- **Sample target is [aatp.vercel.app](https://aatp.vercel.app/)** — public, `data-test-id`-rich, published demo credentials, so `dotnet test` and CI are green on a fresh clone. v1 covers sign in → `/dashboard`.
- **Reqnroll over SpecFlow** — SpecFlow reached end-of-life on 31 Dec 2024; last stable 3.9.74 (May 2022) supports .NET up to 7, its VS connector targets `net7.0` and fails binding discovery on .NET 8 (no step navigation or autocomplete), and the 4.x line stayed in beta under a commercial licence. Reqnroll forked from SpecFlow 3.9.x, keeps feature files and most step definitions source-compatible, and supports .NET 8/9. Migration cost for anyone arriving from SpecFlow is package names plus `TechTalk.SpecFlow` → `Reqnroll` usings.

## 12. Risks to keep in mind

- **Third-party target.** The suite depends on a public Vercel deployment; if it changes markup or goes offline, CI turns red for reasons unrelated to the template. Mitigation: everything environment-specific is confined to `appsettings.json` + `TestSettings__*` overrides, so repointing at an internal app is a config change, not a code change.
- **Published credentials.** Committed on purpose for this playground only; the README must be explicit that this is not the pattern for real targets.

---

## 13. Status

Scaffolded and verified. `dotnet test` passes against [aatp.vercel.app](https://aatp.vercel.app/) in both Debug and Release, and the failure path was exercised deliberately (wrong password via `TestUser__Password`) to confirm the error log, screenshot and trace are produced.

## 14. Deviations from the plan, and why

- **`Steps/DashboardSteps.cs` instead of `AuthSteps.cs`.** The rule is that a business-segment file is for steps spanning several pages. Each step in the sample scenario touches exactly one page, so `AuthSteps` would have been an artificial grouping. The multi-page convention still applies to the next journey that needs it.
- **`Features/Authentication/` instead of `Features/Sample/`.** Folders name business capabilities, which is what a growing suite needs; "Sample" would have to be renamed by the first real feature.
- **DI is Reqnroll's own container (BoDi), not `Microsoft.Extensions.DependencyInjection`.** Reqnroll instantiates step and hook classes from its container, so using it directly gives constructor injection with no bridging plugin. `Microsoft.Extensions.Configuration` is still used for settings binding. `Infra/DependencyInjection.cs` remains the single composition root, registering configuration, `TestSettings` and `TestUser` into the test-run container; per-scenario objects (`IBrowserContext`, `IPage`) are registered by `BeforeScenarioHooks` into the scenario container.
- **Serilog configured in code, no `Serilog.Settings.Configuration`.** Only the minimum level is read from `appsettings.json`, so the extra package (and its .NET 10-era dependency chain) bought nothing.
- **Browsers install themselves.** `PlaywrightDriver` calls Playwright's programmatic installer during test-run setup, so `dotnet test` works on a clean machine with no PowerShell — `pwsh` is not present on this one. CI still runs `playwright.ps1 install --with-deps` because it additionally needs Linux system libraries.
- **Navigation waits for network idle.** The first implementation interacted as soon as the DOM was parsed, which raced React hydration: the click submitted the form natively, the URL became `/?`, and the scenario failed. This passed in Debug and failed consistently in Release. `NavigateToPathAsync` now waits for load and network idle, verified over five consecutive Release runs.
- **`TracingEnabled` setting added.** Traces are recorded per scenario and kept only for failures, which is what makes the artifact upload in CI useful.
- **NUnit's `Assert.EnterMultipleScope()`** replaces `Assert.Multiple`/`Assert.MultipleAsync`, whose overloads are ambiguous for lambdas in NUnit 4.6.
- **`*.feature.cs` is git-ignored.** Reqnroll writes code-behind next to each feature file at build time; it is generated output, not source.
