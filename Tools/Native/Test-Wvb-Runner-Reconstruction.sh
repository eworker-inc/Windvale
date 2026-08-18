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

if check_file "$candidate/Wvb-Runner.wvb" 151488 e5948f52146a5c3be9901e2dc8c3b9e4f1ba7b2fdc75624c43f2a3a7b807d264 &&
    check_file "$candidate/Wvb-Runner.wvo" 1371883 f482eface9f6857e6a851a4503b343c6c848aa99fdbe28385aa951bc8e463905 &&
    check_file "$candidate/windows-x64-wvrun.exe" 1387008 57b91dae115d14da470b265f3ce1f59a44fe94c06f0de4ae99b1c13418118ae4 &&
    check_file "$candidate/linux-x64-wvrun.elf" 1388544 b6914c6b4d5c3bb069b219ce2cb329b179faf032c8b204648628775fbdfbd25e; then
    echo 'PASS candidate inventory'
    passed=$((passed + 1))
else
    echo 'FAIL candidate inventory'
    failed=$((failed + 1))
fi

test_directory=$(mktemp -d "${TMPDIR:-/tmp}/windvale-wvb-runner-reconstruction-test.XXXXXX") || exit 1
trap 'rm -rf -- "$test_directory"' EXIT HUP INT TERM
mkdir -- "$test_directory/Rebuilt" || exit 1

"$constructor" >"$test_directory/Usage.out" 2>"$test_directory/Usage.err"
usage_status=$?
printf '%s\n' 'Usage: ./Tools/Native/Construct-Wvb-Runner-Reconstruction.sh <existing-output-directory>' >"$test_directory/Expected-Usage.err"
if [[ $usage_status -eq 64 && ! -s $test_directory/Usage.out ]] &&
    cmp -s "$test_directory/Usage.err" "$test_directory/Expected-Usage.err" &&
    "$constructor" "$test_directory/Rebuilt" >"$test_directory/Construct.out" 2>"$test_directory/Construct.err" &&
    [[ ! -s $test_directory/Construct.err ]] &&
    [[ $(grep -Fxc 'native WVB runner reconstruction status=Complete artifacts=4' "$test_directory/Construct.out") -eq 1 ]] &&
    cmp -s "$test_directory/Rebuilt/Wvb-Runner.wvb" "$candidate/Wvb-Runner.wvb" &&
    cmp -s "$test_directory/Rebuilt/Wvb-Runner.wvo" "$candidate/Wvb-Runner.wvo" &&
    cmp -s "$test_directory/Rebuilt/windows-x64-wvrun.exe" "$candidate/windows-x64-wvrun.exe" &&
    cmp -s "$test_directory/Rebuilt/linux-x64-wvrun.elf" "$candidate/linux-x64-wvrun.elf"; then
    echo 'PASS exact source-built paired reconstruction'
    passed=$((passed + 1))
else
    echo 'FAIL exact source-built paired reconstruction'
    failed=$((failed + 1))
fi

cp -- "$invalid_fixture" "$test_directory/Invalid.wvb" || exit 1
"$test_directory/Rebuilt/linux-x64-wvrun.elf" "$fixture" >"$test_directory/Run.out" 2>"$test_directory/Run.err"
run_status=$?
"$test_directory/Rebuilt/linux-x64-wvrun.elf" "$fixture" --report-steps >"$test_directory/Report.out" 2>"$test_directory/Report.err"
report_status=$?
"$test_directory/Rebuilt/linux-x64-wvrun.elf" "$fixture" --unknown >"$test_directory/Option.out" 2>"$test_directory/Option.err"
option_status=$?
"$test_directory/Rebuilt/linux-x64-wvrun.elf" "$test_directory/Invalid.wvb" >"$test_directory/Reject.out" 2>"$test_directory/Reject.err"
reject_status=$?
if [[ $run_status -eq 0 && $report_status -eq 0 && $option_status -eq 64 && $reject_status -eq 1 ]] &&
    check_file "$fixture" 174 7933c4ba0cb854477a95750966f9532c2b9eb5888e55ec9ae64ebdf552a08f31 &&
    check_file "$test_directory/Run.out" 11 bf24325cd27b27403c7b8053820193dcce360f640f7f394742b660ce5fe3cd4e &&
    check_file "$test_directory/Report.out" 27 16d83153e975eefdac7828db275b4cbd3cdd4a783ed5430c442ed4717936a3e5 &&
    check_file "$test_directory/Option.err" 43 fd8455c7428eece156befe036c10c6927efee163a7315dad72c730f6e2bcef64 &&
    [[ ! -s $test_directory/Run.err && ! -s $test_directory/Report.err && ! -s $test_directory/Option.out && ! -s $test_directory/Reject.out ]] &&
    check_file "$test_directory/Reject.err" 53 a2e698719194d86fe8d449d741af6b00bad06930727af6b513d23da909f1d28e &&
    check_file "$test_directory/Invalid.wvb" 479 0d1829bbbc77f3ee3910a70f98528e1078117480332adb5a2d09df8b2d25f3b5; then
    echo 'PASS current-host execution reporting and rejection'
    passed=$((passed + 1))
else
    echo 'FAIL current-host execution reporting and rejection'
    failed=$((failed + 1))
fi

echo "Tests: $((passed + failed)), Passed: $passed, Failed: $failed"
[[ $failed -eq 0 ]]
