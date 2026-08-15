#!/usr/bin/env bash
# Compatibility entry point. Current work uses Test-Verification-Owners.sh.
script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P) || exit 1
exec "$script_directory/Test-Verification-Owners.sh" "$@"
