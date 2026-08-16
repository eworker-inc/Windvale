#!/usr/bin/env bash
set -uo pipefail

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
candidate="$repository_root/Artifacts/Native-Compiler-Reconstruction-Candidate"
tests=0
passed=0

verify_file() {
    local path=$1
    local expected_bytes=$2
    local expected_sha256=$3
    [[ -f $path ]] || return 1
    local actual_bytes
    actual_bytes=$(wc -c < "$path") || return 1
    [[ $actual_bytes -eq $expected_bytes ]] || return 1
    local digest_line
    local actual_sha256
    digest_line=$(sha256sum -- "$path") || return 1
    actual_sha256=${digest_line%% *}
    [[ $actual_sha256 == "$expected_sha256" ]]
}
pass() {
    tests=$((tests + 1))
    passed=$((passed + 1))
    echo "PASS  $1"
}
fail() {
    tests=$((tests + 1))
    echo 'FAIL  compiler reconstruction'
    echo "Tests: $tests, Passed: $passed, Failed: $((tests - passed))"
    exit 1
}

verify_file "$candidate/Wvb/Windvale-Compiler.wvb" 931035 \
    13558d9dbc0d185b161b770824aa29ff90b8873903b2b5d7184a23950a6fc1e4 || fail
verify_file "$candidate/windows-x64/wvcompiler.exe" 27898368 \
    4009e6747bbf9a6d2b0b2ec90e2368ca50fda863d445534f15ef96e22a657b34 || fail
verify_file "$candidate/linux-x64/wvcompiler.elf" 27897856 \
    c266adf20fe2927a446483f68880ef323c480f011b0c26384716ea2f651bcd65 || fail
verify_file "$candidate/Wvb/Compiler-Build-Driver.wvb" 1162338 \
    a214662da422443cd70c4be12c8f0bd06cbb5bce9fe3a56e2a52c46a37445a20 || fail
verify_file "$candidate/windows-x64/wvbuild.exe" 30381568 \
    b0d58f4d8d6d32e09d45358035ce4521410ec1280f11fa72b66245e621ba49a3 || fail
verify_file "$candidate/linux-x64/wvbuild.elf" 30380032 \
    b4fdc30a7242e03f7166491bf6b415aa3b5dce8ff0e16444f6ccb24c5bcb03a0 || fail
pass 'candidate inventory'

if "$script_directory/Construct-Compiler-Reconstruction.sh" >/dev/null 2>&1; then
    fail
elif [[ $? -ne 64 ]]; then
    fail
fi
pass 'usage rejection'

temporary_root=${TMPDIR:-/tmp}
test_directory=$(mktemp -d "$temporary_root/windvale-compiler-reconstruction-test.XXXXXXXX") || fail
cleanup() {
    case "$test_directory" in
        "$temporary_root"/windvale-compiler-reconstruction-test.*)
            rm -rf -- "$test_directory"
            ;;
        *)
            echo "Refusing to remove unexpected temporary path: $test_directory" >&2
            return 1
            ;;
    esac
}
trap cleanup EXIT

"$script_directory/Construct-Compiler-Reconstruction.sh" "$test_directory" \
    >"$test_directory/Construct.out" 2>"$test_directory/Construct.err" || fail
grep -Fx 'native compiler reconstruction status=Complete compiler-bytes=931035 native-bytes=27867015 entry-offset=51356 chunks=7 build-driver-bytes=1162338 build-driver-entry-offset=220460 build-driver-chunks=8' \
    "$test_directory/Construct.out" >/dev/null || fail
[[ ! -s $test_directory/Construct.err ]] || fail
verify_file "$test_directory/Wvb/Windvale-Compiler.wvb" 931035 \
    13558d9dbc0d185b161b770824aa29ff90b8873903b2b5d7184a23950a6fc1e4 || fail
verify_file "$test_directory/windows-x64/wvcompiler.exe" 27898368 \
    4009e6747bbf9a6d2b0b2ec90e2368ca50fda863d445534f15ef96e22a657b34 || fail
verify_file "$test_directory/linux-x64/wvcompiler.elf" 27897856 \
    c266adf20fe2927a446483f68880ef323c480f011b0c26384716ea2f651bcd65 || fail
verify_file "$test_directory/Wvb/Compiler-Build-Driver.wvb" 1162338 \
    a214662da422443cd70c4be12c8f0bd06cbb5bce9fe3a56e2a52c46a37445a20 || fail
verify_file "$test_directory/windows-x64/wvbuild.exe" 30381568 \
    b0d58f4d8d6d32e09d45358035ce4521410ec1280f11fa72b66245e621ba49a3 || fail
verify_file "$test_directory/linux-x64/wvbuild.elf" 30380032 \
    b4fdc30a7242e03f7166491bf6b415aa3b5dce8ff0e16444f6ccb24c5bcb03a0 || fail
pass 'native paired reconstruction'

echo "Tests: $tests, Passed: $passed, Failed: 0"
