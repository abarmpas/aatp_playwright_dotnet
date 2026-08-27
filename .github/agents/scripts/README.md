# Workflow scripts

Mechanical guards for the AI automation pipeline. Agents do judgment; these scripts track **phase**, enforce **gates**, and reduce drift.

| Script | Purpose |
|--------|---------|
| [workflow.sh](workflow.sh) | `init` / `status` / `next` / `check` / `advance` / `approve-plan` / `bump-fix` / `stop` |
| [check-artifacts.sh](check-artifacts.sh) | Required files present for a phase |
| [run-tests.sh](run-tests.sh) | `dotnet test` → `06-run/run-NN.log.md` |

## Quick start

From repo root:

```bash
chmod +x .github/agents/scripts/*.sh

.github/agents/scripts/workflow.sh init 2026-08-27_successful-login \
  --ticket .github/agents/samples/AATP-1-successful-login.md \
  --ticket-key AATP-1

.github/agents/scripts/workflow.sh status
.github/agents/scripts/workflow.sh next
```

After a phase agent finishes writing artifacts:

```bash
.github/agents/scripts/workflow.sh check
.github/agents/scripts/workflow.sh advance
```

Human plan gate:

```bash
# after Plan artifacts exist and advance moved you to awaiting_approval:
.github/agents/scripts/workflow.sh approve-plan --answers path/to/answers.md
.github/agents/scripts/workflow.sh advance   # → validate-plan
```

Tests (run-and-triage):

```bash
.github/agents/scripts/run-tests.sh --filter "FullyQualifiedName~Login"
.github/agents/scripts/workflow.sh bump-fix   # before each fix attempt after a FAIL
```

## State

Each run has `.ai-workflow/<run-id>/state.json` (gitignored):

- `phase` — what to launch **now**
- `status` — `in_progress` | `needs_human` | `stopped` | `done`
- `plan_approved`, `validate_verdict`, `fix_count` (max 2)
- `profile`, `last_models`

`.ai-workflow/CURRENT` points at the active `run-id`.

Orchestrator must call `status`/`next` before launching a subagent and `advance` only after `check` passes.
