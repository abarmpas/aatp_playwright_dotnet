---
name: explore
description: >-
  Searches the test suite for reusable Features, Steps, Pages, and Extensions
  before planning automation. Use when the orchestrator reaches Explore or the
  user asks what exists to reuse for a ticket/brief.
---

# Explore agent

Map what already exists vs what must be created. Prefer reuse and extension.

## Input

- `00-intake/brief.md`
- Optional `00-intake/page.html`
- Repo: `AatpTemplateTestSuite/` (Features, Steps, Pages, Infra/Extensions, Hooks)

## Output

Write `01-explore/reuse-map.md`:

```markdown
# Reuse map

## Search notes
- Paths inspected:
- Keywords:

## Existing matches

| Layer | Path | Symbol / scenario | Verdict | Reason |
|-------|------|-------------------|---------|--------|
| Feature | | | reuse / extend / none | |
| Steps | | | reuse / extend / none | |
| Page | | | reuse / extend / none | |
| Extension | | | reuse / extend / none | |
| Hooks/Config | | | reuse / extend / none | |

## Gaps (create only if needed)
| Layer | Proposed name | Why not covered by reuse |
|-------|---------------|--------------------------|
| | | |

## Locator hints (from HTML or known data-test-ids)
- _(evidence only — not for Gherkin)_

## Risks
-
```

## Rules

- Cite real paths and type/method names; do not invent files that are not in the repo.
- **Verdict `create`** requires a gap reason tied to the brief.
- Read method signatures when recommending extend vs reuse-as-is.
- Keep the map short enough for Plan to consume in one pass.
