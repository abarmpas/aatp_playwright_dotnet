#!/usr/bin/env bash
# run-tests.sh — run filtered suite tests and write a run-0N.log.md artifact.
# Usage:
#   run-tests.sh [--run-dir DIR] [--run-number N] [--filter EXPR] [--] [extra dotnet test args]
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
REPO_ROOT="$(cd "$SCRIPT_DIR/../../.." && pwd)"
AI_ROOT="$REPO_ROOT/.ai-workflow"
CURRENT_FILE="$AI_ROOT/CURRENT"

RUN_DIR=""
RUN_NUMBER=""
FILTER=""
EXTRA=()

while [[ $# -gt 0 ]]; do
  case "$1" in
    --run-dir) RUN_DIR="$2"; shift 2 ;;
    --run-number) RUN_NUMBER="$2"; shift 2 ;;
    --filter) FILTER="$2"; shift 2 ;;
    --) shift; EXTRA+=("$@"); break ;;
    *) EXTRA+=("$1"); shift ;;
  esac
done

if [[ -z "$RUN_DIR" ]]; then
  [[ -f "$CURRENT_FILE" ]] || { echo "ERROR: no --run-dir and no $CURRENT_FILE" >&2; exit 1; }
  run_id="$(tr -d '[:space:]' < "$CURRENT_FILE")"
  RUN_DIR="$AI_ROOT/$run_id"
fi

[[ -d "$RUN_DIR" ]] || { echo "ERROR: run dir not found: $RUN_DIR" >&2; exit 1; }

mkdir -p "$RUN_DIR/06-run"

if [[ -z "$RUN_NUMBER" ]]; then
  RUN_NUMBER=1
  shopt -s nullglob
  existing=("$RUN_DIR"/06-run/run-*.log.md)
  shopt -u nullglob
  if [[ ${#existing[@]} -gt 0 ]]; then
    max=0
    for f in "${existing[@]}"; do
      base="$(basename "$f")"
      n="${base#run-}"
      n="${n%.log.md}"
      n=$((10#$n))
      (( n > max )) && max=$n
    done
    RUN_NUMBER=$((max + 1))
  fi
fi

printf -v RUN_LABEL "%02d" "$RUN_NUMBER"
OUT="$RUN_DIR/06-run/run-${RUN_LABEL}.log.md"
RAW="$RUN_DIR/06-run/run-${RUN_LABEL}.raw.txt"

CMD=(dotnet test "$REPO_ROOT/AatpTemplateTestSuite.sln" --nologo)
if [[ -n "$FILTER" ]]; then
  CMD+=(--filter "$FILTER")
fi
CMD+=("${EXTRA[@]+"${EXTRA[@]}"}")

set +e
(
  cd "$REPO_ROOT"
  "${CMD[@]}"
) >"$RAW" 2>&1
code=$?
set -e

if [[ $code -eq 0 ]]; then
  result="PASS"
else
  result="FAIL"
fi

# Keep excerpt short for agents
excerpt="$(tail -n 80 "$RAW" | sed 's/\r$//')"

{
  echo "# Run ${RUN_LABEL}"
  echo
  echo "## Command"
  echo '```bash'
  printf '%q ' "${CMD[@]}"
  echo
  echo '```'
  echo
  echo "## Result"
  echo "$result"
  echo
  echo "## Filter"
  echo "- ${FILTER:-_(none)_}"
  echo
  echo "## Exit code"
  echo "$code"
  echo
  echo "## Excerpt"
  echo '```'
  echo "$excerpt"
  echo '```'
  echo
  echo "## Notes"
  echo "- Raw log: \`06-run/run-${RUN_LABEL}.raw.txt\`"
} >"$OUT"

echo "Wrote $OUT ($result)"
exit "$code"
