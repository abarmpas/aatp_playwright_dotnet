---
name: generate-mr-description
model_tier: cheap
description: >-
  Generate a Pull Request (or Merge Request) description from the current
  branch's git diff against the develop/main branch, using the project's GitHub
  PR template. Use when the user asks to "create a PR description", "write PR
  description", "prepare PR", "create an MR description", "write MR description",
  "prepare MR", or similar.
---

# Generate PR / MR Description

Produce a Pull Request description filled from the current branch’s changes against the integration branch, using the project template. Output the description only; do not create the PR/MR or push unless the user also asks.

## Template (GitHub first)

**Primary template:** [../../pull_request_template.md](../../pull_request_template.md)

That file is the official GitHub PR body template (auto-loaded when opening a PR in the GitHub UI). The generate-* skill must fill **the same headings and order** so the result pastes cleanly into GitHub (or into `gh pr create --body-file`).

Fallback only if the GitHub file is missing:

- Skill-local [mr-template.md](mr-template.md) (points teams at the GitHub path)
- GitLab: `.gitlab/merge_request_templates/Default.md` when present

## When applying

1. Confirm git repo and current branch (`git status`, `git branch --show-current`).
2. Resolve the base branch in this order: `develop` → `main` → `master` (use the first that exists locally or on `origin`). If none exist, ask the user once.
3. Fetch is optional; prefer local refs. If the base ref is missing, say so and ask whether to fetch or which base to use.
4. Collect:
   - `git log --oneline <base>..HEAD`
   - `git diff --stat <base>...HEAD`
   - `git diff <base>...HEAD` (read enough to summarize accurately; do not paste the raw diff into the PR)
5. Read [../../pull_request_template.md](../../pull_request_template.md) and fill every section from that evidence. Leave a section as `N/A` (with a brief reason) only when it truly does not apply — do not invent tickets, test results, or screenshots.
6. Return the finished Markdown description in a single fenced block so it is easy to copy into GitHub. Optionally suggest a short PR title on one line above the block.
7. If the user also asks to open the PR and `gh` is available, you may run `gh pr create` with that body; otherwise stop after writing the description (and optionally `.ai-workflow/<run-id>/09-mr/mr-description.md` when in the AI workflow).

## Writing rules

- Base the Summary and changes on the actual diff and commits, not on branch name alone.
- Prefer why and user/team impact over a file-by-file changelog; mention key paths when they help reviewers.
- Call out risk: flaky tests, config/secrets, CI, layering (Feature / Steps / Pages / Extensions).
- Keep Test plan concrete and checkbox-friendly (commands or scenarios a reviewer can run).
- Do not claim tests were run unless the conversation or CI output shows that.
- Do not include secrets, credentials, or full `.env` values from the diff.
- Match the template’s headings and order exactly so the body matches GitHub’s PR form.

## Response shape

```markdown
**Suggested title:** <concise title>

## Pull Request description

<filled template — all sections, ready to paste into GitHub>
```
