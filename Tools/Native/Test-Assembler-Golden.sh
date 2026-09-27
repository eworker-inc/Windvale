#!/usr/bin/env bash
set -uo pipefail
script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
exec node "$script_directory/Test-Assembler-Golden.mjs" "$@"
