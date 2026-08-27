#!/usr/bin/env bash
set -uo pipefail

script_directory=$(CDPATH= cd -- "$(dirname -- "$0")" && pwd -P)
repository_root=$(CDPATH= cd -- "$script_directory/../.." && pwd -P)
candidate="$repository_root/Artifacts/Native-Wvb-Runner-Candidate"
constructor="$repository_root/Tools/Native/Construct-Wvb-Runner-Reconstruction.sh"
fixture="$repository_root/Artifacts/Native-Wvb-To-Wvo-Candidate/Return-42.wvb"
invalid_fixture="$repository_root/Artifacts/Native-Wvb-To-Wvo-Candidate/Return-42.wvo"
passed=0
failed=0

check_file() {
    local path=$1 expected_bytes=$2 expected_sha=$3
    [[ -f $path ]] || return 1
    [[ $(wc -c < "$path") -eq $expected_bytes ]] || return 1
    printf '%s  %s\n' "$expected_sha" "$path" | sha256sum --check --strict --quiet
}

report_file() {
    local path=$1
    if [[ ! -f $path ]]; then
        printf 'diagnostic file=%s status=Missing\n' "$(basename -- "$path")"
        return
    fi
    local bytes sha
    bytes=$(wc -c < "$path")
    sha=$(sha256sum "$path")
    printf 'diagnostic file=%s bytes=%s sha256=%s\n' \
        "$(basename -- "$path")" "$bytes" "${sha%% *}"
}

check_runtime() {
    local runner=$1
    run_status=255
    report_status=255
    option_status=255
    reject_status=255
    cp -- "$invalid_fixture" "$test_directory/Invalid.wvb" || return 1
    "$runner" "$fixture" >"$test_directory/Run.out" 2>"$test_directory/Run.err"
    run_status=$?
    "$runner" "$fixture" --report-steps >"$test_directory/Report.out" 2>"$test_directory/Report.err"
    report_status=$?
    "$runner" "$fixture" --unknown >"$test_directory/Option.out" 2>"$test_directory/Option.err"
    option_status=$?
    "$runner" "$test_directory/Invalid.wvb" >"$test_directory/Reject.out" 2>"$test_directory/Reject.err"
    reject_status=$?
    [[ $run_status -eq 0 && $report_status -eq 0 && $option_status -eq 64 && $reject_status -eq 1 ]] &&
        check_file "$fixture" 174 7933c4ba0cb854477a95750966f9532c2b9eb5888e55ec9ae64ebdf552a08f31 &&
        check_file "$test_directory/Run.out" 11 bf24325cd27b27403c7b8053820193dcce360f640f7f394742b660ce5fe3cd4e &&
        check_file "$test_directory/Report.out" 27 16d83153e975eefdac7828db275b4cbd3cdd4a783ed5430c442ed4717936a3e5 &&
        check_file "$test_directory/Option.err" 43 fd8455c7428eece156befe036c10c6927efee163a7315dad72c730f6e2bcef64 &&
        [[ ! -s $test_directory/Run.err && ! -s $test_directory/Report.err && ! -s $test_directory/Option.out && ! -s $test_directory/Reject.out ]] &&
        check_file "$test_directory/Reject.err" 68 a88ea127be32ffbde27b0944be4e8c232155bec2cbd8ba3ae0449d7d20dfac0a &&
        check_file "$test_directory/Invalid.wvb" 479 0d1829bbbc77f3ee3910a70f98528e1078117480332adb5a2d09df8b2d25f3b5
}

report_runtime() {
    printf 'diagnostic statuses run=%s report=%s option=%s reject=%s\n' \
        "$run_status" "$report_status" "$option_status" "$reject_status"
    for output in Run.out Run.err Report.out Report.err Option.out Option.err Reject.out Reject.err Invalid.wvb; do
        report_file "$test_directory/$output"
    done
}

if check_file "$candidate/Wvb-Runner.wvb" 446532 56b208d1f892f4bdd1d9c309bb6d4d46257d533a76d79d22efc8f83f27896fbe &&
    check_file "$candidate/windows-x64-wvrun.exe" 5366784 063de8f1fadcf9c37e9cef6526d628b410fa0cd21067fe6f3c795b97623cb519 &&
    check_file "$candidate/linux-x64-wvrun.elf" 5365760 6e18c9c9480df40814b81244b3dcd039c8851ded646a240134d4e2969b9c2e71; then
    echo 'PASS candidate inventory'
    passed=$((passed + 1))
else
    echo 'FAIL candidate inventory'
    echo 'Tests: 1, Passed: 0, Failed: 1'
    exit 1
fi

test_directory=$(mktemp -d "${TMPDIR:-/tmp}/windvale-wvb-runner-reconstruction-test.XXXXXX") || exit 1
trap 'rm -rf -- "$test_directory"' EXIT HUP INT TERM
if ! check_runtime "$candidate/linux-x64-wvrun.elf"; then
    report_runtime
    echo 'FAIL current-host candidate preflight'
    echo 'Tests: 2, Passed: 1, Failed: 1'
    exit 1
fi
mkdir -- "$test_directory/Rebuilt" || exit 1

"$constructor" >"$test_directory/Usage.out" 2>"$test_directory/Usage.err"
usage_status=$?
printf '%s\n' 'Usage: ./Tools/Native/Construct-Wvb-Runner-Reconstruction.sh <existing-output-directory>' >"$test_directory/Expected-Usage.err"
if [[ $usage_status -eq 64 && ! -s $test_directory/Usage.out ]] &&
    cmp -s "$test_directory/Usage.err" "$test_directory/Expected-Usage.err" &&
    "$constructor" "$test_directory/Rebuilt" >"$test_directory/Construct.out" 2>"$test_directory/Construct.err" &&
    [[ ! -s $test_directory/Construct.err ]] &&
    [[ $(grep -Fxc 'native WVB runner reconstruction status=Complete artifacts=3' "$test_directory/Construct.out") -eq 1 ]] &&
    cmp -s "$test_directory/Rebuilt/Wvb-Runner.wvb" "$candidate/Wvb-Runner.wvb" &&
    cmp -s "$test_directory/Rebuilt/windows-x64-wvrun.exe" "$candidate/windows-x64-wvrun.exe" &&
    cmp -s "$test_directory/Rebuilt/linux-x64-wvrun.elf" "$candidate/linux-x64-wvrun.elf"; then
    echo 'PASS exact source-built paired reconstruction'
    passed=$((passed + 1))
else
    [[ ! -f "$test_directory/Construct.out" ]] || cat -- "$test_directory/Construct.out"
    [[ ! -f "$test_directory/Construct.err" ]] || cat -- "$test_directory/Construct.err" >&2
    echo 'FAIL exact source-built paired reconstruction'
    failed=$((failed + 1))
fi

if check_runtime "$test_directory/Rebuilt/linux-x64-wvrun.elf"; then
    echo 'PASS current-host execution reporting and rejection'
    passed=$((passed + 1))
else
    report_runtime
    echo 'FAIL current-host execution reporting and rejection'
    failed=$((failed + 1))
fi

echo "Tests: $((passed + failed)), Passed: $passed, Failed: $failed"
[[ $failed -eq 0 ]]
