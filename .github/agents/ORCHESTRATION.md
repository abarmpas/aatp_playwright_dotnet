# AI workflow orchestration

> **Template / demonstration.** Chat-driven checklist that turns one manual test case (JIRA-style ticket) into suite changes, a test run, a review, and an MR description. Same artifacts work later under CI.

## Roles

| Piece | Location | Purpose |
|-------|----------|---------|
| **Orchestrator** | [orchestrator.agent.md](orchestrator.agent.md) | Launches each phase as a subagent, enforces gates, writes `discussion-summary.md` |
| **Phase agents** | `*.agent.md` in this folder | One job each; `model_tier` + evaluable artifacts |
| **Scripts** | [scripts/](scripts/) | `workflow.sh` phase tracker, `check-artifacts.sh`, `run-tests.sh` |
| **Model map** | [models.md](models.md) | Active profile + tier→Cursor slug (boost here later) |
| **Skills** | [../skills/](../skills/) | How-to playbooks (review, MR, explain, …) reused by agents |
| **Standards** | [../coding.instructions.md](../coding.instructions.md), [../automation.instructions.md](../automation.instructions.md), [../gherkin.instructions.md](../gherkin.instructions.md) | Source of truth for Plan / Validate / Implement / Review |
| **Ticket input** | [ticket-template.md](ticket-template.md) + [samples/](samples/) | Stand-in for JIRA until a real API exists |
| **Run output** | `.ai-workflow/<run-id>/` (**gitignored**) | `state.json` + artifacts per phase |

## Pipeline

```text
Ticket (paste / sample)
  → Intake → Explore → Plan + questions
  → Human feedback + approve plan          ← gate
  → Validate plan                          ← gate
  → Implement → Run → Fix (≤2) → Run
  → Review → MR description
```

| # | Agent | Tier | Artifact folder | Gate |
|---|--------|------|-----------------|------|
| 0 | [intake.agent.md](intake.agent.md) | cheap | `00-intake/` | One scenario; secrets redacted |
| 1 | [explore.agent.md](explore.agent.md) | cheap | `01-explore/` | Reuse map complete |
| 2 | [plan.agent.md](plan.agent.md) | strong | `02-plan/` | Human answers + **approve** |
| 3 | [validate-plan.agent.md](validate-plan.agent.md) | strong | `04-validate-plan/` | Pass (else back to Plan) |
| 4 | [implement.agent.md](implement.agent.md) | mid | `05-implement/` | change-log ⊆ plan |
| 5 | [run-and-triage.agent.md](run-and-triage.agent.md) | cheap | `06-run/`, `07-fix/` | Green or `diagnosis.md` after 2 fixes |
| 6 | [../skills/code-review/SKILL.md](../skills/code-review/SKILL.md) | strong | `08-review/` | Blocking = 0 |
| 7 | [../skills/generate-mr-description/SKILL.md](../skills/generate-mr-description/SKILL.md) | cheap | `09-mr/` | Description ready |

Human feedback lands in `03-feedback/answers.md` after Plan questions.

## How to run (chat)

1. Copy [ticket-template.md](ticket-template.md) or use [samples/AATP-2-wrong-credentials.md](samples/AATP-2-wrong-credentials.md).
2. Ask the assistant to **run the orchestrator** with that ticket (optional: attach page HTML later).
3. Orchestrator runs `scripts/workflow.sh init …`, then `next` → launch phase subagent → `check` → `advance`, stopping at human gates.
4. Answer questions in chat; orchestrator writes `03-feedback/answers.md` and runs `approve-plan` / `advance` only after explicit plan approval.
5. Evaluate any phase by reading its artifact or `state.json`; skim `discussion-summary.md` for the whole run.

Details: [scripts/README.md](scripts/README.md).

## Design rules (v1)

- **One scenario / one journey** per run.
- **Reuse first** — new files only when AC/feedback require them.
- **Fix loops ≤ 2**, then stop with diagnosis (`workflow.sh bump-fix` enforces the cap).
- **Phase truth** lives in `.ai-workflow/<run-id>/state.json` via [scripts/workflow.sh](scripts/workflow.sh) — not chat memory.
- **No JIRA API** — paste ticket or use samples; swap Intake later.
- **Do not commit** `.ai-workflow/` run output.

## Multi-model

- Each agent/skill declares portable `model_tier` (`cheap` | `mid` | `strong`).
- [models.md](models.md) maps tiers → Cursor slugs under an **active profile** (`economy` | `balanced` | `quality`).
- Orchestrator launches **one Task/subagent per phase** with the resolved slug (true multi-model).
- To save cost or boost quality later: change only `models.md` (profile or slug table) — no need to edit every agent.
- Default profile is **balanced** (cheap ≠ mid; room to set `strong` → `claude-opus-5-thinking-high` when integrating).
