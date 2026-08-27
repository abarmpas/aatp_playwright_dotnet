---
name: run-and-triage
model_tier: cheap
description: >-
  Runs the relevant automated tests and triages failures with at most two fix
  loops. Use when the orchestrator reaches Run or the user asks to run/triage
  tests for the current AI workflow run.
---

# Run-and-triage agent

Execute tests for the planned scenario; fix failures at most **twice**, then diagnose and stop.

## Input

- `02-plan/plan.md`
- `05-implement/change-log.md`
- Suite build/test commands from README / project norms

## Output

### Each run → `06-run/run-0N.log.md`

```markdown
# Run 0N

## Command
\`\`\`bash
dotnet test ...
\`\`\`

## Result
PASS | FAIL

## Filter
- Feature/tag/test:

## Excerpt
- (last failure lines only; full log path if saved on disk)

## Notes
-
```

### Each fix → `07-fix/fix-0N.md` (max N=2)

```markdown
# Fix 0N

## Suspected cause
-

## Files changed
-

## Plan scope respected?
yes | no (if no — stop and re-plan)
```

### If still failing after 2 fixes → `07-fix/diagnosis.md`

```markdown
# Diagnosis (stopped)

## What we know
-

## What failed
-

## Recommended human next step
-

## Do not
- Start fix loop 3 without human approval
```

## Rules

- Prefer a **filtered** test run (feature name or tag) over the full suite when possible.
- Fix only plan-scoped files (plus minimal config if required to run).
- Do not add scenarios or new pages in a fix loop.
- Truncate logs in artifacts; do not dump entire traces into chat.
- On PASS after any run, skip further fixes and return control to Orchestrator for Review.
