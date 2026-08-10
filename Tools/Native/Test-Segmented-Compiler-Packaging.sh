#!/usr/bin/env bash
set -uo pipefail

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
exec "$script_directory/Test-Segmented-Compiler-Toolset-Reconstruction.sh" "$@"
