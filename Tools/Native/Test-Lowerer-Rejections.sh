#!/usr/bin/env bash
set -uo pipefail

if [[ $# -ne 0 ]]; then
    echo 'Usage: ./Tools/Native/Test-Lowerer-Rejections.sh' >&2
    exit 64
fi

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
temporary_root=${TMPDIR:-/tmp}
temporary_directory=$(mktemp -d "$temporary_root/windvale-lowerer-rejections.XXXXXXXX") || exit 1
work_directory="$temporary_directory/Work"
mkdir -- "$work_directory" || exit 1
cleanup() {
    case "$temporary_directory" in
        "$temporary_root"/windvale-lowerer-rejections.*)
            rm -f -- \
                "$temporary_directory/Bad-Magic.wvb" \
                "$temporary_directory/Unsupported-Function.wvb" \
                "$temporary_directory/Sentinel.wvo" \
                "$temporary_directory/Destination.wvo" \
                "$temporary_directory/Run.out" \
                "$temporary_directory/Run.err" \
                "$temporary_directory/Decode.err"
            rmdir -- "$work_directory" 2>/dev/null || true
            rmdir -- "$temporary_directory" 2>/dev/null || true
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
        echo "The native lowerer $label fixture could not be decoded." >&2
        return 1
    fi
    if [[ -s $decode_error ]]; then
        echo "The native lowerer $label decoder wrote a diagnostic." >&2
        cat -- "$decode_error" >&2
        return 1
    fi
    if ! check_hash "$output" "$digest"; then
        echo "The native lowerer $label identity differs." >&2
        return 1
    fi
}

total=0
passed=0
invalid="$temporary_directory/Bad-Magic.wvb"
unsupported="$temporary_directory/Unsupported-Function.wvb"
sentinel="$temporary_directory/Sentinel.wvo"
destination="$temporary_directory/Destination.wvo"
run_output="$temporary_directory/Run.out"
run_error="$temporary_directory/Run.err"
decode_error="$temporary_directory/Decode.err"
sentinel_digest=0d1829bbbc77f3ee3910a70f98528e1078117480332adb5a2d09df8b2d25f3b5

decode_fixture \
    "$repository_root/Tests/Native/Malformed-Wvb/Bad-Magic.wvb.b64" \
    "$invalid" \
    '20618498d9df059d52fc0d660bf52f32df291c88b94d4b5ded224078f936108e' \
    'bad-magic input' || exit $?
decode_fixture \
    "$repository_root/Tests/Native/Wvb-To-Wvo-Rejections/Unsupported-Function.wvb.b64" \
    "$unsupported" \
    '605a2528ebad0fc418e9cb1ab8738c6e3a9b2e58cb9e0897cb0bc93fececaf91' \
    'unsupported-function input' || exit $?
decode_fixture \
    "$repository_root/Tests/Native/Wvo/Return-42.wvo.b64" \
    "$sentinel" \
    "$sentinel_digest" \
    'destination sentinel' || exit $?

run_case() {
    local name=$1
    local input=$2
    local report_digest=$3
    total=$((total + 1))
    cp -- "$sentinel" "$destination" || return 1
    TMPDIR="$work_directory" \
        "$repository_root/Tools/Native/Lower-Wvb-To-Wvo.sh" \
        "$input" "$destination" > "$run_output" 2> "$run_error"
    local run_status=$?
    if ((run_status != 1)); then
        echo "FAIL  $name: native lowerer exit differs" >&2
        return 1
    fi
    if [[ -s $run_output ]]; then
        echo "FAIL  $name: rejected lowering wrote standard output" >&2
        cat -- "$run_output" >&2
        return 1
    fi
    if ! check_hash "$run_error" "$report_digest"; then
        echo "FAIL  $name: native lowerer report differs" >&2
        cat -- "$run_error" >&2
        return 1
    fi
    if ! check_hash "$destination" "$sentinel_digest"; then
        echo "FAIL  $name: rejected lowering changed the destination" >&2
        return 1
    fi
    if find "$work_directory" -mindepth 1 -maxdepth 1 -print -quit | grep -q .; then
        echo "FAIL  $name: rejected lowering left private work" >&2
        return 1
    fi
    rm -f -- "$destination" "$run_output" "$run_error"
    passed=$((passed + 1))
    echo "PASS  $name"
}

run_case \
    'malformed' \
    "$invalid" \
    'cb4866cce34d859dabe8d8823f7ad391daed579cdcd61fd6ecbd4e5c324d78dc' || {
        echo "Tests: $total, Passed: $passed, Failed: $((total - passed))" >&2
        exit 1
    }
run_case \
    'unsupported-function' \
    "$unsupported" \
    '0e5a4dc04f822ab0afe79fe48d5126ffbdc825a2abe47f7b7a0bfd67b12830e5' || {
        echo "Tests: $total, Passed: $passed, Failed: $((total - passed))" >&2
        exit 1
    }

echo "Tests: $total, Passed: $passed, Failed: 0"
