#!/usr/bin/env bash
set -uo pipefail

if [[ $# -ne 5 ]]; then
    echo 'Usage: ./Tools/Native/Stage-Console-Segmented.sh <windows-x64-console-v1|linux-x64-console-v1> <native-image.bin> <entry-offset> <chunk-prefix> <manifest.wvcs>' >&2
    exit 64
fi
case "$1:$5" in
    windows-x64-console-v1:*.wvcs|linux-x64-console-v1:*.wvcs) ;;
    *)
        echo 'Usage: ./Tools/Native/Stage-Console-Segmented.sh <windows-x64-console-v1|linux-x64-console-v1> <native-image.bin> <entry-offset> <chunk-prefix> <manifest.wvcs>' >&2
        exit 64
        ;;
esac

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
artifact_root="$repository_root/Artifacts/Native-Console-Segmented-Packager-Candidate"
if ! (cd -- "$artifact_root" && printf '%s  %s\n' \
    '779a87a9246e5d13eab08bf47ab53d329e627c5e64e6cfe86082cc6600450089' \
    'Console-Segmented-Packager.elf' | sha256sum --check --strict --quiet); then
    echo 'The Linux native segmented console-packager artifact digest is invalid.' >&2
    exit 1
fi

exec "$artifact_root/Console-Segmented-Packager.elf" "$1" "$2" "$3" "$4" "$5"
