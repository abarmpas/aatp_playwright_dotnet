---
name: generate-mr-description
description: >-
  Generate a Merge Request description from the current branch's git diff against
  the develop/main branch, using the project's MR template. Use when the user asks
  "to create an MR description", "write MR description", "prepare MR" or similar.
---

# Generate MR Description

Produce a Merge Request description filled from the current branch’s changes against the integration branch, using the project MR template. Output the description only; do not create the MR/PR or push unless the user also asks.

## Template

Use [mr-template.md](mr-template.md) as the default structure.

Teams may later replace that file with (or point this skill at) their real template, for example:

- GitLab: `.gitlab/merge_request_templates/Default.md`
- GitHub: `.github/pull_request_template.md`

If a repo-root or `.gitlab` / `.github` MR/PR template exists and differs from `mr-template.md`, prefer the repo’s official template and still follow the workflow below.

## When applying

1. Confirm git repo and current branch (`git status`, `git branch --show-current`).
2. Resolve the base branch in this order: `develop` → `main` → `master` (use the first that exists locally or on `origin`). If none exist, ask the user once.
3. Fetch is optional; prefer local refs. If the base ref is missing, say so and ask whether to fetch or which base to use.
4. Collect:
   - `git log --oneline <base>..HEAD`
   - `git diff --stat <base>...HEAD`
   - `git diff <base>...HEAD` (read enough to summarize accurately; do not paste the raw diff into the MR)
5. Fill every section of the template from that evidence. Leave a section as `N/A` (with a brief reason) only when it truly does not apply — do not invent tickets, test results, or screenshots.
6. Return the finished Markdown description in a single fenced block so it is easy to copy. Optionally suggest a short MR title on one line above the block.

## Writing rules

- Base the Summary and changes on the actual diff and commits, not on branch name alone.
- Prefer why and user/team impact over a file-by-file changelog; mention key paths when they help reviewers.
- Call out risk: flaky tests, config/secrets, CI, layering (Feature / Steps / Pages / Extensions).
- Keep Test plan concrete and checkbox-friendly (commands or scenarios a reviewer can run).
- Do not claim tests were run unless the conversation or CI output shows that.
- Do not include secrets, credentials, or full `.env` values from the diff.
- Match the template’s headings and order exactly so teams can paste into GitLab/GitHub without reshaping.

## Response shape

```markdown
**Suggested title:** <concise title>

## Merge Request description

<filled template — all sections, ready to paste>
```
