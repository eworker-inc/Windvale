#!/usr/bin/env bash
set -uo pipefail

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
candidate="$repository_root/Artifacts/Native-Wvo-Publisher-Candidate"
tests=0
passed=0

check_file() {
    local path=$1
    local expected_bytes=$2
    local expected_sha256=$3
    local actual_bytes
    local digest_line
    local actual_sha256
    [[ -f $path ]] || return 1
    actual_bytes=$(wc -c < "$path") || return 1
    [[ $actual_bytes -eq $expected_bytes ]] || return 1
    digest_line=$(sha256sum -- "$path") || return 1
    actual_sha256=${digest_line%% *}
    [[ $actual_sha256 == "$expected_sha256" ]]
}
check_equal() {
    [[ -f $1 && -f $2 ]] || return 1
    cmp --silent -- "$1" "$2"
}
pass() {
    tests=$((tests + 1))
    passed=$((passed + 1))
    echo "PASS  $1"
}
fail() {
    tests=$((tests + 1))
    echo 'FAIL  WVO publisher reconstruction'
    echo "Tests: $tests, Passed: $passed, Failed: $((tests - passed))"
    exit 1
}

check_file "$candidate/Wvo-Publisher.wvb" 41365 \
    4e8c81da38f5eb06f9334c2d2c5e35120a13e73bac3a9375b5e6a2eff04438c5 || fail
check_file "$candidate/windows-x64-wvopublish.exe" 430080 \
    76f632ffa7998a6cce0386456fee98f02cbb5ec424d0d914a7e1f06ff3853910 || fail
check_file "$candidate/linux-x64-wvopublish.elf" 426997 \
    2889237d7fdb20b1d420c05834f19183d18b02112e3f4eea0ed7ff43414814f2 || fail
pass 'candidate inventory'

if "$script_directory/Construct-Wvo-Publisher.sh" >/dev/null 2>&1; then
    fail
elif [[ $? -ne 64 ]]; then
    fail
fi

temporary_root=${TMPDIR:-/tmp}
test_directory=$(mktemp -d "$temporary_root/windvale-wvo-publisher-reconstruction-test.XXXXXXXX") || fail
cleanup() {
    case "$test_directory" in
        "$temporary_root"/windvale-wvo-publisher-reconstruction-test.*)
            rm -rf -- "$test_directory"
            ;;
        *)
            echo "Refusing to remove unexpected temporary path: $test_directory" >&2
            return 1
            ;;
    esac
}
trap cleanup EXIT

"$script_directory/Build-Wvb.sh" \
    "$repository_root/Windvale-Wvo-Publisher.wvproj" \
    "$test_directory/Wvo-Publisher.wvb" \
    >"$test_directory/Build.out" 2>"$test_directory/Build.err" || fail
printf '%s\n' \
    'build status=Published verification=compiler-aligned functions=37 code-bytes=34099 module-bytes=41365' \
    'publication status=Complete bytes=0x0000a195 sha256=4e8c81da38f5eb06f9334c2d2c5e35120a13e73bac3a9375b5e6a2eff04438c5' \
    >"$test_directory/Build.expected" || fail
check_equal "$test_directory/Build.out" "$test_directory/Build.expected" || fail
[[ ! -s $test_directory/Build.err ]] || fail
check_equal "$test_directory/Wvo-Publisher.wvb" \
    "$candidate/Wvo-Publisher.wvb" || fail

"$script_directory/Construct-Wvo-Publisher.sh" windows \
    "$test_directory/Wvo-Publisher.exe" \
    >"$test_directory/Windows.out" 2>"$test_directory/Windows.err" || fail
printf '%s\n' 'WVO publisher construction status=Valid target=windows bytes=430080' \
    >"$test_directory/Windows.expected" || fail
check_equal "$test_directory/Windows.out" "$test_directory/Windows.expected" || fail
[[ ! -s $test_directory/Windows.err ]] || fail

"$script_directory/Construct-Wvo-Publisher.sh" linux \
    "$test_directory/Wvo-Publisher.elf" \
    >"$test_directory/Linux.out" 2>"$test_directory/Linux.err" || fail
printf '%s\n' 'WVO publisher construction status=Valid target=linux bytes=426997' \
    >"$test_directory/Linux.expected" || fail
check_equal "$test_directory/Linux.out" "$test_directory/Linux.expected" || fail
[[ ! -s $test_directory/Linux.err ]] || fail

check_equal "$test_directory/Wvo-Publisher.exe" \
    "$candidate/windows-x64-wvopublish.exe" || fail
check_equal "$test_directory/Wvo-Publisher.elf" \
    "$candidate/linux-x64-wvopublish.elf" || fail
pass 'native WVB and paired WVO publisher reconstruction'

echo "Tests: $tests, Passed: $passed, Failed: 0"
