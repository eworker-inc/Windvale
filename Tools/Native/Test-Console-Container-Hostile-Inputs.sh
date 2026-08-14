#!/usr/bin/env bash
set -uo pipefail

if [[ $# -ne 0 ]]; then
    echo 'Usage: ./Tools/Native/Test-Console-Container-Hostile-Inputs.sh' >&2
    exit 64
fi

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
temporary_root=${TMPDIR:-/tmp}
temporary_directory=$(mktemp -d "$temporary_root/windvale-console-container-hostile.XXXXXXXX") || exit 1
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
archive_digest=2aa0a153aaf1c70fe650f99e302ebd2aaa9908228175e0f0bebdd9894a872112
manifest_digest=94f2fb533dabaa57a54c331458ac0f0b478476e2923263840eff85dbd19dd8db
sentinel_digest=0d1829bbbc77f3ee3910a70f98528e1078117480332adb5a2d09df8b2d25f3b5
report_digest=39db034713225109f62c272db447d75cfe93ff0c259c8d9e5211f0df5c007e1f
total=0
passed=0
windows_cases=0
linux_cases=0

cleanup() {
    case "$temporary_directory" in
        "$temporary_root"/windvale-console-container-hostile.*)
            rm -f -- \
                "$corpus_directory"/Windows-???.exe \
                "$corpus_directory"/Linux-???.elf \
                "$manifest"
            rmdir -- "$corpus_directory"
            rm -f -- \
                "$archive" \
                "$sentinel" \
                "$temporary_directory/Destination.exe" \
                "$temporary_directory/Destination.elf" \
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
    echo "FAIL  console-container-hostile: $1" >&2
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
    local target=$2
    local size=$3
    local digest=$4
    local input="$corpus_directory/$name"
    local actual_size
    local run_status

    if [[ $name != "${name##*/}" || $name == *\\* ]]; then
        echo "FAIL  $name: hostile input name is not a filename" >&2
        return 1
    fi
    case "$target:$name" in
        windows-x64-console-v1:Windows-???.exe)
            windows_cases=$((windows_cases + 1))
            destination="$temporary_directory/Destination.exe"
            ;;
        linux-x64-console-v1:Linux-???.elf)
            linux_cases=$((linux_cases + 1))
            destination="$temporary_directory/Destination.elf"
            ;;
        *)
            echo "FAIL  $name: hostile input target or suffix differs" >&2
            return 1
            ;;
    esac

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
    cp -- "$sentinel" "$destination" || return 1
    "$repository_root/Tools/Native/Publish-Console.sh" \
        "$input" "$destination" > "$run_output" 2> "$run_error"
    run_status=$?
    if ((run_status != 1)); then
        echo "FAIL  $name: native console publisher exit differs expected=1 actual=$run_status" >&2
        return 1
    fi
    if [[ -s $run_output ]]; then
        echo "FAIL  $name: rejected publication wrote standard output" >&2
        cat -- "$run_output" >&2
        return 1
    fi
    if ! check_hash "$run_error" "$report_digest"; then
        echo "FAIL  $name: native console publisher report differs" >&2
        cat -- "$run_error" >&2
        return 1
    fi
    check_hash "$destination" "$sentinel_digest" || {
        echo "FAIL  $name: rejected publication changed the destination" >&2
        return 1
    }
    check_hash "$input" "$digest" || {
        echo "FAIL  $name: native console publisher changed the hostile input" >&2
        return 1
    }
    local scratch=("$temporary_directory"/.wvpublish-*)
    if [[ -e ${scratch[0]} ]]; then
        echo "FAIL  $name: rejected publication left scratch" >&2
        return 1
    fi
    rm -f -- "$destination" "$run_output" "$run_error"
    passed=$((passed + 1))
    echo "PASS  $name"
}

decode_fixture \
    "$repository_root/Tests/Native/Console-Container-Hostile-Inputs/Corpus.tar.gz.b64" \
    "$archive" \
    "$archive_digest" \
    'hostile-input archive'
decode_fixture \
    "$repository_root/Tests/Native/Wvo/Return-42.wvo.b64" \
    "$sentinel" \
    "$sentinel_digest" \
    'destination sentinel'

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
if [[ $header != 'windvale-console-container-hostile-corpus 1' ]]; then
    fail 'hostile-input manifest header differs'
fi

while IFS='|' read -r name target size digest; do
    run_case "$name" "$target" "$size" "$digest" || fail "$name failed"
done < <(tail -n +2 -- "$manifest")

if ((total != 256)); then
    fail 'hostile-input case count differs'
fi
if ((windows_cases != 128)); then
    fail 'Windows case count differs'
fi
if ((linux_cases != 128)); then
    fail 'Linux case count differs'
fi

echo "Tests: $total, Passed: $passed, Failed: 0"
