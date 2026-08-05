#!/usr/bin/env bash
set -uo pipefail

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
plan="$repository_root/Tests/Native/Plan.txt"
plan_digest=d04f77c41bbae2c98541b3a0e6dec0ee0c725106dae72e5bb128d52c4abf3fc5

if ! printf '%s  %s\n' "$plan_digest" "$plan" | sha256sum --check --strict --quiet; then
    echo 'The native test plan artifact digest is invalid.' >&2
    exit 1
fi

temporary_root=${TMPDIR:-/tmp}
temporary_directory=$(mktemp -d "$temporary_root/windvale-native-tests.XXXXXXXX") || exit 1
cleanup() {
    case "$temporary_directory" in
        "$temporary_root"/windvale-native-tests.*)
            rm -rf -- "$temporary_directory"
            ;;
        *)
            echo "Refusing to remove unexpected temporary path: $temporary_directory" >&2
            return 1
            ;;
    esac
}
trap cleanup EXIT

total=0
passed=0
IFS= read -r header < "$plan"
if [[ $header != 'windvale-native-tests 1' ]]; then
    echo 'The native test plan header is invalid.' >&2
    exit 1
fi

while IFS='|' read -r name project expected_hash expected_result; do
    [[ -n $name ]] || continue
    [[ $name != 'windvale-native-tests 1' ]] || continue
    total=$((total + 1))
    output="$temporary_directory/Current.wvb"
    build_output="$temporary_directory/Build.out"
    build_error="$temporary_directory/Build.err"
    run_output="$temporary_directory/Run.out"
    run_error="$temporary_directory/Run.err"
    expected_output="$temporary_directory/Expected.out"

    if ! "$repository_root/Tools/Native/Build-Wvb.sh" \
        "$repository_root/$project" "$output" > "$build_output" 2> "$build_error"; then
        echo "FAIL  $name: native build failed" >&2
        cat -- "$build_error" >&2
        echo "Tests: $total, Passed: $passed, Failed: $((total - passed))" >&2
        exit 1
    fi
    if [[ -s $build_error ]]; then
        echo "FAIL  $name: native build wrote a diagnostic" >&2
        cat -- "$build_error" >&2
        echo "Tests: $total, Passed: $passed, Failed: $((total - passed))" >&2
        exit 1
    fi
    if ! (cd -- "$temporary_directory" && printf '%s  %s\n' \
        "$expected_hash" 'Current.wvb' | sha256sum --check --strict --quiet); then
        echo "FAIL  $name: WVB identity differs" >&2
        echo "Tests: $total, Passed: $passed, Failed: $((total - passed))" >&2
        exit 1
    fi

    if ! "$repository_root/Tools/Native/Run-Wvb.sh" "$output" \
        > "$run_output" 2> "$run_error"; then
        echo "FAIL  $name: native execution failed" >&2
        cat -- "$run_error" >&2
        echo "Tests: $total, Passed: $passed, Failed: $((total - passed))" >&2
        exit 1
    fi
    if [[ -s $run_error ]]; then
        echo "FAIL  $name: native execution wrote a diagnostic" >&2
        cat -- "$run_error" >&2
        echo "Tests: $total, Passed: $passed, Failed: $((total - passed))" >&2
        exit 1
    fi
    printf 'Result: %s\n' "$expected_result" > "$expected_output"
    if ! cmp --silent "$expected_output" "$run_output"; then
        echo "FAIL  $name: result report differs" >&2
        echo "Tests: $total, Passed: $passed, Failed: $((total - passed))" >&2
        exit 1
    fi

    passed=$((passed + 1))
    echo "PASS  $name"
done < "$plan"

echo "Tests: $total, Passed: $passed, Failed: 0"
