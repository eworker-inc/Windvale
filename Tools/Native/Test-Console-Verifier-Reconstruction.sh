#!/usr/bin/env bash
set -uo pipefail

if [[ $# -ne 0 ]]; then
    echo 'Usage: ./Tools/Native/Test-Console-Verifier-Reconstruction.sh' >&2
    exit 64
fi

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
candidate="$repository_root/Artifacts/Native-Console-Application-Verifier-Candidate"
fixture="$repository_root/Artifacts/Native-Aot-Composition-Probe/Return-42.elf"
tests=0
passed=0

check_file() {
    local path=$1 expected_bytes=$2 expected_sha256=$3
    local actual_bytes digest_line actual_sha256
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
    echo 'FAIL  console verifier reconstruction'
    echo "Tests: $tests, Passed: $passed, Failed: $((tests - passed))"
    exit 1
}

check_file "$candidate/Console-Application-Verifier.wvb" 105006 \
    1dcd5f2aeebd974649e64c90d9f473e1e75f7d13dbcde2814de1dded72cf2c0c || fail
check_file "$candidate/Console-Application-Verifier.wvo" 1049519 \
    51292e4d300d4a6bb6ce4879915bba5304de70c9deafdf4eb6ff6a54a6dbf150 || fail
check_file "$candidate/windows-x64-wvappverify.exe" \
    1063936 05b5f5b3e3999a0ef3537f0908967069a12f17de09753fc90e8a4c7542dc9d3f || fail
check_file "$candidate/linux-x64-wvappverify.elf" \
    1064960 c2700e5e68711d7b8e8a8f7e9573d87dfa27c3676a034a314310ef59045e5f1a || fail
[[ -x $candidate/linux-x64-wvappverify.elf ]] || fail
pass 'candidate inventory'

temporary_root=${TMPDIR:-/tmp}
test_directory=$(mktemp -d "$temporary_root/windvale-console-verifier-reconstruction-test.XXXXXXXX") || fail
cleanup() {
    case "$test_directory" in
        "$temporary_root"/windvale-console-verifier-reconstruction-test.*)
            rm -rf -- "$test_directory"
            ;;
        *)
            echo "Refusing to remove unexpected path: $test_directory" >&2
            return 1
            ;;
    esac
}
trap cleanup EXIT
if "$script_directory/Construct-Console-Verifier-Reconstruction.sh" \
    >"$test_directory/Usage.out" 2>"$test_directory/Usage.err"; then
    fail
elif [[ $? -ne 64 ]]; then
    fail
fi
[[ ! -s $test_directory/Usage.out ]] || fail
printf '%s\n' 'Usage: ./Tools/Native/Construct-Console-Verifier-Reconstruction.sh <existing-separate-output-directory>' \
    >"$test_directory/Usage.expected" || fail
check_equal "$test_directory/Usage.err" "$test_directory/Usage.expected" || fail
empty_snapshot="$test_directory/Empty.bin"
: > "$empty_snapshot" || fail

"$script_directory/Construct-Console-Verifier-Reconstruction.sh" "$test_directory" \
    >"$test_directory/Construct.out" 2>"$test_directory/Construct.err" || fail
printf '%s\n' 'native console verifier reconstruction status=Complete artifacts=4' \
    >"$test_directory/Construct.expected" || fail
check_equal "$test_directory/Construct.out" "$test_directory/Construct.expected" || fail
[[ ! -s $test_directory/Construct.err ]] || fail
check_equal "$test_directory/Console-Application-Verifier.wvb" "$candidate/Console-Application-Verifier.wvb" || fail
check_equal "$test_directory/Console-Application-Verifier.wvo" "$candidate/Console-Application-Verifier.wvo" || fail
check_equal "$test_directory/windows-x64-wvappverify.exe" "$candidate/windows-x64-wvappverify.exe" || fail
check_equal "$test_directory/linux-x64-wvappverify.elf" "$candidate/linux-x64-wvappverify.elf" || fail
pass 'usage and exact paired reconstruction'

application="$test_directory/linux-x64-wvappverify.elf"
check_file "$fixture" 8304 \
    fe525b84b9bf902677a5c7beb36872dfd72e7d6d0f12bfb5c95d491c4e1cd3f7 || fail
"$application" "$fixture" "$empty_snapshot" \
    >"$test_directory/Compatibility.out" 2>"$test_directory/Compatibility.err" || fail
printf '%s\n' 'console application status=Valid target=2 bytes=8304 native-bytes=406 entry=0' \
    >"$test_directory/Compatibility.expected" || fail
check_equal "$test_directory/Compatibility.out" "$test_directory/Compatibility.expected" || fail
[[ ! -s $test_directory/Compatibility.err ]] || fail

if "$application" "$application" "$empty_snapshot" \
    >"$test_directory/Rejection.out" 2>"$test_directory/Rejection.err"; then
    fail
elif [[ $? -ne 1 ]]; then
    fail
fi
[[ ! -s $test_directory/Rejection.out ]] || fail
check_file "$test_directory/Rejection.err" \
    76 78ca7089fbe5e559ef1820d84e4b7cbc08462a294a03f89f93f4f9ae984055c0 || fail
check_file "$application" \
    1064960 c2700e5e68711d7b8e8a8f7e9573d87dfa27c3676a034a314310ef59045e5f1a || fail
check_file "$fixture" 8304 \
    fe525b84b9bf902677a5c7beb36872dfd72e7d6d0f12bfb5c95d491c4e1cd3f7 || fail
[[ ! -s $empty_snapshot ]] || fail
pass 'current-host two-snapshot compatibility and exact hosted-container rejection'

echo "Tests: $tests, Passed: $passed, Failed: 0"
