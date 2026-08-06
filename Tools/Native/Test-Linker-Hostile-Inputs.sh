#!/usr/bin/env bash
set -uo pipefail

if [[ $# -ne 0 ]]; then
    echo 'Usage: ./Tools/Native/Test-Linker-Hostile-Inputs.sh' >&2
    exit 64
fi

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
temporary_root=${TMPDIR:-/tmp}
temporary_directory=$(mktemp -d "$temporary_root/windvale-linker-hostile-inputs.XXXXXXXX") || exit 1
corpus_directory="$temporary_directory/Corpus"
mkdir -- "$corpus_directory" || exit 1

archive="$temporary_directory/Corpus.tar.gz"
manifest="$corpus_directory/Manifest.txt"
sentinel="$temporary_directory/Sentinel.wvo"
output="$temporary_directory/Output.bin"
run_output="$temporary_directory/Run.out"
run_error="$temporary_directory/Run.err"
decode_error="$temporary_directory/Decode.err"
extract_output="$temporary_directory/Extract.out"
extract_error="$temporary_directory/Extract.err"
archive_digest=3648bc4a00bb822096ad669d0f24828f034df5b69023f1bdb2c3b3ab2a034160
manifest_digest=b3ab716d55e8c2693dbf0610b8638b23780867082bec7e768635a16e8e1fbfef
sentinel_digest=0369f8b34765adb08799e6b852e9d1e249c40d1049976b01ff59355dd111f288
report_digest=18eeeeb5d84e82c54cf14480bc5c54e593f5cd429d68686ad64110d9780a5353
total=0
passed=0

cleanup() {
    case "$temporary_directory" in
        "$temporary_root"/windvale-linker-hostile-inputs.*)
            rm -f -- "$corpus_directory"/Case-???.wvo "$manifest"
            rmdir -- "$corpus_directory"
            rm -f -- \
                "$archive" \
                "$sentinel" \
                "$output" \
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
    echo "FAIL  linker-hostile: $1" >&2
    echo "Tests: $total, Passed: $passed, Failed: $((total - passed))" >&2
    exit 1
}

decode_fixture() {
    local source=$1
    local destination=$2
    local digest=$3
    local description=$4
    if ! base64 --decode "$source" > "$destination" 2> "$decode_error"; then
        fail "$description could not be decoded"
    fi
    if [[ -s $decode_error ]]; then
        cat -- "$decode_error" >&2
        fail "$description decoder wrote a diagnostic"
    fi
    check_hash "$destination" "$digest" || fail "$description identity differs"
}

run_case() {
    local name=$1
    local size=$2
    local digest=$3
    local input="$corpus_directory/$name"
    local actual_size
    local run_status

    total=$((total + 1))
    actual_size=$(wc -c < "$input") || return 1
    actual_size=${actual_size//[[:space:]]/}
    if [[ $actual_size != "$size" ]]; then
        echo "FAIL  $name: hostile input size differs" >&2
        return 1
    fi
    check_hash "$input" "$digest" || {
        echo "FAIL  $name: hostile input identity differs" >&2
        return 1
    }
    cp -- "$sentinel" "$output" || return 1
    "$repository_root/Tools/Native/Link-Wvo.sh" \
        1048576 Main "$output" "$input" > "$run_output" 2> "$run_error"
    run_status=$?
    if ((run_status != 2)); then
        echo "FAIL  $name: native linker exit differs" >&2
        return 1
    fi
    if [[ -s $run_output ]]; then
        echo "FAIL  $name: rejected link wrote standard output" >&2
        cat -- "$run_output" >&2
        return 1
    fi
    if ! check_hash "$run_error" "$report_digest"; then
        echo "FAIL  $name: native linker report differs" >&2
        cat -- "$run_error" >&2
        return 1
    fi
    check_hash "$output" "$sentinel_digest" || {
        echo "FAIL  $name: rejected link changed the output" >&2
        return 1
    }
    check_hash "$input" "$digest" || {
        echo "FAIL  $name: native linker changed the hostile input" >&2
        return 1
    }
    rm -f -- "$output" "$run_output" "$run_error"
    passed=$((passed + 1))
    echo "PASS  $name"
}

decode_fixture \
    "$repository_root/Tests/Native/Linker-Hostile-Inputs/Corpus.tar.gz.b64" \
    "$archive" \
    "$archive_digest" \
    'hostile-input archive'
decode_fixture \
    "$repository_root/Tests/Native/Wvo/Bad-Magic.wvo.b64" \
    "$sentinel" \
    "$sentinel_digest" \
    'output sentinel'

if ! tar -xzf "$archive" -C "$corpus_directory" \
    > "$extract_output" 2> "$extract_error"; then
    cat -- "$extract_error" >&2
    fail 'hostile-input archive could not be extracted'
fi
if [[ -s $extract_output || -s $extract_error ]]; then
    cat -- "$extract_output" "$extract_error" >&2
    fail 'hostile-input extractor wrote output'
fi
check_hash "$manifest" "$manifest_digest" || fail 'hostile-input manifest identity differs'

IFS= read -r header < "$manifest"
if [[ $header != 'windvale-linker-hostile-corpus 1' ]]; then
    fail 'hostile-input manifest header differs'
fi

while IFS='|' read -r name size digest; do
    run_case "$name" "$size" "$digest" || fail "$name failed"
done < <(tail -n +2 -- "$manifest")

if ((total != 200)); then
    fail 'hostile-input case count differs'
fi

echo "Tests: $total, Passed: $passed, Failed: 0"
