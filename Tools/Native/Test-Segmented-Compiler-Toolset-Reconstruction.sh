#!/usr/bin/env bash
set -uo pipefail

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
candidate="$repository_root/Artifacts/Native-Segmented-Compiler-Toolset-Candidate"
tests=0
passed=0

pass() {
    tests=$((tests + 1))
    passed=$((passed + 1))
    echo "PASS  $1"
}
fail() {
    tests=$((tests + 1))
    echo 'FAIL  segmented compiler toolset reconstruction'
    echo "Tests: $tests, Passed: $passed, Failed: $((tests - passed))"
    exit 1
}
verify_exact() {
    [[ -f $1 && -f $2 ]] || return 1
    cmp --silent -- "$1" "$2"
}
verify_family() {
    verify_exact "$test_directory/$1" "$candidate/$1" || return 1
    verify_exact "$test_directory/$2" "$candidate/$2" || return 1
    verify_exact "$test_directory/$3" "$candidate/$3"
}

if "$script_directory/Construct-Segmented-Compiler-Toolset.sh" >/dev/null 2>&1; then
    fail
elif [[ $? -ne 64 ]]; then
    fail
fi

temporary_root=${TMPDIR:-/tmp}
test_directory=$(mktemp -d "$temporary_root/windvale-segmented-toolset-reconstruction-test.XXXXXXXX") || fail
cleanup() {
    case "$test_directory" in
        "$temporary_root"/windvale-segmented-toolset-reconstruction-test.*)
            rm -rf -- "$test_directory"
            ;;
        *)
            echo "Refusing to remove unexpected temporary path: $test_directory" >&2
            return 1
            ;;
    esac
}
trap cleanup EXIT

"$script_directory/Construct-Segmented-Compiler-Toolset.sh" "$test_directory" \
    >"$test_directory/Construct.out" 2>"$test_directory/Construct.err" || fail
grep -Fx 'native segmented compiler toolset construction status=Complete artifacts=9' \
    "$test_directory/Construct.out" >/dev/null || fail
[[ ! -s $test_directory/Construct.err ]] || fail

verify_family Wvo-Staging-Producer.wvb \
    windows-x64-wvstage.exe linux-x64-wvstage.elf || fail
pass 'WVO staging producer reconstruction'

verify_family Compiler-Image-Staging.wvb \
    windows-x64-wvlinkstage.exe linux-x64-wvlinkstage.elf || fail
pass 'compiler-image staging reconstruction'

verify_family Compiler-Image-Canonical-Transport.wvb \
    windows-x64-wvimagetransport.exe linux-x64-wvimagetransport.elf || fail
pass 'compiler-image transport reconstruction'

echo "Tests: $tests, Passed: $passed, Failed: 0"
