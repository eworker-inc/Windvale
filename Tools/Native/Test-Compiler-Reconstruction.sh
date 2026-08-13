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

verify_file "$candidate/Wvb/Windvale-Compiler.wvb" 929711 \
    79150787761c7d5e6013ddcb136e518d1388811c99551de443adb6f7a3a23d91 || fail
verify_file "$candidate/windows-x64/wvcompiler.exe" 27904000 \
    e24feb288cef6284ed0444e73e9317eb7e98df7eeb9be551ac9b13f6f896c455 || fail
verify_file "$candidate/linux-x64/wvcompiler.elf" 27906048 \
    e3d99aefb66b70d468d8e563db9786030a92baeb6c193bb2dcde5ea3b4d446b2 || fail
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
grep -Fx 'native compiler reconstruction status=Complete compiler-bytes=929711 native-bytes=27872534 entry-offset=43146 chunks=7' \
    "$test_directory/Construct.out" >/dev/null || fail
[[ ! -s $test_directory/Construct.err ]] || fail
verify_file "$test_directory/Wvb/Windvale-Compiler.wvb" 929711 \
    79150787761c7d5e6013ddcb136e518d1388811c99551de443adb6f7a3a23d91 || fail
verify_file "$test_directory/windows-x64/wvcompiler.exe" 27904000 \
    e24feb288cef6284ed0444e73e9317eb7e98df7eeb9be551ac9b13f6f896c455 || fail
verify_file "$test_directory/linux-x64/wvcompiler.elf" 27906048 \
    e3d99aefb66b70d468d8e563db9786030a92baeb6c193bb2dcde5ea3b4d446b2 || fail
pass 'native paired reconstruction'

echo "Tests: $tests, Passed: $passed, Failed: 0"
