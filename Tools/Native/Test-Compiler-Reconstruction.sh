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

verify_file "$candidate/Wvb/Windvale-Compiler.wvb" 927274 \
    d3dbadd987f10a98ebd90d1357973dca055094e2dbd3cc3e0e90afb3c3c17fae || fail
verify_file "$candidate/windows-x64/wvcompiler.exe" 27776000 \
    11c01839d63a13570e02873f760614eef42089a29a282083f5def3e968038d78 || fail
verify_file "$candidate/linux-x64/wvcompiler.elf" 27774976 \
    93651adc36557aaa895627e8d8aa022b8765fc4f6cfaafbb5dc7c0a263287f67 || fail
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
grep -Fx 'native compiler reconstruction status=Complete compiler-bytes=927274 native-bytes=27744550 entry-offset=43146 chunks=7' \
    "$test_directory/Construct.out" >/dev/null || fail
[[ ! -s $test_directory/Construct.err ]] || fail
verify_file "$test_directory/Wvb/Windvale-Compiler.wvb" 927274 \
    d3dbadd987f10a98ebd90d1357973dca055094e2dbd3cc3e0e90afb3c3c17fae || fail
verify_file "$test_directory/windows-x64/wvcompiler.exe" 27776000 \
    11c01839d63a13570e02873f760614eef42089a29a282083f5def3e968038d78 || fail
verify_file "$test_directory/linux-x64/wvcompiler.elf" 27774976 \
    93651adc36557aaa895627e8d8aa022b8765fc4f6cfaafbb5dc7c0a263287f67 || fail
pass 'native paired reconstruction'

echo "Tests: $tests, Passed: $passed, Failed: 0"
