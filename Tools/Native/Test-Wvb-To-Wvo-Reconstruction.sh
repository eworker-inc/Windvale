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
    echo "FAIL  WVB-to-WVO reconstruction phase=${phase:-setup}"
    if [[ -n ${test_directory:-} && -f $test_directory/Metadata-Verifier.out ]]; then
        echo "DETAIL metadata-verifier-report bytes=$(wc -c < "$test_directory/Metadata-Verifier.out")"
        cat -- "$test_directory/Metadata-Verifier.out"
    fi
    echo "Tests: $tests, Passed: $passed, Failed: $((tests - passed))"
    exit 1
}

check_file "$candidate/Wvb-To-Wvo.wvb" 522025 \
    318717a608ba37360b9c39f53b9720944ab4463af4ab6a1ec9a267a6ceb85bf6 || fail
check_file "$candidate/Wvb-To-Wvo.exe" 7491072 \
    85c07ef9f07b6b1351a5aa467c4e8f77de33099db9fce3c3adaf0a47191de0a3 || fail
check_file "$candidate/Wvb-To-Wvo.elf" 7491584 \
    deb75ead2af0d06d2357cdf88d8cf58fefd284bf4834e6489198b517f3a4908e || fail
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

phase=metadata-normalizer-build
"$script_directory/Build-Wvb.sh" \
    "$repository_root/Projects/Tests/Windvale-Wvb-Metadata-Normalization-Self-Test.wvproj" \
    "$test_directory/Metadata-Normalization.wvb" \
    >/dev/null 2>"$test_directory/Metadata-Normalization-Build.err" || fail
[[ ! -s $test_directory/Metadata-Normalization-Build.err ]] || fail
"$script_directory/Run-Wvb.sh" "$test_directory/Metadata-Normalization.wvb" \
    >"$test_directory/Metadata-Normalization.out" \
    2>"$test_directory/Metadata-Normalization.err" || fail
printf '%s\n' 'Result: 0' >"$test_directory/Metadata-Normalization-Expected.out" || fail
check_equal \
    "$test_directory/Metadata-Normalization.out" \
    "$test_directory/Metadata-Normalization-Expected.out" || fail
[[ ! -s $test_directory/Metadata-Normalization.err ]] || fail
pass 'portable metadata normalization'

phase=metadata-verifier-build
"$script_directory/Build-Wvb.sh" \
    "$repository_root/Projects/Tools/Windvale-Compiler-Wvb-Verifier.wvproj" \
    "$test_directory/Metadata-Verifier.wvb" \
    >/dev/null 2>"$test_directory/Metadata-Verifier-Build.err" || fail
[[ ! -s $test_directory/Metadata-Verifier-Build.err ]] || fail
phase=metadata-verifier-package
"$script_directory/Package-Hosted-Wvb.sh" 2 \
    "$test_directory/Metadata-Verifier.wvb" \
    "$test_directory/Metadata-Verifier.elf" linux \
    >"$test_directory/Metadata-Verifier-Package.out" \
    2>"$test_directory/Metadata-Verifier-Package.err" || fail
[[ ! -s $test_directory/Metadata-Verifier-Package.err ]] || fail
phase=metadata-verifier-execution
"$test_directory/Metadata-Verifier.elf" "$candidate/Metadata.wvb" \
    >"$test_directory/Metadata-Verifier.out" \
    2>"$test_directory/Metadata-Verifier.err" || fail
phase=metadata-verifier-report
printf '%s\n' 'wvb status=Valid profile=compiler-aligned' \
    >"$test_directory/Metadata-Verifier-Expected.out" || fail
check_equal \
    "$test_directory/Metadata-Verifier.out" \
    "$test_directory/Metadata-Verifier-Expected.out" || fail
[[ ! -s $test_directory/Metadata-Verifier.err ]] || fail
pass 'compiler-aligned metadata verification'

phase=lowerer-reconstruction
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
