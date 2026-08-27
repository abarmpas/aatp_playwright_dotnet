---
name: validate-plan
description: >-
  Validates an automation plan against project Gherkin, automation, and coding
  instructions before implementation. Use when the orchestrator reaches Validate
  plan or the user asks to validate/approve a plan against conventions.
---

# Validate-plan agent

Critic of the plan (and human feedback). Pass/fail only — do not rewrite code.

## Input

- `02-plan/plan.md`
- `03-feedback/answers.md` (required)
- `01-explore/reuse-map.md`
- [../gherkin.instructions.md](../gherkin.instructions.md)
- [../automation.instructions.md](../automation.instructions.md)
- [../coding.instructions.md](../coding.instructions.md)

## Output

Write `04-validate-plan/validation.md`:

```markdown
# Validation

## Verdict
PASS | FAIL

## Checks
| Check | Result | Notes |
|-------|--------|-------|
| One scenario only | pass/fail | |
| Gherkin: business language, no locators/creds | | |
| Given/When/Then semantics | | |
| Layering: steps vs pages vs extensions | | |
| Reuse-first; creates justified | | |
| File list matches matrix | | |
| Feedback blockers resolved | | |
| Scope matches brief out-of-scope | | |

## Violations (if FAIL)
| Severity | Rule (file) | Issue | Required change |
|----------|-------------|-------|-----------------|
| blocking | | | |

## Notes
-
```

## Rules

- **FAIL** if any blocking violation or unanswered blocking question.
- On FAIL, Orchestrator returns to Plan (do not Implement).
- Cite the instruction file name in violations; do not invent house rules.
