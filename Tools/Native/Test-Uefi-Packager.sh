#!/usr/bin/env bash
set -uo pipefail

if [[ $# -ne 0 ]]; then
    echo 'Usage: ./Tools/Native/Test-Uefi-Packager.sh' >&2
    exit 64
fi

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
packager_artifacts="$repository_root/Artifacts/Native-Uefi-Packager-Candidate"
temporary_root=${TMPDIR:-/tmp}
test_directory=$(mktemp -d "$temporary_root/windvale-native-uefi-packager.XXXXXXXX") || exit 1

cleanup() {
    case "$test_directory" in
        "$temporary_root"/windvale-native-uefi-packager.*)
            rm -f -- "$test_directory"/*
            rmdir -- "$test_directory"
            ;;
        *)
            echo "Refusing to remove unexpected test path: $test_directory" >&2
            return 1
            ;;
    esac
}
trap cleanup EXIT

fail() {
    echo "FAIL  UEFI packaging: $1" >&2
    exit 1
}

check_file() {
    local path=$1 expected_bytes=$2 expected_sha256=$3 label=$4
    [[ -f $path ]] || fail "missing $label"
    [[ $(wc -c < "$path") -eq $expected_bytes ]] || fail "$label length differs"
    local digest_line actual_sha256
    digest_line=$(sha256sum -- "$path") || fail "$label digest is unavailable"
    actual_sha256=${digest_line%% *}
    [[ $actual_sha256 == "$expected_sha256" ]] || fail "$label digest differs"
}

main="$test_directory/Main.wvo"
provider="$test_directory/Provider.wvo"
"$script_directory/Assemble-Wva.sh" \
    "$repository_root/Examples/Assembler/Hello-Object.wva" "$main" \
    >"$test_directory/Main-Assemble.out" 2>"$test_directory/Main-Assemble.err" ||
    fail 'main WVO assembly failed'
[[ ! -s $test_directory/Main-Assemble.err ]] || fail 'main WVO assembly wrote a diagnostic'
"$script_directory/Assemble-Wva.sh" \
    "$repository_root/Examples/Linker/Console-Provider.wva" "$provider" \
    >"$test_directory/Provider-Assemble.out" 2>"$test_directory/Provider-Assemble.err" ||
    fail 'provider WVO assembly failed'
[[ ! -s $test_directory/Provider-Assemble.err ]] || fail 'provider WVO assembly wrote a diagnostic'
check_file "$main" 218 \
    992c298a4f9b68dec27b7203a2770f2a37ef2016ea45e88d33ee21994060fe85 'main WVO'
check_file "$provider" 91 \
    486134e34bb32abadd233d1c3303acd9c313aa69d3874cafdce0fcb61b6e72ab 'provider WVO'

"$script_directory/Link-Wvo.sh" 0 Main "$test_directory/Native.bin" \
    "$main" "$provider" \
    >"$test_directory/Link.out" 2>"$test_directory/Link.err" || fail 'native linking failed'
[[ ! -s $test_directory/Link.err ]] || fail 'native linking wrote a diagnostic'
grep -Fx 'entry name=Main address=0' "$test_directory/Link.out" >/dev/null ||
    fail 'native linker did not report entry zero'
check_file "$test_directory/Native.bin" 24 \
    7612954be9dc08e12ab06510e6539a37ab797bc381ee8844908b5f7c475d16a5 'native linked image'

"$script_directory/Package-Uefi.sh" "$test_directory/Native.bin" 0 \
    "$test_directory/Application.efi" >"$test_directory/Package.out" \
    2>"$test_directory/Package.err" || fail 'valid packaging failed'
[[ ! -s $test_directory/Package.err ]] || fail 'valid packaging wrote a diagnostic'
grep -Fx 'uefi-package status=Valid native-image-bytes=24 entry-offset=0 application-bytes=1536' \
    "$test_directory/Package.out" >/dev/null || fail 'valid report differs'
check_file "$test_directory/Application.efi" 1536 \
    7d30fd4d220a2d578b0ce3da4cbb6006175f012268b7d3a08e80543e7e388b09 \
    'canonical UEFI application'
echo 'PASS  UEFI packaging composes native link output'

"$script_directory/Package-Uefi.sh" "$test_directory/Native.bin" 0 \
    "$test_directory/Application-Again.efi" >"$test_directory/Repeat.out" \
    2>"$test_directory/Repeat.err" || fail 'repeat packaging failed'
[[ ! -s $test_directory/Repeat.err ]] || fail 'repeat packaging wrote a diagnostic'
cmp --silent -- "$test_directory/Application.efi" "$test_directory/Application-Again.efi" ||
    fail 'repeated output differs'
echo 'PASS  UEFI packaging is deterministic'

cp -- "$packager_artifacts/Uefi-Packager.wvb" "$test_directory/Rejected.efi" ||
    fail 'destination sentinel staging failed'
"$script_directory/Package-Uefi.sh" "$test_directory/Native.bin" 24 \
    "$test_directory/Rejected.efi" >"$test_directory/Invalid.out" \
    2>"$test_directory/Invalid.err"
status=$?
[[ $status -eq 2 ]] || fail "invalid entry returned $status"
[[ ! -s $test_directory/Invalid.out ]] || fail 'invalid packaging wrote standard output'
grep -F 'entry-offset=24 application-bytes=0' "$test_directory/Invalid.err" >/dev/null ||
    fail 'invalid-entry report differs'
check_file "$test_directory/Rejected.efi" 25999 \
    063f95f53e39390c76bcf31fbf7bdc87eed6194388101fadc4d60ee41b2802e4 \
    'preserved destination'
echo 'PASS  UEFI packaging rejects invalid entry and preserves output'
echo 'Tests: 3, Passed: 3, Failed: 0'
