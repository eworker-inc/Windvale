#!/usr/bin/env bash
set -uo pipefail

filter=
case $# in
    0)
        ;;
    2)
        if [[ $1 != --filter || -z $2 ]]; then
            echo 'Usage: ./Tools/Native/Test-Retirement-Suite.sh [--filter <suite-name>]' >&2
            exit 64
        fi
        filter=$2
        ;;
    *)
        echo 'Usage: ./Tools/Native/Test-Retirement-Suite.sh [--filter <suite-name>]' >&2
        exit 64
        ;;
esac

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
plan="$repository_root/Tests/Native/Retirement-Suite.txt"
plan_digest=399f6a044fd4cca1552a79db8c421c5dd92b36f4555a0ce103c5be44133df587

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

run_suite() {
    local name=$1
    local command=$2
    local cases=$3
    local expected_summary=$4
    local suite_status
    local actual_summary

    selected=$((selected + 1))
    total_suites=$((total_suites + 1))
    total_cases=$((total_cases + cases))
    "$repository_root/Tools/Native/$command.sh" > "$suite_output" 2> "$suite_error"
    suite_status=$?
    if ((suite_status != 0)); then
        echo "FAIL  suite $name: native command exited $suite_status" >&2
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
    echo "PASS  suite $name cases=$cases"
}

IFS= read -r header < "$plan"
if [[ $header != 'windvale-native-retirement-suite 1' ]]; then
    echo 'Native retirement suite header differs' >&2
    exit 1
fi

while IFS='|' read -r name command cases expected_summary; do
    if [[ -n $filter && $filter != "$name" ]]; then
        continue
    fi
    if ! run_suite "$name" "$command" "$cases" "$expected_summary"; then
        echo "Suites: $total_suites, Passed: $passed_suites, Failed: $((total_suites - passed_suites)), Cases: $total_cases" >&2
        exit 1
    fi
done < <(tail -n +2 -- "$plan")

if [[ -n $filter && $selected -eq 0 ]]; then
    echo "Unknown native retirement suite: $filter" >&2
    exit 64
fi

echo "Suites: $total_suites, Passed: $passed_suites, Failed: 0, Cases: $total_cases"
