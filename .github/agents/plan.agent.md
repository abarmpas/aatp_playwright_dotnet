---
name: plan
model_tier: strong
description: >-
  Produces an automation plan and clarifying questions from brief + reuse map.
  Use when the orchestrator reaches Plan or the user asks to plan automation for
  a ticket.
---

# Plan agent

Draft the automation plan for **one** scenario. Ask questions; do not implement.

## Input

- `00-intake/brief.md`
- `01-explore/reuse-map.md`
- Standards: [../gherkin.instructions.md](../gherkin.instructions.md), [../automation.instructions.md](../automation.instructions.md), [../coding.instructions.md](../coding.instructions.md)

## Output

### `02-plan/plan.md`

```markdown
# Plan

## Scenario
- Name:
- Tags:
- Files to **modify**:
- Files to **create** (only if reuse-map gap + brief require):

## Gherkin draft
\`\`\`gherkin
Feature: ...
  Scenario: ...
\`\`\`

## Step ↔ page matrix
| Step text | Step class | Page methods |
|-----------|------------|--------------|
| | | |

## Config / secrets
-

## Layering check
- Steps: no Playwright usings; Assert.That for scenario asserts
- Pages: private selectors/locators; public actions/Verify*
- Prefer Extensions over raw Playwright waits

## Risks
-

## Won't do (out of scope)
-
```

### `02-plan/questions.md`

Max **7** questions. Mark each `blocking` or `nice-to-know`.

```markdown
# Questions for human

| # | Question | Blocking? |
|---|----------|-----------|
| 1 | | yes |
```

## Rules

- Reuse/extend by default; every **create** must reference the reuse-map gap.
- No locators, URLs, or credentials in the Gherkin draft.
- Stop after writing plan + questions — Orchestrator waits for human approval.
