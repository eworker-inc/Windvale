#!/usr/bin/env bash
set -uo pipefail

if [[ $# -ne 0 ]]; then
    echo 'Usage: ./Tools/Native/Test-Baseline-Jit.sh' >&2
    exit 64
fi

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
temporary_root=${TMPDIR:-/tmp}
temporary_directory=$(mktemp -d "$temporary_root/windvale-baseline-jit-suite.XXXXXXXX") || exit 1
plan_output="$temporary_directory/Patch-Plan.out"
publisher_output="$temporary_directory/Publisher.out"
cleanup() {
    case "$temporary_directory" in
        "$temporary_root"/windvale-baseline-jit-suite.*)
            rm -f -- "$plan_output" "$publisher_output"
            rmdir -- "$temporary_directory"
            ;;
        *)
            echo "Refusing to remove unexpected temporary path: $temporary_directory" >&2
            return 1
            ;;
    esac
}
trap cleanup EXIT

"$script_directory/Test-Baseline-Jit-Patch-Plan.sh" > "$plan_output" || exit $?
plan_summary=$(awk 'NF { last = $0 } END { print last }' "$plan_output")
[[ $plan_summary == 'native baseline jit patch plan status=Passed result=0 entry-offset=3808' ]] || exit 1
"$script_directory/Test-Baseline-Jit-Publisher.sh" > "$publisher_output" || exit $?
publisher_summary=$(awk 'NF { last = $0 } END { print last }' "$publisher_output")
[[ $publisher_summary == 'native baseline jit publisher status=Passed result=0 platform=linux-x64' ]] || exit 1

cat -- "$plan_output" "$publisher_output"
echo 'Tests: 6, Passed: 6, Failed: 0'
