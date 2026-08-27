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

Coordinate the pipeline in [ORCHESTRATION.md](ORCHESTRATION.md). You are the **router**, not the implementer: for each phase, launch a **separate subagent (Task)** with the model from [models.md](models.md). Do not run Plan/Implement/Review logic yourself unless subagents are unavailable (see fallback).

## Multi-model launch (required in Cursor)

1. Read [models.md](models.md): note **Active profile** and the tier→slug table.
2. Read the phase file’s `model_tier` (or skill frontmatter).
3. Resolve `model` = slug for that tier under the active profile.
4. Start a **Task** subagent:
   - `model`: resolved slug (never invent slugs; if unknown, use `inherit` and note it in the summary)
   - `subagent_type`: `generalPurpose` (use `explore` only for the Explore phase if helpful)
   - `description`: short label, e.g. `intake AATP-1`
   - `prompt`: see template below
   - Wait for completion before the next phase (no parallel phases)
5. Verify the phase artifact exists; append `discussion-summary.md` including **Model:** `<slug>` (`<tier>` / `<profile>`).

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
- Do not start other workflow phases
- When done, list paths you wrote and a one-line status
```

## Setup

1. Require a ticket: pasted text, [ticket-template.md](ticket-template.md) filled, or [samples/AATP-1-successful-login.md](samples/AATP-1-successful-login.md).
2. Create run dir: `.ai-workflow/<run-id>/` where `run-id` is `YYYY-MM-DD_<slug>` from the ticket summary (e.g. `2026-08-27_successful-login`).
3. Create `discussion-summary.md` (append-only). Copy the ticket into `00-intake/source-ac.md`.
4. If the user attaches page HTML, save as `00-intake/page.html` (gitignored with the run).
5. Record active model profile at the top of `discussion-summary.md`.

## Phase loop

| When | Launch agent | Write under | After phase |
|------|--------------|-------------|-------------|
| Start | [intake.agent.md](intake.agent.md) | `00-intake/` | Continue if one scenario |
| Next | [explore.agent.md](explore.agent.md) | `01-explore/` | Continue |
| Next | [plan.agent.md](plan.agent.md) | `02-plan/` | **STOP — surface questions; wait for answers + explicit plan approval** |
| After approval | Write `03-feedback/answers.md` yourself (orchestrator) | | Continue |
| Next | [validate-plan.agent.md](validate-plan.agent.md) | `04-validate-plan/` | Fail → re-launch Plan; Pass → continue |
| Next | [implement.agent.md](implement.agent.md) | `05-implement/` | Continue |
| Next | [run-and-triage.agent.md](run-and-triage.agent.md) | `06-run/`, `07-fix/` | Green → continue; after 2 failed fixes → **STOP** |
| Next | [../skills/code-review/SKILL.md](../skills/code-review/SKILL.md) | `08-review/review.md` | Blocking → **STOP** or user OK to fix |
| Next | [../skills/generate-mr-description/SKILL.md](../skills/generate-mr-description/SKILL.md) | `09-mr/mr-description.md` | Done |

Human feedback (`03-feedback/`) is written by the orchestrator from chat replies — no subagent required.

## discussion-summary.md format

```markdown
# Run <run-id>
- Profile: balanced|economy|quality (from models.md)

### <ISO timestamp> — <phase name>
- Status: ok | needs_human | failed | stopped
- Model: <cursor-slug> (tier=<model_tier>)
- Artifact: <relative path>
- Notes: <one line>
```

## Fallback (no Task / subagent API)

Run the phase inline, but before Plan, Validate, and Review **tell the user** the recommended slug from [models.md](models.md) so they can switch the chat model. Still write the same artifacts and summary (`Model: inline/<slug or unknown>`).

## Rules

- **One scenario** per run; if the ticket has more, ask the user to pick one before Intake completes.
- Never skip the plan-approval gate.
- Never start a third Fix loop; ensure `07-fix/diagnosis.md` exists, then stop.
- Do not commit `.ai-workflow/` or create the remote MR unless the user asks.
- Prefer reuse; allow new files only when plan + feedback say so.
- Keep subagent prompts small: only the agent file + needed artifact paths — not the whole chat history.
- To change cost/quality, edit [models.md](models.md) only (active profile or slug table) — do not fork every agent file.
