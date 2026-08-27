---
name: explain-code
description: >-
  Explains code with visual diagram and analogies. Use when explaining how code
  works, teaching about a codebase, or when the user asks "how does this work?",
  "walk me through X", "i dont understand how Y works", "explain X", "what does X
  do", "how is X implemented", or any phrasing that asks for an explanation of how
  a feature, mechanism, class or flow works.
---

# Explain Code

Explain how code works using a clear narrative, one strong analogy, and a visual diagram. Prefer teaching over dumping implementation detail.

## When applying

1. Identify the target: file, class, method, feature, or flow the user asked about.
2. Read only the code needed to explain accurately; follow key call sites one level deep when the flow spans files.
3. Match depth to the question: high-level for "what does this do?", step-by-step for "walk me through / how is this implemented?".
4. Do not refactor or edit code unless the user also asks for changes.

## Response structure

Use this order unless the user asks for a different format:

### 1. Plain-language summary
One or two sentences: what it does and why it exists.

### 2. Analogy
One concrete analogy (everyday or domain-appropriate). Keep it short and map the important parts of the analogy back to the code concepts.

### 3. Visual diagram
Include at least one Mermaid diagram (or ASCII if Mermaid is a poor fit):

| Situation | Diagram type |
|-----------|----------------|
| Request/call flow, pipeline, scenario lifecycle | `sequenceDiagram` or `flowchart TD` |
| Layering / ownership (e.g. Feature → Steps → Pages) | `flowchart TD` |
| State / branching logic | `stateDiagram-v2` or `flowchart TD` |
| Types and relationships | `classDiagram` |

Keep diagrams focused: prefer 5–12 nodes. Label edges with the action or data that moves.

### 4. Walkthrough
Explain the flow in order. Cite the real type/method names. Quote only short snippets when a specific line is essential; otherwise point to symbols and files.

### 5. Key takeaways
Bullet the 2–4 ideas the reader should remember (invariants, layer boundaries, extension points, easy mistakes).

## Style rules

- Teach; do not narrate every line.
- Prefer project vocabulary when explaining this repo (e.g. Feature → Steps → Page Objects → Extensions → Playwright).
- Call out non-obvious behavior (side effects, async lifetime, shared state, configuration).
- If something is unclear from the code, say what is ambiguous instead of inventing behavior.
- End with a brief offer to go deeper on one part (one specific follow-up), not a long menu of options.

## Example shape

```markdown
## Summary
...

## Analogy
...

## Diagram
```mermaid
sequenceDiagram
  ...
```

## Walkthrough
1. ...
2. ...

## Key takeaways
- ...
```
