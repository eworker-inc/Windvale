#!/usr/bin/env bash
set -euo pipefail
script_directory="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd)"
node "$script_directory/Test-Language-1.0-Admission-Evidence-Format.mjs"
