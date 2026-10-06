#!/usr/bin/env sh
set -eu

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd)
exec node "$script_directory/Test-Language-1.0-Callable-Semantics.mjs" "$@"
