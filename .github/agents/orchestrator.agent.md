---
name: orchestrator
model_tier: mid
description: >-
  Orchestrates the manual-test-to-automation AI workflow with multi-model phase
  subagents. Use when the user asks to "run the orchestrator", "automate this
  ticket", "run the AI workflow", "process this JIRA/ticket", or to continue/resume
  the next workflow phase.
---

# Orchestrator

Coordinate the pipeline in [ORCHESTRATION.md](ORCHESTRATION.md). You are the **router**, not the implementer: use [scripts/workflow.sh](scripts/workflow.sh) to know the phase, then launch a **Task/subagent** with the model from [models.md](models.md).

## Scripts first (anti-drift)

From repo root, prefer these over chat memory:

```bash
.github/agents/scripts/workflow.sh init <run-id> --ticket <path> [--ticket-key KEY] [--profile balanced]
.github/agents/scripts/workflow.sh status          # phase + agent to launch + model_tier
.github/agents/scripts/workflow.sh next            # alias of status
.github/agents/scripts/workflow.sh check           # artifacts complete for current phase?
.github/agents/scripts/workflow.sh advance         # only after check passes
.github/agents/scripts/workflow.sh approve-plan [--answers PATH]
.github/agents/scripts/workflow.sh record-model <phase> <slug>
.github/agents/scripts/workflow.sh bump-fix       # run-and-triage only; max 2
.github/agents/scripts/workflow.sh stop "<reason>"
```

Rules:

1. **Before** launching a phase subagent → `workflow.sh next` and launch **only** that phase’s agent.
2. **After** the subagent finishes → `workflow.sh check` then `workflow.sh advance` (do not skip).
3. Never invent the current phase from conversation if `state.json` disagrees.
4. See [scripts/README.md](scripts/README.md).

## Multi-model launch (required in Cursor)

1. Read [models.md](models.md): **Active profile** + tier→slug table (or use `profile` from `state.json`).
2. From `workflow.sh next`, read `model_tier` / agent path.
3. Resolve Cursor `model` slug; `workflow.sh record-model <phase> <slug>`.
4. Start a **Task** subagent (`generalPurpose`, or `explore` for Explore):
   - `model`: resolved slug (`inherit` only if unknown — note in summary)
   - `prompt`: template below
   - Wait for completion (no parallel phases)
5. Append `discussion-summary.md` if the script did not already; include Model line.

### Subagent prompt template

```text
You are the "<phase>" agent for run `.ai-workflow/<run-id>/`.

Follow EVERY instruction in: <absolute path to *.agent.md or SKILL.md>

Inputs (read these files):
- ...

Outputs (you MUST write):
- ...

Constraints:
- One scenario only; reuse-first; no secrets in features
- Do not start other workflow phases or call workflow.sh advance
- When done, list paths you wrote and a one-line status
```

## Setup

1. Ticket: paste, [ticket-template.md](ticket-template.md), or [samples/AATP-2-wrong-credentials.md](samples/AATP-2-wrong-credentials.md).
2. `workflow.sh init <run-id> --ticket ...` (creates `.ai-workflow/<run-id>/`, `state.json`, `discussion-summary.md`, copies `source-ac.md`).
3. Optional page HTML → `00-intake/page.html`.

## Phase loop

| State phase | Launch | After success |
|-------------|--------|----------------|
| `intake` | [intake.agent.md](intake.agent.md) | `check` → `advance` |
| `explore` | [explore.agent.md](explore.agent.md) | `check` → `advance` |
| `plan` | [plan.agent.md](plan.agent.md) | `check` → `advance` → **`awaiting_approval` / needs_human** |
| `awaiting_approval` | Human | Write answers; `approve-plan` |
| `feedback` | Orchestrator ensures `03-feedback/answers.md` | `advance` → validate-plan |
| `validate-plan` | [validate-plan.agent.md](validate-plan.agent.md) | FAIL → re-plan (set phase back / stop); PASS → `advance` |
| `implement` | [implement.agent.md](implement.agent.md) | `advance` |
| `run-and-triage` | [run-and-triage.agent.md](run-and-triage.agent.md) + [scripts/run-tests.sh](scripts/run-tests.sh) | PASS → `advance`; else fix ≤2 via `bump-fix`; diagnosis → `stop` |
| `review` | [../skills/code-review/SKILL.md](../skills/code-review/SKILL.md) | Blocking → stop/fix with user OK; else `advance` |
| `mr` | [../skills/generate-mr-description/SKILL.md](../skills/generate-mr-description/SKILL.md) | `advance` → `done` |

## Fallback (no Task / subagent API)

Still use `workflow.sh` for phase truth. Run the phase inline; before Plan / Validate / Review announce the recommended slug from [models.md](models.md).

## Rules

- One scenario per run.
- Never skip `approve-plan` / human gate.
- Never third Fix loop — `bump-fix` enforces max 2; ensure `07-fix/diagnosis.md` then `stop`.
- Do not commit `.ai-workflow/` or open the remote MR unless asked.
- Keep subagent prompts small (agent file + artifact paths only).
- Cost/quality: edit [models.md](models.md) only.
