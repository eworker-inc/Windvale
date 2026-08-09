#!/usr/bin/env bash
set -uo pipefail

if [[ $# -ne 2 || $2 != *.wvo ]]; then
    echo 'Usage: ./Tools/Native/Produce-Os-Probe-Object.sh <exceptions|wvb-admission-bridge|native-bridge-and-support> <output.wvo>' >&2
    exit 64
fi
case $1 in
    exceptions)
        expected_bytes=483
        expected_digest=9caeb7ce353bca33e3bbac729ecca0423d59f8ce6b65ccd6b54fa53c381d617c
        ;;
    wvb-admission-bridge)
        expected_bytes=484
        expected_digest=271c378b1f12bb4affa33474d865611cbf14e5b1b8996c703cb3d3cbe22eee7d
        ;;
    native-bridge-and-support)
        expected_bytes=461
        expected_digest=472a0fbe6497525e634a4785e92aa9ee62c3c7d70fff7510e45acbea644eea0b
        ;;
    *)
        echo 'Usage: ./Tools/Native/Produce-Os-Probe-Object.sh <exceptions|wvb-admission-bridge|native-bridge-and-support> <output.wvo>' >&2
        exit 64
        ;;
esac

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
producer="$repository_root/Artifacts/Native-Os-Probe-Object-Producer-Candidate/linux-x64-os-probe-object.elf"
output_directory=$(dirname -- "$2")
if [[ ! -d $output_directory ]]; then
    echo 'The native OS Probe object output directory does not exist.' >&2
    exit 1
fi
output_directory=$(CDPATH= cd -- "$output_directory" && pwd -P)
output="$output_directory/$(basename -- "$2")"
if [[ -e $output ]]; then
    echo 'The native OS Probe object output already exists.' >&2
    exit 1
fi
if [[ ! -f $producer || $(wc -c < "$producer") -ne 434176 ]] ||
    ! printf '%s  %s\n' \
        '4ae292f582b309b0ce0fad9ccf90a5f115cf46f5f3ff489f587d313cb97231c5' \
        "$producer" | sha256sum --check --strict --quiet; then
    echo 'The Linux native OS Probe object producer identity is invalid.' >&2
    exit 1
fi

"$producer" "$1" "$output"
status=$?
if [[ $status -ne 0 || ! -f $output || $(wc -c < "$output") -ne $expected_bytes ]] ||
    ! printf '%s  %s\n' "$expected_digest" "$output" |
        sha256sum --check --strict --quiet; then
    rm -f -- "$output"
    echo 'The native OS Probe object producer failed.' >&2
    exit 1
fi
