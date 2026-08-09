#!/usr/bin/env bash
set -uo pipefail

if [[ $# -ne 4 || $1 != *.wvo || $4 != *.wvo || -z $2 || -z $3 ]]; then
    echo 'Usage: ./Tools/Native/Rename-Wvo-Export.sh <input.wvo> <old-export> <new-export> <output.wvo>' >&2
    exit 64
fi

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
renamer="$repository_root/Artifacts/Native-Wvo-Export-Renamer-Candidate/linux-x64-wvorename.elf"
output_directory=$(dirname -- "$4")
if [[ ! -d $output_directory ]]; then
    echo 'The native WVO export-renamer output directory does not exist.' >&2
    exit 1
fi
output_directory=$(CDPATH= cd -- "$output_directory" && pwd -P)
output="$output_directory/$(basename -- "$4")"
if [[ -e $output ]]; then
    echo 'The native WVO export-renamer output already exists.' >&2
    exit 1
fi
if [[ ! -f $renamer || $(wc -c < "$renamer") -ne 393216 ]] ||
    ! printf '%s  %s\n' \
        'c27787ee970d551ad0d85026ee7f9c0ac9de72d933e563398ac356d5561ed0ae' \
        "$renamer" | sha256sum --check --strict --quiet; then
    echo 'The Linux native WVO export-renamer identity is invalid.' >&2
    exit 1
fi

"$renamer" "$1" "$2" "$3" "$output"
status=$?
if [[ $status -ne 0 ]]; then
    rm -f -- "$output"
    exit "$status"
fi
