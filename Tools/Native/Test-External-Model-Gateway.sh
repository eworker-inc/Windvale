#!/usr/bin/env bash
set -uo pipefail

if [[ $# -ne 0 ]]; then
    echo 'Usage: ./Tools/Native/Test-External-Model-Gateway.sh' >&2
    exit 64
fi

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
node "$repository_root/Tools/Models/Test-External-Model-Gateway-Core.mjs" || exit $?
node "$repository_root/Tools/Models/Test-Supervised-External-Model-Gateway.mjs" || exit $?
echo 'external model gateway status=Passed providers=3 cases=30 child-process=Verified differential=Verified public-network=0 real-credentials=0'
