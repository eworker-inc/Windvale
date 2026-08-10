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

if check_file "$candidate/Wvb-Runner.wvb" 90009 3b881147e5e6c8298cf249e6e02c9f18ed4a677d49ef0a307427465795a1c626 &&
    check_file "$candidate/Wvb-Runner.wvo" 761854 e92eed5006a7a98609173c0ed73e66a7aec5e152d8556c9174cab928b946a505 &&
    check_file "$candidate/windows-x64-wvrun.exe" 778240 578ddd302da5fbd8d8e14c9410787f5aa05378429a1aca738ee2057e2f9ac1a5 &&
    check_file "$candidate/linux-x64-wvrun.elf" 778240 16f39270c239609c6f58b086d0648609fad46860ba9bdd198fa7e6668b628047; then
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
    echo 'PASS exact retained-WVB paired reconstruction'
    passed=$((passed + 1))
else
    echo 'FAIL exact retained-WVB paired reconstruction'
    failed=$((failed + 1))
fi

cp -- "$invalid_fixture" "$test_directory/Invalid.wvb" || exit 1
"$test_directory/Rebuilt/linux-x64-wvrun.elf" "$fixture" >"$test_directory/Run.out" 2>"$test_directory/Run.err"
run_status=$?
"$test_directory/Rebuilt/linux-x64-wvrun.elf" "$test_directory/Invalid.wvb" >"$test_directory/Reject.out" 2>"$test_directory/Reject.err"
reject_status=$?
if [[ $run_status -eq 0 && $reject_status -eq 1 ]] &&
    check_file "$fixture" 174 7933c4ba0cb854477a95750966f9532c2b9eb5888e55ec9ae64ebdf552a08f31 &&
    check_file "$test_directory/Run.out" 11 bf24325cd27b27403c7b8053820193dcce360f640f7f394742b660ce5fe3cd4e &&
    [[ ! -s $test_directory/Run.err && ! -s $test_directory/Reject.out ]] &&
    check_file "$test_directory/Reject.err" 53 a2e698719194d86fe8d449d741af6b00bad06930727af6b513d23da909f1d28e &&
    check_file "$test_directory/Invalid.wvb" 479 0d1829bbbc77f3ee3910a70f98528e1078117480332adb5a2d09df8b2d25f3b5; then
    echo 'PASS current-host execution and rejection'
    passed=$((passed + 1))
else
    echo 'FAIL current-host execution and rejection'
    failed=$((failed + 1))
fi

echo "Tests: $((passed + failed)), Passed: $passed, Failed: $failed"
[[ $failed -eq 0 ]]
