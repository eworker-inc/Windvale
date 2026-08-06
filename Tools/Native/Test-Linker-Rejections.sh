#!/usr/bin/env bash
set -uo pipefail

if [[ $# -ne 0 ]]; then
    echo 'Usage: ./Tools/Native/Test-Linker-Rejections.sh' >&2
    exit 64
fi

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
temporary_root=${TMPDIR:-/tmp}
temporary_directory=$(mktemp -d "$temporary_root/windvale-linker-rejections.XXXXXXXX") || exit 1
cleanup() {
    case "$temporary_directory" in
        "$temporary_root"/windvale-linker-rejections.*)
            rm -f -- \
                "$temporary_directory/Return-42.wvo" \
                "$temporary_directory/Bad-Magic.wvo" \
                "$temporary_directory/Output.bin" \
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
        echo "The native linker $label fixture could not be decoded." >&2
        return 1
    fi
    if [[ -s $decode_error ]]; then
        echo "The native linker $label fixture decoder wrote a diagnostic." >&2
        cat -- "$decode_error" >&2
        return 1
    fi
    if ! check_hash "$output" "$digest"; then
        echo "The native linker $label fixture identity differs." >&2
        return 1
    fi
}

total=0
passed=0
valid="$temporary_directory/Return-42.wvo"
invalid="$temporary_directory/Bad-Magic.wvo"
output="$temporary_directory/Output.bin"
run_output="$temporary_directory/Run.out"
run_error="$temporary_directory/Run.err"
decode_error="$temporary_directory/Decode.err"
sentinel_digest=0369f8b34765adb08799e6b852e9d1e249c40d1049976b01ff59355dd111f288

decode_fixture \
    "$repository_root/Tests/Native/Wvo/Return-42.wvo.b64" \
    "$valid" \
    '0d1829bbbc77f3ee3910a70f98528e1078117480332adb5a2d09df8b2d25f3b5' \
    'valid WVO' || exit $?
decode_fixture \
    "$repository_root/Tests/Native/Wvo/Bad-Magic.wvo.b64" \
    "$invalid" \
    "$sentinel_digest" \
    'malformed WVO' || exit $?

run_case() {
    local name=$1
    local report_digest=$2
    local base_address=$3
    local entry=$4
    local input=$5
    total=$((total + 1))
    cp -- "$invalid" "$output" || return 1
    "$repository_root/Tools/Native/Link-Wvo.sh" \
        "$base_address" "$entry" "$output" "$input" \
        > "$run_output" 2> "$run_error"
    local run_status=$?
    if ((run_status != 2)); then
        echo "FAIL  $name: native linker exit differs" >&2
        return 1
    fi
    if [[ -s $run_output ]]; then
        echo "FAIL  $name: rejected link wrote standard output" >&2
        cat -- "$run_output" >&2
        return 1
    fi
    if ! check_hash "$run_error" "$report_digest"; then
        echo "FAIL  $name: native linker report differs" >&2
        cat -- "$run_error" >&2
        return 1
    fi
    if ! check_hash "$output" "$sentinel_digest"; then
        echo "FAIL  $name: rejected link changed the output" >&2
        return 1
    fi
    rm -f -- "$output" "$run_output" "$run_error"
    passed=$((passed + 1))
    echo "PASS  $name"
}

run_case \
    'invalid-base' \
    'b5a687af92c9eca7eb5ba850bddf6dec932c94a6be304af35357655a915056b8' \
    'invalid' 'Main' "$valid" || {
        echo "Tests: $total, Passed: $passed, Failed: $((total - passed))" >&2
        exit 1
    }
run_case \
    'missing-entry' \
    '883ad60b71d4c010d4a2ddf168199dfaae04d1e076313ee1cf4dac8bee67a517' \
    '1048576' 'Missing' "$valid" || {
        echo "Tests: $total, Passed: $passed, Failed: $((total - passed))" >&2
        exit 1
    }
run_case \
    'malformed-object' \
    '18eeeeb5d84e82c54cf14480bc5c54e593f5cd429d68686ad64110d9780a5353' \
    '1048576' 'Main' "$invalid" || {
        echo "Tests: $total, Passed: $passed, Failed: $((total - passed))" >&2
        exit 1
    }

echo "Tests: $total, Passed: $passed, Failed: 0"
