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
                echo 'Usage: ./Tools/Native/Test-Verification-Owners.sh [--filter <owner-name>|--shard <1-4>]' >&2
                exit 64
                ;;
        esac
        ;;
    *)
        echo 'Usage: ./Tools/Native/Test-Verification-Owners.sh [--filter <owner-name>|--shard <1-4>]' >&2
        exit 64
        ;;
esac

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
plan="$repository_root/Tests/Native/Verification-Owners.txt"
plan_bytes=22307
plan_digest=fbe4e17c1efa3edbf1ea0c39b06521dc830ffcc9dd55d5079d8518cd73e4fea2
plan_owners=125
plan_cases=5937
plan_shard_1_owners=1
plan_shard_1_cases=57
plan_shard_2_owners=45
plan_shard_2_cases=2848
plan_shard_3_owners=38
plan_shard_3_cases=1783
plan_shard_4_owners=41
plan_shard_4_cases=1249

check_hash() {
    local path=$1
    local digest=$2
    local directory
    directory=$(CDPATH= cd -- "$(dirname -- "$path")" && pwd -P) || return 1
    (cd -- "$directory" && printf '%s  %s\n' \
        "$digest" "$(basename -- "$path")" | sha256sum --check --strict --quiet)
}

if [[ $(wc -c < "$plan") -ne $plan_bytes ]] ||
    ! check_hash "$plan" "$plan_digest"; then
    echo 'Native verification owner plan identity differs' >&2
    exit 1
fi

actual_owners=0
actual_cases=0
actual_shard_1_owners=0
actual_shard_1_cases=0
actual_shard_2_owners=0
actual_shard_2_cases=0
actual_shard_3_owners=0
actual_shard_3_cases=0
actual_shard_4_owners=0
actual_shard_4_cases=0
while IFS='|' read -r name command cases suite_shard expected_summary; do
    owner="$repository_root/Tools/Native/$command.sh"
    if [[ ! -x $owner ]]; then
        echo "Native verification owner is missing or not executable: $owner" >&2
        exit 1
    fi
    case $suite_shard in
        1|2|3|4) ;;
        *)
            echo "Native qualification shard is invalid: $name=$suite_shard" >&2
            exit 1
            ;;
    esac
    actual_owners=$((actual_owners + 1))
    actual_cases=$((actual_cases + cases))
    owner_variable="actual_shard_${suite_shard}_owners"
    case_variable="actual_shard_${suite_shard}_cases"
    printf -v "$owner_variable" '%d' "$(( ${!owner_variable} + 1 ))"
    printf -v "$case_variable" '%d' "$(( ${!case_variable} + cases ))"
done < <(tail -n +2 -- "$plan")

if ((actual_owners != plan_owners || actual_cases != plan_cases)); then
    echo 'Native verification owner plan inventory differs' >&2
    exit 1
fi
for suite_shard in 1 2 3 4; do
    actual_owner_variable="actual_shard_${suite_shard}_owners"
    actual_case_variable="actual_shard_${suite_shard}_cases"
    plan_owner_variable="plan_shard_${suite_shard}_owners"
    plan_case_variable="plan_shard_${suite_shard}_cases"
    if (( ${!actual_owner_variable} != ${!plan_owner_variable} ||
        ${!actual_case_variable} != ${!plan_case_variable} )); then
        echo 'Native verification owner plan inventory differs' >&2
        exit 1
    fi
done

temporary_root=${TMPDIR:-/tmp}
temporary_directory=$(mktemp -d "$temporary_root/windvale-native-verification-owners.XXXXXXXX") || exit 1
suite_output="$temporary_directory/Suite.out"
suite_error="$temporary_directory/Suite.err"
cleanup() {
    case "$temporary_directory" in
        "$temporary_root"/windvale-native-verification-owners.*)
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
planned=0
total_suites=0
passed_suites=0
total_cases=0
total_start=$SECONDS

while IFS='|' read -r name _ _ suite_shard _; do
    if [[ -n $filter && $filter != "$name" ]]; then
        continue
    fi
    if [[ -n $shard && $shard != "$suite_shard" ]]; then
        continue
    fi
    planned=$((planned + 1))
done < <(tail -n +2 -- "$plan")

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
    echo "Progress: step=native-owner item=$selected/$planned owner=$name"
    node "$repository_root/Tools/Native/Stream-Verification-Owner.mjs" \
        "$suite_output" "$suite_error" \
        "$repository_root/Tools/Native/$command.sh"
    suite_status=$?
    local suite_elapsed_ms=$(((SECONDS - suite_start) * 1000))
    if ((suite_status != 0)); then
        echo "FAIL  suite $name: native command exited $suite_status elapsed-ms=$suite_elapsed_ms" >&2
        return 1
    fi
    if [[ -s $suite_error ]]; then
        echo "FAIL  suite $name: native command wrote standard error" >&2
        return 1
    fi
    actual_summary=$(awk 'NF { last = $0 } END { print last }' "$suite_output")
    if [[ $actual_summary != "$expected_summary" ]]; then
        echo "FAIL  suite $name: summary differs" >&2
        return 1
    fi
    rm -f -- "$suite_output" "$suite_error"
    passed_suites=$((passed_suites + 1))
    echo "PASS  suite $name cases=$cases elapsed-ms=$suite_elapsed_ms"
}

IFS= read -r header < "$plan"
if [[ $header != 'windvale-native-verification-owners 1' ]]; then
    echo 'Native verification owner plan header differs' >&2
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
        echo "Unknown native verification owner: $filter" >&2
    else
        echo "Empty native qualification shard: $shard" >&2
    fi
    exit 64
fi

echo "Timing: elapsed-ms=$(((SECONDS - total_start) * 1000))"
echo "Suites: $total_suites, Passed: $passed_suites, Failed: 0, Cases: $total_cases"
