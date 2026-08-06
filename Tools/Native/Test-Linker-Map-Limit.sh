#!/usr/bin/env bash
set -uo pipefail

if [[ $# -ne 0 ]]; then
    echo 'Usage: ./Tools/Native/Test-Linker-Map-Limit.sh' >&2
    exit 64
fi

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
temporary_root=${TMPDIR:-/tmp}
temporary_directory=$(mktemp -d "$temporary_root/windvale-linker-map-limit.XXXXXXXX") || exit 1
cleanup() {
    case "$temporary_directory" in
        "$temporary_root"/windvale-linker-map-limit.*)
            rm -f -- \
                "$temporary_directory/Map-Objects.tar.gz" \
                "$temporary_directory/Entry.wvo" \
                "$temporary_directory/Map-Locals-4096.wvo" \
                "$temporary_directory/Map-Locals-4095.wvo" \
                "$temporary_directory/Output.bin" \
                "$temporary_directory/Run.out" \
                "$temporary_directory/Run.err" \
                "$temporary_directory/Decode.err" \
                "$temporary_directory/Extract.out" \
                "$temporary_directory/Extract.err"
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
    echo "FAIL  canonical-map-limit: $1" >&2
    echo 'Tests: 1, Passed: 0, Failed: 1' >&2
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

archive="$temporary_directory/Map-Objects.tar.gz"
entry="$temporary_directory/Entry.wvo"
locals_4096="$temporary_directory/Map-Locals-4096.wvo"
locals_4095="$temporary_directory/Map-Locals-4095.wvo"
output="$temporary_directory/Output.bin"
run_output="$temporary_directory/Run.out"
run_error="$temporary_directory/Run.err"
decode_error="$temporary_directory/Decode.err"
extract_output="$temporary_directory/Extract.out"
extract_error="$temporary_directory/Extract.err"
archive_digest=1c6227931496f54c93677b4dfecfbfa256214a5da72ecfd05d441e49c809e27d
entry_digest=0d1829bbbc77f3ee3910a70f98528e1078117480332adb5a2d09df8b2d25f3b5
locals_4096_digest=a05c4f51be960c7fc900d8cc9fc39dbc525ccd0b2b1a4c55b12ca8396107ee75
locals_4095_digest=398737cfd465fb976e6319ce7ddc4dbefb9e082d39432d09474cf75f8aafffdc
report_digest=097ad88fa0e4fd48504da8d69516e47ff7f6b5979fccf186e0307b814b5af86e

decode_fixture \
    "$repository_root/Tests/Native/Linker-Map-Limit/Map-Objects.tar.gz.b64" \
    "$archive" \
    "$archive_digest" \
    'map-object archive'
decode_fixture \
    "$repository_root/Tests/Native/Wvo/Return-42.wvo.b64" \
    "$entry" \
    "$entry_digest" \
    'entry WVO'

if ! tar -xzf "$archive" -C "$temporary_directory" \
    > "$extract_output" 2> "$extract_error"; then
    cat -- "$extract_error" >&2
    fail 'map-object archive could not be extracted'
fi
if [[ -s $extract_output || -s $extract_error ]]; then
    cat -- "$extract_output" "$extract_error" >&2
    fail 'map-object extractor wrote output'
fi
check_hash "$locals_4096" "$locals_4096_digest" || \
    fail '4,096-local WVO identity differs'
check_hash "$locals_4095" "$locals_4095_digest" || \
    fail '4,095-local WVO identity differs'

cp -- "$entry" "$output" || fail 'output sentinel could not be created'
"$repository_root/Tools/Native/Link-Wvo.sh" \
    0 Main "$output" \
    "$entry" "$locals_4096" "$locals_4096" "$locals_4096" "$locals_4095" \
    > "$run_output" 2> "$run_error"
run_status=$?
if ((run_status != 2)); then
    fail 'native linker exit differs'
fi
if [[ -s $run_output ]]; then
    cat -- "$run_output" >&2
    fail 'rejected link wrote standard output'
fi
if ! check_hash "$run_error" "$report_digest"; then
    cat -- "$run_error" >&2
    fail 'native linker report differs'
fi
check_hash "$output" "$entry_digest" || fail 'rejected link changed the output'
check_hash "$entry" "$entry_digest" || fail 'entry WVO changed during linking'
check_hash "$locals_4096" "$locals_4096_digest" || \
    fail '4,096-local WVO changed during linking'
check_hash "$locals_4095" "$locals_4095_digest" || \
    fail '4,095-local WVO changed during linking'

echo 'PASS  canonical-map-limit'
echo 'Tests: 1, Passed: 1, Failed: 0'
