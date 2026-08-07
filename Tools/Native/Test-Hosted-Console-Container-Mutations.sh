#!/usr/bin/env bash
set -uo pipefail

if [[ $# -ne 0 ]]; then
    echo 'Usage: ./Tools/Native/Test-Hosted-Console-Container-Mutations.sh' >&2
    exit 64
fi

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
temporary_root=${TMPDIR:-/tmp}
temporary_directory=$(mktemp -d "$temporary_root/windvale-hosted-console-mutations.XXXXXXXX") || exit 1
corpus_directory="$temporary_directory/Corpus"
mkdir -- "$corpus_directory" || exit 1

archive="$temporary_directory/Corpus.tar.gz"
manifest="$corpus_directory/Manifest.txt"
sentinel="$temporary_directory/Sentinel.bin"
destination=
run_output="$temporary_directory/Run.out"
run_error="$temporary_directory/Run.err"
decode_error="$temporary_directory/Decode.err"
extract_output="$temporary_directory/Extract.out"
extract_error="$temporary_directory/Extract.err"
archive_digest=a8027a9d4238767ae9b7ab18e3d0114da4e4fdf3edcbbc044d4358f2ce1fd055
manifest_digest=208a309624bef868b657cc87e2e95d6c085da1528bc5bc471226dc4b22c764f9
sentinel_digest=0d1829bbbc77f3ee3910a70f98528e1078117480332adb5a2d09df8b2d25f3b5
rejected_report_digest=39db034713225109f62c272db447d75cfe93ff0c259c8d9e5211f0df5c007e1f
windows_valid_report_digest=6eb507dd88b808f1a0b8fdc811da18bcfa2e6c5d18d56f8b1fb7a5cca33bff2d
linux_valid_report_digest=0e3fc5697dd9f6b882d0d4b7cc8c1d771a65789278a35f28ec7f3e729952f142
total=0
passed=0
windows_cases=0
linux_cases=0
valid_cases=0
rejected_cases=0
xor_cases=0
rehash_cases=0
truncate_cases=0
append_cases=0

cleanup() {
    case "$temporary_directory" in
        "$temporary_root"/windvale-hosted-console-mutations.*)
            rm -f -- \
                "$corpus_directory"/Windows-*.exe \
                "$corpus_directory"/Linux-*.elf \
                "$manifest"
            rmdir -- "$corpus_directory"
            rm -f -- \
                "$archive" "$sentinel" \
                "$temporary_directory/Destination.exe" \
                "$temporary_directory/Destination.elf" \
                "$run_output" "$run_error" "$decode_error" \
                "$extract_output" "$extract_error"
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
    echo "FAIL  hosted-console-mutations: $1" >&2
    echo "Tests: $total, Passed: $passed, Failed: $((total - passed))" >&2
    exit 1
}

decode_fixture() {
    local source=$1
    local output=$2
    local digest=$3
    local label=$4
    if ! base64 --decode "$source" > "$output" 2> "$decode_error"; then
        fail "$label could not be decoded"
    fi
    if [[ -s $decode_error ]]; then
        cat -- "$decode_error" >&2
        fail "$label decoder wrote a diagnostic"
    fi
    check_hash "$output" "$digest" || fail "$label identity differs"
}

run_case() {
    local name=$1
    local platform=$2
    local expectation=$3
    local stage0=$4
    local operation=$5
    local size=$6
    local digest=$7
    local input="$corpus_directory/$name"
    local valid_report_digest
    local actual_size
    local run_status

    if [[ $name != "${name##*/}" || $name == *\\* ]]; then
        echo "FAIL  $name: input name is not a filename" >&2
        return 1
    fi
    case "$platform:$name" in
        windows:Windows-*.exe)
            windows_cases=$((windows_cases + 1))
            destination="$temporary_directory/Destination.exe"
            valid_report_digest=$windows_valid_report_digest
            ;;
        linux:Linux-*.elf)
            linux_cases=$((linux_cases + 1))
            destination="$temporary_directory/Destination.elf"
            valid_report_digest=$linux_valid_report_digest
            ;;
        *)
            echo "FAIL  $name: platform or suffix differs" >&2
            return 1
            ;;
    esac
    case "$expectation:$stage0:$operation" in
        valid:Valid:base) valid_cases=$((valid_cases + 1));;
        reject:WVW2100:*|reject:WVL2100:*) rejected_cases=$((rejected_cases + 1));;
        *)
            echo "FAIL  $name: expectation, Stage 0 result, or operation differs" >&2
            return 1
            ;;
    esac
    [[ $platform != windows || $expectation != reject || $stage0 == WVW2100 ]] || return 1
    [[ $platform != linux || $expectation != reject || $stage0 == WVL2100 ]] || return 1
    [[ $operation != xor1:* ]] || xor_cases=$((xor_cases + 1))
    [[ $operation != *rehash* ]] || rehash_cases=$((rehash_cases + 1))
    [[ $operation != truncate:500 ]] || truncate_cases=$((truncate_cases + 1))
    [[ $operation != append:00 ]] || append_cases=$((append_cases + 1))

    total=$((total + 1))
    actual_size=$(wc -c < "$input") || return 1
    actual_size=${actual_size//[[:space:]]/}
    [[ $actual_size == "$size" ]] || return 1
    check_hash "$input" "$digest" || return 1
    cp -- "$sentinel" "$destination" || return 1
    "$repository_root/Tools/Native/Publish-Console.sh" \
        "$input" "$destination" > "$run_output" 2> "$run_error"
    run_status=$?
    if [[ $expectation == valid ]]; then
        ((run_status == 0)) || return 1
        check_hash "$run_output" "$valid_report_digest" || return 1
        [[ ! -s $run_error ]] || return 1
        check_hash "$destination" "$digest" || return 1
    else
        ((run_status == 1)) || return 1
        [[ ! -s $run_output ]] || return 1
        check_hash "$run_error" "$rejected_report_digest" || return 1
        check_hash "$destination" "$sentinel_digest" || return 1
    fi
    check_hash "$input" "$digest" || return 1
    local scratch=("$temporary_directory"/.wvpublish-*)
    [[ ! -e ${scratch[0]} ]] || return 1
    rm -f -- "$destination" "$run_output" "$run_error"
    passed=$((passed + 1))
    echo "PASS  $name expectation=$expectation oracle=$stage0 operation=$operation"
}

decode_fixture \
    "$repository_root/Tests/Native/Hosted-Console-Container-Mutations/Corpus.tar.gz.b64" \
    "$archive" "$archive_digest" 'hosted mutation archive'
decode_fixture \
    "$repository_root/Tests/Native/Wvo/Return-42.wvo.b64" \
    "$sentinel" "$sentinel_digest" 'destination sentinel'

if ! tar -xzf "$archive" -C "$corpus_directory" \
    > "$extract_output" 2> "$extract_error"; then
    cat -- "$extract_error" >&2
    fail 'archive could not be extracted'
fi
if [[ -s $extract_output || -s $extract_error ]]; then
    cat -- "$extract_output" "$extract_error" >&2
    fail 'extractor wrote output'
fi
check_hash "$manifest" "$manifest_digest" || fail 'manifest identity differs'
IFS= read -r header < "$manifest"
[[ $header == 'windvale-hosted-console-container-mutations 1' ]] || fail 'manifest header differs'

while IFS='|' read -r name platform expectation stage0 operation size digest; do
    run_case "$name" "$platform" "$expectation" "$stage0" "$operation" "$size" "$digest" ||
        fail "$name failed"
done < <(tail -n +3 -- "$manifest")

((total == 15)) || fail 'total case count differs'
((windows_cases == 8)) || fail 'Windows case count differs'
((linux_cases == 7)) || fail 'Linux case count differs'
((valid_cases == 2)) || fail 'valid base count differs'
((rejected_cases == 13)) || fail 'rejection count differs'
((xor_cases == 9)) || fail 'xor count differs'
((rehash_cases == 2)) || fail 'rehashed-leaf count differs'
((truncate_cases == 2)) || fail 'truncation count differs'
((append_cases == 2)) || fail 'trailing-byte count differs'

echo "Tests: $total, Passed: $passed, Failed: 0"
