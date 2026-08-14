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

check_file "$candidate/Wvo-Object.wvb" 73322 \
    40f7b7efcff5b6e5bbc3c878cf5f0147ee92af208d43d54ab8a04f87ec1e9070 || fail
check_file "$candidate/Wvo-Object.wvo" 1022822 \
    bab6b73e5edd6b0b2726380ba2ff10859fbbcc37481572457b508bbd0d67c2ae || fail
check_file "$candidate/Wvo-Object.exe" 1037312 \
    5362372e826958470eee7d90eb01938de5b91dcb3e1b0f952722e00578a82d03 || fail
check_file "$candidate/Wvo-Object.elf" 1036288 \
    fcfd134222b05482a6ac432fc4acbfb72f3dfce92c3c646fc17595ddb078b840 || fail
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

"$candidate/Wvo-Object.elf" \
    >"$test_directory/Self-Test.out" 2>"$test_directory/Self-Test.err" || fail
[[ ! -s $test_directory/Self-Test.out ]] || fail
[[ ! -s $test_directory/Self-Test.err ]] || fail

"$candidate/Wvo-Object.elf" check "$candidate/Wvo-Object.wvo" \
    >"$test_directory/Check.out" 2>"$test_directory/Check.err" || fail
[[ ! -s $test_directory/Check.out ]] || fail
[[ ! -s $test_directory/Check.err ]] || fail

"$candidate/Wvo-Object.elf" verify "$candidate/Wvo-Object.wvo" \
    >"$test_directory/Verify.out" 2>"$test_directory/Verify.err" || fail
printf '%s\n' \
    'Verified object: X86ˉ64' \
    'SHA-256: bab6b73e5edd6b0b2726380ba2ff10859fbbcc37481572457b508bbd0d67c2ae' \
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
check_file "$candidate/Wvo-Object.wvo" 1022822 \
    bab6b73e5edd6b0b2726380ba2ff10859fbbcc37481572457b508bbd0d67c2ae || fail
pass 'current-host compatibility and profile isolation'

echo "Tests: $tests, Passed: $passed, Failed: 0"
