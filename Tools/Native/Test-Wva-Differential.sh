#!/usr/bin/env bash
set -uo pipefail

mode=all
if [[ $# -eq 1 && $1 == --positive-only ]]; then
    mode=positive
elif [[ $# -ne 0 ]]; then
    echo 'Usage: ./Tools/Native/Test-Wva-Differential.sh [--positive-only]' >&2
    exit 64
fi

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
temporary_root=${TMPDIR:-/tmp}
temporary_directory=$(mktemp -d "$temporary_root/windvale-wva-differential.XXXXXXXX") || exit 1
corpus_directory="$temporary_directory/Corpus"
mkdir -- "$corpus_directory" || exit 1
positive_directory="$temporary_directory/Positive"
mkdir -- "$positive_directory" || exit 1

archive="$temporary_directory/Corpus.tar.gz"
manifest="$corpus_directory/Manifest.txt"
positive_archive="$temporary_directory/Positive-Corpus.tar.gz"
positive_manifest="$positive_directory/Manifest.txt"
destination="$temporary_directory/Destination.wvo"
sentinel="$temporary_directory/Sentinel.wvo"
run_output="$temporary_directory/Run.out"
run_error="$temporary_directory/Run.err"
verify_output="$temporary_directory/Verify.out"
verify_error="$temporary_directory/Verify.err"
decode_error="$temporary_directory/Decode.err"
extract_output="$temporary_directory/Extract.out"
extract_error="$temporary_directory/Extract.err"
archive_digest=b9a076cf9416488d733ed4c4887c052e61548acb45574256cd3c65d94da31970
manifest_digest=50153c0f7a6e9b596f3a7e0c4ce5bc1c6f240b01ce8657d99c5775a61d9391e4
positive_archive_digest=c17bb829636608f8d38b983d5d5979f64c24bfc4b9b3a4d753fdf1620425aaab
positive_manifest_digest=fdf5c5e63cf323fee11a4ac08e0786e5167acbfa8a63e1fc245659936026fde2
sentinel_digest=0d1829bbbc77f3ee3910a70f98528e1078117480332adb5a2d09df8b2d25f3b5
assembly_report_digest=4713cc6a74e88cab45421a8bed22b4c72de19fb330f77212a8193aa0e1224c73
verify_report_digest=4a31e8a0ea20ff90039366745ec6df8ce8abe87361395c0643c95b72a054e4e7
total=0
passed=0
accepted_cases=0
rejected_cases=0
assignment_1=0
assignment_2=0
assignment_3=0
assignment_4=0

cleanup() {
    case "$temporary_directory" in
        "$temporary_root"/windvale-wva-differential.*)
            rm -f -- "$corpus_directory"/Case-???.wva "$manifest"
            rmdir -- "$corpus_directory"
            rm -f -- "$positive_directory"/*.wva "$positive_manifest"
            rmdir -- "$positive_directory"
            rm -f -- \
                "$archive" \
                "$positive_archive" \
                "$destination" \
                "$sentinel" \
                "$run_output" \
                "$run_error" \
                "$verify_output" \
                "$verify_error" \
                "$decode_error" \
                "$extract_output" \
                "$extract_error"
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

fail() {
    echo "FAIL  wva-differential: $1" >&2
    echo "Tests: $total, Passed: $passed, Failed: $((total - passed))" >&2
    exit 1
}

decode_fixture() {
    if ! base64 --decode "$1" > "$2" 2> "$decode_error"; then
        fail 'fixture could not be decoded'
    fi
    if [[ -s $decode_error ]]; then
        cat -- "$decode_error" >&2
        fail 'decoder wrote a diagnostic'
    fi
    check_hash "$2" "$3" || fail 'decoded fixture identity differs'
}

check_rejection_report() {
    local report=$1
    local code=$2
    local lines
    local line
    lines=$(awk 'NF { count++ } END { print count + 0 }' "$report")
    if [[ $lines != 1 ]]; then
        return 1
    fi
    IFS= read -r line < "$report"
    [[ $line == "assembly status=$code "* ]]
}

record_assignment() {
    case "$1" in
        1) assignment_1=$((assignment_1 + 1)) ;;
        2) assignment_2=$((assignment_2 + 1)) ;;
        3) assignment_3=$((assignment_3 + 1)) ;;
        4) assignment_4=$((assignment_4 + 1)) ;;
        *) return 1 ;;
    esac
}

run_case() {
    local name=$1
    local size=$2
    local source_digest=$3
    local outcome=$4
    local oracle_code=$5
    local object_size=$6
    local object_digest=$7
    local report_digest=$8
    local verification_digest=$9
    local input="$active_corpus_directory/$name"
    local actual_size
    local run_status

    if [[ $name != "${name##*/}" || $name == *\\* ]]; then
        echo "FAIL  $name: corpus input name is not a filename" >&2
        return 1
    fi
    total=$((total + 1))
    actual_size=$(wc -c < "$input") || return 1
    actual_size=${actual_size//[[:space:]]/}
    if [[ $actual_size != "$size" ]]; then
        echo "FAIL  $name: input size differs" >&2
        return 1
    fi
    check_hash "$input" "$source_digest" || {
        echo "FAIL  $name: input identity differs" >&2
        return 1
    }
    cp -- "$sentinel" "$destination" || return 1

    "$repository_root/Tools/Native/Assemble-Wva.sh" \
        "$input" "$destination" > "$run_output" 2> "$run_error"
    run_status=$?
    case "$outcome" in
        accepted)
            if ((run_status != 0)); then
                echo "FAIL  $name: native assembler rejected an oracle-accepted input" >&2
                return 1
            fi
            if [[ -s $run_error ]]; then
                echo "FAIL  $name: accepted input wrote a diagnostic" >&2
                cat -- "$run_error" >&2
                return 1
            fi
            check_hash "$run_output" "$report_digest" || {
                echo "FAIL  $name: accepted report differs" >&2
                return 1
            }
            actual_size=$(wc -c < "$destination") || return 1
            actual_size=${actual_size//[[:space:]]/}
            if [[ $actual_size != "$object_size" ]]; then
                echo "FAIL  $name: accepted object size differs" >&2
                return 1
            fi
            check_hash "$destination" "$object_digest" || {
                echo "FAIL  $name: accepted object identity differs" >&2
                return 1
            }
            "$repository_root/Tools/Native/Verify-Wvo.sh" \
                "$destination" > "$verify_output" 2> "$verify_error"
            if (($? != 0)); then
                echo "FAIL  $name: accepted object failed native verification" >&2
                cat -- "$verify_error" >&2
                return 1
            fi
            if [[ -s $verify_error ]]; then
                echo "FAIL  $name: native object verification wrote a diagnostic" >&2
                cat -- "$verify_error" >&2
                return 1
            fi
            check_hash "$verify_output" "$verification_digest" || {
                echo "FAIL  $name: native object verification report differs" >&2
                return 1
            }
            accepted_cases=$((accepted_cases + 1))
            ;;
        rejected)
            if ((run_status != 2)); then
                echo "FAIL  $name: native assembler accepted an oracle-rejected input" >&2
                return 1
            fi
            if [[ -s $run_output ]]; then
                echo "FAIL  $name: rejected input wrote standard output" >&2
                cat -- "$run_output" >&2
                return 1
            fi
            if ! check_rejection_report "$run_error" "$oracle_code"; then
                echo "FAIL  $name: rejected report code differs" >&2
                cat -- "$run_error" >&2
                return 1
            fi
            check_hash "$destination" "$sentinel_digest" || {
                echo "FAIL  $name: rejected assembly changed the destination" >&2
                return 1
            }
            rejected_cases=$((rejected_cases + 1))
            ;;
        *)
            echo "FAIL  $name: oracle outcome differs" >&2
            return 1
            ;;
    esac
    check_hash "$input" "$source_digest" || {
        echo "FAIL  $name: native assembler changed the input" >&2
        return 1
    }
    rm -f -- \
        "$destination" "$run_output" "$run_error" "$verify_output" "$verify_error"
    passed=$((passed + 1))
    echo "PASS  $name oracle=$outcome"
}

decode_fixture \
    "$repository_root/Tests/Native/Wvo/Return-42.wvo.b64" \
    "$sentinel" \
    "$sentinel_digest"

if [[ $mode == all ]]; then
    decode_fixture \
        "$repository_root/Tests/Native/Wva-Differential/Corpus.tar.gz.b64" \
        "$archive" \
        "$archive_digest"

    if ! tar -xzf "$archive" -C "$corpus_directory" \
        > "$extract_output" 2> "$extract_error"; then
        cat -- "$extract_error" >&2
        fail 'corpus archive could not be extracted'
    fi
    if [[ -s $extract_output || -s $extract_error ]]; then
        cat -- "$extract_output" "$extract_error" >&2
        fail 'corpus extractor wrote output'
    fi
    check_hash "$manifest" "$manifest_digest" || fail 'manifest identity differs'

    IFS= read -r header < "$manifest"
    if [[ $header != 'windvale-wva-differential-corpus 1' ]]; then
        fail 'manifest header differs'
    fi

    active_corpus_directory=$corpus_directory
    while IFS='|' read -r name case_number assignment_count operations source_size source_digest outcome oracle_code oracle_line oracle_column object_size object_digest sections symbols relocations; do
        record_assignment "$assignment_count" || fail "$name assignment count differs"
        run_case \
            "$name" "$source_size" "$source_digest" "$outcome" "$oracle_code" \
            "$object_size" "$object_digest" "$assembly_report_digest" \
            "$verify_report_digest" || fail "$name failed"
    done < <(tail -n +3 -- "$manifest")

    if ((total != 200)); then
        fail 'total case count differs'
    fi
    if ((accepted_cases != 1)); then
        fail 'accepted case count differs'
    fi
    if ((rejected_cases != 199)); then
        fail 'rejected case count differs'
    fi
    if ((assignment_1 != 58)); then
        fail 'one-assignment case count differs'
    fi
    if ((assignment_2 != 45)); then
        fail 'two-assignment case count differs'
    fi
    if ((assignment_3 != 50)); then
        fail 'three-assignment case count differs'
    fi
    if ((assignment_4 != 47)); then
        fail 'four-assignment case count differs'
    fi
fi

decode_fixture \
    "$repository_root/Tests/Native/Wva-Differential/Positive-Corpus.tar.gz.b64" \
    "$positive_archive" \
    "$positive_archive_digest"
if ! tar -xzf "$positive_archive" -C "$positive_directory" \
    > "$extract_output" 2> "$extract_error"; then
    cat -- "$extract_error" >&2
    fail 'positive corpus archive could not be extracted'
fi
if [[ -s $extract_output || -s $extract_error ]]; then
    cat -- "$extract_output" "$extract_error" >&2
    fail 'positive corpus extractor wrote output'
fi
check_hash "$positive_manifest" "$positive_manifest_digest" || \
    fail 'positive manifest identity differs'

IFS= read -r positive_header < "$positive_manifest"
if [[ $positive_header != 'windvale-wva-positive-corpus 2' ]]; then
    fail 'positive manifest header differs'
fi

active_corpus_directory=$positive_directory
while IFS='|' read -r name source_size source_digest object_size object_digest sections symbols relocations report_digest verification_digest; do
    run_case \
        "$name" "$source_size" "$source_digest" accepted - \
        "$object_size" "$object_digest" "$report_digest" \
        "$verification_digest" || fail "$name failed"
done < <(tail -n +3 -- "$positive_manifest")

if [[ $mode == positive ]]; then
    if ((total != 69 || accepted_cases != 69 || rejected_cases != 0)); then
        fail 'positive case counts differ'
    fi
elif ((total != 269 || accepted_cases != 70 || rejected_cases != 199)); then
    fail 'positive case counts differ'
fi

echo "Tests: $total, Passed: $passed, Failed: 0"
