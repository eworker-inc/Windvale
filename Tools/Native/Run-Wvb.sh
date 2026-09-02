#!/usr/bin/env bash
set -uo pipefail

task_environment=false
if [[ $# -eq 9 && $1 == --task-environment ]]; then
    task_environment=true
    module=$2
elif [[ $# -ge 1 && $# -le 2 && ( $# -eq 1 || $2 == --report-steps ) ]]; then
    module=$1
else
    echo 'Usage: ./Tools/Native/Run-Wvb.sh <module.wvb> [--report-steps]' >&2
    echo '       ./Tools/Native/Run-Wvb.sh --task-environment <module.wvb> <context-generation> <clock-generation> <deadline> <expected-runtime-generation> <admitted-runtime-generation> <observation-tick> <observed-runtime-generation>' >&2
    exit 64
fi

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
artifact_root="$repository_root/Artifacts/Native-Wvb-Runner-Candidate"
if ! (cd -- "$artifact_root" && printf '%s  %s\n' \
    'c5db1a90ce58f4807de13ca0082014e9ca09634a9ef487859166f15443e7149d' \
    'linux-x64-wvrun.elf' | sha256sum --check --strict --quiet) ||
    [[ $(wc -c < "$artifact_root/linux-x64-wvrun.elf") -ne 10129408 ]]; then
    echo 'The Linux native WVB runner artifact digest is invalid.' >&2
    exit 1
fi

input_directory=$(CDPATH= cd -- "$(dirname -- "$module")" && pwd -P) || exit 1
input_path="$input_directory/$(basename -- "$module")"
if [[ $input_path != *.wvb ]]; then
    echo 'The native runner input must use the .wvb extension.' >&2
    exit 64
fi

if [[ $task_environment == true ]]; then
    "$artifact_root/linux-x64-wvrun.elf" --task-environment "$input_path" \
        "$3" "$4" "$5" "$6" "$7" "$8" "$9"
elif [[ $# -eq 1 ]]; then
    "$artifact_root/linux-x64-wvrun.elf" "$input_path"
else
    "$artifact_root/linux-x64-wvrun.elf" "$input_path" --report-steps
fi
