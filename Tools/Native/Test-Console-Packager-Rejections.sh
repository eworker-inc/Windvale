#!/usr/bin/env bash
set -uo pipefail

if [[ $# -ne 0 ]]; then
    echo 'Usage: ./Tools/Native/Test-Console-Packager-Rejections.sh' >&2
    exit 64
fi

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
temporary_root=${TMPDIR:-/tmp}
temporary_directory=$(mktemp -d "$temporary_root/windvale-packager-rejections.XXXXXXXX") || exit 1
cleanup() {
    case "$temporary_directory" in
        "$temporary_root"/windvale-packager-rejections.*)
            rm -f -- \
                "$temporary_directory/Return-42.bin" \
                "$temporary_directory/Empty.bin" \
                "$temporary_directory/Sentinel.bin" \
                "$temporary_directory/Rejected.elf" \
                "$temporary_directory/Run.out" \
                "$temporary_directory/Run.err" \
                "$temporary_directory/Decode.err"
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
    local directory
    directory=$(CDPATH= cd -- "$(dirname -- "$path")" && pwd -P) || return 1
    (cd -- "$directory" && printf '%s  %s\n' \
        "$digest" "$(basename -- "$path")" | sha256sum --check --strict --quiet)
}

decode_fixture() {
    local source=$1
    local output=$2
    local digest=$3
    local label=$4
    if ! base64 --decode "$source" > "$output" 2> "$decode_error"; then
        echo "The native console-packager $label fixture could not be decoded." >&2
        return 1
    fi
    if [[ -s $decode_error ]]; then
        echo "The native console-packager $label decoder wrote a diagnostic." >&2
        cat -- "$decode_error" >&2
        return 1
    fi
    if ! check_hash "$output" "$digest"; then
        echo "The native console-packager $label identity differs." >&2
        return 1
    fi
}

total=0
passed=0
image="$temporary_directory/Return-42.bin"
empty="$temporary_directory/Empty.bin"
sentinel="$temporary_directory/Sentinel.bin"
output="$temporary_directory/Rejected.elf"
run_output="$temporary_directory/Run.out"
run_error="$temporary_directory/Run.err"
decode_error="$temporary_directory/Decode.err"
sentinel_digest=0369f8b34765adb08799e6b852e9d1e249c40d1049976b01ff59355dd111f288

decode_fixture \
    "$repository_root/Tests/Native/Images/Return-42.bin.b64" \
    "$image" \
    '11db5348e275fb704be582e8005ee7d604f7f17b154d6cc644d240eef29d456a' \
    'native image' || exit $?
decode_fixture \
    "$repository_root/Tests/Native/Wvo/Bad-Magic.wvo.b64" \
    "$sentinel" \
    "$sentinel_digest" \
    'output sentinel' || exit $?
: > "$empty"
if ! check_hash "$empty" \
    'e3b0c44298fc1c149afbf4c8996fb92427ae41e4649b934ca495991b7852b855'; then
    echo 'The native console-packager empty image identity differs.' >&2
    exit 1
fi

run_case() {
    local name=$1
    local report_digest=$2
    local input=$3
    local entry=$4
    total=$((total + 1))
    cp -- "$sentinel" "$output" || return 1
    "$repository_root/Tools/Native/Package-Console.sh" \
        'linux-x64-console-v1' "$input" "$entry" "$output" \
        > "$run_output" 2> "$run_error"
    local run_status=$?
    if ((run_status != 2)); then
        echo "FAIL  $name: native console-packager exit differs" >&2
        return 1
    fi
    if [[ -s $run_output ]]; then
        echo "FAIL  $name: rejected package wrote standard output" >&2
        cat -- "$run_output" >&2
        return 1
    fi
    if ! check_hash "$run_error" "$report_digest"; then
        echo "FAIL  $name: native console-packager report differs" >&2
        cat -- "$run_error" >&2
        return 1
    fi
    if ! check_hash "$output" "$sentinel_digest"; then
        echo "FAIL  $name: rejected package changed the output" >&2
        return 1
    fi
    rm -f -- "$output" "$run_output" "$run_error"
    passed=$((passed + 1))
    echo "PASS  $name"
}

run_case \
    'entry-at-end' \
    'a35789de908a6275c48a6cd25f1969732cec08fe1b39cdea615c35da1e79124e' \
    "$image" '6' || {
        echo "Tests: $total, Passed: $passed, Failed: $((total - passed))" >&2
        exit 1
    }
run_case \
    'invalid-entry' \
    '7ed94e6029a369b7ca0e967dae679e85be37c85017088c1e72b94c2123626c48' \
    "$image" 'invalid' || {
        echo "Tests: $total, Passed: $passed, Failed: $((total - passed))" >&2
        exit 1
    }
run_case \
    'empty-image' \
    '7ed94e6029a369b7ca0e967dae679e85be37c85017088c1e72b94c2123626c48' \
    "$empty" '0' || {
        echo "Tests: $total, Passed: $passed, Failed: $((total - passed))" >&2
        exit 1
    }

echo "Tests: $total, Passed: $passed, Failed: 0"
