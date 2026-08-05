#!/usr/bin/env bash
set -uo pipefail

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
plan="$repository_root/Tests/Native/Plan.txt"
plan_digest=1b5dc525a2a5fc8883e21cbd0502bb2c3af1cb93c32fec11f5379e9f624fd870

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
if [[ $header != 'windvale-native-tests 3' ]]; then
    echo 'The native test plan header is invalid.' >&2
    exit 1
fi

while IFS='|' read -r name input_kind input expected_hash expected_kind expected_value; do
    [[ -n $name ]] || continue
    [[ $name != 'windvale-native-tests 3' ]] || continue
    total=$((total + 1))
    output="$temporary_directory/Current.wvb"
    build_output="$temporary_directory/Build.out"
    build_error="$temporary_directory/Build.err"
    decode_output="$temporary_directory/Decode.out"
    decode_error="$temporary_directory/Decode.err"
    run_output="$temporary_directory/Run.out"
    run_error="$temporary_directory/Run.err"
    expected_output="$temporary_directory/Expected.out"
    expected_error="$temporary_directory/Expected.err"

    if [[ $input_kind == project ]]; then
        if ! "$repository_root/Tools/Native/Build-Wvb.sh" \
            "$repository_root/$input" "$output" > "$build_output" 2> "$build_error"; then
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
    elif [[ $input_kind == fixture-base64 ]]; then
        if ! base64 --decode "$repository_root/$input" \
            > "$output" 2> "$decode_error"; then
            echo "FAIL  $name: malformed fixture decoding failed" >&2
            cat -- "$decode_error" >&2
            echo "Tests: $total, Passed: $passed, Failed: $((total - passed))" >&2
            exit 1
        fi
        if [[ -s $decode_error ]]; then
            echo "FAIL  $name: malformed fixture decoding wrote a diagnostic" >&2
            cat -- "$decode_error" >&2
            echo "Tests: $total, Passed: $passed, Failed: $((total - passed))" >&2
            exit 1
        fi
    else
        echo "FAIL  $name: test input kind is invalid" >&2
        echo "Tests: $total, Passed: $passed, Failed: $((total - passed))" >&2
        exit 1
    fi
    if ! (cd -- "$temporary_directory" && printf '%s  %s\n' \
        "$expected_hash" 'Current.wvb' | sha256sum --check --strict --quiet); then
        echo "FAIL  $name: WVB identity differs" >&2
        echo "Tests: $total, Passed: $passed, Failed: $((total - passed))" >&2
        exit 1
    fi

    if [[ $expected_kind == verify-failure ]]; then
        "$repository_root/Tools/Native/Verify-Wvb.sh" "$output" \
            > "$run_output" 2> "$run_error"
    else
        "$repository_root/Tools/Native/Run-Wvb.sh" "$output" \
            > "$run_output" 2> "$run_error"
    fi
    run_status=$?
    if [[ $expected_kind == result ]]; then
        if ((run_status != 0)); then
            echo "FAIL  $name: native execution failed" >&2
            cat -- "$run_error" >&2
            echo "Tests: $total, Passed: $passed, Failed: $((total - passed))" >&2
            exit 1
        fi
        if [[ -s $run_error ]]; then
            echo "FAIL  $name: successful execution wrote a diagnostic" >&2
            cat -- "$run_error" >&2
            echo "Tests: $total, Passed: $passed, Failed: $((total - passed))" >&2
            exit 1
        fi
        printf 'Result: %s\n' "$expected_value" > "$expected_output"
        if ! cmp --silent "$expected_output" "$run_output"; then
            echo "FAIL  $name: result report differs" >&2
            echo "Tests: $total, Passed: $passed, Failed: $((total - passed))" >&2
            exit 1
        fi
    elif [[ $expected_kind == failure ]]; then
        if ((run_status != 1)); then
            echo "FAIL  $name: native failure exit differs" >&2
            echo "Tests: $total, Passed: $passed, Failed: $((total - passed))" >&2
            exit 1
        fi
        if [[ -s $run_output ]]; then
            echo "FAIL  $name: failed execution wrote standard output" >&2
            cat -- "$run_output" >&2
            echo "Tests: $total, Passed: $passed, Failed: $((total - passed))" >&2
            exit 1
        fi
        expected_code=${expected_value%%:*}
        expected_instructions=${expected_value#*:}
        if [[ $expected_code == "$expected_value" || -z $expected_instructions ]]; then
            echo "FAIL  $name: failure expectation is invalid" >&2
            echo "Tests: $total, Passed: $passed, Failed: $((total - passed))" >&2
            exit 1
        fi
        printf 'wvb run status=Failed code=%s instructions=%s\n' \
            "$expected_code" "$expected_instructions" > "$expected_error"
        if ! cmp --silent "$expected_error" "$run_error"; then
            echo "FAIL  $name: failure report differs" >&2
            cat -- "$run_error" >&2
            echo "Tests: $total, Passed: $passed, Failed: $((total - passed))" >&2
            exit 1
        fi
    elif [[ $expected_kind == verify-failure ]]; then
        if ((run_status != 1)); then
            echo "FAIL  $name: native verification exit differs" >&2
            echo "Tests: $total, Passed: $passed, Failed: $((total - passed))" >&2
            exit 1
        fi
        if [[ -s $run_output ]]; then
            echo "FAIL  $name: rejected verification wrote standard output" >&2
            cat -- "$run_output" >&2
            echo "Tests: $total, Passed: $passed, Failed: $((total - passed))" >&2
            exit 1
        fi
        printf 'wvb status=Invalid phase=%s\n' "$expected_value" > "$expected_error"
        if ! cmp --silent "$expected_error" "$run_error"; then
            echo "FAIL  $name: verification report differs" >&2
            cat -- "$run_error" >&2
            echo "Tests: $total, Passed: $passed, Failed: $((total - passed))" >&2
            exit 1
        fi
    else
        echo "FAIL  $name: test expectation kind is invalid" >&2
        echo "Tests: $total, Passed: $passed, Failed: $((total - passed))" >&2
        exit 1
    fi

    passed=$((passed + 1))
    echo "PASS  $name"
done < "$plan"

echo "Tests: $total, Passed: $passed, Failed: 0"
