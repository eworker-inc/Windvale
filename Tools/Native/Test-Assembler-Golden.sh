#!/usr/bin/env bash
set -uo pipefail

if [[ $# -ne 0 ]]; then
    echo 'Usage: ./Tools/Native/Test-Assembler-Golden.sh' >&2
    exit 64
fi

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
temporary_root=${TMPDIR:-/tmp}
temporary_directory=$(mktemp -d "$temporary_root/windvale-assembler-golden.XXXXXXXX") || exit 1
first="$temporary_directory/First.wvo"
second="$temporary_directory/Second.wvo"
command_output="$temporary_directory/Command.out"
command_error="$temporary_directory/Command.err"
expected_output="$temporary_directory/Expected.out"
verify_output="$temporary_directory/Verify.out"
verify_error="$temporary_directory/Verify.err"

cleanup() {
    case "$temporary_directory" in
        "$temporary_root"/windvale-assembler-golden.*)
            rm -f -- "$first" "$second" "$command_output" "$command_error" \
                "$expected_output" "$verify_output" "$verify_error"
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

total=0
passed=0
run_case() {
    local name=$1
    local relative_input=$2
    local input_digest=$3
    local output_bytes=$4
    local output_digest=$5
    local expected_report=$6
    local input="$repository_root/$relative_input"

    total=$((total + 1))
    if ! check_hash "$input" "$input_digest"; then
        echo "FAIL  $name: WVA input identity differs" >&2
        return 1
    fi

    if ! "$repository_root/Tools/Native/Assemble-Wva.sh" "$input" "$first" \
        > "$command_output" 2> "$command_error"; then
        echo "FAIL  $name: first native assembly failed" >&2
        cat -- "$command_error" >&2
        return 1
    fi
    printf 'wvasm 1\n%s\n' "$expected_report" > "$expected_output"
    if [[ -s $command_error ]] || ! cmp --silent -- "$expected_output" "$command_output"; then
        echo "FAIL  $name: first assembly report differs" >&2
        cat -- "$command_output" "$command_error" >&2
        return 1
    fi
    if [[ $(wc -c < "$first") -ne $output_bytes ]] ||
        ! check_hash "$first" "$output_digest"; then
        echo "FAIL  $name: first object identity differs" >&2
        return 1
    fi

    if ! "$repository_root/Tools/Native/Verify-Wvo.sh" "$first" \
        > "$verify_output" 2> "$verify_error"; then
        echo "FAIL  $name: native WVO verification failed" >&2
        cat -- "$verify_error" >&2
        return 1
    fi
    if [[ -s $verify_error ]]; then
        echo "FAIL  $name: valid WVO wrote a diagnostic" >&2
        cat -- "$verify_error" >&2
        return 1
    fi

    if ! "$repository_root/Tools/Native/Assemble-Wva.sh" "$input" "$second" \
        > "$command_output" 2> "$command_error"; then
        echo "FAIL  $name: repeated native assembly failed" >&2
        cat -- "$command_error" >&2
        return 1
    fi
    if [[ -s $command_error ]] || ! cmp --silent -- "$expected_output" "$command_output"; then
        echo "FAIL  $name: repeated assembly report differs" >&2
        cat -- "$command_output" "$command_error" >&2
        return 1
    fi
    if [[ $(wc -c < "$second") -ne $output_bytes ]] ||
        ! check_hash "$second" "$output_digest" ||
        ! cmp --silent -- "$first" "$second"; then
        echo "FAIL  $name: repeated native object differs" >&2
        return 1
    fi

    rm -f -- "$first" "$second" "$command_output" "$command_error" \
        "$expected_output" "$verify_output" "$verify_error"
    passed=$((passed + 1))
    echo "PASS  $name"
}

run_case hello-object Examples/Assembler/Hello-Object.wva \
    a88f748ba87df1a291752ee8bda896279edd8d9f8a7811692c2229bbaba8cea0 \
    218 992c298a4f9b68dec27b7203a2770f2a37ef2016ea45e88d33ee21994060fe85 \
    'assembly status=valid object-bytes=218 sections=2 symbols=3 relocations=2 offset=403 line=22 column=1' || exit 1
run_case expanded-x64 Examples/Assembler/Expanded-X64.wva \
    27a324b5c26c1e6a982c6f02b0a157ccfdcbb7500521dd8c95a381aa2ed20646 \
    238 678551e9936ca1c901e2dc5ec129d2add73427edb1ea3d086bb4badbf1b6e4ad \
    'assembly status=valid object-bytes=238 sections=2 symbols=2 relocations=1 offset=740 line=35 column=1' || exit 1
run_case scalar-x64 Examples/Assembler/Scalar-X64.wva \
    e76cb94b82857e097e734f6bdf01b3383487fd8a69f05214d74a1b69e261ae0e \
    199 e1cce07329b6183ebae26ebe252be7d2e754c4aeea08ffe6452c74d60d6ea64a \
    'assembly status=valid object-bytes=199 sections=1 symbols=1 relocations=0 offset=639 line=29 column=1' || exit 1
run_case typed-scalar-x64 Examples/Assembler/Typed-Scalar-X64.wva \
    a66a36a06ac6375da7ed5287fe6fdae55901f5b8b236c3098723e7a6f856a4ef \
    396 860680074517025c69a2a6edf1dd9ff196475e05f9c50f95b53480c848c650c5 \
    'assembly status=valid object-bytes=396 sections=2 symbols=2 relocations=5 offset=942 line=52 column=1' || exit 1

echo "Tests: $total, Passed: $passed, Failed: 0"
