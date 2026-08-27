# AI workflow orchestration

> **Template / demonstration.** Chat-driven checklist that turns one manual test case (JIRA-style ticket) into suite changes, a test run, a review, and an MR description. Same artifacts work later under CI.

## Roles

| Piece | Location | Purpose |
|-------|----------|---------|
| **Orchestrator** | [orchestrator.md](orchestrator.md) | Runs phases in order, enforces gates, writes `discussion-summary.md` |
| **Phase agents** | `*.md` in this folder | One job each; write evaluable artifacts |
| **Skills** | [../skills/](../skills/) | How-to playbooks (review, MR, explain, …) reused by agents |
| **Standards** | [../coding.instructions.md](../coding.instructions.md), [../automation.instructions.md](../automation.instructions.md), [../gherkin.instructions.md](../gherkin.instructions.md) | Source of truth for Plan / Validate / Implement / Review |
| **Ticket input** | [ticket-template.md](ticket-template.md) + [samples/](samples/) | Stand-in for JIRA until a real API exists |
| **Run output** | `.ai-workflow/<run-id>/` (**gitignored**) | Artifacts per phase |

## Pipeline

```text
Ticket (paste / sample)
  → Intake → Explore → Plan + questions
  → Human feedback + approve plan          ← gate
  → Validate plan                          ← gate
  → Implement → Run → Fix (≤2) → Run
  → Review → MR description
```

| # | Agent | Artifact folder | Gate |
|---|--------|-----------------|------|
| 0 | [intake.md](intake.md) | `00-intake/` | One scenario; secrets redacted |
| 1 | [explore.md](explore.md) | `01-explore/` | Reuse map complete |
| 2 | [plan.md](plan.md) | `02-plan/` | Human answers + **approve** |
| 3 | [validate-plan.md](validate-plan.md) | `04-validate-plan/` | Pass (else back to Plan) |
| 4 | [implement.md](implement.md) | `05-implement/` | change-log ⊆ plan |
| 5 | [run-and-triage.md](run-and-triage.md) | `06-run/`, `07-fix/` | Green or `diagnosis.md` after 2 fixes |
| 6 | Review via [../skills/code-review/SKILL.md](../skills/code-review/SKILL.md) | `08-review/` | Blocking = 0 |
| 7 | MR via [../skills/generate-mr-description/SKILL.md](../skills/generate-mr-description/SKILL.md) | `09-mr/` | Description ready |

Human feedback lands in `03-feedback/answers.md` after Plan questions.

## How to run (chat)

1. Copy [ticket-template.md](ticket-template.md) or use [samples/AATP-1-successful-login.md](samples/AATP-1-successful-login.md).
2. Ask the assistant to **run the orchestrator** with that ticket (optional: attach page HTML later).
3. Orchestrator creates `.ai-workflow/<run-id>/`, follows [orchestrator.md](orchestrator.md), stops at human gates.
4. Answer questions in chat; orchestrator writes `03-feedback/answers.md` and continues only after explicit plan approval.
5. Evaluate any phase by reading its artifact; skim `discussion-summary.md` for the whole run.

## Design rules (v1)

- **One scenario / one journey** per run.
- **Reuse first** — new files only when AC/feedback require them.
- **Fix loops ≤ 2**, then stop with diagnosis.
- **No JIRA API** — paste ticket or use samples; swap Intake later.
- **Do not commit** `.ai-workflow/` run output.

## Model hint (optional)

Prefer cheaper models for Intake / Explore / Run / MR; stronger for Plan / Validate / Review. Orchestrator only passes the current artifact bundle — not the whole repo — into each phase.
