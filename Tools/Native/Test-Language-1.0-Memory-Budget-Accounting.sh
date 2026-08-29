#!/usr/bin/env bash
set -uo pipefail

if [[ $# -ne 0 ]]; then
    echo 'Usage: ./Tools/Native/Test-Language-1.0-Memory-Budget-Accounting.sh' >&2
    exit 64
fi

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
project="$repository_root/Projects/Tests/Windvale-Native-Test-Language-1-Memory-Budget-Accounting.wvproj"
temporary_root=${TMPDIR:-/tmp}
work=$(mktemp -d "$temporary_root/windvale-memory-budget-accounting.XXXXXXXX") || exit 1
cleanup() {
    case "$work" in
        "$temporary_root"/windvale-memory-budget-accounting.*)
            rm -f -- "$work"/*
            rmdir -- "$work"
            ;;
        *)
            echo "Refusing to remove unexpected temporary path: $work" >&2
            return 1
            ;;
    esac
}
trap cleanup EXIT

fail() {
    local step=$1
    shift
    printf 'FAIL  language 1 memory budget accounting step=%s' "$step" >&2
    if [[ $# -gt 0 ]]; then
        printf ' %s' "$*" >&2
    fi
    printf '\n' >&2
    for evidence in \
        "$work/Build-A.err" \
        "$work/Build-B.err" \
        "$work/Package.err" \
        "$work/Run.err"; do
        if [[ -s $evidence ]]; then
            cat -- "$evidence" >&2
        fi
    done
    exit 1
}

echo 'START language 1 memory budget accounting phase=build item=1/4'
"$script_directory/Build-Wvb.sh" "$project" "$work/Accounting-A.wvb" \
    >"$work/Build-A.out" 2>"$work/Build-A.err" || fail build-a
[[ ! -s $work/Build-A.err ]] || fail build-a-stderr

echo 'START language 1 memory budget accounting phase=build item=2/4'
"$script_directory/Build-Wvb.sh" "$project" "$work/Accounting-B.wvb" \
    >"$work/Build-B.out" 2>"$work/Build-B.err" || fail build-b
[[ ! -s $work/Build-B.err ]] || fail build-b-stderr
cmp -s -- "$work/Accounting-A.wvb" "$work/Accounting-B.wvb" || \
    fail deterministic-wvb
wvb_bytes=$(wc -c < "$work/Accounting-A.wvb")
[[ $wvb_bytes -eq 37445 ]] || \
    fail wvb-size "expected=37445 actual=$wvb_bytes"

echo 'START language 1 memory budget accounting phase=package item=3/4'
"$script_directory/Package-Hosted-Wvb.sh" 1 "$work/Accounting-A.wvb" \
    "$work/Accounting.elf" linux \
    >"$work/Package.out" 2>"$work/Package.err" || fail package
[[ ! -s $work/Package.err ]] || fail package-stderr

echo 'START language 1 memory budget accounting phase=execute item=4/4'
"$work/Accounting.elf" >"$work/Run.out" 2>"$work/Run.err"
execution_result=$?
[[ $execution_result -eq 42 ]] || \
    fail execute-result "expected=42 actual=$execution_result"
[[ ! -s $work/Run.out && ! -s $work/Run.err ]] || fail execute-output

printf 'native language 1 memory budget accounting status=Passed cases=29 result=42 state-bytes=2616 capacity=65 lease-token-bytes=28 wvb-bytes=%s\n' \
    "$wvb_bytes"
