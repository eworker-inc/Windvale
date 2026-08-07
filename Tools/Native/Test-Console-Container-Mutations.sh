#!/usr/bin/env bash
set -uo pipefail

if [[ $# -ne 0 ]]; then
    echo 'Usage: ./Tools/Native/Test-Console-Container-Mutations.sh' >&2
    exit 64
fi

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
temporary_root=${TMPDIR:-/tmp}
temporary_directory=$(mktemp -d "$temporary_root/windvale-console-container-mutations.XXXXXXXX") || exit 1
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
archive_digest=63b7d5187aa0f5407aa5a68be851c03fb0b64991c418f8c2407548f0ad6c89c9
manifest_digest=35794ce75d80a06b099f705a8c0fce91295a5d627cee2a76803617f372e13669
sentinel_digest=0d1829bbbc77f3ee3910a70f98528e1078117480332adb5a2d09df8b2d25f3b5
report_digest=39db034713225109f62c272db447d75cfe93ff0c259c8d9e5211f0df5c007e1f
total=0
passed=0
windows_cases=0
linux_cases=0
truncate_cases=0
xor_cases=0
append_cases=0
wvw2001=0; wvw2002=0; wvw2003=0; wvw2004=0; wvw2005=0
wvw2006=0; wvw2007=0; wvw2008=0; wvw2009=0
wvl2001=0; wvl2002=0; wvl2003=0; wvl2004=0
wvl2005=0; wvl2006=0; wvl2007=0; wvl2008=0

cleanup() {
    case "$temporary_directory" in
        "$temporary_root"/windvale-console-container-mutations.*)
            rm -f -- \
                "$corpus_directory"/Windows-*.exe \
                "$corpus_directory"/Linux-*.elf \
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
    echo "FAIL  console-container-mutations: $1" >&2
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

record_code() {
    local target=$1
    local code=$2
    case "$target:$code" in
        windows-x64-console-v1:WVW2001) wvw2001=$((wvw2001 + 1));;
        windows-x64-console-v1:WVW2002) wvw2002=$((wvw2002 + 1));;
        windows-x64-console-v1:WVW2003) wvw2003=$((wvw2003 + 1));;
        windows-x64-console-v1:WVW2004) wvw2004=$((wvw2004 + 1));;
        windows-x64-console-v1:WVW2005) wvw2005=$((wvw2005 + 1));;
        windows-x64-console-v1:WVW2006) wvw2006=$((wvw2006 + 1));;
        windows-x64-console-v1:WVW2007) wvw2007=$((wvw2007 + 1));;
        windows-x64-console-v1:WVW2008) wvw2008=$((wvw2008 + 1));;
        windows-x64-console-v1:WVW2009) wvw2009=$((wvw2009 + 1));;
        linux-x64-console-v1:WVL2001) wvl2001=$((wvl2001 + 1));;
        linux-x64-console-v1:WVL2002) wvl2002=$((wvl2002 + 1));;
        linux-x64-console-v1:WVL2003) wvl2003=$((wvl2003 + 1));;
        linux-x64-console-v1:WVL2004) wvl2004=$((wvl2004 + 1));;
        linux-x64-console-v1:WVL2005) wvl2005=$((wvl2005 + 1));;
        linux-x64-console-v1:WVL2006) wvl2006=$((wvl2006 + 1));;
        linux-x64-console-v1:WVL2007) wvl2007=$((wvl2007 + 1));;
        linux-x64-console-v1:WVL2008) wvl2008=$((wvl2008 + 1));;
        *) return 1;;
    esac
}

run_case() {
    local name=$1
    local target=$2
    local operation=$3
    local offset=$4
    local oracle_code=$5
    local size=$6
    local digest=$7
    local input="$corpus_directory/$name"
    local base_bytes
    local expected_bytes
    local actual_size
    local run_status

    if [[ $name != "${name##*/}" || $name == *\\* ]]; then
        echo "FAIL  $name: mutation input name is not a filename" >&2
        return 1
    fi
    case "$target:$name" in
        windows-x64-console-v1:Windows-*.exe)
            windows_cases=$((windows_cases + 1))
            base_bytes=5120
            destination="$temporary_directory/Destination.exe"
            ;;
        linux-x64-console-v1:Linux-*.elf)
            linux_cases=$((linux_cases + 1))
            base_bytes=8304
            destination="$temporary_directory/Destination.elf"
            ;;
        *)
            echo "FAIL  $name: mutation target or suffix differs" >&2
            return 1
            ;;
    esac
    case "$operation" in
        truncate-last)
            truncate_cases=$((truncate_cases + 1))
            expected_bytes=$((base_bytes - 1))
            [[ $offset == "$expected_bytes" ]] || return 1
            ;;
        xor-one)
            xor_cases=$((xor_cases + 1))
            expected_bytes=$base_bytes
            ((offset >= 0 && offset < base_bytes)) || return 1
            ;;
        append-zero)
            append_cases=$((append_cases + 1))
            expected_bytes=$((base_bytes + 1))
            [[ $offset == "$base_bytes" ]] || return 1
            ;;
        *)
            echo "FAIL  $name: mutation operation differs" >&2
            return 1
            ;;
    esac
    if [[ $size != "$expected_bytes" ]]; then
        echo "FAIL  $name: operation and size disagree" >&2
        return 1
    fi
    record_code "$target" "$oracle_code" || {
        echo "FAIL  $name: Stage 0 oracle code differs" >&2
        return 1
    }

    total=$((total + 1))
    actual_size=$(wc -c < "$input") || return 1
    actual_size=${actual_size//[[:space:]]/}
    if [[ $actual_size != "$size" ]]; then
        echo "FAIL  $name: mutation input size differs" >&2
        return 1
    fi
    check_hash "$input" "$digest" || {
        echo "FAIL  $name: mutation input identity differs" >&2
        return 1
    }
    cp -- "$sentinel" "$destination" || return 1
    "$repository_root/Tools/Native/Publish-Console.sh" \
        "$input" "$destination" > "$run_output" 2> "$run_error"
    run_status=$?
    if ((run_status != 1)); then
        echo "FAIL  $name: native console publisher exit differs" >&2
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
        echo "FAIL  $name: native console publisher changed the mutation input" >&2
        return 1
    }
    local scratch=("$temporary_directory"/.wvpublish-*)
    if [[ -e ${scratch[0]} ]]; then
        echo "FAIL  $name: rejected publication left scratch" >&2
        return 1
    fi
    rm -f -- "$destination" "$run_output" "$run_error"
    passed=$((passed + 1))
    echo "PASS  $name operation=$operation offset=$offset oracle=$oracle_code"
}

decode_fixture \
    "$repository_root/Tests/Native/Console-Container-Mutations/Corpus.tar.gz.b64" \
    "$archive" \
    "$archive_digest" \
    'mutation archive'
decode_fixture \
    "$repository_root/Tests/Native/Wvo/Return-42.wvo.b64" \
    "$sentinel" \
    "$sentinel_digest" \
    'destination sentinel'

if ! tar -xzf "$archive" -C "$corpus_directory" \
    > "$extract_output" 2> "$extract_error"; then
    cat -- "$extract_error" >&2
    fail 'mutation archive could not be extracted'
fi
if [[ -s $extract_output || -s $extract_error ]]; then
    cat -- "$extract_output" "$extract_error" >&2
    fail 'mutation extractor wrote output'
fi
check_hash "$manifest" "$manifest_digest" || fail 'mutation manifest identity differs'

IFS= read -r header < "$manifest"
if [[ $header != 'windvale-console-container-mutation-corpus 1' ]]; then
    fail 'mutation manifest header differs'
fi

while IFS='|' read -r name target operation offset oracle_code size digest; do
    run_case "$name" "$target" "$operation" "$offset" "$oracle_code" "$size" "$digest" || fail "$name failed"
done < <(tail -n +2 -- "$manifest")

((total == 19)) || fail 'total case count differs'
((windows_cases == 10)) || fail 'Windows case count differs'
((linux_cases == 9)) || fail 'Linux case count differs'
((truncate_cases == 2)) || fail 'truncation case count differs'
((xor_cases == 15)) || fail 'one-byte mutation case count differs'
((append_cases == 2)) || fail 'trailing-byte case count differs'
((wvw2001 == 2 && wvw2002 == 1 && wvw2003 == 1 && wvw2004 == 1 &&
    wvw2005 == 1 && wvw2006 == 1 && wvw2007 == 1 && wvw2008 == 1 &&
    wvw2009 == 1)) || fail 'Windows Stage 0 code inventory differs'
((wvl2001 == 2 && wvl2002 == 1 && wvl2003 == 1 && wvl2004 == 1 &&
    wvl2005 == 1 && wvl2006 == 1 && wvl2007 == 1 && wvl2008 == 1)) ||
    fail 'Linux Stage 0 code inventory differs'

echo "Tests: $total, Passed: $passed, Failed: 0"
