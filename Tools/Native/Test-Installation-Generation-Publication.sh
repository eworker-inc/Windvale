#!/usr/bin/env bash
set -u

if [[ $# -ne 0 ]]; then
    echo 'Usage: Tools/Native/Test-Installation-Generation-Publication.sh' >&2
    exit 64
fi

script_directory=$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")" && pwd) || exit 1
repository_root=$(cd -- "$script_directory/../.." && pwd) || exit 1
node "$repository_root/Tools/Package/Verify-Installation-Generation-Publisher.mjs"
