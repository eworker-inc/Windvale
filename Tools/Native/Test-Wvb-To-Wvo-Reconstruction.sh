#!/usr/bin/env bash
set -uo pipefail

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
candidate="$repository_root/Artifacts/Native-Wvb-To-Wvo-Candidate"
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
    echo 'FAIL  WVB-to-WVO reconstruction'
    echo "Tests: $tests, Passed: $passed, Failed: $((tests - passed))"
    exit 1
}

check_file "$candidate/Wvb-To-Wvo.wvb" 416464 \
    8ce81d1c657f558f66b5b503d0edabad371b1f601297892f0d139e06176bdc27 || fail
check_file "$candidate/Wvb-To-Wvo.exe" 5997056 \
    4c2bf61306b81851424dbc02e7eb345ac34bcc9ffacd7727e8378c7b9e5dae07 || fail
check_file "$candidate/Wvb-To-Wvo.elf" 5996544 \
    55df0cd2a679491bc1a3da50b6bc67b5f512f2a4698721faea6becb556a2a46f || fail
check_file "$candidate/Return-42.wvb" 174 \
    7933c4ba0cb854477a95750966f9532c2b9eb5888e55ec9ae64ebdf552a08f31 || fail
check_file "$candidate/Return-42.wvo" 479 \
    0d1829bbbc77f3ee3910a70f98528e1078117480332adb5a2d09df8b2d25f3b5 || fail
pass 'candidate inventory'

if "$script_directory/Construct-Wvb-To-Wvo-Reconstruction.sh" >/dev/null 2>&1; then
    fail
elif [[ $? -ne 64 ]]; then
    fail
fi

temporary_root=${TMPDIR:-/tmp}
test_directory=$(mktemp -d "$temporary_root/windvale-wvb-to-wvo-reconstruction-test.XXXXXXXX") || fail
cleanup() {
    case "$test_directory" in
        "$temporary_root"/windvale-wvb-to-wvo-reconstruction-test.*)
            rm -rf -- "$test_directory"
            ;;
        *)
            echo "Refusing to remove unexpected temporary path: $test_directory" >&2
            return 1
            ;;
    esac
}
trap cleanup EXIT

"$script_directory/Construct-Wvb-To-Wvo-Reconstruction.sh" "$test_directory" \
    >"$test_directory/Construct.out" 2>"$test_directory/Construct.err" || fail
printf '%s\n' 'native WVB-to-WVO reconstruction status=Complete artifacts=5' \
    >"$test_directory/Expected.out" || fail
check_equal "$test_directory/Construct.out" "$test_directory/Expected.out" || fail
[[ ! -s $test_directory/Construct.err ]] || fail

check_equal "$test_directory/Wvb-To-Wvo.wvb" "$candidate/Wvb-To-Wvo.wvb" || fail
check_equal "$test_directory/Wvb-To-Wvo.exe" "$candidate/Wvb-To-Wvo.exe" || fail
check_equal "$test_directory/Wvb-To-Wvo.elf" "$candidate/Wvb-To-Wvo.elf" || fail
pass 'native paired lowerer reconstruction'

check_equal "$test_directory/Return-42.wvb" "$candidate/Return-42.wvb" || fail
check_equal "$test_directory/Return-42.wvo" "$candidate/Return-42.wvo" || fail
pass 'current-host Return-42 lowering'

echo "Tests: $tests, Passed: $passed, Failed: 0"
