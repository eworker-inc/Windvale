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
                "$temporary_directory/Many-Sections.wvo" \
                "$temporary_directory/Unresolved-Import.wvo" \
                "$temporary_directory/Wrong-Kind-Provider.wvo" \
                "$temporary_directory/Absolute-Overflow.wvo" \
                "$temporary_directory/Relative-Overflow.wvo" \
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
many_sections="$temporary_directory/Many-Sections.wvo"
unresolved_import="$temporary_directory/Unresolved-Import.wvo"
wrong_kind_provider="$temporary_directory/Wrong-Kind-Provider.wvo"
absolute_overflow="$temporary_directory/Absolute-Overflow.wvo"
relative_overflow="$temporary_directory/Relative-Overflow.wvo"
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
decode_fixture \
    "$repository_root/Tests/Native/Wvo-Linker-Rejections/Many-Sections.wvo.b64" \
    "$many_sections" \
    '09cad03b9bf0543db2dec815f3f20deff044f5226e9347314b8c4d9a9e1020f8' \
    'many-sections WVO' || exit $?
decode_fixture \
    "$repository_root/Tests/Native/Wvo-Linker-Rejections/Unresolved-Import.wvo.b64" \
    "$unresolved_import" \
    '569926307b578cd1bf90dfb2b3c70eeb4b5ec7eff8e638e83613e89463717617' \
    'unresolved-import WVO' || exit $?
decode_fixture \
    "$repository_root/Tests/Native/Wvo-Linker-Rejections/Wrong-Kind-Provider.wvo.b64" \
    "$wrong_kind_provider" \
    '1276a484c52d48996a7d781121f85cab93ecde729cb6ce18dd7c77b4bdb98ce6' \
    'wrong-kind provider WVO' || exit $?
decode_fixture \
    "$repository_root/Tests/Native/Wvo-Linker-Rejections/Absolute-Overflow.wvo.b64" \
    "$absolute_overflow" \
    '994bc31ed39548dbd9339e7b0d2ac9b58936250b3603f90e84bda51f74b8bb11' \
    'absolute-overflow WVO' || exit $?
decode_fixture \
    "$repository_root/Tests/Native/Wvo-Linker-Rejections/Relative-Overflow.wvo.b64" \
    "$relative_overflow" \
    '4d6dcc8211e02399e8ba38fbbec94dcd11c15842efe09fd8af615e25b57d7a48' \
    'relative-overflow WVO' || exit $?

run_case() {
    local name=$1
    local report_digest=$2
    local base_address=$3
    local entry=$4
    shift 4
    total=$((total + 1))
    cp -- "$invalid" "$output" || return 1
    "$repository_root/Tools/Native/Link-Wvo.sh" \
        "$base_address" "$entry" "$output" "$@" \
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
    'malformed-object' \
    '18eeeeb5d84e82c54cf14480bc5c54e593f5cd429d68686ad64110d9780a5353' \
    '1048576' 'Main' "$invalid" || {
        echo "Tests: $total, Passed: $passed, Failed: $((total - passed))" >&2
        exit 1
    }
run_case \
    'aggregate-limit' \
    '33ecb82d77ff1f307b60a18993edf46807a39bf66ab7091054fc9ee7ad04ef61' \
    '1048576' 'Main' \
    "$many_sections" "$many_sections" "$many_sections" "$many_sections" "$many_sections" || {
        echo "Tests: $total, Passed: $passed, Failed: $((total - passed))" >&2
        exit 1
    }
run_case \
    'duplicate-export' \
    'cd8c0a1c80784f3d6db68984fe07f9bcbc0657c12e548bd923efad7f2666c324' \
    '1048576' 'Main' "$valid" "$valid" || {
        echo "Tests: $total, Passed: $passed, Failed: $((total - passed))" >&2
        exit 1
    }
run_case \
    'undefined-import' \
    '448d3e4eb8053d1aca41ebcdcf61af3d8519f3fea033859f82eb95d63ac275e0' \
    '1048576' 'Main' "$unresolved_import" || {
        echo "Tests: $total, Passed: $passed, Failed: $((total - passed))" >&2
        exit 1
    }
run_case \
    'kind-mismatch' \
    '047bea593cba87e948ea03c3cee09c5b04879683a1eb5856b9d0d30f7f774441' \
    '1048576' 'Main' "$unresolved_import" "$wrong_kind_provider" || {
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
    'layout-overflow' \
    '9c393cdbef3dc4a6dbe28ae5ba0c77fc56166a84b30c845bee78475f2679912d' \
    '4294967295' 'Main' "$valid" || {
        echo "Tests: $total, Passed: $passed, Failed: $((total - passed))" >&2
        exit 1
    }
run_case \
    'absolute-overflow' \
    '1867b048e4c725d2ea76f0ed0dd28b80f360fe07395d17ff62b743d5bc974b74' \
    '2147483649' 'Main' "$absolute_overflow" || {
        echo "Tests: $total, Passed: $passed, Failed: $((total - passed))" >&2
        exit 1
    }
run_case \
    'relative-overflow' \
    'd8a7ac5340b29066470b5656c840654221b508702cbc62ebfcecf7f36aa66e67' \
    '0' 'Main' "$relative_overflow" || {
        echo "Tests: $total, Passed: $passed, Failed: $((total - passed))" >&2
        exit 1
    }

echo "Tests: $total, Passed: $passed, Failed: 0"
