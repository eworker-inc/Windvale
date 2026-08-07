#!/usr/bin/env bash
set -uo pipefail

if [[ $# -ne 0 ]]; then
    echo 'Usage: ./Tools/Native/Test-Wvo-Differential.sh' >&2
    exit 64
fi

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
temporary_root=${TMPDIR:-/tmp}
temporary_directory=$(mktemp -d "$temporary_root/windvale-wvo-differential.XXXXXXXX") || exit 1
corpus_directory="$temporary_directory/Corpus"
mkdir -- "$corpus_directory" || exit 1

archive="$temporary_directory/Corpus.tar.gz"
manifest="$corpus_directory/Manifest.txt"
run_output="$temporary_directory/Run.out"
run_error="$temporary_directory/Run.err"
decode_error="$temporary_directory/Decode.err"
extract_output="$temporary_directory/Extract.out"
extract_error="$temporary_directory/Extract.err"
archive_digest=74d90d981ef3665eee2fb16a5abb57ae2e9d308a8e56b1aff56c49d97997d684
manifest_digest=ef6a187dfc5d0bbffcfb61df40146af54f74d76302dee1358b4a3fbefd7aa556
total=0
passed=0
mutation_cases=0
random_cases=0
accepted_cases=0
rejected_cases=0

cleanup() {
    case "$temporary_directory" in
        "$temporary_root"/windvale-wvo-differential.*)
            rm -f -- \
                "$corpus_directory"/Mutation-???.wvo \
                "$corpus_directory"/Random-???.wvo \
                "$manifest"
            rmdir -- "$corpus_directory"
            rm -f -- \
                "$archive" \
                "$run_output" \
                "$run_error" \
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
    echo "FAIL  wvo-differential: $1" >&2
    echo "Tests: $total, Passed: $passed, Failed: $((total - passed))" >&2
    exit 1
}

decode_fixture() {
    if ! base64 --decode "$1" > "$2" 2> "$decode_error"; then
        fail 'corpus archive could not be decoded'
    fi
    if [[ -s $decode_error ]]; then
        cat -- "$decode_error" >&2
        fail 'decoder wrote a diagnostic'
    fi
    check_hash "$2" "$3" || fail 'archive identity differs'
}

check_rejection_report() {
    local report=$1
    local lines
    local line
    lines=$(awk 'NF { count++ } END { print count + 0 }' "$report")
    if [[ $lines != 1 ]]; then
        return 1
    fi
    IFS= read -r line < "$report"
    [[ $line == 'object status='* ]]
}

run_case() {
    local name=$1
    local family=$2
    local case_number=$3
    local detail=$4
    local size=$5
    local digest=$6
    local outcome=$7
    local oracle_code=$8
    local oracle_offset=$9
    local report_digest=${10}
    local input="$corpus_directory/$name"
    local actual_size
    local run_status

    if [[ $name != "${name##*/}" || $name == *\\* ]]; then
        echo "FAIL  $name: corpus input name is not a filename" >&2
        return 1
    fi
    case "$family:$name" in
        mutation:Mutation-???.wvo)
            mutation_cases=$((mutation_cases + 1))
            ;;
        random:Random-???.wvo)
            random_cases=$((random_cases + 1))
            ;;
        *)
            echo "FAIL  $name: corpus family or filename differs" >&2
            return 1
            ;;
    esac

    total=$((total + 1))
    actual_size=$(wc -c < "$input") || return 1
    actual_size=${actual_size//[[:space:]]/}
    if [[ $actual_size != "$size" ]]; then
        echo "FAIL  $name: input size differs" >&2
        return 1
    fi
    check_hash "$input" "$digest" || {
        echo "FAIL  $name: input identity differs" >&2
        return 1
    }

    "$repository_root/Tools/Native/Verify-Wvo.sh" \
        "$input" > "$run_output" 2> "$run_error"
    run_status=$?
    case "$outcome" in
        accepted)
            if ((run_status != 0)); then
                echo "FAIL  $name: native verifier rejected an oracle-accepted input" >&2
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
            accepted_cases=$((accepted_cases + 1))
            ;;
        rejected)
            if ((run_status != 2)); then
                echo "FAIL  $name: native verifier accepted an oracle-rejected input" >&2
                return 1
            fi
            if [[ -s $run_output ]]; then
                echo "FAIL  $name: rejected input wrote standard output" >&2
                cat -- "$run_output" >&2
                return 1
            fi
            if ! check_rejection_report "$run_error"; then
                echo "FAIL  $name: rejected report shape differs" >&2
                cat -- "$run_error" >&2
                return 1
            fi
            rejected_cases=$((rejected_cases + 1))
            ;;
        *)
            echo "FAIL  $name: oracle outcome differs" >&2
            return 1
            ;;
    esac
    check_hash "$input" "$digest" || {
        echo "FAIL  $name: native verifier changed the input" >&2
        return 1
    }
    rm -f -- "$run_output" "$run_error"
    passed=$((passed + 1))
    echo "PASS  $name oracle=$outcome"
}

decode_fixture \
    "$repository_root/Tests/Native/Wvo-Differential/Corpus.tar.gz.b64" \
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
if [[ $header != 'windvale-wvo-differential-corpus 1' ]]; then
    fail 'manifest header differs'
fi

while IFS='|' read -r name family case_number detail size digest outcome oracle_code oracle_offset report_digest; do
    run_case \
        "$name" "$family" "$case_number" "$detail" "$size" "$digest" \
        "$outcome" "$oracle_code" "$oracle_offset" "$report_digest" || \
        fail "$name failed"
done < <(tail -n +2 -- "$manifest")

if ((total != 256)); then
    fail 'total case count differs'
fi
if ((mutation_cases != 128)); then
    fail 'mutation case count differs'
fi
if ((random_cases != 128)); then
    fail 'random case count differs'
fi
if ((accepted_cases != 32)); then
    fail 'accepted case count differs'
fi
if ((rejected_cases != 224)); then
    fail 'rejected case count differs'
fi

echo "Tests: $total, Passed: $passed, Failed: 0"
