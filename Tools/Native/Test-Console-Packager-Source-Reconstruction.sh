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
lower_output="$temporary_directory/Lower.out"
lower_error="$temporary_directory/Lower.err"
total=0
passed=0

cleanup() {
    case "$temporary_directory" in
        "$temporary_root"/windvale-console-packager-source.*)
            rm -f -- \
                "$temporary_directory/ordinary-packager-source.wvb" \
                "$temporary_directory/ordinary-packager-source.wvo" \
                "$temporary_directory/segmented-packager-source.wvb" \
                "$temporary_directory/segmented-packager-source.wvo" \
                "$build_output" "$build_error" "$lower_output" "$lower_error"
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
    local expected_object_bytes=$6
    local expected_object_digest=$7
    local candidate="$temporary_directory/$case_name.wvb"
    local candidate_object="$temporary_directory/$case_name.wvo"

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
    "$script_directory/Lower-Wvb-To-Wvo.sh" "$candidate" "$candidate_object" \
        > "$lower_output" 2> "$lower_error" || {
            cat "$lower_error" >&2
            fail "$case_name native lowering exit"
        }
    [[ ! -s $lower_error ]] || fail "$case_name native lowering diagnostic"
    [[ $(wc -c < "$candidate_object") -eq "$expected_object_bytes" ]] || \
        fail "$case_name reconstructed WVO size"
    check_hash \
        "$candidate_object" "$expected_object_digest" \
        "$case_name reconstructed WVO identity"
    passed=$((passed + 1))
    echo "PASS  $case_name"
    rm -f -- \
        "$candidate" "$candidate_object" \
        "$build_output" "$build_error" "$lower_output" "$lower_error"
}

run_case \
    ordinary-packager-source Projects/Linker/Windvale-Console-Application-Packager.wvproj 60797 \
    f4c75495321736bbce22582213133e7cc09157a8439dc198d9848ec95683e89c \
    341a870f592b06d7be116af995efae06bed3ba7e7c90ef19bc344ef8799730e5 \
    692425 2a73e1a03d71cbec54de085cce2901580310105a1cb01e78563242242893186e
run_case \
    segmented-packager-source Projects/Linker/Windvale-Console-Application-Segmented-Packager.wvproj 70033 \
    c4941f396f76467cb6455472f7f4711c21a6f65c12c09a9b5f4135987628f20e \
    50488906e6b0bc9ae14da8194170ba5412bd441435e423d7e51392c45d12bbd4 \
    789653 cd0d79b92ee1b80242732f4d7419a08e71c5c5e132e462c5ae4b39953c56ede9

[[ $total -eq 2 && $passed -eq 2 ]] || fail 'case count'
echo 'Tests: 2, Passed: 2, Failed: 0'
