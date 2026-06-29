#!/usr/bin/env bash
set -euo pipefail

ROOT="$(cd "$(dirname "$0")/.." && pwd)"
PID_FILE="$ROOT/scripts/.backend.pids"

if [[ ! -f "$PID_FILE" ]]; then
  echo "No PID file found ($PID_FILE). Nothing to stop."
  exit 0
fi

stopped=0
while read -r name pid _; do
  [[ -z "${pid:-}" ]] && continue
  if kill -0 "$pid" 2>/dev/null; then
    # Kill child dotnet processes too
    pkill -P "$pid" 2>/dev/null || true
    kill "$pid" 2>/dev/null || true
    echo "Stopped $name (PID $pid)"
    stopped=$((stopped + 1))
  else
    echo "Already stopped: $name (PID $pid)"
  fi
done < "$PID_FILE"

rm -f "$PID_FILE"
echo ""
echo "Stopped $stopped process(es)."
