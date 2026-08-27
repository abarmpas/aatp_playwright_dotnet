---
name: orchestrator
description: >-
  Orchestrates the manual-test-to-automation AI workflow. Use when the user asks
  to "run the orchestrator", "automate this ticket", "run the AI workflow",
  "process this JIRA/ticket", or to continue/resume the next workflow phase.
---

# Orchestrator

Coordinate the pipeline in [ORCHESTRATION.md](ORCHESTRATION.md). You do **not** implement tests yourself unless the current phase is Implement. For each phase: load that agent file, do only that job, write artifacts, append the discussion summary, then stop at gates.

## Setup

1. Require a ticket: pasted text, [ticket-template.md](ticket-template.md) filled, or [samples/AATP-1-successful-login.md](samples/AATP-1-successful-login.md).
2. Create run dir: `.ai-workflow/<run-id>/` where `run-id` is `YYYY-MM-DD_<slug>` from the ticket summary (e.g. `2026-08-27_successful-login`).
3. Create `discussion-summary.md` (append-only). Copy the ticket into `00-intake/source-ac.md`.
4. If the user attaches page HTML, save as `00-intake/page.html` (gitignored with the run).

## Phase loop

Run in order. Before each phase, read the previous artifacts only (plus standards/skills linked by that agent).

| When | Load agent | Write under | After phase |
|------|------------|-------------|-------------|
| Start | [intake.md](intake.md) | `00-intake/` | Continue if one scenario |
| Next | [explore.md](explore.md) | `01-explore/` | Continue |
| Next | [plan.md](plan.md) | `02-plan/` | **STOP — ask questions; wait for answers + explicit plan approval** |
| After approval | Write `03-feedback/answers.md` from human replies | | Continue |
| Next | [validate-plan.md](validate-plan.md) | `04-validate-plan/` | Fail → return to Plan (note in summary); Pass → continue |
| Next | [implement.md](implement.md) | `05-implement/` | Continue |
| Next | [run-and-triage.md](run-and-triage.md) | `06-run/`, `07-fix/` | Green → continue; after 2 failed fixes → **STOP** with diagnosis |
| Next | Apply [../skills/code-review/SKILL.md](../skills/code-review/SKILL.md) | `08-review/review.md` | Blocking → **STOP** or fix only with user OK |
| Next | Apply [../skills/generate-mr-description/SKILL.md](../skills/generate-mr-description/SKILL.md) | `09-mr/mr-description.md` | Done |

## discussion-summary.md format

Append one block per phase:

```markdown
### <ISO timestamp> — <phase name>
- Status: ok | needs_human | failed | stopped
- Artifact: <relative path>
- Notes: <one line>
```

## Rules

- **One scenario** per run; if the ticket has more, ask the user to pick one before Intake completes.
- Never skip the plan-approval gate.
- Never start a third Fix loop; write `07-fix/diagnosis.md` and stop.
- Do not commit `.ai-workflow/` or create the remote MR unless the user asks.
- Prefer reuse; allow new files only when plan + feedback say so.
- Keep phase context small: pass brief / reuse-map / plan / feedback / last run excerpt — not unrelated history.
