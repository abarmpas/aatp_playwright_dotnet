#!/usr/bin/env bash
# check-artifacts.sh — verify required outputs exist for a workflow phase.
# Usage: check-artifacts.sh <run-dir> <phase>
set -euo pipefail

RUN_DIR="${1:-}"
PHASE="${2:-}"

if [[ -z "$RUN_DIR" || -z "$PHASE" ]]; then
  echo "Usage: $0 <run-dir> <phase>" >&2
  exit 2
fi

if [[ ! -d "$RUN_DIR" ]]; then
  echo "ERROR: run dir not found: $RUN_DIR" >&2
  exit 1
fi

fail() { echo "FAIL: $*" >&2; exit 1; }
ok() { echo "OK: $phase_label — $*"; }

require_file() {
  local f="$1"
  [[ -f "$RUN_DIR/$f" ]] || fail "missing required file: $f"
}

phase_label="$PHASE"

case "$PHASE" in
  intake)
    require_file "00-intake/brief.md"
    require_file "00-intake/source-ac.md"
    ok "brief.md + source-ac.md"
    ;;
  explore)
    require_file "01-explore/reuse-map.md"
    ok "reuse-map.md"
    ;;
  plan)
    require_file "02-plan/plan.md"
    require_file "02-plan/questions.md"
    ok "plan.md + questions.md"
    ;;
  awaiting_approval)
    require_file "02-plan/plan.md"
    require_file "02-plan/questions.md"
    ok "plan ready for human approval"
    ;;
  feedback)
    require_file "03-feedback/answers.md"
    ok "answers.md"
    ;;
  validate-plan)
    require_file "04-validate-plan/validation.md"
    # Portable check (macOS BSD grep has weak \\b support)
    verdict_block="$(grep -i -A5 'Verdict' "$RUN_DIR/04-validate-plan/validation.md" | head -n 6 || true)"
    if echo "$verdict_block" | grep -Eqi '(^|[[:space:]])FAIL($|[[:space:]])'; then
      fail "validation verdict is FAIL"
    fi
    if ! echo "$verdict_block" | grep -Eqi '(^|[[:space:]])PASS($|[[:space:]])'; then
      fail "validation Verdict is not PASS (re-plan required)"
    fi
    ok "validation.md PASS"
    ;;
  implement)
    require_file "05-implement/change-log.md"
    ok "change-log.md"
    ;;
  run-and-triage)
    shopt -s nullglob
    logs=("$RUN_DIR"/06-run/run-*.log.md)
    shopt -u nullglob
    if [[ ${#logs[@]} -eq 0 ]]; then
      fail "no 06-run/run-*.log.md files"
    fi
    passed=0
    for log in "${logs[@]}"; do
      result_block="$(grep -i -A3 '^## Result' "$log" | head -n 4 || true)"
      if echo "$result_block" | grep -Eqi '(^|[[:space:]])PASS($|[[:space:]])'; then
        passed=1
        break
      fi
      if grep -Eqi '^Result:[[:space:]]*PASS' "$log"; then
        passed=1
        break
      fi
    done
    if [[ "$passed" -ne 1 ]]; then
      if [[ -f "$RUN_DIR/07-fix/diagnosis.md" ]]; then
        fail "tests not green; diagnosis.md present — workflow should STOP (not advance)"
      fi
      fail "no run log with PASS result"
    fi
    ok "at least one PASS run log"
    ;;
  review)
    require_file "08-review/review.md"
    ok "review.md"
    ;;
  mr)
    require_file "09-mr/mr-description.md"
    ok "mr-description.md"
    ;;
  done|stopped)
    ok "terminal phase ($PHASE) — nothing to check"
    ;;
  *)
    fail "unknown phase: $PHASE"
    ;;
esac
