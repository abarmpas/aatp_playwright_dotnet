#!/usr/bin/env bash
# workflow.sh — phase navigation and drift control for the AI automation workflow.
#
# Usage:
#   workflow.sh init <run-id> [--ticket PATH] [--profile NAME] [--ticket-key KEY]
#   workflow.sh use <run-id>
#   workflow.sh status [run-id]
#   workflow.sh next [run-id]
#   workflow.sh check [run-id]
#   workflow.sh advance [run-id]
#   workflow.sh approve-plan [run-id] [--answers PATH]
#   workflow.sh record-model <phase> <slug> [run-id]
#   workflow.sh bump-fix [run-id]
#   workflow.sh stop <reason> [run-id]
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/../../.." && pwd)"
AI_ROOT="$REPO_ROOT/.ai-workflow"
CURRENT_FILE="$AI_ROOT/CURRENT"
CHECK="$SCRIPT_DIR/check-artifacts.sh"

PHASES=(
  intake
  explore
  plan
  awaiting_approval
  feedback
  validate-plan
  implement
  run-and-triage
  review
  mr
  done
)

AGENT_FOR_PHASE() {
  case "$1" in
    intake) echo "intake.agent.md|cheap|00-intake/" ;;
    explore) echo "explore.agent.md|cheap|01-explore/" ;;
    plan) echo "plan.agent.md|strong|02-plan/" ;;
    awaiting_approval) echo "(human)|-|02-plan/ + approve-plan" ;;
    feedback) echo "(orchestrator writes answers)|-|03-feedback/" ;;
    validate-plan) echo "validate-plan.agent.md|strong|04-validate-plan/" ;;
    implement) echo "implement.agent.md|mid|05-implement/" ;;
    run-and-triage) echo "run-and-triage.agent.md|cheap|06-run/ 07-fix/" ;;
    review) echo "../skills/code-review/SKILL.md|strong|08-review/" ;;
    mr) echo "../skills/generate-mr-description/SKILL.md|cheap|09-mr/" ;;
    done|stopped) echo "(none)|-|-" ;;
    *) echo "unknown|-|-" ;;
  esac
}

usage() {
  sed -n '2,16p' "$0" | sed 's/^# \{0,1\}//'
  exit 2
}

need_python() {
  command -v python3 >/dev/null 2>&1 || { echo "ERROR: python3 required for state.json" >&2; exit 1; }
}

run_dir_for() {
  local id="$1"
  echo "$AI_ROOT/$id"
}

resolve_run_id() {
  local id="${1:-}"
  if [[ -n "$id" ]]; then
    echo "$id"
    return
  fi
  [[ -f "$CURRENT_FILE" ]] || { echo "ERROR: no run-id arg and no $CURRENT_FILE — run: workflow.sh use <run-id>" >&2; exit 1; }
  tr -d '[:space:]' < "$CURRENT_FILE"
}

state_get() {
  need_python
  local dir="$1" key="$2"
  python3 - "$dir/state.json" "$key" <<'PY'
import json, sys
path, key = sys.argv[1], sys.argv[2]
with open(path) as f:
    data = json.load(f)
val = data
for part in key.split("."):
    val = val[part]
if isinstance(val, (dict, list)):
    print(json.dumps(val))
else:
    print(val)
PY
}

state_set() {
  need_python
  local dir="$1"
  shift
  python3 - "$dir/state.json" "$@" <<'PY'
import json, sys
path = sys.argv[1]
args = sys.argv[2:]
with open(path) as f:
    data = json.load(f)
# args are key=value pairs; value JSON-decoded when possible
i = 0
while i < len(args):
    item = args[i]
    if "=" not in item:
        raise SystemExit(f"bad kv: {item}")
    k, v = item.split("=", 1)
    try:
        parsed = json.loads(v)
    except Exception:
        parsed = v
    cur = data
    parts = k.split(".")
    for p in parts[:-1]:
        cur = cur.setdefault(p, {})
    cur[parts[-1]] = parsed
    i += 1
with open(path, "w") as f:
    json.dump(data, f, indent=2)
    f.write("\n")
PY
}

append_summary() {
  local dir="$1" line="$2"
  local summary="$dir/discussion-summary.md"
  {
    echo
    echo "### $(date -u +"%Y-%m-%dT%H:%M:%SZ") — workflow"
    echo "- Notes: $line"
  } >>"$summary"
}

cmd_init() {
  local run_id="${1:-}"
  shift || true
  [[ -n "$run_id" ]] || usage

  local ticket="" profile="balanced" ticket_key=""
  while [[ $# -gt 0 ]]; do
    case "$1" in
      --ticket) ticket="$2"; shift 2 ;;
      --profile) profile="$2"; shift 2 ;;
      --ticket-key) ticket_key="$2"; shift 2 ;;
      *) echo "Unknown option: $1" >&2; usage ;;
    esac
  done

  local dir
  dir="$(run_dir_for "$run_id")"
  mkdir -p "$AI_ROOT" \
    "$dir/00-intake" "$dir/01-explore" "$dir/02-plan" "$dir/03-feedback" \
    "$dir/04-validate-plan" "$dir/05-implement" "$dir/06-run" "$dir/07-fix" \
    "$dir/08-review" "$dir/09-mr"

  if [[ -n "$ticket" ]]; then
    [[ -f "$ticket" ]] || { echo "ERROR: ticket not found: $ticket" >&2; exit 1; }
    cp "$ticket" "$dir/00-intake/source-ac.md"
    if [[ -z "$ticket_key" ]]; then
      ticket_key="$(grep -E '^`?[A-Z]+-[0-9]+`?$' "$ticket" | head -n1 | tr -d '`' || true)"
    fi
  else
    [[ -f "$dir/00-intake/source-ac.md" ]] || echo "_Paste ticket here_" >"$dir/00-intake/source-ac.md"
  fi

  need_python
  python3 - "$dir/state.json" "$run_id" "$profile" "${ticket_key:-}" <<'PY'
import json, sys
path, run_id, profile, ticket_key = sys.argv[1:5]
state = {
  "run_id": run_id,
  "ticket_key": ticket_key or None,
  "profile": profile,
  "phase": "intake",
  "status": "in_progress",
  "plan_approved": False,
  "validate_verdict": None,
  "fix_count": 0,
  "last_models": {},
  "stop_reason": None,
}
with open(path, "w") as f:
    json.dump(state, f, indent=2)
    f.write("\n")
PY

  cat >"$dir/discussion-summary.md" <<EOF
# Run $run_id
- Profile: $profile
- Ticket: ${ticket_key:-_(unset)_}
- Created: $(date -u +"%Y-%m-%dT%H:%M:%SZ")
EOF

  echo "$run_id" >"$CURRENT_FILE"
  echo "Initialized $dir"
  echo "CURRENT=$run_id"
  cmd_status "$run_id"
}

cmd_use() {
  local run_id="${1:-}"
  [[ -n "$run_id" ]] || usage
  local dir
  dir="$(run_dir_for "$run_id")"
  [[ -d "$dir" ]] || { echo "ERROR: missing $dir — run init first" >&2; exit 1; }
  mkdir -p "$AI_ROOT"
  echo "$run_id" >"$CURRENT_FILE"
  echo "CURRENT=$run_id"
}

cmd_status() {
  local run_id
  run_id="$(resolve_run_id "${1:-}")"
  local dir
  dir="$(run_dir_for "$run_id")"
  [[ -f "$dir/state.json" ]] || { echo "ERROR: no state.json in $dir" >&2; exit 1; }

  need_python
  python3 - "$dir/state.json" <<'PY'
import json, sys
with open(sys.argv[1]) as f:
    s = json.load(f)
print(f"run_id:           {s.get('run_id')}")
print(f"ticket_key:       {s.get('ticket_key')}")
print(f"profile:          {s.get('profile')}")
print(f"phase:            {s.get('phase')}")
print(f"status:           {s.get('status')}")
print(f"plan_approved:    {s.get('plan_approved')}")
print(f"validate_verdict: {s.get('validate_verdict')}")
print(f"fix_count:       {s.get('fix_count')}")
print(f"stop_reason:      {s.get('stop_reason')}")
print(f"last_models:      {json.dumps(s.get('last_models') or {})}")
PY

  local phase
  phase="$(state_get "$dir" phase)"
  local meta agent tier arts
  meta="$(AGENT_FOR_PHASE "$phase")"
  IFS='|' read -r agent tier arts <<<"$meta"
  echo "launch_now:       $agent"
  echo "model_tier:       $tier"
  echo "artifact_dir:     $arts"
  echo "run_dir:          $dir"
}

cmd_next() {
  cmd_status "$@"
}

cmd_check() {
  local run_id
  run_id="$(resolve_run_id "${1:-}")"
  local dir phase
  dir="$(run_dir_for "$run_id")"
  phase="$(state_get "$dir" phase)"
  # For "in progress" phases, check whether THIS phase's outputs are complete
  case "$phase" in
    awaiting_approval) "$CHECK" "$dir" awaiting_approval ;;
    *) "$CHECK" "$dir" "$phase" ;;
  esac
}

phase_index() {
  local target="$1" i=0
  for p in "${PHASES[@]}"; do
    if [[ "$p" == "$target" ]]; then
      echo "$i"
      return
    fi
    i=$((i + 1))
  done
  echo "-1"
}

cmd_advance() {
  local run_id
  run_id="$(resolve_run_id "${1:-}")"
  local dir phase status
  dir="$(run_dir_for "$run_id")"
  phase="$(state_get "$dir" phase)"
  status="$(state_get "$dir" status)"

  if [[ "$status" == "stopped" || "$phase" == "stopped" ]]; then
    echo "ERROR: run is stopped — will not advance" >&2
    exit 1
  fi
  if [[ "$phase" == "done" ]]; then
    echo "Already done."
    exit 0
  fi
  if [[ "$phase" == "awaiting_approval" ]]; then
    echo "ERROR: phase is awaiting_approval — run: workflow.sh approve-plan" >&2
    exit 1
  fi
  if [[ "$status" == "needs_human" && "$phase" == "feedback" ]]; then
    :
  elif [[ "$status" == "needs_human" ]]; then
    echo "ERROR: status is needs_human — resolve human gate before advance" >&2
    exit 1
  fi

  # Completeness check for the phase we are leaving
  case "$phase" in
    feedback)
      "$CHECK" "$dir" feedback
      local approved
      approved="$(state_get "$dir" plan_approved)"
      [[ "$approved" == "True" || "$approved" == "true" ]] || {
        echo "ERROR: plan_approved is false — run approve-plan first" >&2
        exit 1
      }
      ;;
    plan)
      "$CHECK" "$dir" plan
      state_set "$dir" "phase=awaiting_approval" "status=needs_human"
      append_summary "$dir" "advance: plan complete → awaiting_approval (human gate)"
      cmd_status "$run_id"
      return
      ;;
    validate-plan)
      "$CHECK" "$dir" validate-plan
      state_set "$dir" "validate_verdict=PASS"
      ;;
    run-and-triage)
      if [[ -f "$dir/07-fix/diagnosis.md" ]]; then
        state_set "$dir" "phase=stopped" "status=stopped" "stop_reason=\"max fix loops / diagnosis\""
        append_summary "$dir" "advance blocked: diagnosis.md present → stopped"
        cmd_status "$run_id"
        exit 1
      fi
      "$CHECK" "$dir" run-and-triage
      ;;
    *)
      "$CHECK" "$dir" "$phase"
      ;;
  esac

  local idx next
  idx="$(phase_index "$phase")"
  [[ "$idx" -ge 0 ]] || { echo "ERROR: unknown phase $phase" >&2; exit 1; }
  next="${PHASES[$((idx + 1))]}"

  # Skip awaiting_approval when advancing from feedback path is N/A;
  # from plan we already returned. From feedback → validate-plan.
  if [[ "$phase" == "feedback" ]]; then
    next="validate-plan"
  fi

  if [[ "$next" == "awaiting_approval" ]]; then
    state_set "$dir" "phase=awaiting_approval" "status=needs_human"
  elif [[ "$next" == "done" ]]; then
    state_set "$dir" "phase=done" "status=done"
  else
    state_set "$dir" "phase=$next" "status=in_progress"
  fi

  append_summary "$dir" "advance: $phase → $(state_get "$dir" phase)"
  cmd_status "$run_id"
}

cmd_approve_plan() {
  local run_id="" answers=""
  while [[ $# -gt 0 ]]; do
    case "$1" in
      --answers) answers="$2"; shift 2 ;;
      *)
        if [[ -z "$run_id" && "$1" != --* ]]; then run_id="$1"; shift; else
          echo "Unknown option: $1" >&2; usage
        fi
        ;;
    esac
  done
  run_id="$(resolve_run_id "$run_id")"
  local dir
  dir="$(run_dir_for "$run_id")"

  "$CHECK" "$dir" plan

  if [[ -n "$answers" ]]; then
    [[ -f "$answers" ]] || { echo "ERROR: answers file not found: $answers" >&2; exit 1; }
    mkdir -p "$dir/03-feedback"
    cp "$answers" "$dir/03-feedback/answers.md"
  fi

  if [[ ! -f "$dir/03-feedback/answers.md" ]]; then
    mkdir -p "$dir/03-feedback"
    cat >"$dir/03-feedback/answers.md" <<'EOF'
# Answers

_(Orchestrator: replace this stub with the human's replies to 02-plan/questions.md.)_
EOF
    state_set "$dir" "plan_approved=true" "phase=feedback" "status=needs_human"
    append_summary "$dir" "approve-plan: approved; answers.md stub — fill then advance"
    echo "Plan approved. Fill 03-feedback/answers.md then: workflow.sh advance"
    cmd_status "$run_id"
    return
  fi

  state_set "$dir" "plan_approved=true" "phase=feedback" "status=in_progress"
  # If answers look like a stub, keep needs_human
  if grep -q 'replace this stub' "$dir/03-feedback/answers.md"; then
    state_set "$dir" "status=needs_human"
    echo "Plan approved but answers.md still stub — fill it, then advance."
  else
    # Ready to leave feedback on next advance
    state_set "$dir" "status=in_progress"
    echo "Plan approved with answers. Next: workflow.sh advance  # → validate-plan"
  fi
  append_summary "$dir" "approve-plan: plan_approved=true"
  cmd_status "$run_id"
}

cmd_record_model() {
  local phase="${1:-}" slug="${2:-}"
  [[ -n "$phase" && -n "$slug" ]] || usage
  local run_id
  run_id="$(resolve_run_id "${3:-}")"
  local dir
  dir="$(run_dir_for "$run_id")"
  need_python
  python3 - "$dir/state.json" "$phase" "$slug" <<'PY'
import json, sys
path, phase, slug = sys.argv[1:4]
with open(path) as f:
    s = json.load(f)
s.setdefault("last_models", {})[phase] = slug
with open(path, "w") as f:
    json.dump(s, f, indent=2)
    f.write("\n")
PY
  append_summary "$dir" "model: $phase → $slug"
  echo "Recorded $phase → $slug"
}

cmd_bump_fix() {
  local run_id
  run_id="$(resolve_run_id "${1:-}")"
  local dir count
  dir="$(run_dir_for "$run_id")"
  count="$(state_get "$dir" fix_count)"
  count=$((count + 1))
  if [[ "$count" -gt 2 ]]; then
    state_set "$dir" "fix_count=$count" "phase=stopped" "status=stopped" "stop_reason=\"fix_count exceeded 2\""
    append_summary "$dir" "bump-fix: count=$count → STOP"
    echo "ERROR: fix_count=$count exceeds max 2 — write 07-fix/diagnosis.md and stop" >&2
    cmd_status "$run_id"
    exit 1
  fi
  state_set "$dir" "fix_count=$count"
  append_summary "$dir" "bump-fix: count=$count"
  echo "fix_count=$count (max 2)"
  cmd_status "$run_id"
}

cmd_stop() {
  local reason="${1:-unspecified}"
  local run_id
  run_id="$(resolve_run_id "${2:-}")"
  local dir
  dir="$(run_dir_for "$run_id")"
  need_python
  python3 - "$dir/state.json" "$reason" <<'PY'
import json, sys
path, reason = sys.argv[1:3]
with open(path) as f:
    s = json.load(f)
s["phase"] = "stopped"
s["status"] = "stopped"
s["stop_reason"] = reason
with open(path, "w") as f:
    json.dump(s, f, indent=2)
    f.write("\n")
PY
  append_summary "$dir" "stop: $reason"
  cmd_status "$run_id"
}

main() {
  local cmd="${1:-}"
  shift || true
  case "$cmd" in
    init) cmd_init "$@" ;;
    use) cmd_use "$@" ;;
    status|next|next-phase) cmd_next "$@" ;;
    check) cmd_check "$@" ;;
    advance) cmd_advance "$@" ;;
    approve-plan) cmd_approve_plan "$@" ;;
    record-model) cmd_record_model "$@" ;;
    bump-fix) cmd_bump_fix "$@" ;;
    stop) cmd_stop "$@" ;;
    ""|-h|--help|help) usage ;;
    *) echo "Unknown command: $cmd" >&2; usage ;;
  esac
}

main "$@"
