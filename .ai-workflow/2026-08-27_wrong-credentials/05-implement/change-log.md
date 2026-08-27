# Change log

## Created
- `AatpTemplateTestSuite/Infra/Configuration/InvalidTestUser.cs` — provide separately injectable invalid credentials.

## Modified
- `AatpTemplateTestSuite/Features/Authentication/Login.feature` — add the approved wrong-credentials scenario and tags.
- `AatpTemplateTestSuite/Steps/LoginSteps.cs` — keep sign-in setup assertion-free and bind invalid sign-in outcomes.
- `AatpTemplateTestSuite/Steps/DashboardSteps.cs` — bind the dashboard-not-displayed outcome.
- `AatpTemplateTestSuite/Pages/LoginPage.cs` — verify exact sign-in failure feedback and redact credentials from logging.
- `AatpTemplateTestSuite/Pages/DashboardPage.cs` — verify that the dashboard remains hidden.
- `AatpTemplateTestSuite/Infra/DependencyInjection.cs` — register invalid-user configuration.
- `AatpTemplateTestSuite/appsettings.json` — configure approved invalid-user placeholder credentials.

## Not touched (out of plan)
- _

## Deviations from plan
- none
