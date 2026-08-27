# Answers

| # | Answer |
|---|--------|
| 1 | Commit non-sensitive placeholders in `appsettings.json`: `admin@admin.com` / `admin` under `InvalidTestUser`. Still overridable via env/user-secrets later. |
| 2 | Assert exact failure message text: `Invalid credentials` (in addition to visibility). |

## Plan approval

**Approved** — proceed with validate → implement → run → review → MR description.

## Orchestrator resolution (review Blocking)

Review flagged that screenshots/traces could still show filled credential fields. **Waived for this template run** because human answer #1 explicitly commits non-sensitive placeholders (`admin@admin.com` / `admin`) for InvalidTestUser. Serilog email logging was already removed from `SignInAsync`. Full trace/screenshot masking remains a future hardening item, not Blocking for AATP-2.
