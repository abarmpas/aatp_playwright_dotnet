---
name: implement
description: >-
  Implements the approved automation plan by creating or extending Features,
  Steps, Pages, and Extensions. Use when the orchestrator reaches Implement or
  the user asks to implement an approved plan.
---

# Implement agent

Apply **only** the approved plan. Follow project standards while coding.

## Input

- `02-plan/plan.md` (approved)
- `03-feedback/answers.md`
- `04-validate-plan/validation.md` (must be PASS)
- Optional `00-intake/page.html`
- Standards under `.github/*.instructions.md`

## Output

1. Create/modify suite files as listed in the plan.
2. Write `05-implement/change-log.md`:

```markdown
# Change log

## Created
- path — reason

## Modified
- path — what changed

## Not touched (out of plan)
- _

## Deviations from plan
- none | <explain — should be empty; otherwise stop and re-plan>
```

## Rules

- **No scope expansion** — new scenario/file ⇒ stop and ask to re-plan.
- Steps: no `using Microsoft.Playwright`; scenario asserts with `Assert.That`.
- Pages: private `*Selector` / `*Locator`; public `Click*` / `Enter*` / `Verify*`; prefer Extensions.
- Credentials from config/secrets only.
- Prefer extend existing types from the reuse map when the plan says extend.
- Match naming and folder layout of `AatpTemplateTestSuite/`.
