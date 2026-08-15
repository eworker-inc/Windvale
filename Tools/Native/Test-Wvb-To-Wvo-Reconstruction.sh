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

check_file "$candidate/Wvb-To-Wvo.wvb" 520966 \
    ce190159783b48912ff71326d937a72a27b5178b07b7e52de71742a53cd12b56 || fail
check_file "$candidate/Wvb-To-Wvo.exe" 7483904 \
    496d9e00c682af316b69c0e7639858fa0da171b4d467c4838b1ed774ac18b4cc || fail
check_file "$candidate/Wvb-To-Wvo.elf" 7483392 \
    30ffb3ce953b173d1bbee77c8e440e901806a676f7ec17683b5cfe3953ebb441 || fail
check_file "$candidate/Return-42.wvb" 174 \
    7933c4ba0cb854477a95750966f9532c2b9eb5888e55ec9ae64ebdf552a08f31 || fail
check_file "$candidate/Return-42.wvo" 479 \
    0d1829bbbc77f3ee3910a70f98528e1078117480332adb5a2d09df8b2d25f3b5 || fail
check_file "$candidate/Metadata.wvb" 369 \
    94b41f5016722c9e5bf16ace5ec933acc35c14efdd4e08fe11fd582a62b58ffa || fail
check_file "$candidate/Metadata.wvo" 1151 \
    6f1cb53ec55448a7552f2ff5b380446964d16ed32a60aa28b8e55a9ca590845d || fail
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
printf '%s\n' 'native WVB-to-WVO reconstruction status=Complete artifacts=7' \
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

check_equal "$test_directory/Metadata.wvb" "$candidate/Metadata.wvb" || fail
check_equal "$test_directory/Metadata.wvo" "$candidate/Metadata.wvo" || fail
pass 'current-host independent-metadata lowering'

echo "Tests: $tests, Passed: $passed, Failed: 0"
