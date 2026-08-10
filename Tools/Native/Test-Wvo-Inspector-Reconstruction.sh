#!/usr/bin/env bash
set -uo pipefail

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
candidate="$repository_root/Artifacts/Native-Wvo-Object-Candidate"
tests=0
passed=0

check_file() {
    local path=$1 expected_bytes=$2 expected_sha256=$3
    local actual_bytes actual_line actual_sha256
    [[ -f $path ]] || return 1
    actual_bytes=$(wc -c < "$path") || return 1
    [[ $actual_bytes -eq $expected_bytes ]] || return 1
    actual_line=$(sha256sum -- "$path") || return 1
    actual_sha256=${actual_line%% *}
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
    echo 'FAIL  WVO inspector reconstruction'
    echo "Tests: $tests, Passed: $passed, Failed: $((tests - passed))"
    exit 1
}

check_file "$candidate/Wvo-Object.wvb" 61008 \
    a630d49f0549c865644d8052fbff7e8bf2b6a6dcd013e1187d4356d49cd188db || fail
check_file "$candidate/Wvo-Object.wvo" 591723 \
    f45b14c33a7615209a2a16f6caf0bee041bdb5e2f46fd868792222e774fdb30c || fail
check_file "$candidate/Wvo-Object.exe" 606208 \
    bb39e58d51e7b6c3eab2690995ee52fc958557ab03cfcbcb9b5ef0f3070157d2 || fail
check_file "$candidate/Wvo-Object.elf" 606208 \
    bf94145cee63a4d7014bd7a31a40832017f025b7d8086a4ae3875385ba8345c1 || fail
[[ -x $candidate/Wvo-Object.elf ]] || fail
pass 'candidate inventory'

if "$script_directory/Construct-Wvo-Inspector-Reconstruction.sh" >/dev/null 2>&1; then
    fail
elif [[ $? -ne 64 ]]; then
    fail
fi

temporary_root=${TMPDIR:-/tmp}
test_directory=$(mktemp -d "$temporary_root/windvale-wvo-inspector-reconstruction-test.XXXXXXXX") || fail
cleanup() {
    case "$test_directory" in
        "$temporary_root"/windvale-wvo-inspector-reconstruction-test.*)
            rm -rf -- "$test_directory"
            ;;
        *)
            echo "Refusing to remove unexpected temporary path: $test_directory" >&2
            return 1
            ;;
    esac
}
trap cleanup EXIT

"$script_directory/Construct-Wvo-Inspector-Reconstruction.sh" "$test_directory" \
    >"$test_directory/Construct.out" 2>"$test_directory/Construct.err" || fail
printf '%s\n' 'native WVO inspector reconstruction status=Complete artifacts=4' \
    >"$test_directory/Construct.expected" || fail
check_equal "$test_directory/Construct.out" "$test_directory/Construct.expected" || fail
[[ ! -s $test_directory/Construct.err ]] || fail
check_equal "$test_directory/Wvo-Object.wvb" "$candidate/Wvo-Object.wvb" || fail
check_equal "$test_directory/Wvo-Object.wvo" "$candidate/Wvo-Object.wvo" || fail
check_equal "$test_directory/Wvo-Object.exe" "$candidate/Wvo-Object.exe" || fail
check_equal "$test_directory/Wvo-Object.elf" "$candidate/Wvo-Object.elf" || fail
pass 'exact paired reconstruction'

"$candidate/Wvo-Object.elf" verify "$candidate/Wvo-Object.wvo" \
    >"$test_directory/Verify.out" 2>"$test_directory/Verify.err" || fail
printf '%s\n' \
    'Verified object: X86ˉ64' \
    'SHA-256: f45b14c33a7615209a2a16f6caf0bee041bdb5e2f46fd868792222e774fdb30c' \
    >"$test_directory/Verify.expected" || fail
check_equal "$test_directory/Verify.out" "$test_directory/Verify.expected" || fail
[[ ! -s $test_directory/Verify.err ]] || fail

if "$script_directory/Admit-Hosted-Verifier-Publisher.sh" linux \
    "$candidate/Wvo-Object.elf" \
    >"$test_directory/Isolation.out" 2>"$test_directory/Isolation.err"; then
    fail
elif [[ $? -ne 2 ]]; then
    fail
fi
[[ ! -s $test_directory/Isolation.out ]] || fail
printf '%s\n' 'native hosted verifier publisher application status=Rejected' \
    >"$test_directory/Isolation.expected" || fail
check_equal "$test_directory/Isolation.err" "$test_directory/Isolation.expected" || fail
check_file "$candidate/Wvo-Object.wvo" 591723 \
    f45b14c33a7615209a2a16f6caf0bee041bdb5e2f46fd868792222e774fdb30c || fail
pass 'current-host compatibility and profile isolation'

echo "Tests: $tests, Passed: $passed, Failed: 0"
