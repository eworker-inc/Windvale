#!/usr/bin/env bash
set -uo pipefail

if [[ $# -ne 0 ]]; then
    echo 'Usage: ./Tools/Native/Test-Console-Packager-Source-Reconstruction.sh' >&2
    exit 64
fi

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
temporary_root=${TMPDIR:-/tmp}
temporary_directory=$(mktemp -d "$temporary_root/windvale-console-packager-source.XXXXXXXX") || exit 1
build_output="$temporary_directory/Build.out"
build_error="$temporary_directory/Build.err"
total=0
passed=0

cleanup() {
    case "$temporary_directory" in
        "$temporary_root"/windvale-console-packager-source.*)
            rm -f -- \
                "$temporary_directory/ordinary-packager-source.wvb" \
                "$temporary_directory/segmented-packager-source.wvb" \
                "$build_output" "$build_error"
            rmdir -- "$temporary_directory" 2>/dev/null || true
            ;;
        *)
            echo "Refusing to remove unexpected temporary path: $temporary_directory" >&2
            return 1
            ;;
    esac
}
trap cleanup EXIT

fail() {
    echo "FAIL  console-packager-source-reconstruction: $1" >&2
    echo "Tests: $total, Passed: $passed, Failed: $((total - passed))" >&2
    exit 1
}

check_hash() {
    local path=$1
    local expected=$2
    local description=$3
    local actual
    actual=$(sha256sum --binary -- "$path" | cut -d' ' -f1) || fail "$description could not be hashed"
    [[ $actual == "$expected" ]] || fail "$description differs; expected $expected, actual $actual"
}

run_case() {
    local case_name=$1
    local project_name=$2
    local expected_bytes=$3
    local expected_digest=$4
    local expected_report_digest=$5
    local candidate="$temporary_directory/$case_name.wvb"

    total=$((total + 1))
    "$script_directory/Build-Wvb.sh" \
        "$repository_root/$project_name" "$candidate" \
        > "$build_output" 2> "$build_error" || {
            cat "$build_error" >&2
            fail "$case_name native build exit"
        }
    [[ ! -s $build_error ]] || fail "$case_name native build diagnostic"
    [[ $(wc -c < "$candidate") -eq "$expected_bytes" ]] || \
        fail "$case_name reconstructed WVB size"
    check_hash "$candidate" "$expected_digest" "$case_name reconstructed WVB identity"
    check_hash "$build_output" "$expected_report_digest" "$case_name build report"
    passed=$((passed + 1))
    echo "PASS  $case_name"
    rm -f -- "$candidate" "$build_output" "$build_error"
}

run_case \
    ordinary-packager-source Windvale-Console-Application-Packager.wvproj 58127 \
    7b055d4e6a456680a79eb28eaafa577e0019ea0ff1e34d9e713e9178428acc29 \
    de75af11831f8d681042df015a13c33e243f613b9738c5a7177747d63538b892
run_case \
    segmented-packager-source Windvale-Console-Application-Segmented-Packager.wvproj 68451 \
    33d7619c6115295a9eb612fd559031ab99c85196e3133a9405f880a19ac9ded2 \
    003dea772fb69bbfc4a485dd6a024e9c0e451726745675e38afab4292f75f61b

[[ $total -eq 2 && $passed -eq 2 ]] || fail 'case count'
echo 'Tests: 2, Passed: 2, Failed: 0'
