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

check_file "$candidate/Wvo-Object.wvb" 74713 \
    fbea7318001a67c464f0ceb8a7d590cbf73244de184659f8254e9f222a4053bf || fail
check_file "$candidate/Wvo-Object.wvo" 1043860 \
    ffaab3f711c7fe84ec7ed85eababc9eb77d9897c87c1b8289bce86fbce41a874 || fail
check_file "$candidate/Wvo-Object.exe" 1058304 \
    182739a91046cf3563924668cf724ba1ad17ac5007d91c023e6687de7f2b83a4 || fail
check_file "$candidate/Wvo-Object.elf" 1056768 \
    b8f0367a8ced12227c9554101152bd5199ec0fd32e5e78210f5dd8a0761b81c7 || fail
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
    'SHA-256: ffaab3f711c7fe84ec7ed85eababc9eb77d9897c87c1b8289bce86fbce41a874' \
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
check_file "$candidate/Wvo-Object.wvo" 1043860 \
    ffaab3f711c7fe84ec7ed85eababc9eb77d9897c87c1b8289bce86fbce41a874 || fail
pass 'current-host compatibility and profile isolation'

echo "Tests: $tests, Passed: $passed, Failed: 0"
