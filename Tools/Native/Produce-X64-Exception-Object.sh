#!/usr/bin/env bash
set -uo pipefail

if [[ $# -ne 1 || $1 != *.wvo ]]; then
    echo 'Usage: ./Tools/Native/Produce-X64-Exception-Object.sh <output.wvo>' >&2
    exit 64
fi

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
producer="$repository_root/Artifacts/Native-X64-Exception-Object-Producer-Candidate/linux-x64-exception-object.elf"
output_directory=$(dirname -- "$1")
if [[ ! -d $output_directory ]]; then
    echo 'The native x64 exception-object output directory does not exist.' >&2
    exit 1
fi
output_directory=$(CDPATH= cd -- "$output_directory" && pwd -P)
output="$output_directory/$(basename -- "$1")"
if [[ -e $output ]]; then
    echo 'The native x64 exception-object output already exists.' >&2
    exit 1
fi
if [[ ! -f $producer || $(wc -c < "$producer") -ne 389120 ]] ||
    ! printf '%s  %s\n' \
        'fa385758a5e167e5cf489e84a50efd34f85be9fbdeefb391e3292285554ba945' \
        "$producer" | sha256sum --check --strict --quiet; then
    echo 'The Linux native x64 exception-object producer identity is invalid.' >&2
    exit 1
fi

"$producer" "$output"
status=$?
if [[ $status -ne 0 || ! -f $output || $(wc -c < "$output") -ne 483 ]] ||
    ! printf '%s  %s\n' \
        '9caeb7ce353bca33e3bbac729ecca0423d59f8ce6b65ccd6b54fa53c381d617c' \
        "$output" | sha256sum --check --strict --quiet; then
    rm -f -- "$output"
    echo 'The native x64 exception-object producer failed.' >&2
    exit 1
fi
