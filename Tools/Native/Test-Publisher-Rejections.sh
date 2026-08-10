#!/usr/bin/env bash
set -uo pipefail

if [[ $# -ne 0 ]]; then
    echo 'Usage: ./Tools/Native/Test-Publisher-Rejections.sh' >&2
    exit 64
fi

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
temporary_root=${TMPDIR:-/tmp}
temporary_directory=$(mktemp -d "$temporary_root/windvale-publisher-rejections.XXXXXXXX") || exit 1
cleanup() {
    case "$temporary_directory" in
        "$temporary_root"/windvale-publisher-rejections.*)
            rm -f -- \
                "$temporary_directory/Invalid.bin" \
                "$temporary_directory/Sentinel.bin" \
                "$temporary_directory/Candidate.elf" \
                "$temporary_directory/Destination.elf" \
                "$temporary_directory/Candidate.wvo" \
                "$temporary_directory/Destination.wvo" \
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
        echo "The native publisher $label fixture could not be decoded." >&2
        return 1
    fi
    if [[ -s $decode_error ]]; then
        echo "The native publisher $label decoder wrote a diagnostic." >&2
        cat -- "$decode_error" >&2
        return 1
    fi
    if ! check_hash "$output" "$digest"; then
        echo "The native publisher $label identity differs." >&2
        return 1
    fi
}

total=0
passed=0
invalid="$temporary_directory/Invalid.bin"
sentinel="$temporary_directory/Sentinel.bin"
run_output="$temporary_directory/Run.out"
run_error="$temporary_directory/Run.err"
decode_error="$temporary_directory/Decode.err"
sentinel_digest=0d1829bbbc77f3ee3910a70f98528e1078117480332adb5a2d09df8b2d25f3b5

decode_fixture \
    "$repository_root/Tests/Native/Wvo/Bad-Magic.wvo.b64" \
    "$invalid" \
    '0369f8b34765adb08799e6b852e9d1e249c40d1049976b01ff59355dd111f288' \
    'invalid candidate' || exit $?
decode_fixture \
    "$repository_root/Tests/Native/Wvo/Return-42.wvo.b64" \
    "$sentinel" \
    "$sentinel_digest" \
    'destination sentinel' || exit $?

run_case() {
    local name=$1
    local launcher=$2
    local candidate_name=$3
    local destination_name=$4
    local report_digest=$5
    local candidate="$temporary_directory/$candidate_name"
    local destination="$temporary_directory/$destination_name"
    total=$((total + 1))
    cp -- "$invalid" "$candidate" || return 1
    cp -- "$sentinel" "$destination" || return 1
    "$repository_root/Tools/Native/$launcher" \
        "$candidate" "$destination" > "$run_output" 2> "$run_error"
    local run_status=$?
    if ((run_status != 1)); then
        echo "FAIL  $name: native publisher exit differs" >&2
        return 1
    fi
    if [[ -s $run_output ]]; then
        echo "FAIL  $name: rejected publication wrote standard output" >&2
        cat -- "$run_output" >&2
        return 1
    fi
    if ! check_hash "$run_error" "$report_digest"; then
        echo "FAIL  $name: native publisher report differs" >&2
        cat -- "$run_error" >&2
        return 1
    fi
    if ! check_hash "$destination" "$sentinel_digest"; then
        echo "FAIL  $name: rejected publication changed the destination" >&2
        return 1
    fi
    if ! check_hash "$candidate" "$invalid_digest"; then
        echo "FAIL  $name: rejected publication changed the candidate" >&2
        return 1
    fi
    local scratch=("$temporary_directory"/.wvpublish-*)
    if [[ -e ${scratch[0]} ]]; then
        echo "FAIL  $name: rejected publication left scratch" >&2
        return 1
    fi
    rm -f -- "$candidate" "$destination" "$run_output" "$run_error"
    passed=$((passed + 1))
    echo "PASS  $name"
}

run_case \
    'console-application' \
    'Publish-Console.sh' \
    'Candidate.elf' \
    'Destination.elf' \
    '39db034713225109f62c272db447d75cfe93ff0c259c8d9e5211f0df5c007e1f' || {
        echo "Tests: $total, Passed: $passed, Failed: $((total - passed))" >&2
        exit 1
    }
run_case \
    'hosted-verifier-application' \
    'Publish-Hosted-Verifier-Application.sh' \
    'Candidate.elf' \
    'Destination.elf' \
    'd56759e7c74de5f7c15f2940b87f5d89cd7c5d9dff647854560cdd8cd1749c24' || {
        echo "Tests: $total, Passed: $passed, Failed: $((total - passed))" >&2
        exit 1
    }
run_case \
    'hosted-verifier-publisher' \
    'Install-Hosted-Verifier-Publisher.sh' \
    'Candidate.elf' \
    'Destination.elf' \
    '22e5d25049052ee2a38f1775cc0c4ba1d5a5bbb95397c0b38a62ed310effe053' || {
        echo "Tests: $total, Passed: $passed, Failed: $((total - passed))" >&2
        exit 1
    }
run_case \
    'wvo' \
    'Publish-Wvo.sh' \
    'Candidate.wvo' \
    'Destination.wvo' \
    'e7a127a800310d9fbaf8b511b20c7b8184159521dec1be56b641793939a5c69f' || {
        echo "Tests: $total, Passed: $passed, Failed: $((total - passed))" >&2
        exit 1
    }

echo "Tests: $total, Passed: $passed, Failed: 0"
