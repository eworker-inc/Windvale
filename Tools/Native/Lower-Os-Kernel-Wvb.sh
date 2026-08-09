#!/usr/bin/env bash
set -uo pipefail

if [[ $# -ne 2 || $1 != *.wvb || $2 != *.wvo ]]; then
    echo 'Usage: ./Tools/Native/Lower-Os-Kernel-Wvb.sh <input.wvb> <output.wvo>' >&2
    exit 64
fi

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
input_directory=$(CDPATH= cd -- "$(dirname -- "$1")" && pwd -P) || exit 1
input="$input_directory/$(basename -- "$1")"
output_directory=$(CDPATH= cd -- "$(dirname -- "$2")" && pwd -P) || exit 1
output="$output_directory/$(basename -- "$2")"
candidate="$repository_root/Artifacts/Native-Os-Kernel-Target-Candidate"
module="$candidate/Os-Kernel-Target.wvb"
target="$candidate/linux-x64-os-kernel-target.elf"

if [[ ! -f $input ]]; then
    echo 'The OS kernel target input is missing.' >&2
    exit 1
fi
if [[ -e $output ]]; then
    echo 'The OS kernel target output already exists.' >&2
    exit 1
fi
verify_identity() {
    local path=$1
    local bytes=$2
    local digest=$3
    local label=$4
    if [[ ! -f $path || $(wc -c < "$path") -ne $bytes ]] ||
        ! printf '%s  %s\n' "$digest" "$path" |
            sha256sum --check --strict --quiet; then
        echo "The $label identity is invalid." >&2
        return 1
    fi
}
verify_identity "$module" 57129 \
    9a7149ee7e0cb7533ef95baa199af24c36b5819217e634e362dd4f70e92bd3e8 \
    'kernel-target module' || exit 1
verify_identity "$target" 614400 \
    ca3730b7da3dcc645d353743cc14771a9bee9d669ecef89111d0342dabbf0147 \
    'Linux kernel target' || exit 1
if ! "$script_directory/Verify-Wvb.sh" "$input" >/dev/null 2>&1; then
    echo 'The OS kernel target input is not a verified WVB module.' >&2
    exit 1
fi

"$target" "$input" "$output"
status=$?
if [[ $status -ne 0 || ! -f $output ]] ||
    ! "$script_directory/Verify-Wvo.sh" "$output" >/dev/null 2>&1; then
    rm -f -- "$output"
    echo 'The OS kernel target rejected the module or produced an invalid object.' >&2
    exit 1
fi
