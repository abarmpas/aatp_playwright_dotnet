---
name: code-review
description: >-
  Review code changes, files or implementation for compliance with project coding
  Standards, automation best practices and architectural conventions. Use when the
  user asks to "review my changes", "review this file", "review my MR", "review my
  implementation", "check my code", "check this file", "check this step Definition",
  "validate my changes", "does this follow conventions", "is this correct", "is this
  the right way?", "look at my page object", "code review", or any request to
  inspect, critique or validate code against project standards.
---

# Code Review

Review the target code against this repo’s standards. Prefer concrete, actionable findings over generic praise. Do not change files unless the user also asks for fixes.

## Source of truth

Read and apply these before judging compliance (only the sections relevant to the files under review):

| Target | Standards |
|--------|-----------|
| Any C# | [coding.instructions.md](../../coding.instructions.md) |
| Pages, Steps, Extensions, Hooks, Playwright usage | [automation.instructions.md](../../automation.instructions.md) |
| `*.feature` | [gherkin.instructions.md](../../gherkin.instructions.md) |

Architectural layering (non-negotiable):

```text
Feature (Gherkin)
  → Step Definitions   (business orchestration + scenario assertions)
    → Page Objects     (locators + UI actions + reusable UI verifies)
      → Extensions     (wait/retry helpers)
        → Playwright API
```

## When applying

1. Identify scope: named file(s), selection, uncommitted diff, or MR/PR. If unclear, ask once or default to the active file / current changes.
2. Read the target code and only the nearby collaborators needed to judge layer boundaries (e.g. a step’s page methods).
3. Load the matching instruction files above; do not invent house rules that contradict them.
4. Review only; do not implement fixes unless asked.

## Review checklist (apply what fits)

### Coding ([coding.instructions.md](../../coding.instructions.md))
- Naming, single responsibility, composition over inheritance, DI where appropriate
- Async/await only — no `.Wait()`, `.Result()`, or blocking calls
- Serilog for diagnostics — no `Console.WriteLine` / `Debug.WriteLine`
- Meaningful, specific exception handling; no dead or commented-out code
- No noisy XML/explanatory comments by default

### Layering & automation ([automation.instructions.md](../../automation.instructions.md))
- **Steps:** no `using Microsoft.Playwright;` / no FluentAssertions; orchestration + `Assert.That(...)` with clear messages; one-page → `*PageSteps` / `*Steps` tied to that page; multi-page → business-segment `*Steps`
- **Pages:** private selectors (`*Selector`) and locators (`*Locator`); public `Click*` / `Enter*` / `Select*` / `Navigate*` / `Verify*`; member order: fields → ctor → selectors/locators → public → private; no business orchestration
- **Selectors:** prefer `data-test-id` / `data-qa` + scoped CSS; avoid absolute XPath and text-only targeting; no `force: true` on clicks
- **Waits:** Playwright auto-wait; project extensions first (`IsLocatorVisibleAsync`, `WaitForLocatorToBeVisibleAsync`, etc.); no `Task.Delay` / thread sleeps
- **Assertions:** scenario asserts in steps; reusable UI verifies may live on pages with `Assert.Multiple` / `Assert.MultipleAsync`

### Gherkin ([gherkin.instructions.md](../../gherkin.instructions.md))
- Business language; independent scenarios; correct Given/When/Then semantics
- No locators, CSS, XPath, or technical plumbing in step text
- No credentials or secrets hard-coded in features

## Response structure

### 1. Verdict
One short line: **Compliant** / **Minor issues** / **Needs changes** — plus what was reviewed (files or scope).

### 2. Findings
List issues ordered by severity. Skip empty severity sections.

| Severity | Meaning |
|----------|---------|
| Blocking | Violates layering, standards, or will cause flakes/incorrect tests |
| Should fix | Clear convention breach or maintainability problem |
| Nit | Optional polish |

For each finding:

- **Where:** file and symbol (or line if known)
- **Issue:** what is wrong
- **Rule:** which instruction/convention it breaks
- **Fix:** concrete suggestion (short snippet only when it clarifies)

### 3. What looks good
Brief bullets for correctly applied patterns (keeps the review balanced; omit if nothing notable).

### 4. Next step
One offer: e.g. apply Blocking/Should-fix fixes, or zoom in on one file — not a long menu.

## Style rules

- Be direct and specific; cite real names from the code.
- Prefer fewer high-signal findings over exhaustive pedantry.
- If standards conflict or the code is ambiguous, say so and recommend the safer interpretation aligned with the instruction files.
- Do not restate entire instruction docs — point to them and judge the code.
