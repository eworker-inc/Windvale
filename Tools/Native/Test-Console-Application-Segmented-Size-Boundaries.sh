#!/usr/bin/env bash
set -uo pipefail

if [[ $# -ne 0 ]]; then
    echo 'Usage: ./Tools/Native/Test-Console-Application-Segmented-Size-Boundaries.sh' >&2
    exit 64
fi

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
temporary_root=${TMPDIR:-/tmp}
temporary_directory=$(mktemp -d "$temporary_root/windvale-console-segmented-size.XXXXXXXX") || exit 1
corpus_directory="$temporary_directory/Corpus"
mkdir -- "$corpus_directory" || exit 1
archive="$temporary_directory/Corpus.tar.gz"
manifest="$corpus_directory/Manifest.txt"
run_output="$temporary_directory/Run.out"
run_error="$temporary_directory/Run.err"
decode_error="$temporary_directory/Decode.err"
extract_output="$temporary_directory/Extract.out"
extract_error="$temporary_directory/Extract.err"
archive_digest=d0e9aa4f6e31d3bd28fb0468606f43b275c320adb470e4d3b78034d440573200
manifest_digest=50c1c87ac9dcaaccbd5036c2d67677dde044a6b24f11fe78149784741c72ca29
total=0
passed=0

cleanup() {
    case "$temporary_directory" in
        "$temporary_root"/windvale-console-segmented-size.*)
            rm -f -- \
                "$corpus_directory/Manifest.txt" \
                "$corpus_directory/Windows-First.bin" \
                "$corpus_directory/Windows-Second.bin" \
                "$corpus_directory/Linux-First.bin" \
                "$corpus_directory/Linux-Second.bin"
            rmdir -- "$corpus_directory"
            rm -f -- "$archive" "$run_output" "$run_error" \
                "$decode_error" "$extract_output" "$extract_error"
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
    echo "FAIL  console-segmented-size: $1" >&2
    echo "Tests: $total, Passed: $passed, Failed: $((total - passed))" >&2
    exit 1
}

run_case() {
    local case_name=$1
    local platform=$2
    local stage0=$3
    local first_name=$4
    local first_bytes=$5
    local first_digest=$6
    local second_name=$7
    local second_bytes=$8
    local second_digest=$9
    local report_digest
    case "$case_name:$platform:$stage0:$first_name:$first_bytes:$second_name:$second_bytes" in
        windows-max-plus-one:windows:WVW2001:Windows-First.bin:4194304:Windows-Second.bin:2049)
            report_digest=d0b1304c62778d71c7df11b2c9d3759139810b0acca3115e77bb44aae1b052ba
            ;;
        linux-max-plus-one:linux:WVL2001:Linux-First.bin:4194304:Linux-Second.bin:8305)
            report_digest=9b8b2d84bdb475db94d5a0e1be47a73f12d9663e966c2c8708ce4f556aacb1d2
            ;;
        *) fail "$case_name manifest row differs";;
    esac
    local first="$corpus_directory/$first_name"
    local second="$corpus_directory/$second_name"
    local actual_first_bytes
    local actual_second_bytes
    local run_status
    actual_first_bytes=$(wc -c < "$first") || return 1
    actual_second_bytes=$(wc -c < "$second") || return 1
    actual_first_bytes=${actual_first_bytes//[[:space:]]/}
    actual_second_bytes=${actual_second_bytes//[[:space:]]/}
    [[ $actual_first_bytes == "$first_bytes" ]] || return 1
    [[ $actual_second_bytes == "$second_bytes" ]] || return 1
    check_hash "$first" "$first_digest" || return 1
    check_hash "$second" "$second_digest" || return 1

    total=$((total + 1))
    "$repository_root/Tools/Native/Verify-Console-Segmented.sh" \
        "$first" "$second" > "$run_output" 2> "$run_error"
    run_status=$?
    ((run_status == 1)) || return 1
    [[ ! -s $run_output ]] || return 1
    check_hash "$run_error" "$report_digest" || return 1
    check_hash "$first" "$first_digest" || return 1
    check_hash "$second" "$second_digest" || return 1
    passed=$((passed + 1))
    echo "PASS  $case_name oracle=$stage0"
}

if ! base64 --decode \
    "$repository_root/Tests/Native/Console-Application-Segmented-Size-Boundaries/Corpus.tar.gz.b64" \
    > "$archive" 2> "$decode_error"; then
    fail 'corpus could not be decoded'
fi
[[ ! -s $decode_error ]] || fail 'decoder wrote a diagnostic'
check_hash "$archive" "$archive_digest" || fail 'archive identity differs'
if ! tar -xzf "$archive" -C "$corpus_directory" \
    > "$extract_output" 2> "$extract_error"; then
    fail 'archive could not be extracted'
fi
[[ ! -s $extract_output && ! -s $extract_error ]] || fail 'extractor wrote output'
check_hash "$manifest" "$manifest_digest" || fail 'manifest identity differs'
IFS= read -r header < "$manifest"
[[ $header == 'windvale-console-application-segmented-size-boundaries 1' ]] ||
    fail 'manifest header differs'

while IFS='|' read -r case_name platform stage0 first_name first_bytes first_digest \
    second_name second_bytes second_digest; do
    run_case "$case_name" "$platform" "$stage0" \
        "$first_name" "$first_bytes" "$first_digest" \
        "$second_name" "$second_bytes" "$second_digest" ||
        fail "$case_name failed"
done < <(tail -n +3 -- "$manifest")

((total == 2)) || fail 'total case count differs'
echo "Tests: $total, Passed: $passed, Failed: 0"
