#!/usr/bin/env bash
set -uo pipefail

if [[ $# -ne 0 ]]; then
    echo 'Usage: ./Tools/Native/Test-Baseline-Jit-Patch-Plan.sh' >&2
    exit 64
fi

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
temporary_root=${TMPDIR:-/tmp}
temporary_directory=$(mktemp -d "$temporary_root/windvale-baseline-jit-plan.XXXXXXXX") || exit 1
cleanup() {
    case "$temporary_directory" in
        "$temporary_root"/windvale-baseline-jit-plan.*)
            rm -f -- \
                "$temporary_directory/Baseline-Jit-Patch-Plan.wvb" \
                "$temporary_directory/Baseline-Jit-Patch-Plan.wvo" \
                "$temporary_directory/Baseline-Jit-Patch-Plan.bin" \
                "$temporary_directory/Baseline-Jit-Patch-Plan.elf" \
                "$temporary_directory/Baseline-Jit-Patch-Plan.wvmap" \
                "$temporary_directory/Application.err"
            rmdir -- "$temporary_directory"
            ;;
        *)
            echo "Refusing to remove unexpected temporary path: $temporary_directory" >&2
            return 1
            ;;
    esac
}
trap cleanup EXIT

check_hash() {
    local path=$1
    local digest=$2
    local label=$3
    local directory
    directory=$(CDPATH= cd -- "$(dirname -- "$path")" && pwd -P) || return 1
    if ! (cd -- "$directory" && printf '%s  %s\n' \
        "$digest" "$(basename -- "$path")" | sha256sum --check --strict --quiet); then
        echo "The native baseline-JIT patch-plan $label identity differs." >&2
        return 1
    fi
}

wvb="$temporary_directory/Baseline-Jit-Patch-Plan.wvb"
wvo="$temporary_directory/Baseline-Jit-Patch-Plan.wvo"
image="$temporary_directory/Baseline-Jit-Patch-Plan.bin"
application="$temporary_directory/Baseline-Jit-Patch-Plan.elf"
map="$temporary_directory/Baseline-Jit-Patch-Plan.wvmap"
application_error="$temporary_directory/Application.err"

"$repository_root/Tools/Native/Build-Wvb.sh" \
    "$repository_root/Projects/Tests/Windvale-Native-Baseline-Jit-Patch-Plan-Self-Test.wvproj" \
    "$wvb" >/dev/null || exit $?
check_hash "$wvb" \
    '2934df86db71047bfd325d50fd9549362bc60953e6924d6242b56eb79be658ea' \
    'WVB' || exit $?

"$repository_root/Tools/Native/Lower-Wvb-To-Wvo.sh" "$wvb" "$wvo" \
    >/dev/null || exit $?
"$repository_root/Tools/Native/Verify-Wvo.sh" "$wvo" >/dev/null || exit $?
check_hash "$wvo" \
    'fe3f9af8cb9315b866bc898814e1d954807a3486256cc23cdcbfbfbfa2608149' \
    'WVO' || exit $?

"$repository_root/Tools/Native/Link-Wvo.sh" 1048576 Main "$image" "$wvo" \
    > "$map" || exit $?
check_hash "$image" \
    'cbc1a556659a3e7829e60b759920c931f68cb61d6f0a4696823b1094b0ebfdcc' \
    'flat image' || exit $?
check_hash "$map" \
    'fb89f964f40deae96d46d157d78a69f6212865ad23347805e987c80dccbf5256' \
    'link map' || exit $?

entry_address=$(sed -n 's/^entry name=Main address=//p' "$map")
if [[ ! $entry_address =~ ^[0-9]+$ ]]; then
    echo 'The native baseline-JIT patch-plan entry is missing from the link map.' >&2
    exit 1
fi
entry_offset=$((entry_address - 1048576))
if ((entry_offset != 3808)); then
    echo "The native baseline-JIT patch-plan entry offset is $entry_offset, expected 3808." >&2
    exit 1
fi

"$repository_root/Tools/Native/Package-Console.sh" linux-x64-console-v1 \
    "$image" "$entry_offset" "$application" >/dev/null || exit $?
check_hash "$application" \
    '23a6059d53c349264332a565395dbcb8a159167a760f3ffe52896eb86b3fee54' \
    'Linux application' || exit $?

"$application" >/dev/null 2> "$application_error"
application_result=$?
if ((application_result != 0)); then
    echo "The native baseline-JIT patch-plan result is $application_result, expected 0." >&2
    if [[ -s $application_error ]]; then
        cat -- "$application_error" >&2
    fi
    exit 1
fi
if [[ -s $application_error ]]; then
    echo 'The native baseline-JIT patch-plan application wrote a diagnostic.' >&2
    cat -- "$application_error" >&2
    exit 1
fi

echo 'native baseline jit patch plan status=Passed result=0 entry-offset=3808'
