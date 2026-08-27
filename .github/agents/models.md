# Model routing (multi-model)

Portable **tiers** live on each `*.agent.md` (`model_tier`).  
This file maps tiers → **Cursor model slugs** for true multi-model runs via subagents.

Integrators: change the **active profile** or edit the slug table. Do not hard-code slugs inside phase agents.

## Active profile

```text
balanced
```

Allowed: `economy` | `balanced` | `quality`

| Profile | Intent |
|---------|--------|
| `economy` | Cheapest demo; few distinct models |
| `balanced` | Default for this small template — real multi-model, moderate cost |
| `quality` | Boost Plan / Validate / Review (and optionally Implement) |

## Tier → Cursor slug

Slugs must match what your Cursor build allows for Task/subagents. Adjust names if your org’s catalog differs.

### economy

| Tier | Cursor `model` slug |
|------|---------------------|
| cheap | `composer-2.5-fast` |
| mid | `composer-2.5-fast` |
| strong | `gpt-5.6-sol-medium` |

### balanced (default)

| Tier | Cursor `model` slug |
|------|---------------------|
| cheap | `composer-2.5-fast` |
| mid | `gpt-5.6-sol-medium` |
| strong | `gpt-5.6-sol-medium` |

> **Boost later:** set `strong` → `claude-opus-5-thinking-high` (or your org’s best reasoning model) without changing agent files.

### quality

| Tier | Cursor `model` slug |
|------|---------------------|
| cheap | `gpt-5.6-sol-medium` |
| mid | `gpt-5.6-sol-medium` |
| strong | `claude-opus-5-thinking-high` |

## Agent → tier (source of truth on each file)

| Agent / skill | `model_tier` |
|---------------|--------------|
| orchestrator | mid |
| intake | cheap |
| explore | cheap |
| plan | strong |
| validate-plan | strong |
| implement | mid |
| run-and-triage | cheap |
| code-review (skill) | strong |
| generate-mr-description (skill) | cheap |

## How Cursor enforces this

1. Parent chat loads [orchestrator.agent.md](orchestrator.agent.md) (orchestrator tier / mid).
2. For **each phase**, Orchestrator starts a **Task/subagent** with:
   - `model`: slug from the **active profile** table for that agent’s `model_tier`
   - `prompt`: path to the `*.agent.md` (or skill), run-id, input artifact paths, required output paths
   - sequential (wait for one phase before the next), except where ORCHESTRATION says stop for humans
3. Record the resolved slug in `discussion-summary.md` (`Model: …`).

If Task/subagents are unavailable, fall back to single-chat mode and **announce** the recommended model at each gate (Plan / Validate / Review) so a human can switch manually.

## Non-Cursor IDEs

Honor `model_tier` with whatever multi-agent or multi-call API you use. Keep this file’s **tier column** stable; replace the slug tables with your provider’s model IDs.
