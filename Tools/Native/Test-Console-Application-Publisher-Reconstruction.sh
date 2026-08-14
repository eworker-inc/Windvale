#!/usr/bin/env bash
set -uo pipefail

if [[ $# -ne 0 ]]; then
    echo 'Usage: ./Tools/Native/Test-Console-Application-Publisher-Reconstruction.sh' >&2
    exit 64
fi

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
candidate="$repository_root/Artifacts/Native-Console-Application-Publisher-Candidate"
raw_lowerer="$repository_root/Artifacts/Native-Wvb-To-Wvo-Candidate/Wvb-To-Wvo.elf"
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
    echo 'FAIL  console-application publisher reconstruction'
    echo "Tests: $tests, Passed: $passed, Failed: $((tests - passed))"
    exit 1
}

check_file "$candidate/Console-Application-Publisher.wvb" 115107 \
    e8121fb76c7cc39b159d53a3c28d1da8bc2d44968d630495c692a7761656923d || fail
check_file "$candidate/Console-Application-Publisher.wvo" 1139440 \
    259c7d746c3a217c32706bfd617cf66894066bd2e50850cbe5733ac3338e4952 || fail
check_file "$candidate/windows-x64-wvappublish.exe" 1158656 \
    0bafe84096859f4b88dc14be92c6cdc5336d791b7c5b0a332dccb76b913dd24e || fail
check_file "$candidate/linux-x64-wvappublish.elf" 1156085 \
    e9b8771978c9fb06c3a8ecc55c7b9a3ba1acd24faa541dc669920c10ed792925 || fail
pass 'candidate inventory'

temporary_root=${TMPDIR:-/tmp}
test_directory=$(mktemp -d \
    "$temporary_root/windvale-console-application-publisher-reconstruction-test.XXXXXXXX") || fail
cleanup() {
    case "$test_directory" in
        "$temporary_root"/windvale-console-application-publisher-reconstruction-test.*)
            rm -rf -- "$test_directory"
            ;;
        *)
            echo "Refusing to remove unexpected temporary path: $test_directory" >&2
            return 1
            ;;
    esac
}
trap cleanup EXIT

if "$script_directory/Construct-Console-Application-Publisher.sh" \
    >"$test_directory/Usage.out" 2>"$test_directory/Usage.err"; then
    fail
elif [[ $? -ne 64 ]]; then
    fail
fi
[[ ! -s $test_directory/Usage.out ]] || fail
printf '%s\n' \
    'Usage: ./Tools/Native/Construct-Console-Application-Publisher.sh <windows|linux> <output.exe|output.elf>' \
    >"$test_directory/Usage.expected" || fail
check_equal "$test_directory/Usage.err" "$test_directory/Usage.expected" || fail

"$script_directory/Build-Wvb.sh" \
    "$repository_root/Projects/Tools/Windvale-Console-Application-Publisher.wvproj" \
    "$test_directory/Console-Application-Publisher.wvb" \
    >"$test_directory/Build.out" 2>"$test_directory/Build.err" || fail
printf '%s\n' \
    'build status=Published verification=compiler-aligned functions=95 code-bytes=99846 module-bytes=115107' \
    'publication status=Complete bytes=0x0001c1a3 sha256=e8121fb76c7cc39b159d53a3c28d1da8bc2d44968d630495c692a7761656923d' \
    >"$test_directory/Build.expected" || fail
check_equal "$test_directory/Build.out" "$test_directory/Build.expected" || fail
[[ ! -s $test_directory/Build.err ]] || fail
check_equal "$test_directory/Console-Application-Publisher.wvb" \
    "$candidate/Console-Application-Publisher.wvb" || fail

check_file "$raw_lowerer" 6500352 \
    de7bdb40637208ee05a7987aba0ea88366638e132fb3f7ba5d9730befde316b5 || fail
"$raw_lowerer" "$test_directory/Console-Application-Publisher.wvb" \
    "$test_directory/Console-Application-Publisher.wvo" \
    >"$test_directory/Lower.out" 2>"$test_directory/Lower.err" || fail
printf '%s\n' 'native x64 status=Valid abi=22 code-bytes=1134976 object-bytes=1139440' \
    >"$test_directory/Lower.expected" || fail
check_equal "$test_directory/Lower.out" "$test_directory/Lower.expected" || fail
[[ ! -s $test_directory/Lower.err ]] || fail
check_equal "$test_directory/Console-Application-Publisher.wvo" \
    "$candidate/Console-Application-Publisher.wvo" || fail

"$script_directory/Construct-Console-Application-Publisher.sh" windows \
    "$test_directory/Console-Application-Publisher.exe" \
    >"$test_directory/Windows.out" 2>"$test_directory/Windows.err" || fail
printf '%s\n' \
    'console-application publisher construction status=Valid target=windows bytes=1158656' \
    >"$test_directory/Windows.expected" || fail
check_equal "$test_directory/Windows.out" "$test_directory/Windows.expected" || fail
[[ ! -s $test_directory/Windows.err ]] || fail

"$script_directory/Construct-Console-Application-Publisher.sh" linux \
    "$test_directory/Console-Application-Publisher.elf" \
    >"$test_directory/Linux.out" 2>"$test_directory/Linux.err" || fail
printf '%s\n' \
    'console-application publisher construction status=Valid target=linux bytes=1156085' \
    >"$test_directory/Linux.expected" || fail
check_equal "$test_directory/Linux.out" "$test_directory/Linux.expected" || fail
[[ ! -s $test_directory/Linux.err ]] || fail
check_equal "$test_directory/Console-Application-Publisher.exe" \
    "$candidate/windows-x64-wvappublish.exe" || fail
check_equal "$test_directory/Console-Application-Publisher.elf" \
    "$candidate/linux-x64-wvappublish.elf" || fail
pass 'exact native WVB, WVO, and paired publisher reconstruction'

probe="$test_directory/Aot-Probe"
mkdir -- "$probe" || fail
"$script_directory/Construct-Aot-Composition-Probe.sh" "$probe" \
    >"$test_directory/Probe.out" 2>"$test_directory/Probe.err" || fail
printf '%s\n' 'native AOT composition probe status=Complete artifacts=6' \
    >"$test_directory/Probe.expected" || fail
check_equal "$test_directory/Probe.out" "$test_directory/Probe.expected" || fail
[[ ! -s $test_directory/Probe.err ]] || fail
fixture="$probe/Return-42.elf"

check_file "$fixture" 8304 \
    fe525b84b9bf902677a5c7beb36872dfd72e7d6d0f12bfb5c95d491c4e1cd3f7 || fail
cp -- "$fixture" \
    "$test_directory/Subject.elf" || fail
cp -- "$candidate/Console-Application-Publisher.wvb" \
    "$test_directory/Destination.elf" || fail
chmod u+x "$test_directory/Console-Application-Publisher.elf" || fail
"$test_directory/Console-Application-Publisher.elf" \
    "$test_directory/Subject.elf" "$test_directory/Destination.elf" \
    >"$test_directory/Publish.out" 2>"$test_directory/Publish.err" || fail
printf '%s\n' \
    'publication status=Complete bytes=0x00002070 sha256=fe525b84b9bf902677a5c7beb36872dfd72e7d6d0f12bfb5c95d491c4e1cd3f7' \
    >"$test_directory/Publish.expected" || fail
check_equal "$test_directory/Publish.out" "$test_directory/Publish.expected" || fail
[[ ! -s $test_directory/Publish.err ]] || fail
check_equal "$test_directory/Subject.elf" "$test_directory/Destination.elf" || fail

cp -- "$candidate/Console-Application-Publisher.wvb" \
    "$test_directory/Invalid.elf" || fail
if "$test_directory/Console-Application-Publisher.elf" \
    "$test_directory/Invalid.elf" "$test_directory/Destination.elf" \
    >"$test_directory/Reject.out" 2>"$test_directory/Reject.err"; then
    fail
elif [[ $? -ne 1 ]]; then
    fail
fi
[[ ! -s $test_directory/Reject.out ]] || fail
printf '%s\n' 'publication status=Rejected phase=console-application' \
    >"$test_directory/Reject.expected" || fail
check_equal "$test_directory/Reject.err" "$test_directory/Reject.expected" || fail
check_equal "$test_directory/Invalid.elf" \
    "$candidate/Console-Application-Publisher.wvb" || fail
check_equal "$test_directory/Destination.elf" \
    "$fixture" || fail
[[ -z $(find "$test_directory" -maxdepth 1 -name '.wvpublish-*' -print -quit) ]] || fail
pass 'current-host independent version-1 publication and rejected-input preservation'

echo "Tests: $tests, Passed: $passed, Failed: 0"
