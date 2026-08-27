# AI workflow runs

Working runs are written under `.ai-workflow/<run-id>/` and are **gitignored** by default.

## Committed example

[`2026-08-27_wrong-credentials/`](2026-08-27_wrong-credentials/) is the first end-to-end orchestrator demo for ticket **AATP-2** (wrong-credentials login). Keep it as a reference for phase artifacts, `state.json`, and `discussion-summary.md`.

New local runs (other `run-id`s, `CURRENT`) stay untracked. To commit another example later, add a matching `!.ai-workflow/<run-id>/` exception in `.gitignore`.
