# AATP Playwright Template Test Suite

A .NET 8 UI automation template: **Playwright** for browser control, **Reqnroll** for BDD, **NUnit** for assertions, **Serilog** for diagnostics.

Out of the box it runs one scenario against the public [Agile Actors' Testing Playground](https://aatp.vercel.app/) — sign in and land on the dashboard — so a fresh clone proves the whole stack works before you write a line of your own.

## Requirements

- .NET SDK 8.0 (pinned in `global.json`)
- No manual Playwright install: browsers are provisioned on the first test run

## Run

```bash
dotnet test
```

The first run downloads the Chromium build Playwright needs, so expect it to take a couple of minutes. Later runs reuse it.

Useful variations:

```bash
# see the browser
TestSettings__Headless=false dotnet test

# point at your own environment
TestSettings__BaseUrl=https://my-app.example dotnet test

# verbose step output
dotnet test --logger "console;verbosity=detailed"
```

## Layout

```text
AatpTemplateTestSuite/
├── Features/        Gherkin only — business language, no selectors or URLs
├── Steps/           Step definitions: orchestration + scenario assertions
├── Pages/           Page Objects: private locators, public actions and Verify* helpers
├── Hooks/           One file per lifecycle phase
└── Infra/           Driver, configuration, DI, extension helpers
```

The layering rule is one-directional: features call steps, steps call pages, pages call extensions, extensions call Playwright. Step definitions never reference `Microsoft.Playwright` directly, and page objects never contain scenario logic.

`.github/*.instructions.md` holds the coding, automation and Gherkin rules this project is built on. Read those before extending the suite — they apply to humans and AI agents alike.

## Configuration

`AatpTemplateTestSuite/appsettings.json` carries the defaults:

| Setting | Meaning |
|---------|---------|
| `TestSettings:BaseUrl` | Application under test |
| `TestSettings:Browser` | `Chromium`, `Firefox` or `WebKit` |
| `TestSettings:Headless` | Headless by default; set `false` to watch a run |
| `TestSettings:DefaultTimeoutMs` | Per-action timeout |
| `TestSettings:NavigationTimeoutMs` | Per-navigation timeout |
| `TestSettings:TracingEnabled` | Record a Playwright trace, kept only for failures |
| `TestSettings:Viewport` | Browser window size |
| `Serilog:MinimumLevel` | `Debug`, `Information`, `Warning`, `Error` |

Any value can be overridden by environment variable using the double-underscore convention, e.g. `TestSettings__Viewport__Width=1280`. `appsettings.Development.json` layers on top locally; set `DOTNET_ENVIRONMENT` to load a different file.

### Credentials

`TestUser:Email` and `TestUser:Password` are committed here **only** because the playground publishes them on its own sign-in page. For any real application, keep them out of the repository:

```bash
cd AatpTemplateTestSuite
dotnet user-secrets set "TestUser:Password" "your-password"
```

In CI, supply them as environment variables from repository secrets (`TestUser__Password`). Configuration resolves in the order JSON → user-secrets → environment variables, so the later source always wins.

## Failure diagnostics

When a scenario fails, the after-scenario hook writes a full-page screenshot and a Playwright trace to `bin/<configuration>/net8.0/test-artifacts/`, and logs the error with the exception attached. Inspect a trace with:

```bash
pwsh AatpTemplateTestSuite/bin/Debug/net8.0/playwright.ps1 show-trace <file>.zip
```

Passing scenarios leave nothing behind.

## Extending the suite

1. Write the scenario in a `.feature` file using business language.
2. Add a Page Object per screen: private `*Selector` constants and `*Locator` properties, public `Click*` / `Enter*` / `Verify*Async` methods.
3. Add step definitions that call those methods and assert with `Assert.That`.
4. Prefer the helpers in `Infra/Extensions` (`IsLocatorVisibleAsync`, `WaitForLocatorToBeVisibleAsync`, `CountElementsAsync`, …) over raw Playwright calls — they carry the wait and retry behaviour.

Place a step in `PageNameSteps.cs` when it touches a single page, or in a business-segment file when it spans several.

## CI

`.github/workflows/tests.yml` builds in Release, installs Chromium with its system dependencies, runs the suite headless, and uploads the TRX plus any failure artifacts. Set a `BASE_URL` repository variable to point CI at your own environment.
