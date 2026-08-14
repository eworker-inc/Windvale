#!/usr/bin/env bash
set -uo pipefail

filter=
shard=
case $# in
    0)
        ;;
    2)
        case $1 in
            --filter)
                [[ -n $2 ]] || exit 64
                filter=$2
                ;;
            --shard)
                case $2 in
                    1|2|3|4) shard=$2 ;;
                    *) exit 64 ;;
                esac
                ;;
            *)
                echo 'Usage: ./Tools/Native/Test-Retirement-Suite.sh [--filter <suite-name>|--shard <1-4>]' >&2
                exit 64
                ;;
        esac
        ;;
    *)
        echo 'Usage: ./Tools/Native/Test-Retirement-Suite.sh [--filter <suite-name>|--shard <1-4>]' >&2
        exit 64
        ;;
esac

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
plan="$repository_root/Tests/Native/Retirement-Suite.txt"
plan_digest=1000510438a1b3a7d60849e9bc5794a406943232ed7dd5690f190464d58028f1

check_hash() {
    local path=$1
    local digest=$2
    local directory
    directory=$(CDPATH= cd -- "$(dirname -- "$path")" && pwd -P) || return 1
    (cd -- "$directory" && printf '%s  %s\n' \
        "$digest" "$(basename -- "$path")" | sha256sum --check --strict --quiet)
}

if ! check_hash "$plan" "$plan_digest"; then
    echo 'Native retirement suite plan identity differs' >&2
    exit 1
fi

while IFS='|' read -r name command cases suite_shard expected_summary; do
    owner="$repository_root/Tools/Native/$command.sh"
    if [[ ! -x $owner ]]; then
        echo "Native retirement suite owner is missing or not executable: $owner" >&2
        exit 1
    fi
    case $suite_shard in
        1|2|3|4) ;;
        *)
            echo "Native retirement suite shard is invalid: $name=$suite_shard" >&2
            exit 1
            ;;
    esac
done < <(tail -n +2 -- "$plan")

temporary_root=${TMPDIR:-/tmp}
temporary_directory=$(mktemp -d "$temporary_root/windvale-native-retirement-suite.XXXXXXXX") || exit 1
suite_output="$temporary_directory/Suite.out"
suite_error="$temporary_directory/Suite.err"
cleanup() {
    case "$temporary_directory" in
        "$temporary_root"/windvale-native-retirement-suite.*)
            rm -f -- "$suite_output" "$suite_error"
            rmdir -- "$temporary_directory"
            ;;
        *)
            echo "Refusing to remove unexpected temporary path: $temporary_directory" >&2
            return 1
            ;;
    esac
}
trap cleanup EXIT

selected=0
total_suites=0
passed_suites=0
total_cases=0
total_start=$SECONDS

run_suite() {
    local name=$1
    local command=$2
    local cases=$3
    local expected_summary=$4
    local suite_status
    local actual_summary
    local suite_start=$SECONDS

    selected=$((selected + 1))
    total_suites=$((total_suites + 1))
    total_cases=$((total_cases + cases))
    "$repository_root/Tools/Native/$command.sh" > "$suite_output" 2> "$suite_error"
    suite_status=$?
    local suite_elapsed_ms=$(((SECONDS - suite_start) * 1000))
    if ((suite_status != 0)); then
        echo "FAIL  suite $name: native command exited $suite_status elapsed-ms=$suite_elapsed_ms" >&2
        cat -- "$suite_output" "$suite_error" >&2
        return 1
    fi
    if [[ -s $suite_error ]]; then
        echo "FAIL  suite $name: native command wrote standard error" >&2
        cat -- "$suite_error" >&2
        return 1
    fi
    actual_summary=$(awk 'NF { last = $0 } END { print last }' "$suite_output")
    if [[ $actual_summary != "$expected_summary" ]]; then
        echo "FAIL  suite $name: summary differs" >&2
        cat -- "$suite_output" >&2
        return 1
    fi
    rm -f -- "$suite_output" "$suite_error"
    passed_suites=$((passed_suites + 1))
    echo "PASS  suite $name cases=$cases elapsed-ms=$suite_elapsed_ms"
}

IFS= read -r header < "$plan"
if [[ $header != 'windvale-native-retirement-suite 2' ]]; then
    echo 'Native retirement suite header differs' >&2
    exit 1
fi

while IFS='|' read -r name command cases suite_shard expected_summary; do
    if [[ -n $filter && $filter != "$name" ]]; then
        continue
    fi
    if [[ -n $shard && $shard != "$suite_shard" ]]; then
        continue
    fi
    if ! run_suite "$name" "$command" "$cases" "$expected_summary"; then
        echo "Timing: elapsed-ms=$(((SECONDS - total_start) * 1000))" >&2
        echo "Suites: $total_suites, Passed: $passed_suites, Failed: $((total_suites - passed_suites)), Cases: $total_cases" >&2
        exit 1
    fi
done < <(tail -n +2 -- "$plan")

if ((selected == 0)); then
    if [[ -n $filter ]]; then
        echo "Unknown native retirement suite: $filter" >&2
    else
        echo "Empty native retirement shard: $shard" >&2
    fi
    exit 64
fi

echo "Timing: elapsed-ms=$(((SECONDS - total_start) * 1000))"
echo "Suites: $total_suites, Passed: $passed_suites, Failed: 0, Cases: $total_cases"
