#!/usr/bin/env bash
set -uo pipefail

if [[ $# -ne 0 ]]; then
    echo 'Usage: ./Tools/Native/Test-Wvo-Hostile-Size.sh' >&2
    exit 64
fi

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
temporary_root=${TMPDIR:-/tmp}
temporary_directory=$(mktemp -d "$temporary_root/windvale-wvo-hostile-size.XXXXXXXX") || exit 1
archive="$temporary_directory/Corpus.tar.gz"
input="$temporary_directory/Oversized.wvo"
sentinel="$temporary_directory/Sentinel.wvo"
linked="$temporary_directory/Linked.bin"
published="$temporary_directory/Published.wvo"
run_output="$temporary_directory/Run.out"
run_error="$temporary_directory/Run.err"
decode_error="$temporary_directory/Decode.err"
extract_output="$temporary_directory/Extract.out"
extract_error="$temporary_directory/Extract.err"
archive_digest=4c9e5ed9aa6a822c64e799378ede641d86c37a6cc639003286afd2277144ef89
input_digest=95e441ca65cd41fa01b2a71799e79fd60db59ed34f13af32a91e85f90378676c
sentinel_digest=0d1829bbbc77f3ee3910a70f98528e1078117480332adb5a2d09df8b2d25f3b5
total=0
passed=0

cleanup() {
    case "$temporary_directory" in
        "$temporary_root"/windvale-wvo-hostile-size.*)
            rm -f -- \
                "$archive" "$input" "$sentinel" "$linked" "$published" \
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
    echo "FAIL  wvo-hostile-size: $1" >&2
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

require_empty_channels() {
    local name=$1
    if [[ -s $run_output || -s $run_error ]]; then
        echo "FAIL  $name: hostile-size rejection wrote output" >&2
        cat -- "$run_output" "$run_error" >&2
        return 1
    fi
}

run_read_only() {
    local name=$1
    local command=$2
    local status
    total=$((total + 1))
    "$repository_root/Tools/Native/$command" "$input" > "$run_output" 2> "$run_error"
    status=$?
    ((status == 1)) || return 1
    require_empty_channels "$name" || return 1
    check_hash "$input" "$input_digest" || return 1
    rm -f -- "$run_output" "$run_error"
    passed=$((passed + 1))
    echo "PASS  $name boundary=file-snapshot oracle=WVO1001"
}

run_linker() {
    local status
    total=$((total + 1))
    cp -- "$sentinel" "$linked" || return 1
    "$repository_root/Tools/Native/Link-Wvo.sh" \
        1048576 Main "$linked" "$input" > "$run_output" 2> "$run_error"
    status=$?
    ((status == 1)) || return 1
    require_empty_channels link || return 1
    check_hash "$input" "$input_digest" || return 1
    check_hash "$linked" "$sentinel_digest" || return 1
    rm -f -- "$linked" "$run_output" "$run_error"
    passed=$((passed + 1))
    echo 'PASS  link boundary=file-snapshot oracle=WVL1002'
}

run_publisher() {
    local status
    total=$((total + 1))
    cp -- "$sentinel" "$published" || return 1
    "$repository_root/Tools/Native/Publish-Wvo.sh" \
        "$input" "$published" > "$run_output" 2> "$run_error"
    status=$?
    ((status == 1)) || return 1
    require_empty_channels publish || return 1
    check_hash "$input" "$input_digest" || return 1
    check_hash "$published" "$sentinel_digest" || return 1
    local scratch=("$temporary_directory"/.wvpublish-*)
    [[ ! -e ${scratch[0]} ]] || return 1
    rm -f -- "$published" "$run_output" "$run_error"
    passed=$((passed + 1))
    echo 'PASS  publish boundary=file-snapshot oracle=WVO1001'
}

decode_fixture \
    "$repository_root/Tests/Native/Wvo-Hostile-Size/Corpus.tar.gz.b64" \
    "$archive" "$archive_digest" 'hostile-size archive'
decode_fixture \
    "$repository_root/Tests/Native/Wvo/Return-42.wvo.b64" \
    "$sentinel" "$sentinel_digest" 'destination sentinel'

if ! tar -xzf "$archive" -C "$temporary_directory" \
    > "$extract_output" 2> "$extract_error"; then
    cat -- "$extract_error" >&2
    fail 'archive could not be extracted'
fi
if [[ -s $extract_output || -s $extract_error ]]; then
    cat -- "$extract_output" "$extract_error" >&2
    fail 'extractor wrote output'
fi
actual_size=$(wc -c < "$input") || fail 'input size could not be read'
actual_size=${actual_size//[[:space:]]/}
[[ $actual_size == 4194305 ]] || fail 'input size differs'
check_hash "$input" "$input_digest" || fail 'input identity differs'

run_read_only verify Verify-Wvo.sh || fail 'verify failed'
run_read_only inspect Inspect-Wvo.sh || fail 'inspect failed'
run_linker || fail 'link failed'
run_publisher || fail 'publish failed'
((total == 4)) || fail 'case count differs'

echo "Tests: $total, Passed: $passed, Failed: 0"
