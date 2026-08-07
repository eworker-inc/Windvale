#!/usr/bin/env bash
set -uo pipefail

if [[ $# -ne 0 ]]; then
    echo 'Usage: ./Tools/Native/Test-Console-Application-Segmented-Construction.sh' >&2
    exit 64
fi

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
temporary_directory=$(mktemp -d "${TMPDIR:-/tmp}/windvale-console-segmented-construction.XXXXXXXX") || exit 1
corpus_directory="$temporary_directory/Corpus"
mkdir -- "$corpus_directory" || exit 1
archive="$temporary_directory/Corpus.tar.gz"
manifest="$corpus_directory/Manifest.txt"
native_image="$corpus_directory/Maximum-Native.bin"
run_output="$temporary_directory/Run.out"
run_error="$temporary_directory/Run.err"
verify_output="$temporary_directory/Verify.out"
verify_error="$temporary_directory/Verify.err"
archive_digest='3363b3edc5c05f6665566f236793761cf9f7dd03aacfb29334f1535bcfcba7c9'
manifest_digest='27cd7d83d6c44a5b53c26c6b732523a46036a76e1be78f6b0ae590d6f873b005'
native_digest='25711ae262e606e61654606b563aa7cdc93bb5288558bba0b3e533ab6eab238c'
total=0
passed=0

cleanup() {
    rm -f -- "$corpus_directory/Manifest.txt" "$corpus_directory/Maximum-Native.bin"
    rmdir -- "$corpus_directory" 2>/dev/null || true
    rm -f -- "$archive" "$run_output" "$run_error" "$verify_output" "$verify_error" \
        "$temporary_directory"/*.chunk-0 "$temporary_directory"/*.chunk-1 \
        "$temporary_directory"/*.wvcs "$temporary_directory"/*.application
    rmdir -- "$temporary_directory" 2>/dev/null || true
}
trap cleanup EXIT

fail() {
    echo "FAIL  console-segmented-construction: $1" >&2
    echo "Tests: $total, Passed: $passed, Failed: $((total - passed))" >&2
    exit 1
}

check_hash() {
    local path=$1
    local expected=$2
    local description=$3
    local actual
    actual=$(sha256sum --binary -- "$path" | cut -d' ' -f1) || fail "$description could not be hashed"
    [[ $actual == "$expected" ]] || fail "$description differs; expected $expected, actual $actual"
}

base64 -d -- "$repository_root/Tests/Native/Console-Application-Segmented-Construction/Corpus.tar.gz.b64" > "$archive" || fail 'corpus could not be decoded'
check_hash "$archive" "$archive_digest" 'archive identity'
tar -xzf "$archive" -C "$corpus_directory" || fail 'archive could not be extracted'
check_hash "$manifest" "$manifest_digest" 'manifest identity'
check_hash "$native_image" "$native_digest" 'maximum native-image identity'
[[ $(wc -c < "$native_image") -eq 4194304 ]] || fail 'maximum native-image size'
IFS= read -r header < "$manifest" || fail 'manifest header could not be read'
[[ $header == 'windvale-console-application-segmented-construction 1' ]] || fail 'manifest header'

run_case() {
    local case_name=$1
    local target=$2
    local application_bytes=$3
    local application_digest=$4
    local first_digest=$5
    local second_bytes=$6
    local second_digest=$7
    local staging_digest=$8
    local package_report_digest=$9
    local verify_report_digest=${10}
    local chunk_prefix="$temporary_directory/$case_name"
    local first="$chunk_prefix.chunk-0"
    local second="$chunk_prefix.chunk-1"
    local staging="$temporary_directory/$case_name.wvcs"
    local joined="$temporary_directory/$case_name.application"

    total=$((total + 1))
    "$script_directory/Stage-Console-Segmented.sh" \
        "$target" "$native_image" 4194303 "$chunk_prefix" "$staging" \
        > "$run_output" 2> "$run_error" || {
            cat "$run_error" >&2
            fail "$case_name segmented constructor exit"
        }
    [[ ! -s $run_error ]] || fail "$case_name segmented constructor diagnostic"
    check_hash "$run_output" "$package_report_digest" "$case_name package report"
    [[ $(wc -c < "$first") -eq 4194304 ]] || fail "$case_name first-chunk size"
    [[ $(wc -c < "$second") -eq "$second_bytes" ]] || fail "$case_name second-chunk size"
    [[ $(wc -c < "$staging") -eq 60 ]] || fail "$case_name staging-manifest size"
    check_hash "$first" "$first_digest" "$case_name first chunk"
    check_hash "$second" "$second_digest" "$case_name second chunk"
    check_hash "$staging" "$staging_digest" "$case_name staging manifest"
    cat -- "$first" "$second" > "$joined" || fail "$case_name application join"
    [[ $(wc -c < "$joined") -eq "$application_bytes" ]] || fail "$case_name application size"
    check_hash "$joined" "$application_digest" "$case_name Stage 0 application identity"

    "$script_directory/Verify-Console-Segmented.sh" "$first" "$second" \
        > "$verify_output" 2> "$verify_error" || {
            cat "$verify_error" >&2
            fail "$case_name segmented verification exit"
        }
    [[ ! -s $verify_error ]] || fail "$case_name segmented verification diagnostic"
    check_hash "$verify_output" "$verify_report_digest" "$case_name verification report"
    check_hash "$native_image" "$native_digest" "$case_name native-image preservation"
    passed=$((passed + 1))
    echo "PASS  $case_name"
    rm -f -- "$first" "$second" "$staging" "$joined" \
        "$run_output" "$run_error" "$verify_output" "$verify_error"
}

run_case \
    windows-maximum windows-x64-console-v1 4196352 \
    9cf6ab6650778969c97fad9e149a58d19de8334b806a6375ccc7150c3ad7091c \
    355595cad76cd8bf27cb4e8a0435ff85dadf3aa6a7afd642a2a9ca992de5522c \
    2048 2a34c2aac9cafc66984ca2407a4ad46652dd0a123f3cc6e28b609e0ea05c56f3 \
    18f9b4cab9be796da23c9b686e139f031a7ebc51a44ca299cbb0f7ec09c55a26 \
    53f0150046c8049298d59c3929a9015607e9c001d93f574fa647aa608b22c421 \
    3e771b72b5431a75e3f13de2504b91d48e7280ded0e8bbe601a13b0746ef2dd1
run_case \
    linux-maximum linux-x64-console-v1 4202608 \
    7b5eb125ce971b53071be80c3424a34436d082b806918fd06690b32e86e87d3a \
    ad83d04b438b4acfea880214a031a78490d0c06da67e72a64fea8105b03a3234 \
    8304 df08c9de1b2c12007861f7cddc1e5d28a02b188c6cf41a15dab77f3b25dd780b \
    632f4cfcc240c19f5009385eaf1bfe8e66c1f648c2302c5ab25335f8331c0aeb \
    868efd57fde343176900f1de742f5f7de6da8d3690b5511ba815937fa4ab9532 \
    03a0fd7f95baf46d78590ce3888cc29e80787f77553e00e790ac77bf8dafdd15

[[ $total -eq 2 && $passed -eq 2 ]] || fail 'case count'
echo 'Tests: 2, Passed: 2, Failed: 0'
